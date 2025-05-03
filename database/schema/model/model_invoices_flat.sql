
CREATE SCHEMA IF NOT EXISTS model;

CREATE OR REPLACE VIEW model.invoices AS
SELECT
    i.invoice_id,
    i.date,
    i.due_date,
    i.reference,
    i.status,
    i.type,
    i.currency_code,
    i.total,
    i.total_tax,
    i.sub_total,
    i.amount_paid,
    i.amount_due,
    i.amount_credited,
    i.updated_date_utc,
    l.id AS line_id,
    l.description AS line_description,
    l.quantity,
    l.unit_amount,
    l.tax_type AS line_tax_type,
    l.tax_amount AS line_tax_amount,
    l.line_amount,
    l.account_code AS line_account_code,
    l.tracking AS line_tracking,
    c.contact_id,
    c.name AS contact_name,
    c.email_address AS contact_email,
    c.is_customer,
    c.default_currency AS contact_currency
FROM ods.invoices i
LEFT JOIN ods.invoice_lines l ON i.invoice_id = l.invoice_id
LEFT JOIN ods.contacts c ON i.contact_id = c.contact_id;
