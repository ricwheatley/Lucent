CREATE TABLE landing.report_trial_balance ( id UUID PRIMARY KEY DEFAULT gen_random_uuid(), source_run_id UUID NOT NULL, loaded_at TIMESTAMP NOT NULL DEFAULT now(), fetched_at TIMESTAMP NOT NULL DEFAULT now(), report_json JSONB NOT NULL );
10:54:15
CREATE TABLE landing.report_profit_and_loss ( id UUID PRIMARY KEY DEFAULT gen_random_uuid(), source_run_id UUID NOT NULL, loaded_at TIMESTAMP NOT NULL DEFAULT now(), fetched_at TIMESTAMP NOT NULL DEFAULT now(), report_json JSONB NOT NULL );
10:54:04
CREATE TABLE landing.report_balance_sheet ( id UUID PRIMARY KEY DEFAULT gen_random_uuid(), source_run_id UUID NOT NULL, loaded_at TIMESTAMP NOT NULL DEFAULT now(), fetched_at TIMESTAMP NOT NULL DEFAULT now(), report_json JSONB NOT NULL );
10:53:42
CREATE TABLE landing.users ( id UUID PRIMARY KEY DEFAULT gen_random_uuid(), source_run_id UUID NOT NULL, loaded_at TIMESTAMP NOT NULL DEFAULT now(), user_id UUID, first_name TEXT, last_name TEXT, email_address TEXT, is_subscriber BOOLEAN, organisation_role TEXT, updated_date_utc TIMESTAMP );
10:53:22
CREATE TABLE landing.tracking_options ( id UUID PRIMARY KEY DEFAULT gen_random_uuid(), tracking_category_id UUID NOT NULL, tracking_option_id UUID, name TEXT, status TEXT );
10:52:58
CREATE TABLE landing.tracking_categories ( id UUID PRIMARY KEY DEFAULT gen_random_uuid(), source_run_id UUID NOT NULL, loaded_at TIMESTAMP NOT NULL DEFAULT now(), tracking_category_id UUID, name TEXT, status TEXT, option_status TEXT, updated_date_utc TIMESTAMP );
10:52:52
CREATE TABLE landing.tax_rates ( id UUID PRIMARY KEY DEFAULT gen_random_uuid(), source_run_id UUID NOT NULL, loaded_at TIMESTAMP NOT NULL DEFAULT now(), tax_type TEXT, name TEXT, tax_components JSONB, status TEXT, report_tax_type TEXT, can_apply_to_assets BOOLEAN, can_apply_to_equity BOOLEAN, can_apply_to_expenses BOOLEAN, can_apply_to_liabilities BOOLEAN, can_apply_to_revenue BOOLEAN );
10:52:31
CREATE TABLE landing.repeating_invoice_lines ( id UUID PRIMARY KEY DEFAULT gen_random_uuid(), repeating_invoice_id UUID NOT NULL, description TEXT, quantity NUMERIC, unit_amount NUMERIC, tax_type TEXT, tax_amount NUMERIC, line_amount NUMERIC, account_code TEXT, tracking JSONB );
10:52:07
CREATE TABLE landing.repeating_invoices ( id UUID PRIMARY KEY DEFAULT gen_random_uuid(), source_run_id UUID NOT NULL, loaded_at TIMESTAMP NOT NULL DEFAULT now(), repeating_invoice_id UUID, contact_id UUID, status TEXT, reference TEXT, start_date DATE, next_scheduled_date DATE, end_date DATE, schedule TEXT, total NUMERIC, updated_date_utc TIMESTAMP );
10:52:02
CREATE TABLE landing.quote_lines ( id UUID PRIMARY KEY DEFAULT gen_random_uuid(), quote_id UUID NOT NULL, description TEXT, quantity NUMERIC, unit_amount NUMERIC, tax_type TEXT, tax_amount NUMERIC, line_amount NUMERIC, account_code TEXT, tracking JSONB );
10:51:43
CREATE TABLE landing.quotes ( id UUID PRIMARY KEY DEFAULT gen_random_uuid(), source_run_id UUID NOT NULL, loaded_at TIMESTAMP NOT NULL DEFAULT now(), quote_id UUID, contact_id UUID, status TEXT, title TEXT, summary TEXT, quote_number TEXT, expiry_date DATE, total NUMERIC, updated_date_utc TIMESTAMP );
10:51:38
CREATE TABLE landing.purchase_order_lines ( id UUID PRIMARY KEY DEFAULT gen_random_uuid(), purchase_order_id UUID NOT NULL, description TEXT, quantity NUMERIC, unit_amount NUMERIC, tax_type TEXT, tax_amount NUMERIC, line_amount NUMERIC, account_code TEXT, tracking JSONB );
10:51:21
CREATE TABLE landing.purchase_orders ( id UUID PRIMARY KEY DEFAULT gen_random_uuid(), source_run_id UUID NOT NULL, loaded_at TIMESTAMP NOT NULL DEFAULT now(), purchase_order_id UUID, contact_id UUID, date DATE, delivery_date DATE, reference TEXT, status TEXT, total NUMERIC, updated_date_utc TIMESTAMP );
10:51:16
CREATE TABLE landing.prepayment_allocations ( id UUID PRIMARY KEY DEFAULT gen_random_uuid(), prepayment_id UUID NOT NULL, allocation_id UUID, invoice_id UUID, amount NUMERIC, date DATE );
10:51:00
CREATE TABLE landing.prepayment_lines ( id UUID PRIMARY KEY DEFAULT gen_random_uuid(), prepayment_id UUID NOT NULL, description TEXT, quantity NUMERIC, unit_amount NUMERIC, account_code TEXT, tax_type TEXT, tax_amount NUMERIC, line_amount NUMERIC, tracking JSONB );
10:50:54
CREATE TABLE landing.prepayments ( id UUID PRIMARY KEY DEFAULT gen_random_uuid(), source_run_id UUID NOT NULL, loaded_at TIMESTAMP NOT NULL DEFAULT now(), prepayment_id UUID, type TEXT, contact_id UUID, date DATE, currency_code TEXT, total NUMERIC, status TEXT, updated_date_utc TIMESTAMP );
10:50:49
CREATE TABLE landing.payments ( id UUID PRIMARY KEY DEFAULT gen_random_uuid(), source_run_id UUID NOT NULL, loaded_at TIMESTAMP NOT NULL DEFAULT now(), payment_id UUID, invoice_id UUID, credit_note_id UUID, prepayment_id UUID, overpayment_id UUID, account_id UUID, date DATE, amount NUMERIC, currency_rate NUMERIC, reference TEXT, status TEXT, updated_date_utc TIMESTAMP );
10:50:09
CREATE TABLE landing.payment_services ( id UUID PRIMARY KEY DEFAULT gen_random_uuid(), source_run_id UUID NOT NULL, loaded_at TIMESTAMP NOT NULL DEFAULT now(), payment_service_id UUID, payment_service_name TEXT, payment_service_url TEXT, payment_service_type TEXT, pay_now_text TEXT );
10:49:00
CREATE TABLE landing.overpayment_allocations ( id UUID PRIMARY KEY DEFAULT gen_random_uuid(), overpayment_id UUID NOT NULL, allocation_id UUID, invoice_id UUID, amount NUMERIC, date DATE );
10:48:43
CREATE TABLE landing.overpayment_lines ( id UUID PRIMARY KEY DEFAULT gen_random_uuid(), overpayment_id UUID NOT NULL, description TEXT, quantity NUMERIC, unit_amount NUMERIC, account_code TEXT, tax_type TEXT, tax_amount NUMERIC, line_amount NUMERIC, tracking JSONB );
10:48:36
CREATE TABLE landing.overpayments ( id UUID PRIMARY KEY DEFAULT gen_random_uuid(), source_run_id UUID NOT NULL, loaded_at TIMESTAMP NOT NULL DEFAULT now(), overpayment_id UUID, type TEXT, contact_id UUID, date DATE, currency_code TEXT, total NUMERIC, status TEXT, updated_date_utc TIMESTAMP );
10:48:30
CREATE TABLE landing.organisation ( id UUID PRIMARY KEY DEFAULT gen_random_uuid(), source_run_id UUID NOT NULL, loaded_at TIMESTAMP NOT NULL DEFAULT now(), organisation_id UUID, name TEXT, legal_name TEXT, organisation_type TEXT, base_currency TEXT, country_code TEXT, is_demo_company BOOLEAN, organisation_status TEXT, updated_date_utc TIMESTAMP );
10:48:01
CREATE TABLE landing.manual_journal_lines ( id UUID PRIMARY KEY DEFAULT gen_random_uuid(), manual_journal_id UUID NOT NULL, description TEXT, account_code TEXT, account_id UUID, tax_type TEXT, tax_amount NUMERIC, line_amount NUMERIC, tracking JSONB );
10:45:21
CREATE TABLE landing.manual_journals ( id UUID PRIMARY KEY DEFAULT gen_random_uuid(), source_run_id UUID NOT NULL, loaded_at TIMESTAMP NOT NULL DEFAULT now(), manual_journal_id UUID, narration TEXT, status TEXT, journal_date DATE, show_on_cash_basis_reports BOOLEAN, updated_date_utc TIMESTAMP );
10:45:11
CREATE TABLE landing.linked_transactions ( id UUID PRIMARY KEY DEFAULT gen_random_uuid(), source_run_id UUID NOT NULL, loaded_at TIMESTAMP NOT NULL DEFAULT now(), linked_transaction_id UUID, source_transaction_id UUID, source_line_item_id UUID, contact_id UUID, target_transaction_id UUID, target_line_item_id UUID, status TEXT, type TEXT, line_item_description TEXT, updated_date_utc TIMESTAMP );
10:44:46
CREATE TABLE landing.journal_lines ( id UUID PRIMARY KEY DEFAULT gen_random_uuid(), journal_id UUID NOT NULL, account_id UUID, account_code TEXT, description TEXT, net_amount NUMERIC, gross_amount NUMERIC, tax_amount NUMERIC, tracking JSONB );
10:44:19
CREATE TABLE landing.journals ( id UUID PRIMARY KEY DEFAULT gen_random_uuid(), source_run_id UUID NOT NULL, loaded_at TIMESTAMP NOT NULL DEFAULT now(), journal_id UUID, journal_date DATE, journal_number TEXT, reference TEXT, narration TEXT, source_id UUID, source_type TEXT, updated_date_utc TIMESTAMP );
10:44:13
CREATE TABLE landing.items ( id UUID PRIMARY KEY DEFAULT gen_random_uuid(), source_run_id UUID NOT NULL, loaded_at TIMESTAMP NOT NULL DEFAULT now(), item_id UUID, code TEXT, name TEXT, description TEXT, purchase_description TEXT, is_sold BOOLEAN, is_purchased BOOLEAN, sales_details JSONB, purchase_details JSONB, updated_date_utc TIMESTAMP );
10:43:44
CREATE TABLE landing.invoice_allocations ( id UUID PRIMARY KEY DEFAULT gen_random_uuid(), invoice_id UUID NOT NULL, allocation_id UUID, credit_note_id UUID, amount NUMERIC, date DATE );
10:43:08
CREATE TABLE landing.invoice_payments ( id UUID PRIMARY KEY DEFAULT gen_random_uuid(), invoice_id UUID NOT NULL, payment_id UUID, date DATE, amount NUMERIC, reference TEXT );
10:43:02
CREATE TABLE landing.invoice_lines ( id UUID PRIMARY KEY DEFAULT gen_random_uuid(), invoice_id UUID NOT NULL, description TEXT, quantity NUMERIC, unit_amount NUMERIC, tax_type TEXT, tax_amount NUMERIC, line_amount NUMERIC, account_code TEXT, tracking JSONB );
10:42:13
CREATE TABLE landing.invoices ( id UUID PRIMARY KEY DEFAULT gen_random_uuid(), source_run_id UUID NOT NULL, loaded_at TIMESTAMP NOT NULL DEFAULT now(), invoice_id UUID, contact_id UUID, status TEXT, type TEXT, date DATE, due_date DATE, currency_code TEXT, reference TEXT, total NUMERIC, total_tax NUMERIC, sub_total NUMERIC, amount_due NUMERIC, amount_paid NUMERIC, amount_credited NUMERIC, updated_date_utc TIMESTAMP );
10:42:06
CREATE TABLE landing.invoice_reminders ( id UUID PRIMARY KEY DEFAULT gen_random_uuid(), source_run_id UUID NOT NULL, loaded_at TIMESTAMP NOT NULL DEFAULT now(), enabled BOOLEAN, reminder_day INTEGER, message TEXT );
10:41:25
CREATE TABLE landing.employees ( id UUID PRIMARY KEY DEFAULT gen_random_uuid(), source_run_id UUID NOT NULL, loaded_at TIMESTAMP NOT NULL DEFAULT now(), employee_id UUID, status TEXT, first_name TEXT, last_name TEXT, external_link TEXT, updated_date_utc TIMESTAMP );
10:40:44
CREATE TABLE landing.currencies ( id UUID PRIMARY KEY DEFAULT gen_random_uuid(), source_run_id UUID NOT NULL, loaded_at TIMESTAMP NOT NULL DEFAULT now(), code TEXT, description TEXT );
10:40:17
CREATE TABLE landing.credit_note_allocations ( id UUID PRIMARY KEY DEFAULT gen_random_uuid(), credit_note_id UUID NOT NULL, allocation_id UUID, invoice_id UUID, amount NUMERIC, date DATE );
10:39:55
CREATE TABLE landing.credit_note_payments ( id UUID PRIMARY KEY DEFAULT gen_random_uuid(), credit_note_id UUID NOT NULL, payment_id UUID, date DATE, amount NUMERIC, reference TEXT );
10:39:50
CREATE TABLE landing.credit_note_lines ( id UUID PRIMARY KEY DEFAULT gen_random_uuid(), credit_note_id UUID NOT NULL, description TEXT, quantity NUMERIC, unit_amount NUMERIC, tax_type TEXT, tax_amount NUMERIC, line_amount NUMERIC, account_code TEXT, tracking JSONB );
10:39:08
CREATE TABLE landing.credit_notes ( id UUID PRIMARY KEY DEFAULT gen_random_uuid(), source_run_id UUID NOT NULL, loaded_at TIMESTAMP NOT NULL DEFAULT now(), credit_note_id UUID, type TEXT, contact_id UUID, status TEXT, date DATE, total NUMERIC, updated_date_utc TIMESTAMP, currency_code TEXT, reference TEXT );
10:38:59
CREATE TABLE landing.contact_persons ( id UUID PRIMARY KEY DEFAULT gen_random_uuid(), contact_id UUID NOT NULL, first_name TEXT, last_name TEXT, email_address TEXT );
10:38:34
CREATE TABLE landing.contact_phones ( id UUID PRIMARY KEY DEFAULT gen_random_uuid(), contact_id UUID NOT NULL, phone_type TEXT, phone_number TEXT, phone_area_code TEXT, phone_country_code TEXT );
10:38:29
CREATE TABLE landing.contact_addresses ( id UUID PRIMARY KEY DEFAULT gen_random_uuid(), contact_id UUID NOT NULL, address_type TEXT, address_line1 TEXT, address_line2 TEXT, address_line3 TEXT, address_line4 TEXT, city TEXT, region TEXT, postal_code TEXT, country TEXT );
10:38:23
CREATE TABLE landing.manual_journal_lines (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    manual_journal_id UUID NOT NULL,
    description TEXT,
    account_code TEXT,
    account_id UUID,
    tax_type TEXT,
    tax_amount NUMERIC,
    line_amount NUMERIC,
    tracking JSONB
);

CREATE TABLE

Query returned successfully in 35 msec.
Total rows:
Query complete 00:00:00.035
LF
Ln 1, Col 42