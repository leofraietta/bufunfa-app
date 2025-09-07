# Bufunfa - Refined Requirements Specification

## 1. PROJECT OVERVIEW

### 1.1 Objective
Personal financial control web application with future mobile version capability.

### 1.2 Target Users
- Individual users managing personal finances
- Users with joint financial accounts requiring shared management

### 1.3 Core Value Proposition
Comprehensive financial control with provisional vs. real value tracking, joint account management, and automated recurring transaction handling.

## 2. TECHNICAL ARCHITECTURE

### 2.1 Backend Stack
- **Framework**: .NET 8
- **Architecture**: Clean Architecture with RESTful API
- **ORM**: Entity Framework Core
- **Database**: PostgreSQL
- **Authentication**: Google OAuth2

### 2.2 Frontend Stack
- **Framework**: Angular with TypeScript
- **UI Library**: Angular Material (primary choice)
- **Styling**: SCSS with custom themes
- **Build Tool**: Angular CLI

### 2.3 Infrastructure Requirements
- **Hosting**: Cloud-based (Azure/AWS recommended)
- **Security**: HTTPS, JWT tokens, CORS configuration
- **Performance**: Response time < 2 seconds for standard operations

## 3. FUNCTIONAL REQUIREMENTS

### 3.1 User Management

#### 3.1.1 Authentication
- **FR-001**: Users must authenticate exclusively via Google OAuth2
- **FR-002**: System must store user profile (name, email, Google ID)
- **FR-003**: Session management with automatic token refresh
- **FR-004**: Secure logout with token invalidation

#### 3.1.2 User Profile
- **FR-005**: Display user name and profile picture from Google account
- **FR-006**: Allow user preferences configuration (currency, date format, language)

### 3.2 Account Management

#### 3.2.1 Account Types
- **FR-007**: Support two account types:
  - **Conta Corrente/Principal**: Standard checking account with initial balance
  - **Cartão de Crédito**: Credit card with closing date and due date

#### 3.2.2 Account Operations
- **FR-008**: Create, read, update, delete accounts
- **FR-009**: Set initial balance manually for new accounts
- **FR-010**: Configure closing date (1-31) and due date (1-31) for credit cards
- **FR-011**: Automatic credit card statement consolidation on due date
- **FR-020**: Users can manage credit cards without having main accounts
- **FR-021**: Multiple accounts per user with no limits
- **FR-022**: Credit card payments auto-generated as expenses in payment source account

#### 3.2.3 Joint Accounts
- **FR-012**: Any user can create joint accounts and becomes administrator
- **FR-013**: Administrator invites users via email (must have Google account)
- **FR-014**: Administrator configures sharing percentages (must sum to 100%)
- **FR-015**: Administrator assigns permission levels: view-only or full access
- **FR-016**: Only administrator can invite additional users
- **FR-017**: Only administrator can modify sharing percentages
- **FR-018**: Monthly settlement on last day of month with automatic transfers
- **FR-019**: Users don't need main account to participate in joint accounts

### 3.3 Transaction Management (Lançamentos)

#### 3.3.1 Transaction Types Hierarchy
```
Lançamento (Base)
├── Esporádico (One-time)
│   ├── Receita Esporádica
│   └── Despesa Esporádica
├── Recorrente (Recurring)
│   ├── Receita Recorrente
│   │   ├── Semanal (Weekly)
│   │   ├── Quinzenal (Bi-weekly)
│   │   ├── Mensal (Monthly)
│   │   ├── Bimestral (Bi-monthly)
│   │   ├── Trimestral (Quarterly)
│   │   ├── Semestral (Semi-annual)
│   │   ├── Anual (Annual)
│   │   ├── Todo Dia Útil (Every Business Day)
│   │   └── Customizado (1-6 days interval)
│   └── Despesa Recorrente
│       ├── Semanal (Weekly)
│       ├── Quinzenal (Bi-weekly)
│       ├── Mensal (Monthly)
│       ├── Bimestral (Bi-monthly)
│       ├── Trimestral (Quarterly)
│       ├── Semestral (Semi-annual)
│       ├── Anual (Annual)
│       ├── Todo Dia Útil (Every Business Day)
│       └── Customizado (1-6 days interval)
└── Parcelado (Installments)
    ├── Receita Parcelada
    └── Despesa Parcelada
```

#### 3.3.2 Transaction Properties
- **FR-017**: All transactions must have:
  - Description (required, max 200 chars)
  - Category (required, user-defined)
  - Account (required)
  - Provisional value (required)
  - Real value (optional, defaults to provisional)
  - Status (Provisional, Realized, Cancelled)

#### 3.3.3 Sporadic Transactions
- **FR-018**: One-time transactions created directly in monthly sheet
- **FR-019**: Can be income or expense
- **FR-020**: Immediate impact on account balance when realized

#### 3.3.4 Recurring Transactions
- **FR-021**: Repeat monthly on same day
- **FR-022**: Optional end date
- **FR-023**: Auto-generate entries in monthly sheets
- **FR-024**: Allow modification of individual occurrences

#### 3.3.5 Installment Transactions
- **FR-025**: Fixed number of installments (N)
- **FR-026**: Start date and automatic end date calculation
- **FR-027**: Each installment can have different real value
- **FR-028**: Track installment progress (X of N)

#### 3.3.6 Recurring Transaction Patterns
- **FR-029**: Flexible recurrence patterns:
  - Semanal (every 7 days)
  - Quinzenal (every 14 days)
  - Mensal (every 30 days)
  - Bimestral (every 2 months)
  - Trimestral (every 3 months)
  - Semestral (every 6 months)
  - Anual (every 365 days)
  - Todo Dia Útil (every business day of the month)
  - Customizado (every 1-6 days)
- **FR-030**: Optional end date or occurrence count
- **FR-031**: Weekend collision handling: auto-adjust to next business day
- **FR-032**: Custom interval range: minimum 1 day, maximum 6 days

#### 3.3.7 Transaction Lifecycle Management
- **FR-033**: Recurring transaction modifications apply only to future occurrences
- **FR-034**: Modifications affect past occurrences only if monthly sheets are not closed
- **FR-035**: Individual occurrences can be marked as "Cancelled" (not deleted)
- **FR-036**: Installment transactions support early payoff with "Quitado" status
- **FR-037**: Early payoff creates single transaction on specified date with real amount

### 3.4 Monthly Sheet (Folha Mensal)

#### 3.4.1 Sheet Generation
- **FR-038**: Auto-generate monthly sheets for current and future months
- **FR-039**: Initial balance = previous month's closing balance
- **FR-040**: Auto-populate recurring, installment transactions
- **FR-041**: Allow manual addition of sporadic transactions

#### 3.4.2 Monthly Sheet Status Management
- **FR-042**: Each monthly sheet has status: Open or Closed
- **FR-043**: Credit card sheets close on closing date, others close on last day of month
- **FR-044**: Only administrator can reopen closed monthly sheets
- **FR-045**: Can only reopen month M if month M+1 is open
- **FR-046**: Cannot close month M-1 if month M-2 is open
- **FR-047**: Changes to open sheets propagate to all subsequent open sheets
- **FR-048**: No transactions allowed on closed sheets (except by reopening)

#### 3.4.3 Balance Calculations
- **FR-049**: Track provisional vs. real balances separately
- **FR-050**: Real balance = Initial + Realized Income - Realized Expenses
- **FR-051**: Provisional balance = Initial + All Provisional Income - All Provisional Expenses
- **FR-052**: Display balance projections for future months

#### 3.4.4 Sheet Operations
- **FR-053**: View monthly sheets for any month (past/present/future)
- **FR-054**: Mark transactions as realized with actual values
- **FR-055**: Add/edit/delete transactions within open months only
- **FR-056**: Export monthly sheet to PDF/Excel

### 3.5 Dashboard

#### 3.5.1 Balance Overview
- **FR-047**: Display current balance for all accounts
- **FR-048**: Show total net worth (sum of all accounts)
- **FR-049**: Balance trend chart (last 6 months)

#### 3.5.2 Projections
- **FR-067**: End-of-month balance projection (up to 1 year)
- **FR-068**: Projections based only on realized transactions with real values
- **FR-069**: Projection accuracy indicator

#### 3.5.3 Upcoming Items
- **FR-070**: Next 7 days due transactions
- **FR-071**: Credit card due dates
- **FR-072**: Joint account settlement dates

#### 3.5.4 Category Dashboard
- **FR-073**: Display all categories with provisional amounts
- **FR-074**: Show related transactions per category
- **FR-075**: Track provisional vs. real spending per category
- **FR-076**: Category balance can be negative without warnings

### 3.6 Account Relationships and Payment Sources

#### 3.6.1 Account Relationships
- **FR-057**: Accounts can have 0 or N related accounts
- **FR-058**: Accounts can have 0 or 1 payment source account (fonte pagadoura)
- **FR-059**: Joint accounts can configure payment source for settlement transfers
- **FR-060**: Credit card accounts can configure payment source for automatic payments

### 3.7 Categories and Provisioning

#### 3.7.1 Category Management
- **FR-061**: Create, edit, delete custom categories (e.g., "Mercado", "Transporte", "Saúde")
- **FR-062**: Categories have provisional monthly amounts set by user
- **FR-063**: Transactions can be assigned to categories (optional)
- **FR-064**: Category tracking shows provisional vs. real spending
- **FR-065**: Category real spending = sum of realized transaction amounts
- **FR-066**: Categories can show negative balances without restrictions

## 4. NON-FUNCTIONAL REQUIREMENTS

### 4.1 Performance
- **NFR-001**: Page load time < 3 seconds
- **NFR-002**: API response time < 2 seconds
- **NFR-003**: Support 100 concurrent users
- **NFR-004**: Database queries optimized with proper indexing

### 4.2 Security
- **NFR-005**: All data transmission over HTTPS
- **NFR-006**: JWT token expiration and refresh mechanism
- **NFR-007**: Input validation and sanitization
- **NFR-008**: SQL injection prevention
- **NFR-009**: CORS policy configuration

### 4.3 Usability
- **NFR-010**: Responsive design (mobile-first approach)
- **NFR-011**: Brazilian Portuguese localization
- **NFR-012**: Currency formatting (R$ 1.234,56)
- **NFR-013**: Date formatting (DD/MM/YYYY)
- **NFR-014**: Accessibility compliance (WCAG 2.1 AA)

### 4.4 Reliability
- **NFR-015**: 99.5% uptime availability
- **NFR-016**: Automated database backups (daily)
- **NFR-017**: Error logging and monitoring
- **NFR-018**: Graceful error handling with user-friendly messages

### 4.5 Scalability
- **NFR-019**: Horizontal scaling capability
- **NFR-020**: Database connection pooling
- **NFR-021**: Caching strategy for frequently accessed data
- **NFR-022**: API rate limiting

## 5. DATA REQUIREMENTS

### 5.1 Data Entities
- **Users**: Google OAuth profile data
- **Accounts**: Account details, balances, configurations
- **Transactions**: All transaction types with inheritance
- **Categories**: User-defined and system categories
- **Monthly Sheets**: Generated monthly financial summaries
- **Joint Account Relationships**: User associations and percentages

### 5.2 Data Retention
- **DR-001**: Transaction data retained indefinitely
- **DR-002**: User session data retained for 30 days
- **DR-003**: Audit logs retained for 1 year
- **DR-004**: Backup data retained for 7 years

### 5.3 Data Privacy
- **DR-005**: LGPD compliance for Brazilian users
- **DR-006**: Data encryption at rest and in transit
- **DR-007**: User data deletion capability
- **DR-008**: Data export functionality

## 6. INTEGRATION REQUIREMENTS

### 6.1 External Services
- **INT-001**: Google OAuth2 API integration
- **INT-002**: Email service for joint account invitations
- **INT-003**: Future: Bank API integration (Open Banking)
- **INT-004**: Future: OFX/PDF import capability

### 6.2 API Design
- **INT-005**: RESTful API with OpenAPI documentation
- **INT-006**: API versioning strategy
- **INT-007**: Consistent error response format
- **INT-008**: Request/response logging

## 7. DEPLOYMENT REQUIREMENTS

### 7.1 Environment Setup
- **DEP-001**: Development, staging, and production environments
- **DEP-002**: Automated CI/CD pipeline
- **DEP-003**: Database migration strategy
- **DEP-004**: Environment-specific configuration management

### 7.2 Monitoring
- **DEP-005**: Application performance monitoring
- **DEP-006**: Database performance monitoring
- **DEP-007**: Error tracking and alerting
- **DEP-008**: User activity analytics

## 8. FUTURE ENHANCEMENTS

### 8.1 Phase 2 Features
- **FUT-001**: Mobile application (React Native/Flutter)
- **FUT-002**: Advanced reporting and analytics
- **FUT-003**: Financial goals and budgeting
- **FUT-004**: Investment portfolio tracking
- **FUT-005**: Bank statement import (OFX/PDF)

### 8.2 Phase 3 Features
- **FUT-006**: Multi-currency support
- **FUT-007**: Financial advisor integration
- **FUT-008**: Tax calculation and reporting
- **FUT-009**: Bill payment integration
- **FUT-010**: Cryptocurrency tracking
