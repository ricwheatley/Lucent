
CREATE TABLE ods.overpayments (
    overpayment_id UUID PRIMARY KEY,
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

CREATE TABLE ods.overpayment_lines (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    overpayment_id UUID NOT NULL,
    description TEXT,
    quantity NUMERIC,
    unit_amount NUMERIC,
    account_code TEXT,
    tax_type TEXT,
    tax_amount NUMERIC,
    line_amount NUMERIC,
    tracking JSONB,
    CONSTRAINT fk_overpayment_line FOREIGN KEY (overpayment_id) REFERENCES ods.overpayments(overpayment_id)
);

CREATE TABLE ods.overpayment_allocations (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    overpayment_id UUID NOT NULL,
    allocation_id UUID,
    invoice_id UUID,
    amount NUMERIC,
    date DATE,
    CONSTRAINT fk_overpayment_allocation FOREIGN KEY (overpayment_id) REFERENCES ods.overpayments(overpayment_id)
);
