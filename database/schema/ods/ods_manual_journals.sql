
CREATE TABLE ods.manual_journals (
    manual_journal_id UUID PRIMARY KEY,
    narration TEXT,
    status TEXT,
    journal_date DATE,
    show_on_cash_basis_reports BOOLEAN,
    updated_date_utc TIMESTAMP,
    source_run_id UUID NOT NULL,
    loaded_at TIMESTAMP NOT NULL DEFAULT now()
);

CREATE TABLE ods.manual_journal_lines (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    manual_journal_id UUID NOT NULL,
    description TEXT,
    account_code TEXT,
    account_id UUID,
    tax_type TEXT,
    tax_amount NUMERIC,
    line_amount NUMERIC,
    tracking JSONB,
    CONSTRAINT fk_manual_journal_line FOREIGN KEY (manual_journal_id) REFERENCES ods.manual_journals(manual_journal_id)
);
