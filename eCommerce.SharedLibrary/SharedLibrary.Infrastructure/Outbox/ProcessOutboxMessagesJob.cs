using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Quartz;
using SharedLibrary.Domain.Abstractions;

namespace SharedLibrary.Infrastructure.Outbox;

/// <summary>
/// Quartz job that publishes persisted domain events from a service outbox table.
/// </summary>
/// <typeparam name="TContext">The service DbContext type that owns the outbox table.</typeparam>
[DisallowConcurrentExecution]
public sealed class ProcessOutboxMessagesJob<TContext>(
    TContext dbContext,
    IPublisher publisher,
    IOptions<ProcessOutboxMessagesOptions> options,
    ILogger<ProcessOutboxMessagesJob<TContext>> logger) : IJob
    where TContext : DbContext, IOutboxDbContext
{
    private static readonly JsonSerializerSettings JsonSerializerSettings = new()
    {
        TypeNameHandling = TypeNameHandling.All
    };

    /// <inheritdoc />
    public async Task Execute(IJobExecutionContext context)
    {
        var messages = await dbContext.OutboxMessages
            .Where(message => message.ProcessedOnUtc == null)
            .OrderBy(message => message.OccurredOnUtc)
            .Take(options.Value.PageSize)
            .ToListAsync(context.CancellationToken);

        foreach (var message in messages)
        {
            await ProcessMessageAsync(message, context.CancellationToken);
        }

        if (messages.Count > 0)
        {
            await dbContext.SaveChangesAsync(context.CancellationToken);

            logger.LogInformation("Processed {ProcessedOutboxMessageCount} outbox messages", messages.Count);
        }
    }

    private async Task ProcessMessageAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        try
        {
            var domainEvent = JsonConvert.DeserializeObject<IDomainEvent>(
                message.Content,
                JsonSerializerSettings);

            if (domainEvent is null)
            {
                message.MarkFailed("Outbox content could not be deserialized into a domain event.");
                return;
            }

            await publisher.Publish(domainEvent, cancellationToken);
            message.MarkProcessed(DateTime.UtcNow);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to process outbox message {OutboxMessageId}", message.Id);
            message.MarkFailed(exception.ToString());
        }
    }
}
