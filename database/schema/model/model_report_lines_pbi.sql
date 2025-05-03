
CREATE SCHEMA IF NOT EXISTS model;

CREATE OR REPLACE VIEW model.report_profit_and_loss_lines AS
SELECT
    r.id AS "Report Line Id",
    r.report_section AS "Section",
    r.line_type AS "Line Type",
    r.account_code AS "Account Code",
    r.account_name AS "Account Name",
    r.amount AS "Amount",
    r.display_order AS "Display Order"
FROM ods.report_profit_and_loss_lines r;

CREATE OR REPLACE VIEW model.report_balance_sheet_lines AS
SELECT
    r.id AS "Report Line Id",
    r.report_section AS "Section",
    r.line_type AS "Line Type",
    r.account_code AS "Account Code",
    r.account_name AS "Account Name",
    r.amount AS "Amount",
    r.display_order AS "Display Order"
FROM ods.report_balance_sheet_lines r;

CREATE OR REPLACE VIEW model.report_trial_balance_lines AS
SELECT
    r.id AS "Report Line Id",
    r.account_code AS "Account Code",
    r.account_name AS "Account Name",
    r.debit AS "Debit",
    r.credit AS "Credit",
    r.balance AS "Balance",
    r.display_order AS "Display Order"
FROM ods.report_trial_balance_lines r;
