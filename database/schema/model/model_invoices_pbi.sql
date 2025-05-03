
CREATE SCHEMA IF NOT EXISTS model;

CREATE OR REPLACE VIEW model.invoices AS
SELECT
    i.invoice_id AS "Invoice Id",
    i.date AS "Invoice Date",
    i.due_date AS "Due Date",
    i.reference AS "Reference",
    i.status AS "Status",
    i.type AS "Type",
    i.currency_code AS "Currency",
    i.total AS "Invoice Total",
    i.total_tax AS "Total Tax",
    i.sub_total AS "Subtotal",
    i.amount_paid AS "Amount Paid",
    i.amount_due AS "Amount Due",
    i.amount_credited AS "Amount Credited",
    i.updated_date_utc AS "Invoice Updated",
    l.id AS "Line Id",
    l.description AS "Line Description",
    l.quantity AS "Line Quantity",
    l.unit_amount AS "Line Unit Amount",
    l.tax_type AS "Line Tax Type",
    l.tax_amount AS "Line Tax Amount",
    l.line_amount AS "Line Total",
    l.account_code AS "Line Account Code",
    l.tracking AS "Line Tracking",
    c.contact_id AS "Customer Id",
    c.name AS "Customer Name",
    c.email_address AS "Customer Email",
    c.is_customer AS "Is Customer",
    c.default_currency AS "Customer Currency"
FROM ods.invoices i
LEFT JOIN ods.invoice_lines l ON i.invoice_id = l.invoice_id
LEFT JOIN ods.contacts c ON i.contact_id = c.contact_id;
