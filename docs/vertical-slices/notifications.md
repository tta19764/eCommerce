# Notifications Slice

## General Description

The notifications slice owns durable notification jobs and email delivery. It is implemented in `NotificationApi` and uses Quartz to process pending jobs from PostgreSQL. SMTP sends email messages.

## Backend Projects

| Project | Responsibility |
| --- | --- |
| `NotificationApi.Api` | Service host and background processing runtime |
| `NotificationApi.Application` | Notification job processor and message consumers |
| `NotificationApi.Domain` | Notification job entity and status |
| `NotificationApi.Infrastructure` | EF Core, Quartz, SMTP sender, repositories |
| `NotificationApi.Messages` | Notification request contracts |

## Main Workflows

### Email Confirmation

Authentication publishes a confirmation request after registration. NotificationApi stores a notification job and sends an HTML email containing the frontend confirmation link.

The configured URL template supports `{accountId}` and `{email}` placeholders. Both replacement values are URI-escaped. Consumer redelivery is not deduplicated and can create another confirmation job.

### Marketplace Chat Messages

MessagingApi publishes `MessageSentIntegrationEvent` after a conversation message is saved. NotificationApi checks the recipient account through AuthenticationApi and queues an HTML email only when the email address is confirmed.

NotificationApi asks UserApi for recipient and sender display names. Missing profiles use generic names and do not block the email. Message-event redelivery is not deduplicated and can create another job. Order-status notification requests have the same duplicate-job limitation.

### Durable Retry

Failed notification jobs are stored with attempts, last error, and next-attempt time. Quartz selects the oldest due jobs up to the configured page size and processes them in sequence. The first four failures are retried after 30, 60, 120, and 240 seconds. The fifth failure changes the job to terminal `Failed` status.

Before SMTP delivery, the processor commits the job as `Processing`. It commits success or retry state after the send attempt. SMTP and PostgreSQL do not share a transaction. A service stop after the first commit can leave the job in `Processing`; the current poller selects only `Pending` jobs and does not recover that record automatically. A service stop after SMTP accepts a message but before the success commit can also cause duplicate delivery if the job is manually recovered.

Job state persists through AppHost restarts as long as the PostgreSQL volume/database is not cleared.

## Configuration

Notification settings are injected from AppHost and service configuration:

| Section | Purpose |
| --- | --- |
| `Email` | From address and confirmation URL template |
| `Smtp` | Host, port, SSL, username, password, from name, timeout |
| `BackgroundJobs:ProcessNotifications` | Quartz processing interval and page size |

Development can use Mailpit for local SMTP capture or a real SMTP provider such as Gmail with an app password.

## Secrets

Do not store real SMTP sender addresses, usernames, or passwords in committed appsettings files. AppHost reads them as Aspire parameters, so local development should supply them through environment variables or user secrets.

AppHost parameter keys:

| Value | Environment variable | User-secrets key |
| --- | --- | --- |
| Sender address | `Parameters__notification-from-address` | `Parameters:notification-from-address` |
| SMTP username | `Parameters__notification-smtp-user-name` | `Parameters:notification-smtp-user-name` |
| SMTP password | `Parameters__notification-smtp-password` | `Parameters:notification-smtp-password` |

PowerShell environment example:

```powershell
Set-Item -Path Env:'Parameters__notification-from-address' -Value '<sender-email>'
Set-Item -Path Env:'Parameters__notification-smtp-user-name' -Value '<smtp-user-name>'
Set-Item -Path Env:'Parameters__notification-smtp-password' -Value '<gmail-app-password-without-spaces>'
```

Use the exact double-underscore names from the table when setting persistent environment variables. For Gmail, use an app password without spaces.

When running `NotificationApi` directly without AppHost, use service configuration keys instead:

| Value | Environment variable |
| --- | --- |
| Sender address | `Email__FromAddress` |
| SMTP username | `Smtp__UserName` |
| SMTP password | `Smtp__Password` |

## Frontend Mapping

The frontend does not call NotificationApi directly. It receives the email link and then calls the Authentication API confirmation endpoint.
