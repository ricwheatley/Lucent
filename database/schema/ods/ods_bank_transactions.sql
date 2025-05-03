
CREATE TABLE ods.bank_transactions (
    bank_transaction_id UUID PRIMARY KEY,
    contact_id UUID,
    type TEXT,
    reference TEXT,
    is_reconciled BOOLEAN,
    currency_code TEXT,
    date DATE,
    status TEXT,
    sub_total NUMERIC,
    total_tax NUMERIC,
    total NUMERIC,
    updated_date_utc TIMESTAMP,
    source_run_id UUID NOT NULL,
    loaded_at TIMESTAMP NOT NULL DEFAULT now()
);

CREATE TABLE ods.bank_transaction_lines (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    bank_transaction_id UUID NOT NULL,
    description TEXT,
    unit_amount NUMERIC,
    tax_type TEXT,
    tax_amount NUMERIC,
    line_amount NUMERIC,
    account_code TEXT,
    quantity NUMERIC,
    account_id UUID,
    CONSTRAINT fk_bank_transaction FOREIGN KEY (bank_transaction_id) REFERENCES ods.bank_transactions(bank_transaction_id)
);
