
CREATE SCHEMA IF NOT EXISTS model;

CREATE OR REPLACE VIEW model.report_profit_and_loss AS
SELECT
    id AS "Report Id",
    fetched_at AS "Fetched At",
    report_json AS "Profit and Loss Report"
FROM ods.report_profit_and_loss;
