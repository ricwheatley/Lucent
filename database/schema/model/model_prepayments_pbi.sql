
CREATE SCHEMA IF NOT EXISTS model;

CREATE OR REPLACE VIEW model.prepayments AS
SELECT
    p.prepayment_id AS "Prepayment Id",
    p.date AS "Prepayment Date",
    p.amount AS "Amount",
    p.currency_code AS "Currency",
    p.reference AS "Reference",
    p.status AS "Status",
    p.updated_date_utc AS "Prepayment Updated",
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
FROM ods.prepayments p
LEFT JOIN ods.prepayment_lines l ON p.prepayment_id = l.prepayment_id
LEFT JOIN ods.contacts c ON p.contact_id = c.contact_id;
