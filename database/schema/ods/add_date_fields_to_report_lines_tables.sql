
ALTER TABLE ods.report_profit_and_loss_lines
ADD COLUMN as_at_date DATE,
ADD COLUMN period_from DATE,
ADD COLUMN period_to DATE;

ALTER TABLE ods.report_balance_sheet_lines
ADD COLUMN as_at_date DATE,
ADD COLUMN period_from DATE,
ADD COLUMN period_to DATE;

ALTER TABLE ods.report_trial_balance_lines
ADD COLUMN as_at_date DATE,
ADD COLUMN period_from DATE,
ADD COLUMN period_to DATE;
