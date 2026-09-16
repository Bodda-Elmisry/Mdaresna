# School registration requests consumed by Platform

The Schools application submits a registration request by publishing a persistent
`SchoolRegistrationRequestedV1` envelope. The Platform Worker consumes that fact,
creates the matching tenant and school control-plane records, and moves the school
directly to `PendingVerification` for operator review.

## RabbitMQ topology

| Item | Value |
| --- | --- |
| Exchange | `mdaresna.school-registry` (durable topic) |
| Routing key | `school.registration.requested` |
| Platform queue | `platform.school-registration-requested.v1` |
| Dead-letter exchange | `mdaresna.school-registry.dead` |
| Dead-letter queue | `platform.school-registration-requested.v1.dead` |

The producer must be exactly `schools`, and the envelope must use tenant scope
only. `scope.tenantId` must equal `data.tenantId`; school and branch scope values
must be absent because the school does not exist in Platform before consumption.

The v1 payload carries the registration request ID, tenant ID, requester global
Identity account ID, school code/name/type/deployment mode, optional legal name,
address and unit type, plus the UTC request timestamp. The Platform verifies that
the requester exists in the shared Identity database and that an optional unit
type is active.

## Reliability and processing

The consumer uses manual acknowledgements with prefetch one. It writes the tenant,
school, lifecycle audit/outbox records and `messaging.inbox_messages` receipt in a
single Platform-database transaction. RabbitMQ is acknowledged only after commit.
A repeated message ID is acknowledged without applying the request again; the
business registration request ID also guards replay with changed data.

Malformed contracts, wrong producer/scope, missing requester, invalid unit type,
or conflicting request data go to the one-hour dead-letter queue. Transient
database failures are requeued. Event bodies and broker credentials must never be
written to logs.

## Configuration

The consumer is disabled by default until the Schools outbox publisher is live.
Enable it in the Platform Worker using deployment environment variables or user
secrets:

```text
SchoolRegistrationConsumer__Enabled=true
SchoolRegistrationConsumer__BrokerUri=amqps://<user>:<password>@<host>/<vhost>
```

Do not commit the broker URI. Grant the Schools publisher write access only to the
registration routing key, and grant the Platform Worker read access only to its
queue. The producer still needs publisher confirms, mandatory routing, and its own
transactional outbox before the flow is end-to-end complete.
