
CREATE TABLE ods.bank_transfers (
    bank_transfer_id UUID PRIMARY KEY,
    date DATE,
    amount NUMERIC,
    from_bank_account_id UUID,
    from_bank_account_name TEXT,
    to_bank_account_id UUID,
    to_bank_account_name TEXT,
    from_bank_transaction_id UUID,
    to_bank_transaction_id UUID,
    from_is_reconciled BOOLEAN,
    to_is_reconciled BOOLEAN,
    reference TEXT,
    has_attachments BOOLEAN,
    source_run_id UUID NOT NULL,
    loaded_at TIMESTAMP NOT NULL DEFAULT now()
);
