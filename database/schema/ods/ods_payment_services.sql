
CREATE TABLE ods.payment_services (
    payment_service_id UUID PRIMARY KEY,
    payment_service_name TEXT,
    payment_service_url TEXT,
    payment_service_type TEXT,
    pay_now_text TEXT,
    source_run_id UUID NOT NULL,
    loaded_at TIMESTAMP NOT NULL DEFAULT now()
);
