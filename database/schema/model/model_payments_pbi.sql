
CREATE SCHEMA IF NOT EXISTS model;

CREATE OR REPLACE VIEW model.payments AS
SELECT
    p.payment_id AS "Payment Id",
    p.date AS "Payment Date",
    p.amount AS "Amount",
    p.currency_rate AS "Currency Rate",
    p.reference AS "Reference",
    p.status AS "Status",
    p.updated_date_utc AS "Payment Updated",
    i.invoice_id AS "Invoice Id",
    i.reference AS "Invoice Reference",
    i.currency_code AS "Invoice Currency",
    c.contact_id AS "Customer Id",
    c.name AS "Customer Name",
    c.email_address AS "Customer Email"
FROM ods.payments p
LEFT JOIN ods.invoices i ON p.invoice_id = i.invoice_id
LEFT JOIN ods.contacts c ON i.contact_id = c.contact_id;
