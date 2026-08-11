[CmdletBinding(SupportsShouldProcess = $true, ConfirmImpact = 'High')]
param(
    [string]$EnvironmentName = $env:ASPNETCORE_ENVIRONMENT,
    [switch]$Execute,
    [string]$Confirmation,
    [string]$PostgresContainer = 'postgres',
    [string]$PostgresUser = 'postgres',
    [string]$RabbitMqContainer = 'rabbitmq',
    [string]$RedisContainer = 'redis'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$requiredConfirmation = 'RESET-ECOMMERCE-DEVELOPMENT-DATA'
$databaseNames = @(
    'order_db',
    'payment_db',
    'messaging_db',
    'notification_db'
)

Write-Host 'Development data reset preview'
Write-Host "Environment: $EnvironmentName"
Write-Host "PostgreSQL container: $PostgresContainer"
Write-Host "Databases: $($databaseNames -join ', ')"
Write-Host "RabbitMQ container: $RabbitMqContainer (all queues will be purged)"
Write-Host "Redis container: $RedisContainer (all keys will be flushed)"
Write-Host 'Preserved state: Keycloak, authentication_db, user_db, product_db, image_db'

if (-not $Execute)
{
    Write-Host "Preview only. Re-run with -Execute -Confirmation '$requiredConfirmation'."
    return
}

if ($EnvironmentName -ne 'Development')
{
    throw 'The reset tool runs only when EnvironmentName is exactly Development.'
}

if ($Confirmation -cne $requiredConfirmation)
{
    throw "Execution requires the exact confirmation token '$requiredConfirmation'."
}

foreach ($container in @($PostgresContainer, $RabbitMqContainer, $RedisContainer))
{
    $running = docker inspect --format '{{.State.Running}}' $container 2>$null
    if ($LASTEXITCODE -ne 0 -or $running -ne 'true')
    {
        throw "Required container '$container' is not running. Pass its exact Aspire/Docker container name."
    }
}

if (-not $PSCmdlet.ShouldProcess('all local eCommerce application state', 'irreversibly reset'))
{
    return
}

function Invoke-PostgresCommand
{
    param([Parameter(Mandatory)][string]$Sql)

    # Aspire already injects POSTGRES_PASSWORD into the PostgreSQL container. Expand it inside the
    # container so the secret is not required as a script argument or exposed in the host command.
    docker exec $PostgresContainer sh -c `
        'PGPASSWORD="$POSTGRES_PASSWORD" psql -v ON_ERROR_STOP=1 -U "$1" -d postgres -c "$2"' `
        -- $PostgresUser $Sql

    if ($LASTEXITCODE -ne 0)
    {
        throw 'A PostgreSQL reset command failed.'
    }
}

foreach ($databaseName in $databaseNames)
{
    # Database names come exclusively from the fixed allow-list above. Active service connections
    # are terminated before DROP DATABASE so a partially running AppHost cannot retain legacy rows.
    try
    {
        Invoke-PostgresCommand `
            -Sql "SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = '$databaseName' AND pid <> pg_backend_pid();"
        Invoke-PostgresCommand -Sql "DROP DATABASE IF EXISTS `"$databaseName`";"
        Invoke-PostgresCommand -Sql "CREATE DATABASE `"$databaseName`";"
    }
    catch
    {
        throw "Failed to recreate database '$databaseName'. $($_.Exception.Message)"
    }
}

$queueNames = docker exec $RabbitMqContainer rabbitmqctl list_queues --quiet name
if ($LASTEXITCODE -ne 0)
{
    throw 'Failed to enumerate RabbitMQ queues.'
}

foreach ($queueName in $queueNames)
{
    $normalizedQueueName = $queueName.Trim()

    # Some RabbitMQ CLI versions emit the requested column name even with --quiet. It is a header,
    # not a real queue, and attempting to purge it produces a misleading not-found failure.
    if (-not [string]::IsNullOrWhiteSpace($normalizedQueueName) -and $normalizedQueueName -ne 'name')
    {
        docker exec $RabbitMqContainer rabbitmqctl purge_queue $normalizedQueueName
        if ($LASTEXITCODE -ne 0)
        {
            throw "Failed to purge RabbitMQ queue '$normalizedQueueName'."
        }
    }
}

docker exec $RedisContainer redis-cli FLUSHALL
if ($LASTEXITCODE -ne 0)
{
    throw 'Failed to flush Redis.'
}

Write-Host 'Reset completed. Restart AppHost to apply migrations to the recreated databases.'
