
CREATE TABLE ods.report_profit_and_loss_lines (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    source_run_id UUID NOT NULL,
    fetched_at TIMESTAMP NOT NULL DEFAULT now(),
    report_section TEXT,              -- e.g. Revenue, Expenses, Gross Profit
    line_type TEXT,                   -- e.g. Header, Row, Total
    account_code TEXT,
    account_name TEXT,
    amount NUMERIC,
    display_order INTEGER
);
