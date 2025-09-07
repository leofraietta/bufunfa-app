# Bufunfa - Clarifying Questions for Requirements Refinement

## 1. TRANSACTION TYPES AND HIERARCHY

### 1.1 Periodic vs Recurrent Transactions ✅ RESOLVED
**Q1.1**: Should "Periódico" be a separate main category or a subcategory of "Recorrente"? The current specification is ambiguous.
**ANSWER**: "Periódico" is the same as "Recorrente" - just vocabulary adjustment. Use only "Recorrente" going forward.

**Q1.2**: For periodic transactions with custom intervals (N days), what are the minimum and maximum allowed values for N?
**ANSWER**: Minimum 1 day, maximum 6 days. Above 6 days, users should use "Semanal" pattern.

**Q1.3**: How should the system handle periodic transactions when the calculated date falls on weekends or holidays? Should it auto-adjust to the next business day?
**ANSWER**: Yes, auto-adjust to next business day for weekends. Holiday handling deferred to future (requires configurable holiday table).

**NEW REQUIREMENTS GENERATED**:
- New recurring type: "Todo dia útil" (every business day of the month)
- New recurring types: "Bimestral" (every 2 months), "Trimestral" (every 3 months), "Semestral" (every 6 months)

### 1.2 Transaction Lifecycle ✅ RESOLVED
**Q1.4**: When a recurring/periodic transaction is modified, should the change apply to:
- Only future occurrences?
- All occurrences from a specific date forward?
- Only the current occurrence?
**ANSWER**: Apply only to future occurrences. Past occurrences affected only if monthly sheets are not closed. New requirement for monthly sheet closure status management.

**Q1.5**: Can users delete individual occurrences of recurring transactions, or should they only be able to mark them as "cancelled"?
**ANSWER**: Mark as "cancelled" to maintain historical coherence and visibility.

**Q1.6**: For installment transactions, if a user wants to pay off remaining installments early, how should the system handle this scenario?
**ANSWER**: User can "quitar" (pay off) all future installments from transaction management screen. System generates single transaction on specified date with real amount. Future installments get "Quitado" status and don't impact future sheets.

## 2. JOINT ACCOUNTS MANAGEMENT

### 2.1 Account Creation and Invitations ✅ PARTIALLY RESOLVED
**Q2.1**: Who can create joint accounts - any user, or only premium users?
**ANSWER**: Any user can create joint accounts and becomes the administrator (like family manager). User doesn't need main account to create joint account.

**Q2.2**: What happens if an invited user doesn't have a Google account? Should the system send instructions to create one?
**STATUS**: Deferred - not refined at this time.

**Q2.3**: How long should invitation links remain valid? What happens if they expire?
**STATUS**: Deferred - not refined at this time.

**Q2.4**: Can sharing percentages be modified after account creation? If yes, do both users need to approve the change?
**ANSWER**: Only administrator can modify percentages. No approval process needed - assume users have agreement.

### 2.2 Permissions and Access Control ✅ PARTIALLY RESOLVED
**Q2.5**: Should joint account users have different permission levels (e.g., view-only vs. full access)?
**ANSWER**: YES - Administrator assigns permission levels: view-only or full access.

**Q2.6**: Can joint account users invite additional users, or only the original creator?
**ANSWER**: Only the original creator (administrator) can invite additional users.

**Q2.7**: What happens to joint account data if one user deletes their Google account or revokes access?
**STATUS**: Deferred - not refined at this time.

### 2.3 Settlement Process ✅ RESOLVED
**Q2.8**: For monthly settlement, what specific date should the system use? Last day of the month, or a configurable date?
**ANSWER**: Last day of the month.

**Q2.9**: If a joint account has a positive balance, should the system:
- Automatically distribute as income based on sharing percentages?
- Require manual user decision?
- Keep the balance in the joint account?
**ANSWER**: Keep balance in joint account. Administrator decides when to redistribute values manually.

**Q2.10**: How should the system handle settlement when one user's main account has insufficient funds for their share of negative balance?
**ANSWER**: Continue calculations, show negative values in red, provision negative values.

## 3. CREDIT CARD INTEGRATION

### 3.1 Statement Processing ✅ RESOLVED
**Q3.1**: Should credit card transactions be automatically consolidated on the closing date, or should users manually trigger the consolidation?
**ANSWER**: Credit card accounts work like regular accounts. Consolidation occurs on due date. After closing date, no expenses can be added to that month's sheet. Payment transaction generated on due date.

**Q3.2**: How should the system handle credit card payments? Should they be:
- Automatic transfers from main account on due date?
- Manual transactions that users must create?
- Optional automatic with manual override capability?
**ANSWER**: Automatic expense transactions generated in payment source account on due date with provisional and real values equal to statement amount. User manually manages payment in main account.

**Q3.3**: Should the system support multiple credit cards per user? If yes, what are the limits?
**ANSWER**: Yes, no limits specified.

### 3.2 Credit Card Transactions ✅ RESOLVED
**Q3.4**: Can users add transactions to credit card accounts for future dates (beyond current statement period)?
**ANSWER**: Yes, credit card accounts work like regular accounts with all transaction types.

**Q3.5**: How should the system handle credit card refunds or cashback transactions?
**ANSWER**: User manually creates sporadic income transaction on credit card account.

**Q3.6**: Should credit card accounts show available credit limit, or only current balance?
**ANSWER**: Only current balance - not working with limits at this time.

## 4. MARKET PROVISIONING

### 4.1 Provisioning Logic ✅ RESOLVED - CONCEPT CHANGED
**Q4.1**: For "Mercado" provisioning, should the system:
- Create individual sporadic transactions that reduce the provisional balance?
- Track a running total that's reconciled at month end?
- Allow both approaches with user preference?
**ANSWER**: System doesn't need special "Mercado" provisioning. Categories redesigned completely.

**Q4.2**: What happens if total market expenses exceed the provisional amount? Should the system:
- Allow negative provisional balance?
- Warn the user when approaching the limit?
- Block additional expenses?
**ANSWER**: Values can appear negative in dashboard. No warnings or blocks needed.

**Q4.3**: Can users have multiple provisioning categories beyond "Mercado" (e.g., "Combustível", "Lazer")?
**ANSWER**: Yes, but category concept redesigned. Categories are independent entities with provisional amounts, not special recurring expenses.

### 4.2 Month-End Reconciliation ✅ RESOLVED - NOT APPLICABLE
**Q4.4**: If provisional market amount exceeds actual expenses, should the difference be:
- Automatically transferred to savings/another account?
- Kept as positive balance for next month?
- Distributed as "bonus" income?
**ANSWER**: Not applicable - system doesn't treat categories as special "Mercado" provisioning.

## 5. BALANCE CALCULATIONS AND PROJECTIONS

### 5.1 Initial Balance Setup ✅ RESOLVED
**Q5.1**: For new users, how should they set initial account balances? Should the system:
- Require bank statement import?
- Allow manual entry with verification?
- Start from zero with a specific start date?
**ANSWER**: User creates first account and sets value manually.

**Q5.2**: How should the system handle historical data import if users want to migrate from other financial apps?
**STATUS**: Deferred - not refined at this time.

### 5.3 Future Projections ✅ RESOLVED
**Q5.3**: For balance projections, should the system consider:
- Only confirmed recurring transactions?
- All provisional transactions including estimates?
- User-defined confidence levels for different transaction types?
**ANSWER**: Consider only transactions marked as realized by user, using real values.

**Q5.4**: How far into the future should projections extend? 3 months, 6 months, 1 year?
**ANSWER**: Up to 1 year in the future.

**Q5.5**: Should projection calculations include seasonal variations or trend analysis based on historical data?
**ANSWER**: Projections won't reach this level of detail.

## 6. USER INTERFACE AND EXPERIENCE

### 6.1 Mobile Responsiveness
**Q6.1**: What are the minimum supported screen sizes? Should the app work on tablets and small phones equally well?

**Q6.2**: Are there any features that should be desktop-only or mobile-only?

**Q6.3**: Should the mobile version have simplified navigation or full feature parity?

### 6.2 Data Entry and Validation
**Q6.4**: What are the maximum limits for:
- Number of accounts per user?
- Number of transactions per month?
- Number of categories per user?
- Description length for transactions?

**Q6.5**: Should the system provide auto-complete suggestions for:
- Transaction descriptions based on history?
- Category assignment based on description patterns?
- Merchant names and common transactions?

### 6.3 Localization
**Q6.6**: Beyond Brazilian Portuguese, are there plans for other Portuguese variants (Portugal) or Spanish localization?

**Q6.7**: Should date and currency formats be user-configurable or always follow Brazilian standards?

## 7. SECURITY AND PRIVACY

### 7.1 Data Access and Sharing
**Q7.1**: Should users be able to export their complete financial data? In what formats (JSON, CSV, PDF)?

**Q7.2**: Are there any data retention requirements for deleted accounts? How long should data be kept after account deletion?

**Q7.3**: Should the system provide audit logs showing who accessed or modified joint account data?

### 7.2 Authentication and Sessions
**Q7.4**: What should happen if Google OAuth is temporarily unavailable? Should there be a fallback authentication method?

**Q7.5**: How long should user sessions remain active? Should there be different timeouts for different actions?

**Q7.6**: Should the system support multiple concurrent sessions per user across different devices?

## 8. PERFORMANCE AND SCALABILITY

### 8.1 Data Volume Handling
**Q8.1**: What's the expected data growth per user per year? How many transactions should the system handle efficiently?

**Q8.2**: Should there be archiving mechanisms for old data (e.g., transactions older than 5 years)?

**Q8.3**: Are there any real-time requirements for balance updates, or is eventual consistency acceptable?

### 8.2 Backup and Recovery
**Q8.4**: What's the acceptable Recovery Time Objective (RTO) and Recovery Point Objective (RPO) for the system?

**Q8.5**: Should users receive notifications about system backups or maintenance windows?

## 9. INTEGRATION AND FUTURE FEATURES

### 9.1 Banking Integration
**Q9.1**: Which Brazilian banks should be prioritized for future Open Banking integration?

**Q9.2**: Should the system support manual bank statement upload (OFX, CSV) before automated integration is available?

**Q9.3**: How should the system handle duplicate transactions when importing from banks?

### 9.2 Reporting and Analytics
**Q9.4**: What specific reports should be available in the initial version vs. future phases?

**Q9.5**: Should the system provide spending insights and recommendations, or focus purely on tracking?

**Q9.6**: Are there any regulatory reporting requirements (e.g., for tax purposes) that should be considered?

## 10. BUSINESS RULES AND EDGE CASES

### 10.1 Error Handling
**Q10.1**: How should the system handle leap years for recurring transactions set for February 29th?

**Q10.2**: What should happen if a user tries to create a transaction with a date in the past for a closed/reconciled month?

**Q10.3**: How should the system handle currency conversion if users travel or have foreign transactions?

### 10.2 Data Validation
**Q10.4**: What are the minimum and maximum allowed values for:
- Transaction amounts?
- Account balances (can they go negative)?
- Future transaction dates?

**Q10.5**: Should the system prevent users from creating transactions that would result in mathematically impossible scenarios?

## PRIORITY CLASSIFICATION

### High Priority (Critical for MVP)
- Questions 1.1, 1.4, 2.1, 2.8, 3.1, 4.1, 5.1, 6.4, 7.1, 8.1

### Medium Priority (Important for User Experience)
- Questions 1.2, 2.4, 3.2, 4.2, 5.3, 6.1, 7.4, 8.4, 9.1, 10.1

### Low Priority (Future Enhancement Considerations)
- Questions 1.3, 2.6, 3.6, 4.3, 5.5, 6.6, 7.6, 8.5, 9.2, 10.3
