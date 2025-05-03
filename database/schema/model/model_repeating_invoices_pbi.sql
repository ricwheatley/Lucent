
CREATE SCHEMA IF NOT EXISTS model;

CREATE OR REPLACE VIEW model.repeating_invoices AS
SELECT
    r.repeating_invoice_id AS "Repeating Invoice Id",
    r.start_date AS "Start Date",
    r.next_scheduled_date AS "Next Scheduled Date",
    r.end_date AS "End Date",
    r.schedule AS "Schedule",
    r.status AS "Status",
    r.total AS "Total Amount",
    r.updated_date_utc AS "Repeating Invoice Updated",
    l.id AS "Line Id",
    l.description AS "Line Description",
    l.quantity AS "Line Quantity",
    l.unit_amount AS "Line Unit Amount",
    l.tax_type AS "Line Tax Type",
    l.tax_amount AS "Line Tax Amount",
    l.line_amount AS "Line Total",
    l.account_code AS "Line Account Code",
    c.contact_id AS "Customer Id",
    c.name AS "Customer Name",
    c.email_address AS "Customer Email"
FROM ods.repeating_invoices r
LEFT JOIN ods.repeating_invoice_lines l ON r.repeating_invoice_id = l.repeating_invoice_id
LEFT JOIN ods.contacts c ON r.contact_id = c.contact_id;
