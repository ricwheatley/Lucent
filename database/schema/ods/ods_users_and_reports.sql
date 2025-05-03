
CREATE TABLE ods.users (
    user_id UUID PRIMARY KEY,
    first_name TEXT,
    last_name TEXT,
    email_address TEXT,
    is_subscriber BOOLEAN,
    organisation_role TEXT,
    updated_date_utc TIMESTAMP,
    source_run_id UUID NOT NULL,
    loaded_at TIMESTAMP NOT NULL DEFAULT now()
);

CREATE TABLE ods.report_balance_sheet (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    fetched_at TIMESTAMP NOT NULL DEFAULT now(),
    source_run_id UUID NOT NULL,
    loaded_at TIMESTAMP NOT NULL DEFAULT now(),
    report_json JSONB NOT NULL
);

CREATE TABLE ods.report_trial_balance (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    fetched_at TIMESTAMP NOT NULL DEFAULT now(),
    source_run_id UUID NOT NULL,
    loaded_at TIMESTAMP NOT NULL DEFAULT now(),
    report_json JSONB NOT NULL
);

CREATE TABLE ods.report_profit_and_loss (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    fetched_at TIMESTAMP NOT NULL DEFAULT now(),
    source_run_id UUID NOT NULL,
    loaded_at TIMESTAMP NOT NULL DEFAULT now(),
    report_json JSONB NOT NULL
);
