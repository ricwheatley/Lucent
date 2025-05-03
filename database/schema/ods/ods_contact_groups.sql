
CREATE TABLE ods.contact_groups (
    contact_group_id UUID PRIMARY KEY,
    name TEXT NOT NULL,
    status TEXT,
    source_run_id UUID NOT NULL,
    loaded_at TIMESTAMP NOT NULL DEFAULT now()
);
