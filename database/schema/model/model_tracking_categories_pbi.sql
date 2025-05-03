
CREATE SCHEMA IF NOT EXISTS model;

CREATE OR REPLACE VIEW model.tracking_categories AS
SELECT
    c.tracking_category_id AS "Tracking Category Id",
    c.name AS "Category Name",
    c.status AS "Category Status",
    c.option_status AS "Option Status",
    c.updated_date_utc AS "Last Updated",
    o.tracking_option_id AS "Option Id",
    o.name AS "Option Name",
    o.status AS "Option Status"
FROM ods.tracking_categories c
LEFT JOIN ods.tracking_options o ON c.tracking_category_id = o.tracking_category_id;
