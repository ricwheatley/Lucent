
CREATE SCHEMA IF NOT EXISTS model;

CREATE OR REPLACE VIEW model.bank_transactions AS
SELECT
    t.bank_transaction_id AS "Transaction Id",
    t.date AS "Transaction Date",
    t.reference AS "Reference",
    t.status AS "Status",
    t.type AS "Transaction Type",
    t.currency_code AS "Currency",
    t.sub_total AS "Subtotal",
    t.total_tax AS "Total Tax",
    t.total AS "Total Amount",
    t.is_reconciled AS "Is Reconciled",
    t.updated_date_utc AS "Last Updated",
    l.id AS "Line Id",
    l.description AS "Line Description",
    l.quantity AS "Line Quantity",
    l.unit_amount AS "Line Unit Amount",
    l.tax_type AS "Line Tax Type",
    l.tax_amount AS "Line Tax Amount",
    l.line_amount AS "Line Total",
    l.account_code AS "Line Account Code",
    c.contact_id AS "Contact Id",
    c.name AS "Contact Name",
    c.email_address AS "Contact Email"
FROM ods.bank_transactions t
LEFT JOIN ods.bank_transaction_lines l ON t.bank_transaction_id = l.bank_transaction_id
LEFT JOIN ods.contacts c ON t.contact_id = c.contact_id;
