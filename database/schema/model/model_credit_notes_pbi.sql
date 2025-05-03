
CREATE SCHEMA IF NOT EXISTS model;

CREATE OR REPLACE VIEW model.credit_notes AS
SELECT
    c.credit_note_id AS "Credit Note Id",
    c.date AS "Credit Note Date",
    c.reference AS "Reference",
    c.status AS "Status",
    c.type AS "Type",
    c.currency_code AS "Currency",
    c.total AS "Credit Note Total",
    c.updated_date_utc AS "Credit Note Updated",
    l.id AS "Line Id",
    l.description AS "Line Description",
    l.quantity AS "Line Quantity",
    l.unit_amount AS "Line Unit Amount",
    l.tax_type AS "Line Tax Type",
    l.tax_amount AS "Line Tax Amount",
    l.line_amount AS "Line Total",
    l.account_code AS "Line Account Code",
    l.tracking AS "Line Tracking",
    ct.contact_id AS "Customer Id",
    ct.name AS "Customer Name",
    ct.email_address AS "Customer Email",
    ct.is_customer AS "Is Customer",
    ct.default_currency AS "Customer Currency"
FROM ods.credit_notes c
LEFT JOIN ods.credit_note_lines l ON c.credit_note_id = l.credit_note_id
LEFT JOIN ods.contacts ct ON c.contact_id = ct.contact_id;
