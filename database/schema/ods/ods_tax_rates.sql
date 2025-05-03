
CREATE TABLE ods.tax_rates (
    tax_type TEXT PRIMARY KEY,
    name TEXT,
    tax_components JSONB,
    status TEXT,
    report_tax_type TEXT,
    can_apply_to_assets BOOLEAN,
    can_apply_to_equity BOOLEAN,
    can_apply_to_expenses BOOLEAN,
    can_apply_to_liabilities BOOLEAN,
    can_apply_to_revenue BOOLEAN,
    source_run_id UUID NOT NULL,
    loaded_at TIMESTAMP NOT NULL DEFAULT now()
);
