
CREATE SCHEMA IF NOT EXISTS model;

CREATE OR REPLACE VIEW model.purchase_orders AS
SELECT
    p.purchase_order_id AS "Purchase Order Id",
    p.date AS "Purchase Order Date",
    p.reference AS "Reference",
    p.status AS "Status",
    p.total AS "Total Amount",
    p.updated_date_utc AS "Purchase Order Updated",
    l.id AS "Line Id",
    l.description AS "Line Description",
    l.quantity AS "Line Quantity",
    l.unit_amount AS "Line Unit Amount",
    l.tax_type AS "Line Tax Type",
    l.tax_amount AS "Line Tax Amount",
    l.line_amount AS "Line Total",
    l.account_code AS "Line Account Code",
    c.contact_id AS "Supplier Id",
    c.name AS "Supplier Name",
    c.email_address AS "Supplier Email"
FROM ods.purchase_orders p
LEFT JOIN ods.purchase_order_lines l ON p.purchase_order_id = l.purchase_order_id
LEFT JOIN ods.contacts c ON p.contact_id = c.contact_id;
