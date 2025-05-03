
CREATE SCHEMA IF NOT EXISTS model;

CREATE OR REPLACE VIEW model.journals AS
SELECT
    j.journal_id AS "Journal Id",
    j.journal_date AS "Journal Date",
    j.journal_number AS "Journal Number",
    j.reference AS "Reference",
    j.narration AS "Narration",
    j.source_type AS "Source Type",
    j.source_id AS "Source Id",
    j.updated_date_utc AS "Journal Updated",
    l.id AS "Line Id",
    l.account_code AS "Line Account Code",
    l.account_id AS "Line Account Id",
    l.description AS "Line Description",
    l.net_amount AS "Line Net Amount",
    l.gross_amount AS "Line Gross Amount",
    l.tax_amount AS "Line Tax Amount",
    l.tracking AS "Line Tracking"
FROM ods.journals j
LEFT JOIN ods.journal_lines l ON j.journal_id = l.journal_id;
