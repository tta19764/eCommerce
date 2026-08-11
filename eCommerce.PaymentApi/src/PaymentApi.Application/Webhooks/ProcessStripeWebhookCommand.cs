using SharedLibrary.Application.Abstractions.Messaging;

namespace PaymentApi.Application.Webhooks;

/// <summary>Carries the exact raw Stripe request body and signature header for verification.</summary>
public sealed record ProcessStripeWebhookCommand(string Payload, string Signature) : ICommand;
