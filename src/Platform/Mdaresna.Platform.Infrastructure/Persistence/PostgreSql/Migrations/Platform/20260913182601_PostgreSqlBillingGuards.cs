using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mdaresna.Platform.Infrastructure.Persistence.PostgreSql.Migrations.Platform
{
    /// <inheritdoc />
    public partial class PostgreSqlBillingGuards : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE FUNCTION billing.reject_append_only_change() RETURNS trigger
                LANGUAGE plpgsql AS $body$
                BEGIN
                    RAISE EXCEPTION '% is append-only', TG_TABLE_NAME USING ERRCODE = '23514';
                END
                $body$;

                CREATE TRIGGER reject_payment_ledger_change
                    BEFORE UPDATE OR DELETE ON billing.school_platform_payment_ledger
                    FOR EACH ROW EXECUTE FUNCTION billing.reject_append_only_change();
                CREATE TRIGGER reject_unit_intent_change
                    BEFORE UPDATE OR DELETE ON billing.unit_purchase_intents
                    FOR EACH ROW EXECUTE FUNCTION billing.reject_append_only_change();
                CREATE TRIGGER reject_unit_grant_change
                    BEFORE UPDATE OR DELETE ON billing.unit_grants
                    FOR EACH ROW EXECUTE FUNCTION billing.reject_append_only_change();

                CREATE FUNCTION billing.guard_payment_request_change() RETURNS trigger
                LANGUAGE plpgsql AS $body$
                BEGIN
                    IF TG_OP = 'DELETE' THEN
                        RAISE EXCEPTION 'Payment requests cannot be deleted' USING ERRCODE = '23514';
                    END IF;
                    IF OLD."Status" <> 'Pending'
                       OR NEW."Status" NOT IN ('Approved', 'Rejected')
                       OR NEW."TenantId" IS DISTINCT FROM OLD."TenantId"
                       OR NEW."SchoolId" IS DISTINCT FROM OLD."SchoolId"
                       OR NEW."Amount" IS DISTINCT FROM OLD."Amount"
                       OR NEW."Currency" IS DISTINCT FROM OLD."Currency"
                       OR NEW."TransferReference" IS DISTINCT FROM OLD."TransferReference"
                       OR NEW."RequestedByAccountId" IS DISTINCT FROM OLD."RequestedByAccountId"
                       OR NEW."RequestedAtUtc" IS DISTINCT FROM OLD."RequestedAtUtc" THEN
                        RAISE EXCEPTION 'Payment request may transition only once from Pending'
                            USING ERRCODE = '23514';
                    END IF;
                    IF NEW."Status" <> 'Approved' AND EXISTS (
                        SELECT 1 FROM billing.school_platform_payment_ledger AS ledger
                        WHERE ledger."PaymentRequestId" = NEW."Id") THEN
                        RAISE EXCEPTION 'Posted payment must remain approved' USING ERRCODE = '23514';
                    END IF;
                    RETURN NEW;
                END
                $body$;
                CREATE TRIGGER guard_payment_request_change
                    BEFORE UPDATE OR DELETE ON billing.school_platform_payment_requests
                    FOR EACH ROW EXECUTE FUNCTION billing.guard_payment_request_change();

                CREATE FUNCTION billing.require_approved_ledger_request() RETURNS trigger
                LANGUAGE plpgsql AS $body$
                BEGIN
                    PERFORM 1 FROM billing.school_platform_payment_requests AS request
                    WHERE request."Id" = NEW."PaymentRequestId"
                      AND request."Status" = 'Approved'
                      AND request."TenantId" = NEW."TenantId"
                      AND request."SchoolId" = NEW."SchoolId"
                      AND request."Amount" = NEW."Amount"
                      AND request."Currency" = NEW."Currency"
                      AND request."TransferReference" = NEW."TransferReference"
                    FOR SHARE;
                    IF NOT FOUND THEN
                        RAISE EXCEPTION 'Ledger entry requires matching approved request'
                            USING ERRCODE = '23514';
                    END IF;
                    RETURN NEW;
                END
                $body$;
                CREATE TRIGGER require_approved_ledger_request
                    BEFORE INSERT ON billing.school_platform_payment_ledger
                    FOR EACH ROW EXECUTE FUNCTION billing.require_approved_ledger_request();

                CREATE FUNCTION billing.require_pending_unit_intent() RETURNS trigger
                LANGUAGE plpgsql AS $body$
                BEGIN
                    PERFORM 1 FROM billing.school_platform_payment_requests AS request
                    WHERE request."Id" = NEW."PaymentRequestId" AND request."Status" = 'Pending'
                    FOR SHARE;
                    IF NOT FOUND THEN
                        RAISE EXCEPTION 'Unit purchase intent requires pending request'
                            USING ERRCODE = '23514';
                    END IF;
                    RETURN NEW;
                END
                $body$;
                CREATE TRIGGER require_pending_unit_intent
                    BEFORE INSERT ON billing.unit_purchase_intents
                    FOR EACH ROW EXECUTE FUNCTION billing.require_pending_unit_intent();

                CREATE FUNCTION billing.require_approved_unit_grant() RETURNS trigger
                LANGUAGE plpgsql AS $body$
                BEGIN
                    PERFORM 1 FROM billing.school_platform_payment_requests AS request
                    WHERE request."Id" = NEW."PaymentRequestId"
                      AND request."Status" = 'Approved'
                      AND request."ReviewedAtUtc" IS NOT NULL
                      AND NEW."GrantedAtUtc" >= request."ReviewedAtUtc"
                    FOR SHARE;
                    IF NOT FOUND THEN
                        RAISE EXCEPTION 'Unit grant requires reviewed approved request'
                            USING ERRCODE = '23514';
                    END IF;
                    RETURN NEW;
                END
                $body$;
                CREATE TRIGGER require_approved_unit_grant
                    BEFORE INSERT ON billing.unit_grants
                    FOR EACH ROW EXECUTE FUNCTION billing.require_approved_unit_grant();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS require_approved_unit_grant ON billing.unit_grants;
                DROP TRIGGER IF EXISTS require_pending_unit_intent ON billing.unit_purchase_intents;
                DROP TRIGGER IF EXISTS require_approved_ledger_request ON billing.school_platform_payment_ledger;
                DROP TRIGGER IF EXISTS guard_payment_request_change ON billing.school_platform_payment_requests;
                DROP TRIGGER IF EXISTS reject_unit_grant_change ON billing.unit_grants;
                DROP TRIGGER IF EXISTS reject_unit_intent_change ON billing.unit_purchase_intents;
                DROP TRIGGER IF EXISTS reject_payment_ledger_change ON billing.school_platform_payment_ledger;
                DROP FUNCTION IF EXISTS billing.require_approved_unit_grant();
                DROP FUNCTION IF EXISTS billing.require_pending_unit_intent();
                DROP FUNCTION IF EXISTS billing.require_approved_ledger_request();
                DROP FUNCTION IF EXISTS billing.guard_payment_request_change();
                DROP FUNCTION IF EXISTS billing.reject_append_only_change();
                """);
        }
    }
}
