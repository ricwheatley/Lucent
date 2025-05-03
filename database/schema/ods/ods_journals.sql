
CREATE TABLE ods.journals (
    journal_id UUID PRIMARY KEY,
    journal_date DATE,
    journal_number TEXT,
    reference TEXT,
    narration TEXT,
    source_id UUID,
    source_type TEXT,
    updated_date_utc TIMESTAMP,
    source_run_id UUID NOT NULL,
    loaded_at TIMESTAMP NOT NULL DEFAULT now()
);

CREATE TABLE ods.journal_lines (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    journal_id UUID NOT NULL,
    account_id UUID,
    account_code TEXT,
    description TEXT,
    net_amount NUMERIC,
    gross_amount NUMERIC,
    tax_amount NUMERIC,
    tracking JSONB,
    CONSTRAINT fk_journal_line_journal FOREIGN KEY (journal_id) REFERENCES ods.journals(journal_id)
);
