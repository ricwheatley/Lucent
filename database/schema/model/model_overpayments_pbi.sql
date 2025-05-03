
CREATE SCHEMA IF NOT EXISTS model;

CREATE OR REPLACE VIEW model.overpayments AS
SELECT
    o.overpayment_id AS "Overpayment Id",
    o.date AS "Overpayment Date",
    o.amount AS "Amount",
    o.currency_code AS "Currency",
    o.reference AS "Reference",
    o.status AS "Status",
    o.updated_date_utc AS "Overpayment Updated",
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
FROM ods.overpayments o
LEFT JOIN ods.overpayment_lines l ON o.overpayment_id = l.overpayment_id
LEFT JOIN ods.contacts c ON o.contact_id = c.contact_id;
