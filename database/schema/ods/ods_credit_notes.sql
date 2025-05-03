
CREATE TABLE ods.credit_notes (
    credit_note_id UUID PRIMARY KEY,
    type TEXT,
    contact_id UUID,
    status TEXT,
    date DATE,
    total NUMERIC,
    updated_date_utc TIMESTAMP,
    currency_code TEXT,
    reference TEXT,
    source_run_id UUID NOT NULL,
    loaded_at TIMESTAMP NOT NULL DEFAULT now()
);

CREATE TABLE ods.credit_note_lines (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    credit_note_id UUID NOT NULL,
    description TEXT,
    quantity NUMERIC,
    unit_amount NUMERIC,
    tax_type TEXT,
    tax_amount NUMERIC,
    line_amount NUMERIC,
    account_code TEXT,
    tracking JSONB,
    CONSTRAINT fk_credit_note_line_credit_note FOREIGN KEY (credit_note_id) REFERENCES ods.credit_notes(credit_note_id)
);

CREATE TABLE ods.credit_note_payments (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    credit_note_id UUID NOT NULL,
    payment_id UUID,
    date DATE,
    amount NUMERIC,
    reference TEXT,
    CONSTRAINT fk_credit_note_payment_credit_note FOREIGN KEY (credit_note_id) REFERENCES ods.credit_notes(credit_note_id)
);

CREATE TABLE ods.credit_note_allocations (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    credit_note_id UUID NOT NULL,
    allocation_id UUID,
    invoice_id UUID,
    amount NUMERIC,
    date DATE,
    CONSTRAINT fk_credit_note_allocation_credit_note FOREIGN KEY (credit_note_id) REFERENCES ods.credit_notes(credit_note_id)
);
