
CREATE TABLE raw.accounts (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    page_number INTEGER,
    fetched_at TIMESTAMP NOT NULL DEFAULT now(),
    processed BOOLEAN NOT NULL DEFAULT false,
    payload_json JSONB NOT NULL
);


CREATE TABLE raw.attachments (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    page_number INTEGER,
    fetched_at TIMESTAMP NOT NULL DEFAULT now(),
    processed BOOLEAN NOT NULL DEFAULT false,
    payload_json JSONB NOT NULL
);


CREATE TABLE raw.bank_transactions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    page_number INTEGER,
    fetched_at TIMESTAMP NOT NULL DEFAULT now(),
    processed BOOLEAN NOT NULL DEFAULT false,
    payload_json JSONB NOT NULL
);


CREATE TABLE raw.bank_transfers (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    page_number INTEGER,
    fetched_at TIMESTAMP NOT NULL DEFAULT now(),
    processed BOOLEAN NOT NULL DEFAULT false,
    payload_json JSONB NOT NULL
);


CREATE TABLE raw.batch_payments (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    page_number INTEGER,
    fetched_at TIMESTAMP NOT NULL DEFAULT now(),
    processed BOOLEAN NOT NULL DEFAULT false,
    payload_json JSONB NOT NULL
);


CREATE TABLE raw.branding_themes (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    page_number INTEGER,
    fetched_at TIMESTAMP NOT NULL DEFAULT now(),
    processed BOOLEAN NOT NULL DEFAULT false,
    payload_json JSONB NOT NULL
);


CREATE TABLE raw.budgets (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    page_number INTEGER,
    fetched_at TIMESTAMP NOT NULL DEFAULT now(),
    processed BOOLEAN NOT NULL DEFAULT false,
    payload_json JSONB NOT NULL
);


CREATE TABLE raw.contact_groups (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    page_number INTEGER,
    fetched_at TIMESTAMP NOT NULL DEFAULT now(),
    processed BOOLEAN NOT NULL DEFAULT false,
    payload_json JSONB NOT NULL
);


CREATE TABLE raw.contacts (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    page_number INTEGER,
    fetched_at TIMESTAMP NOT NULL DEFAULT now(),
    processed BOOLEAN NOT NULL DEFAULT false,
    payload_json JSONB NOT NULL
);


CREATE TABLE raw.credit_notes (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    page_number INTEGER,
    fetched_at TIMESTAMP NOT NULL DEFAULT now(),
    processed BOOLEAN NOT NULL DEFAULT false,
    payload_json JSONB NOT NULL
);


CREATE TABLE raw.currencies (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    page_number INTEGER,
    fetched_at TIMESTAMP NOT NULL DEFAULT now(),
    processed BOOLEAN NOT NULL DEFAULT false,
    payload_json JSONB NOT NULL
);


CREATE TABLE raw.employees (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    page_number INTEGER,
    fetched_at TIMESTAMP NOT NULL DEFAULT now(),
    processed BOOLEAN NOT NULL DEFAULT false,
    payload_json JSONB NOT NULL
);


CREATE TABLE raw.invoice_reminders (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    page_number INTEGER,
    fetched_at TIMESTAMP NOT NULL DEFAULT now(),
    processed BOOLEAN NOT NULL DEFAULT false,
    payload_json JSONB NOT NULL
);


CREATE TABLE raw.invoices (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    page_number INTEGER,
    fetched_at TIMESTAMP NOT NULL DEFAULT now(),
    processed BOOLEAN NOT NULL DEFAULT false,
    payload_json JSONB NOT NULL
);


CREATE TABLE raw.items (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    page_number INTEGER,
    fetched_at TIMESTAMP NOT NULL DEFAULT now(),
    processed BOOLEAN NOT NULL DEFAULT false,
    payload_json JSONB NOT NULL
);


CREATE TABLE raw.journals (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    page_number INTEGER,
    fetched_at TIMESTAMP NOT NULL DEFAULT now(),
    processed BOOLEAN NOT NULL DEFAULT false,
    payload_json JSONB NOT NULL
);


CREATE TABLE raw.linked_transactions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    page_number INTEGER,
    fetched_at TIMESTAMP NOT NULL DEFAULT now(),
    processed BOOLEAN NOT NULL DEFAULT false,
    payload_json JSONB NOT NULL
);


CREATE TABLE raw.manual_journals (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    page_number INTEGER,
    fetched_at TIMESTAMP NOT NULL DEFAULT now(),
    processed BOOLEAN NOT NULL DEFAULT false,
    payload_json JSONB NOT NULL
);


CREATE TABLE raw.organisation (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    page_number INTEGER,
    fetched_at TIMESTAMP NOT NULL DEFAULT now(),
    processed BOOLEAN NOT NULL DEFAULT false,
    payload_json JSONB NOT NULL
);


CREATE TABLE raw.overpayments (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    page_number INTEGER,
    fetched_at TIMESTAMP NOT NULL DEFAULT now(),
    processed BOOLEAN NOT NULL DEFAULT false,
    payload_json JSONB NOT NULL
);


CREATE TABLE raw.payment_services (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    page_number INTEGER,
    fetched_at TIMESTAMP NOT NULL DEFAULT now(),
    processed BOOLEAN NOT NULL DEFAULT false,
    payload_json JSONB NOT NULL
);


CREATE TABLE raw.payments (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    page_number INTEGER,
    fetched_at TIMESTAMP NOT NULL DEFAULT now(),
    processed BOOLEAN NOT NULL DEFAULT false,
    payload_json JSONB NOT NULL
);


CREATE TABLE raw.prepayments (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    page_number INTEGER,
    fetched_at TIMESTAMP NOT NULL DEFAULT now(),
    processed BOOLEAN NOT NULL DEFAULT false,
    payload_json JSONB NOT NULL
);


CREATE TABLE raw.purchase_orders (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    page_number INTEGER,
    fetched_at TIMESTAMP NOT NULL DEFAULT now(),
    processed BOOLEAN NOT NULL DEFAULT false,
    payload_json JSONB NOT NULL
);


CREATE TABLE raw.quotes (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    page_number INTEGER,
    fetched_at TIMESTAMP NOT NULL DEFAULT now(),
    processed BOOLEAN NOT NULL DEFAULT false,
    payload_json JSONB NOT NULL
);


CREATE TABLE raw.repeating_invoices (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    page_number INTEGER,
    fetched_at TIMESTAMP NOT NULL DEFAULT now(),
    processed BOOLEAN NOT NULL DEFAULT false,
    payload_json JSONB NOT NULL
);


CREATE TABLE raw.tax_rates (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    page_number INTEGER,
    fetched_at TIMESTAMP NOT NULL DEFAULT now(),
    processed BOOLEAN NOT NULL DEFAULT false,
    payload_json JSONB NOT NULL
);


CREATE TABLE raw.tracking_categories (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    page_number INTEGER,
    fetched_at TIMESTAMP NOT NULL DEFAULT now(),
    processed BOOLEAN NOT NULL DEFAULT false,
    payload_json JSONB NOT NULL
);


CREATE TABLE raw.users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    page_number INTEGER,
    fetched_at TIMESTAMP NOT NULL DEFAULT now(),
    processed BOOLEAN NOT NULL DEFAULT false,
    payload_json JSONB NOT NULL
);


CREATE TABLE raw.report_balance_sheet (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    page_number INTEGER,
    fetched_at TIMESTAMP NOT NULL DEFAULT now(),
    processed BOOLEAN NOT NULL DEFAULT false,
    payload_json JSONB NOT NULL
);


CREATE TABLE raw.report_trial_balance (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    page_number INTEGER,
    fetched_at TIMESTAMP NOT NULL DEFAULT now(),
    processed BOOLEAN NOT NULL DEFAULT false,
    payload_json JSONB NOT NULL
);


CREATE TABLE raw.report_profit_and_loss (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    page_number INTEGER,
    fetched_at TIMESTAMP NOT NULL DEFAULT now(),
    processed BOOLEAN NOT NULL DEFAULT false,
    payload_json JSONB NOT NULL
);
