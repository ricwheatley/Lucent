
CREATE SCHEMA IF NOT EXISTS model;

CREATE OR REPLACE VIEW model.budgets AS
SELECT
    b.budget_id AS "Budget Id",
    b.status AS "Status",
    b.description AS "Description",
    b.type AS "Type",
    b.updated_date_utc AS "Budget Updated",
    bl.id AS "Line Id",
    bl.account_id AS "Line Account Id",
    bl.account_code AS "Line Account Code"
FROM ods.budgets b
LEFT JOIN ods.budget_lines bl ON b.budget_id = bl.budget_id;
