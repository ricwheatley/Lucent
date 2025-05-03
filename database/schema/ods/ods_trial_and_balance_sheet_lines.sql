
CREATE TABLE ods.report_trial_balance_lines (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    source_run_id UUID NOT NULL,
    fetched_at TIMESTAMP NOT NULL DEFAULT now(),
    account_code TEXT,
    account_name TEXT,
    debit NUMERIC,
    credit NUMERIC,
    balance NUMERIC,
    display_order INTEGER
);

CREATE TABLE ods.report_balance_sheet_lines (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    source_run_id UUID NOT NULL,
    fetched_at TIMESTAMP NOT NULL DEFAULT now(),
    report_section TEXT,
    line_type TEXT,
    account_code TEXT,
    account_name TEXT,
    amount NUMERIC,
    display_order INTEGER
);
