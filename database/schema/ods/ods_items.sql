
CREATE TABLE ods.items (
    item_id UUID PRIMARY KEY,
    code TEXT,
    name TEXT,
    description TEXT,
    purchase_description TEXT,
    is_sold BOOLEAN,
    is_purchased BOOLEAN,
    sales_details JSONB,
    purchase_details JSONB,
    updated_date_utc TIMESTAMP,
    source_run_id UUID NOT NULL,
    loaded_at TIMESTAMP NOT NULL DEFAULT now()
);
