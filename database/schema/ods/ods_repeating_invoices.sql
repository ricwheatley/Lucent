
CREATE TABLE ods.repeating_invoices (
    repeating_invoice_id UUID PRIMARY KEY,
    contact_id UUID,
    status TEXT,
    reference TEXT,
    start_date DATE,
    next_scheduled_date DATE,
    end_date DATE,
    schedule TEXT,
    total NUMERIC,
    updated_date_utc TIMESTAMP,
    source_run_id UUID NOT NULL,
    loaded_at TIMESTAMP NOT NULL DEFAULT now()
);

CREATE TABLE ods.repeating_invoice_lines (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    repeating_invoice_id UUID NOT NULL,
    description TEXT,
    quantity NUMERIC,
    unit_amount NUMERIC,
    tax_type TEXT,
    tax_amount NUMERIC,
    line_amount NUMERIC,
    account_code TEXT,
    tracking JSONB,
    CONSTRAINT fk_repeating_invoice_line FOREIGN KEY (repeating_invoice_id) REFERENCES ods.repeating_invoices(repeating_invoice_id)
);
