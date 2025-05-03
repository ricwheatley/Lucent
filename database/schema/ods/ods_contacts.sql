
CREATE TABLE ods.contacts (
    contact_id UUID PRIMARY KEY,
    contact_status TEXT,
    name TEXT,
    first_name TEXT,
    last_name TEXT,
    email_address TEXT,
    is_supplier BOOLEAN,
    is_customer BOOLEAN,
    default_currency TEXT,
    updated_date_utc TIMESTAMP,
    source_run_id UUID NOT NULL,
    loaded_at TIMESTAMP NOT NULL DEFAULT now()
);

CREATE TABLE ods.contact_addresses (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    contact_id UUID NOT NULL,
    address_type TEXT,
    address_line1 TEXT,
    address_line2 TEXT,
    address_line3 TEXT,
    address_line4 TEXT,
    city TEXT,
    region TEXT,
    postal_code TEXT,
    country TEXT,
    CONSTRAINT fk_contact_address_contact FOREIGN KEY (contact_id) REFERENCES ods.contacts(contact_id)
);

CREATE TABLE ods.contact_phones (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    contact_id UUID NOT NULL,
    phone_type TEXT,
    phone_number TEXT,
    phone_area_code TEXT,
    phone_country_code TEXT,
    CONSTRAINT fk_contact_phone_contact FOREIGN KEY (contact_id) REFERENCES ods.contacts(contact_id)
);

CREATE TABLE ods.contact_persons (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    contact_id UUID NOT NULL,
    first_name TEXT,
    last_name TEXT,
    email_address TEXT,
    CONSTRAINT fk_contact_person_contact FOREIGN KEY (contact_id) REFERENCES ods.contacts(contact_id)
);
