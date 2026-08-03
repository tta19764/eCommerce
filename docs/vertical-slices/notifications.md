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

### Durable Retry

Failed notification jobs are stored with attempts, last error, and next-attempt time. Quartz polls pending jobs and retries them until the maximum attempt count is reached.

Job state persists through AppHost restarts as long as the PostgreSQL volume/database is not cleared.

## Configuration

Notification settings are injected from AppHost and service configuration:

| Section | Purpose |
| --- | --- |
| `Email` | From address and confirmation URL template |
| `Smtp` | Host, port, SSL, username, password, from name, timeout |
| `BackgroundJobs:ProcessNotifications` | Quartz processing interval and page size |

Development can use Mailpit for local SMTP capture or a real SMTP provider such as Gmail with an app password.

## Frontend Mapping

The frontend does not call NotificationApi directly. It receives the email link and then calls the Authentication API confirmation endpoint.
