
CREATE TABLE ods.attachments (
    attachment_id UUID PRIMARY KEY,
    parent_type TEXT NOT NULL,
    parent_id UUID NOT NULL,
    file_name TEXT NOT NULL,
    url TEXT,
    mime_type TEXT,
    content_length INTEGER,
    source_run_id UUID NOT NULL,
    loaded_at TIMESTAMP NOT NULL DEFAULT now()
);
