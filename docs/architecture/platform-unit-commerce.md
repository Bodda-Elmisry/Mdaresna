# Platform unit commerce — first slice

This slice adds a Platform-owned unit catalog and a separate unit-purchase workflow without treating every generic school-to-platform payment as a unit purchase. It does not change the legacy coin balance or the Flutter application.

## Ownership and workflow

1. A Platform staff member with `platform.billing.manage` defines a global unit type: stable code, display name, positive unit price, ISO currency, and active state. Editing the offer increments its version; deactivation prevents new purchases. Existing purchases keep their original price snapshot.
2. A Platform backoffice user submits a unit-purchase request for a registered school. This is a temporary entry point until the School App and school-scoped authorization exist. The request specifies the unit type and expected offer version, quantity, payment method code, transfer date/reference, and tenant/school ID. The server calculates `amount = quantity × unit price`; callers cannot set the credited quantity or override the total. A client-supplied request ID makes identical submissions idempotent.
3. The normal payment review endpoint verifies the reported transfer. Rejection records the decision but grants no units. Approval creates the financial ledger entry and exactly one immutable `UnitGrant`, records the audit action, and stages `SchoolUnitsGrantedV1` in the Platform outbox. Review runs in one database transaction with two flushes: the payment decision first, then ledger/grant/audit/outbox, followed by one commit. If either flush fails, the whole transaction rolls back. A previously reviewed request cannot be approved again.
4. `SchoolUnitsGrantedV1` carries `grantId`, `paymentRequestId`, tenant/school ID, unit type ID/code/name, quantity, unit price, amount/currency, payment method, transfer reference/time, and grant time. The event is school-scoped. The future School App must use `grantId` as a unique inbox/idempotency key, then credit its school-owned balance by `quantity` in one local transaction. Student activation/consumption remains School App business logic.

The current Platform Worker has no RabbitMQ publisher, and the School App/consumer does not exist yet. Therefore approval **persists an event ready for RabbitMQ**, but does not publish to a broker or credit a school balance today. Delivery, retry/dead-letter behavior, consumer inbox, and balance reconciliation are subsequent integration work. Do not present a staged outbox row as proof of broker delivery.

## Data boundaries

`billing.unit_types` owns the catalog; `billing.unit_purchase_intents` stores the one-to-one immutable purchase/price snapshot attached to a pending payment; `billing.unit_grants` stores the unique entitlement associated with its approved payment. Platform does not directly update a School database. This prevents the old amount-as-unit-count behavior and avoids database-to-database synchronization. Generic `billing.school_platform_payment_requests` without a unit intent continue to produce only their existing review event.

The fixed-price-per-unit rule is an explicit first-slice assumption based on the old `CoinType.Value` field. If the business actually prices bundles or determines the quantity manually after approval, change the contract and workflow before connecting the School consumer; do not silently reinterpret already-issued grants.
