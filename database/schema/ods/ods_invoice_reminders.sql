
CREATE TABLE ods.invoice_reminders (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    enabled BOOLEAN,
    reminder_day INTEGER,
    message TEXT,
    source_run_id UUID NOT NULL,
    loaded_at TIMESTAMP NOT NULL DEFAULT now()
);
