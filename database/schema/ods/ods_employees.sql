
CREATE TABLE ods.employees (
    employee_id UUID PRIMARY KEY,
    status TEXT,
    first_name TEXT,
    last_name TEXT,
    external_link TEXT,
    updated_date_utc TIMESTAMP,
    source_run_id UUID NOT NULL,
    loaded_at TIMESTAMP NOT NULL DEFAULT now()
);
