
CREATE TABLE ods.tracking_categories (
    tracking_category_id UUID PRIMARY KEY,
    name TEXT,
    status TEXT,
    option_status TEXT,
    updated_date_utc TIMESTAMP,
    source_run_id UUID NOT NULL,
    loaded_at TIMESTAMP NOT NULL DEFAULT now()
);

CREATE TABLE ods.tracking_options (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tracking_category_id UUID NOT NULL,
    tracking_option_id UUID,
    name TEXT,
    status TEXT,
    CONSTRAINT fk_tracking_option_category FOREIGN KEY (tracking_category_id) REFERENCES ods.tracking_categories(tracking_category_id)
);
