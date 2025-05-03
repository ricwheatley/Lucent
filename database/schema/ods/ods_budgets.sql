
CREATE TABLE ods.budgets (
    budget_id UUID PRIMARY KEY,
    status TEXT,
    description TEXT,
    type TEXT,
    updated_date_utc TIMESTAMP,
    source_run_id UUID NOT NULL,
    loaded_at TIMESTAMP NOT NULL DEFAULT now()
);

CREATE TABLE ods.budget_lines (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    budget_id UUID NOT NULL,
    account_id UUID,
    account_code TEXT,
    CONSTRAINT fk_budget_line_budget FOREIGN KEY (budget_id) REFERENCES ods.budgets(budget_id)
);

CREATE TABLE ods.budget_balances (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    budget_line_id UUID NOT NULL,
    period TEXT,
    amount NUMERIC,
    notes TEXT,
    CONSTRAINT fk_budget_balance_line FOREIGN KEY (budget_line_id) REFERENCES ods.budget_lines(id)
);
