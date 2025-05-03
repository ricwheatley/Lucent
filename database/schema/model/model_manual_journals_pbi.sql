
CREATE SCHEMA IF NOT EXISTS model;

CREATE OR REPLACE VIEW model.manual_journals AS
SELECT
    j.manual_journal_id AS "Manual Journal Id",
    j.journal_date AS "Journal Date",
    j.narration AS "Narration",
    j.status AS "Status",
    j.show_on_cash_basis_reports AS "Cash Basis",
    j.updated_date_utc AS "Journal Updated",
    l.id AS "Line Id",
    l.description AS "Line Description",
    l.account_code AS "Line Account Code",
    l.account_id AS "Line Account Id",
    l.tax_type AS "Line Tax Type",
    l.tax_amount AS "Line Tax Amount",
    l.line_amount AS "Line Amount",
    l.tracking AS "Line Tracking"
FROM ods.manual_journals j
LEFT JOIN ods.manual_journal_lines l ON j.manual_journal_id = l.manual_journal_id;
