
CREATE TABLE ods.branding_themes (
    branding_theme_id UUID PRIMARY KEY,
    name TEXT NOT NULL,
    sort_order INTEGER,
    created_date_utc TIMESTAMP,
    source_run_id UUID NOT NULL,
    loaded_at TIMESTAMP NOT NULL DEFAULT now()
);
