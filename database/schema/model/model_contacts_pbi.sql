
CREATE SCHEMA IF NOT EXISTS model;

CREATE OR REPLACE VIEW model.contacts AS
SELECT
    c.contact_id AS "Contact Id",
    c.name AS "Contact Name",
    c.email_address AS "Email Address",
    c.first_name AS "First Name",
    c.last_name AS "Last Name",
    c.default_currency AS "Currency",
    c.updated_date_utc AS "Last Updated"
FROM ods.contacts c;
