
CREATE TABLE ods.linked_transactions (
    linked_transaction_id UUID PRIMARY KEY,
    source_transaction_id UUID,
    source_line_item_id UUID,
    contact_id UUID,
    target_transaction_id UUID,
    target_line_item_id UUID,
    status TEXT,
    type TEXT,
    line_item_description TEXT,
    updated_date_utc TIMESTAMP,
    source_run_id UUID NOT NULL,
    loaded_at TIMESTAMP NOT NULL DEFAULT now()
);
