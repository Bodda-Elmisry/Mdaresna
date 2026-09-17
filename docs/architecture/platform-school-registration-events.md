# School registration requests consumed by Platform

The Schools application submits a registration request by publishing a persistent
`SchoolRegistrationRequestedV2` envelope. The Platform Worker consumes that fact,
resolves the owner by the global phone login identifier (or creates a pending
shared Identity account), creates the matching tenant and school control-plane
records, and moves the school directly to `PendingVerification` for operator review.

## RabbitMQ topology

| Item | Value |
| --- | --- |
| Exchange | `mdaresna.school-registry` (durable topic) |
| Routing key | `school.registration.requested.v2` |
| Platform queue | `platform.school-registration-requested.v2` |
| Dead-letter exchange | `mdaresna.school-registry.dead` |
| Dead-letter queue | `platform.school-registration-requested.v2.dead` |

The producer must be exactly `schools`, and the envelope must use tenant scope
only. `scope.tenantId` must equal `data.tenantId`; school and branch scope values
must be absent because the school does not exist in Platform before consumption.

The v2 payload carries the registration request ID, tenant ID, school code, name,
type, address, primary phone, owner name and phone, plus the UTC request timestamp.
The owner phone is normalized to 8-16 ASCII digits. Existing shared accounts are
reused and new accounts start as `PendingVerification`.

## Reliability and processing

The consumer uses manual acknowledgements with prefetch one. It writes the tenant,
school, lifecycle audit/outbox records and `messaging.inbox_messages` receipt in a
single Platform-database transaction. RabbitMQ is acknowledged only after commit.
A repeated message ID is acknowledged without applying the request again; the
business registration request ID also guards replay with changed data.

Malformed contracts, wrong producer/scope, invalid owner data, or conflicting
request data go to the one-hour dead-letter queue. Transient
database failures are requeued. Event bodies and broker credentials must never be
written to logs.

## Configuration

The consumer is disabled by default outside Development. Configure both hosts
using deployment environment variables or user secrets:

```text
SchoolRegistrationConsumer__Enabled=true
SchoolRegistrationConsumer__BrokerUri=amqps://<user>:<password>@<host>/<vhost>
SchoolRegistrationPublisher__BrokerUri=amqps://<user>:<password>@<host>/<vhost>
```

Do not commit the broker URI. Grant the Schools publisher write access only to the
registration routing key, and grant the Platform Worker read access only to its
queue. The API uses persistent delivery, mandatory routing and publisher confirms;
it returns `202 Accepted` only after RabbitMQ confirms the message.
