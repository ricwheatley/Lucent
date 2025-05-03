
CREATE TABLE ods.quotes (
    quote_id UUID PRIMARY KEY,
    contact_id UUID,
    status TEXT,
    title TEXT,
    summary TEXT,
    quote_number TEXT,
    expiry_date DATE,
    total NUMERIC,
    updated_date_utc TIMESTAMP,
    source_run_id UUID NOT NULL,
    loaded_at TIMESTAMP NOT NULL DEFAULT now()
);

CREATE TABLE ods.quote_lines (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    quote_id UUID NOT NULL,
    description TEXT,
    quantity NUMERIC,
    unit_amount NUMERIC,
    tax_type TEXT,
    tax_amount NUMERIC,
    line_amount NUMERIC,
    account_code TEXT,
    tracking JSONB,
    CONSTRAINT fk_quote_line_quote FOREIGN KEY (quote_id) REFERENCES ods.quotes(quote_id)
);
