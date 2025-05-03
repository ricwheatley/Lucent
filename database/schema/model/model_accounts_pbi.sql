
CREATE SCHEMA IF NOT EXISTS model;

CREATE OR REPLACE VIEW model.accounts AS
SELECT
    account_id AS "Account Id",
    code AS "Account Code",
    name AS "Account Name",
    type AS "Account Type",
    tax_type AS "Tax Type",
    description AS "Description",
    class AS "Class",
    enable_payments_to_account AS "Enable Payments",
    show_in_expense_claims AS "Show In Expense Claims",
    reporting_code AS "Reporting Code",
    reporting_code_name AS "Reporting Code Name",
    currency_code AS "Currency",
    bank_account_number AS "Bank Account Number",
    bank_account_type AS "Bank Account Type",
    updated_date_utc AS "Last Updated"
FROM ods.accounts;
