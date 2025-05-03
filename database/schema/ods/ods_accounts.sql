
CREATE TABLE ods.accounts (
    account_id UUID PRIMARY KEY,
    code TEXT NOT NULL,
    name TEXT NOT NULL,
    type TEXT,
    tax_type TEXT,
    description TEXT,
    class TEXT,
    enable_payments_to_account BOOLEAN,
    show_in_expense_claims BOOLEAN,
    reporting_code TEXT,
    reporting_code_name TEXT,
    updated_date_utc TIMESTAMP,
    currency_code TEXT,
    bank_account_number TEXT,
    bank_account_type TEXT,
    source_run_id UUID NOT NULL,
    loaded_at TIMESTAMP NOT NULL DEFAULT now()
);
