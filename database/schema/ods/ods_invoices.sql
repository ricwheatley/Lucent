
CREATE TABLE ods.invoices (
    invoice_id UUID PRIMARY KEY,
    contact_id UUID,
    status TEXT,
    type TEXT,
    date DATE,
    due_date DATE,
    currency_code TEXT,
    reference TEXT,
    total NUMERIC,
    total_tax NUMERIC,
    sub_total NUMERIC,
    amount_due NUMERIC,
    amount_paid NUMERIC,
    amount_credited NUMERIC,
    updated_date_utc TIMESTAMP,
    source_run_id UUID NOT NULL,
    loaded_at TIMESTAMP NOT NULL DEFAULT now()
);

CREATE TABLE ods.invoice_lines (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    invoice_id UUID NOT NULL,
    description TEXT,
    quantity NUMERIC,
    unit_amount NUMERIC,
    tax_type TEXT,
    tax_amount NUMERIC,
    line_amount NUMERIC,
    account_code TEXT,
    tracking JSONB,
    CONSTRAINT fk_invoice_line_invoice FOREIGN KEY (invoice_id) REFERENCES ods.invoices(invoice_id)
);

CREATE TABLE ods.invoice_payments (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    invoice_id UUID NOT NULL,
    payment_id UUID,
    date DATE,
    amount NUMERIC,
    reference TEXT,
    CONSTRAINT fk_invoice_payment_invoice FOREIGN KEY (invoice_id) REFERENCES ods.invoices(invoice_id)
);

CREATE TABLE ods.invoice_allocations (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    invoice_id UUID NOT NULL,
    allocation_id UUID,
    credit_note_id UUID,
    amount NUMERIC,
    date DATE,
    CONSTRAINT fk_invoice_allocation_invoice FOREIGN KEY (invoice_id) REFERENCES ods.invoices(invoice_id)
);
