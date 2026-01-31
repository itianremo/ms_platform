using System;

namespace Shared.Messaging.Events;

public record PaymentSucceededEvent(
    Guid UserId,
    Guid AppId,
    Guid PackageId,
    string TransactionId,
    decimal Amount,
    string Currency,
    string FullResponseJson
);
