### Sprint 0 Summary

#### **What we've done:**

1. **Project Setup and Architecture**:

   * Finalised the **modular monolith** approach, simplifying dependencies into `Lucent.Infrastructure`.
   * Integrated **PostgreSQL** as the backend warehouse for normalisation and ETL processing.
   * Ensured **Power BI-ready views** are created for the `model` layer, denormalising fact tables and presenting clean, presentation-friendly columns.

2. **ETL Table Design**:

   * Built out `raw`, `landing`, `ods`, and `model` schemas in PostgreSQL for the Lucent accounting warehouse.
   * Defined necessary fields for reports, including `period`, `as_at_date`, and date ranges for Profit & Loss, Balance Sheet, and Trial Balance.

3. **Power BI Views**:

   * Created a suite of **denormalised views** for Power BI reporting:

     * `model.invoices`, `model.overpayments`, `model.purchase_orders`, `model.quotes`, `model.repeating_invoices`, `model.budgets`, `model.contacts`, `model.organisation`.
   * Applied **Power BI naming conventions** to enhance usability for report users.

4. **Report Handling**:

   * Added the necessary date fields (`as_at_date`, `period_from`, `period_to`) to the report tables (`report_profit_and_loss_lines`, `report_balance_sheet_lines`, `report_trial_balance_lines`).

#### **What’s still left to do:**

1. **ETL Logic**:

   * Implement the **shredding logic** for extracting the `period`, `as_at_date`, `period_from`, and `period_to` fields from the report JSON blobs in the **ETL process**.

2. **Testing & Validation**:

   * Perform testing on the **ETL pipeline** once implemented to ensure data consistency and integrity between the raw JSON, `landing`, `ods`, and `model` layers.

3. **Final Documentation**:

   * Ensure documentation around the **ETL processes** and how the Power BI views are being used is complete.

### ADR-001 Updates:

The ADR has been updated to reflect the following changes:

* Addition of `period`, `as_at_date`, `period_from`, and `period_to` fields to handle date ranges and snapshots in the report data.
* **`model` views** now reflect these changes with appropriate time-based fields for different reports (P\&L, Balance Sheet, Trial Balance).

The full content of the updated ADR is included in the document.
