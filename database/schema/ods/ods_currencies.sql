
CREATE TABLE ods.currencies (
    code TEXT PRIMARY KEY,
    description TEXT,
    source_run_id UUID NOT NULL,
    loaded_at TIMESTAMP NOT NULL DEFAULT now()
);
