
CREATE TABLE ods.prepayments (
    prepayment_id UUID PRIMARY KEY,
    type TEXT,
    contact_id UUID,
    date DATE,
    currency_code TEXT,
    total NUMERIC,
    status TEXT,
    updated_date_utc TIMESTAMP,
    source_run_id UUID NOT NULL,
    loaded_at TIMESTAMP NOT NULL DEFAULT now()
);

CREATE TABLE ods.prepayment_lines (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    prepayment_id UUID NOT NULL,
    description TEXT,
    quantity NUMERIC,
    unit_amount NUMERIC,
    account_code TEXT,
    tax_type TEXT,
    tax_amount NUMERIC,
    line_amount NUMERIC,
    tracking JSONB,
    CONSTRAINT fk_prepayment_line FOREIGN KEY (prepayment_id) REFERENCES ods.prepayments(prepayment_id)
);

CREATE TABLE ods.prepayment_allocations (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    prepayment_id UUID NOT NULL,
    allocation_id UUID,
    invoice_id UUID,
    amount NUMERIC,
    date DATE,
    CONSTRAINT fk_prepayment_allocation FOREIGN KEY (prepayment_id) REFERENCES ods.prepayments(prepayment_id)
);
