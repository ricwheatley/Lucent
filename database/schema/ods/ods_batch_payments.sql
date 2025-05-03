
CREATE TABLE ods.batch_payments (
    batch_payment_id UUID PRIMARY KEY,
    reference TEXT,
    date DATE,
    type TEXT,
    status TEXT,
    total_amount NUMERIC,
    is_reconciled BOOLEAN,
    updated_date_utc TIMESTAMP,
    account_id UUID,
    source_run_id UUID NOT NULL,
    loaded_at TIMESTAMP NOT NULL DEFAULT now()
);

CREATE TABLE ods.batch_payment_lines (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    batch_payment_id UUID NOT NULL,
    payment_id UUID,
    invoice_id UUID,
    amount NUMERIC,
    CONSTRAINT fk_batch_payment FOREIGN KEY (batch_payment_id) REFERENCES ods.batch_payments(batch_payment_id)
);
