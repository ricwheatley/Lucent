
CREATE TABLE ods.purchase_orders (
    purchase_order_id UUID PRIMARY KEY,
    contact_id UUID,
    date DATE,
    delivery_date DATE,
    reference TEXT,
    status TEXT,
    total NUMERIC,
    updated_date_utc TIMESTAMP,
    source_run_id UUID NOT NULL,
    loaded_at TIMESTAMP NOT NULL DEFAULT now()
);

CREATE TABLE ods.purchase_order_lines (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    purchase_order_id UUID NOT NULL,
    description TEXT,
    quantity NUMERIC,
    unit_amount NUMERIC,
    tax_type TEXT,
    tax_amount NUMERIC,
    line_amount NUMERIC,
    account_code TEXT,
    tracking JSONB,
    CONSTRAINT fk_po_line_po FOREIGN KEY (purchase_order_id) REFERENCES ods.purchase_orders(purchase_order_id)
);
