
CREATE SCHEMA IF NOT EXISTS model;

CREATE OR REPLACE VIEW model.customers AS
SELECT
    contact_id AS "Customer Id",
    name AS "Customer Name",
    email_address AS "Email Address",
    first_name AS "First Name",
    last_name AS "Last Name",
    default_currency AS "Currency",
    updated_date_utc AS "Last Updated"
FROM ods.contacts
WHERE is_customer = TRUE;

CREATE OR REPLACE VIEW model.suppliers AS
SELECT
    contact_id AS "Supplier Id",
    name AS "Supplier Name",
    email_address AS "Email Address",
    first_name AS "First Name",
    last_name AS "Last Name",
    default_currency AS "Currency",
    updated_date_utc AS "Last Updated"
FROM ods.contacts
WHERE is_supplier = TRUE;
