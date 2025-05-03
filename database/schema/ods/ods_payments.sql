
CREATE TABLE ods.payments (
    payment_id UUID PRIMARY KEY,
    invoice_id UUID,
    credit_note_id UUID,
    prepayment_id UUID,
    overpayment_id UUID,
    account_id UUID,
    date DATE,
    amount NUMERIC,
    currency_rate NUMERIC,
    reference TEXT,
    status TEXT,
    updated_date_utc TIMESTAMP,
    source_run_id UUID NOT NULL,
    loaded_at TIMESTAMP NOT NULL DEFAULT now()
);
