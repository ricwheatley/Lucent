
CREATE TABLE ods.organisation (
    organisation_id UUID PRIMARY KEY,
    name TEXT,
    legal_name TEXT,
    organisation_type TEXT,
    base_currency TEXT,
    country_code TEXT,
    is_demo_company BOOLEAN,
    organisation_status TEXT,
    updated_date_utc TIMESTAMP,
    source_run_id UUID NOT NULL,
    loaded_at TIMESTAMP NOT NULL DEFAULT now()
);
