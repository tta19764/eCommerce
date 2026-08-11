[CmdletBinding(SupportsShouldProcess = $true, ConfirmImpact = 'High')]
param(
    [string]$EnvironmentName = $env:ASPNETCORE_ENVIRONMENT,
    [switch]$Execute,
    [string]$Confirmation,
    [string]$PostgresContainer = 'postgres',
    [string]$PostgresUser = 'postgres',
    [string]$RabbitMqContainer = 'rabbitmq',
    [string]$RedisContainer = 'redis',
    [string]$KeycloakBaseUrl = 'http://localhost:8080',
    [string]$KeycloakRealm = 'ecommerce',
    [string]$KeycloakAdminUser = 'admin',
    [Security.SecureString]$KeycloakAdminPassword
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$requiredConfirmation = 'RESET-ECOMMERCE-DEVELOPMENT-DATA'
$databaseNames = @(
    'authentication_db',
    'user_db',
    'product_db',
    'order_db',
    'payment_db',
    'image_db',
    'messaging_db',
    'notification_db'
)

Write-Host 'Development data reset preview'
Write-Host "Environment: $EnvironmentName"
Write-Host "PostgreSQL container: $PostgresContainer"
Write-Host "Databases: $($databaseNames -join ', ')"
Write-Host "RabbitMQ container: $RabbitMqContainer (all queues will be purged)"
Write-Host "Redis container: $RedisContainer (all keys will be flushed)"
Write-Host "Keycloak realm: $KeycloakRealm (application users only; realm configuration is retained)"

if (-not $Execute)
{
    Write-Host "Preview only. Re-run with -Execute -Confirmation '$requiredConfirmation' and the required credentials."
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

if ($null -eq $KeycloakAdminPassword)
{
    throw 'KeycloakAdminPassword is required for execution and must be supplied as a SecureString.'
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

foreach ($databaseName in $databaseNames)
{
    # Database names come exclusively from the fixed allow-list above. Active service connections
    # are terminated before DROP DATABASE so a partially running AppHost cannot retain legacy rows.
    $sql = "SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = '$databaseName' AND pid <> pg_backend_pid(); DROP DATABASE IF EXISTS `"$databaseName`"; CREATE DATABASE `"$databaseName`";"
    docker exec $PostgresContainer psql -v ON_ERROR_STOP=1 -U $PostgresUser -d postgres -c $sql
    if ($LASTEXITCODE -ne 0)
    {
        throw "Failed to recreate database '$databaseName'."
    }
}

$queueNames = docker exec $RabbitMqContainer rabbitmqctl list_queues --quiet name
if ($LASTEXITCODE -ne 0)
{
    throw 'Failed to enumerate RabbitMQ queues.'
}

foreach ($queueName in $queueNames)
{
    if (-not [string]::IsNullOrWhiteSpace($queueName))
    {
        docker exec $RabbitMqContainer rabbitmqctl purge_queue $queueName
        if ($LASTEXITCODE -ne 0)
        {
            throw "Failed to purge RabbitMQ queue '$queueName'."
        }
    }
}

docker exec $RedisContainer redis-cli FLUSHALL
if ($LASTEXITCODE -ne 0)
{
    throw 'Failed to flush Redis.'
}

$passwordPointer = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($KeycloakAdminPassword)
try
{
    $plainPassword = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($passwordPointer)
    $tokenResponse = Invoke-RestMethod -Method Post `
        -Uri "$KeycloakBaseUrl/realms/master/protocol/openid-connect/token" `
        -ContentType 'application/x-www-form-urlencoded' `
        -Body @{
            client_id = 'admin-cli'
            grant_type = 'password'
            username = $KeycloakAdminUser
            password = $plainPassword
        }

    $headers = @{ Authorization = "Bearer $($tokenResponse.access_token)" }
    $users = Invoke-RestMethod -Method Get `
        -Uri "$KeycloakBaseUrl/admin/realms/$KeycloakRealm/users?max=10000" `
        -Headers $headers

    foreach ($user in $users)
    {
        if ($user.username -ne $KeycloakAdminUser -and -not $user.username.StartsWith('service-account-'))
        {
            Invoke-RestMethod -Method Delete `
                -Uri "$KeycloakBaseUrl/admin/realms/$KeycloakRealm/users/$($user.id)" `
                -Headers $headers
        }
    }
}
finally
{
    if ($passwordPointer -ne [IntPtr]::Zero)
    {
        [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($passwordPointer)
    }

    $plainPassword = $null
}

Write-Host 'Reset completed. Restart AppHost to apply migrations and run the opt-in administrator bootstrap.'
