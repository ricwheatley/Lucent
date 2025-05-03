
CREATE SCHEMA IF NOT EXISTS model;

CREATE OR REPLACE VIEW model.quotes AS
SELECT
    q.quote_id AS "Quote Id",
    q.date AS "Quote Date",  -- Adding missing date column
    q.reference AS "Reference",
    q.status AS "Status",
    q.title AS "Title",
    q.summary AS "Summary",
    q.quote_number AS "Quote Number",
    q.expiry_date AS "Expiry Date",
    q.total AS "Total Amount",
    q.updated_date_utc AS "Quote Updated",
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
FROM ods.quotes q
LEFT JOIN ods.quote_lines l ON q.quote_id = l.quote_id
LEFT JOIN ods.contacts c ON q.contact_id = c.contact_id;
