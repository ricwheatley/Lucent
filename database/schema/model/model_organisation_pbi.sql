
CREATE SCHEMA IF NOT EXISTS model;

CREATE OR REPLACE VIEW model.organisation AS
SELECT
    o.organisation_id AS "Organisation Id",
    o.name AS "Organisation Name",
    o.legal_name AS "Legal Name",
    o.organisation_type AS "Organisation Type",
    o.base_currency AS "Base Currency",
    o.country_code AS "Country Code",
    o.is_demo_company AS "Is Demo Company",
    o.organisation_status AS "Organisation Status",
    o.updated_date_utc AS "Organisation Updated"
FROM ods.organisation o;
