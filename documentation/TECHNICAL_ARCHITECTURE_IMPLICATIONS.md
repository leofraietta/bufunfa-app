# Bufunfa - Technical Architecture Implications

## Overview

This document analyzes the technical architecture implications of the refined requirements, identifying key design decisions, database schema changes, and implementation challenges that must be addressed during development.

## Database Schema Implications

### Core Entity Changes

#### 1. Transaction Hierarchy Refactoring
**Current State**: Simple transaction types
**New Requirements**: Complex recurring patterns with status management

```sql
-- New Transaction Status Enum
CREATE TYPE transaction_status AS ENUM (
    'Provisional', 'Realized', 'Cancelled', 'Quitado'
);

-- Enhanced Transaction Base Table
ALTER TABLE Transactions ADD COLUMN status transaction_status DEFAULT 'Provisional';
ALTER TABLE Transactions ADD COLUMN category_id INTEGER REFERENCES Categories(id);

-- New Recurring Pattern Types
CREATE TYPE recurring_pattern AS ENUM (
    'Semanal', 'Quinzenal', 'Mensal', 'Bimestral', 
    'Trimestral', 'Semestral', 'Anual', 'TodoDiaUtil', 'Customizado'
);

-- Enhanced Recurring Transaction Table
ALTER TABLE RecurringTransactions ADD COLUMN pattern recurring_pattern;
ALTER TABLE RecurringTransactions ADD COLUMN custom_interval_days INTEGER;
ALTER TABLE RecurringTransactions ADD COLUMN weekend_adjustment BOOLEAN DEFAULT true;
```

#### 2. Monthly Sheet Status Management
**New Concept**: Sheet-level status tracking with propagation rules

```sql
-- New Monthly Sheet Status
CREATE TYPE sheet_status AS ENUM ('Open', 'Closed');

CREATE TABLE MonthlySheetStatus (
    id SERIAL PRIMARY KEY,
    account_id INTEGER NOT NULL REFERENCES Accounts(id),
    year INTEGER NOT NULL,
    month INTEGER NOT NULL,
    status sheet_status DEFAULT 'Open',
    closed_date TIMESTAMP,
    closed_by_user_id INTEGER REFERENCES Users(id),
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),
    UNIQUE(account_id, year, month)
);

-- Indexes for performance
CREATE INDEX idx_monthly_sheet_status_account_date ON MonthlySheetStatus(account_id, year, month);
CREATE INDEX idx_monthly_sheet_status_status ON MonthlySheetStatus(status);
```

#### 3. Account Relationships System
**New Concept**: Flexible account interconnections

```sql
-- Account Relationship Types
CREATE TYPE relationship_type AS ENUM ('PaymentSource', 'Related');

CREATE TABLE AccountRelationships (
    id SERIAL PRIMARY KEY,
    source_account_id INTEGER NOT NULL REFERENCES Accounts(id),
    target_account_id INTEGER NOT NULL REFERENCES Accounts(id),
    relationship_type relationship_type NOT NULL,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT NOW(),
    CONSTRAINT no_self_reference CHECK (source_account_id != target_account_id)
);

-- Ensure only one payment source per account
CREATE UNIQUE INDEX idx_unique_payment_source 
ON AccountRelationships(source_account_id) 
WHERE relationship_type = 'PaymentSource' AND is_active = true;
```

#### 4. Joint Account Administration
**Enhanced Concept**: Administrator roles with permission levels

```sql
-- Permission Levels
CREATE TYPE permission_level AS ENUM ('ViewOnly', 'FullAccess');

-- Enhanced Joint Account User Relationship
ALTER TABLE ContaUsuarios ADD COLUMN is_administrator BOOLEAN DEFAULT false;
ALTER TABLE ContaUsuarios ADD COLUMN permission_level permission_level DEFAULT 'FullAccess';
ALTER TABLE ContaUsuarios ADD COLUMN invited_by_user_id INTEGER REFERENCES Users(id);
ALTER TABLE ContaUsuarios ADD COLUMN invited_at TIMESTAMP;

-- Ensure only one administrator per joint account
CREATE UNIQUE INDEX idx_unique_administrator 
ON ContaUsuarios(conta_id) 
WHERE is_administrator = true AND ativo = true;
```

#### 5. Category Provisioning System
**New Concept**: Independent category entities with provisioning

```sql
CREATE TABLE Categories (
    id SERIAL PRIMARY KEY,
    user_id INTEGER NOT NULL REFERENCES Users(id),
    name VARCHAR(100) NOT NULL,
    provisional_monthly_amount DECIMAL(10,2) DEFAULT 0,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),
    UNIQUE(user_id, name)
);

-- Category spending calculation view
CREATE VIEW CategorySpending AS
SELECT 
    c.id as category_id,
    c.name,
    c.provisional_monthly_amount,
    COALESCE(SUM(CASE WHEN t.status = 'Realized' THEN t.valor_real ELSE 0 END), 0) as real_spending,
    c.provisional_monthly_amount - COALESCE(SUM(CASE WHEN t.status = 'Realized' THEN t.valor_real ELSE 0 END), 0) as balance
FROM Categories c
LEFT JOIN Transactions t ON c.id = t.category_id
WHERE c.is_active = true
GROUP BY c.id, c.name, c.provisional_monthly_amount;
```

## Business Logic Architecture

### 1. Monthly Sheet Propagation Engine

**Challenge**: Changes to past months must propagate through all subsequent open months
**Solution**: Event-driven propagation system with transaction safety

```csharp
public interface IMonthlySheetPropagationService
{
    Task PropagateChangesAsync(int accountId, DateTime fromMonth, decimal balanceChange);
    Task ValidateReopeningRulesAsync(int accountId, DateTime monthToReopen);
    Task<bool> CanReopenMonthAsync(int accountId, DateTime month);
}

public class MonthlySheetPropagationService : IMonthlySheetPropagationService
{
    public async Task PropagateChangesAsync(int accountId, DateTime fromMonth, decimal balanceChange)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var openSheets = await GetSubsequentOpenSheetsAsync(accountId, fromMonth);
            foreach (var sheet in openSheets)
            {
                await UpdateSheetBalanceAsync(sheet, balanceChange);
            }
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
```

### 2. Business Day Calculation Service

**Challenge**: Weekend collision handling for recurring transactions
**Solution**: Configurable business day service with future holiday support

```csharp
public interface IBusinessDayService
{
    DateTime AdjustToNextBusinessDay(DateTime date);
    bool IsBusinessDay(DateTime date);
    DateTime CalculateNextOccurrence(DateTime baseDate, RecurringPattern pattern, int? customInterval = null);
}

public class BusinessDayService : IBusinessDayService
{
    public DateTime AdjustToNextBusinessDay(DateTime date)
    {
        while (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
        {
            date = date.AddDays(1);
        }
        return date;
    }

    public DateTime CalculateNextOccurrence(DateTime baseDate, RecurringPattern pattern, int? customInterval = null)
    {
        var nextDate = pattern switch
        {
            RecurringPattern.Semanal => baseDate.AddDays(7),
            RecurringPattern.Quinzenal => baseDate.AddDays(14),
            RecurringPattern.Mensal => baseDate.AddMonths(1),
            RecurringPattern.Bimestral => baseDate.AddMonths(2),
            RecurringPattern.Trimestral => baseDate.AddMonths(3),
            RecurringPattern.Semestral => baseDate.AddMonths(6),
            RecurringPattern.Anual => baseDate.AddYears(1),
            RecurringPattern.Customizado => baseDate.AddDays(customInterval ?? 1),
            RecurringPattern.TodoDiaUtil => CalculateNextBusinessDay(baseDate),
            _ => throw new ArgumentException($"Unknown pattern: {pattern}")
        };

        return AdjustToNextBusinessDay(nextDate);
    }
}
```

### 3. Permission-Based Access Control

**Challenge**: Complex permission validation for joint accounts
**Solution**: Attribute-based authorization with role checking

```csharp
[AttributeUsage(AttributeTargets.Method)]
public class RequireAccountAccessAttribute : Attribute
{
    public PermissionLevel MinimumPermission { get; set; } = PermissionLevel.ViewOnly;
    public bool RequireAdministrator { get; set; } = false;
}

public class AccountAccessService : IAccountAccessService
{
    public async Task<bool> HasAccessAsync(int userId, int accountId, PermissionLevel requiredPermission)
    {
        var userAccount = await _context.ContaUsuarios
            .FirstOrDefaultAsync(cu => cu.UsuarioId == userId && cu.ContaId == accountId && cu.Ativo);

        if (userAccount == null) return false;

        return requiredPermission switch
        {
            PermissionLevel.ViewOnly => true,
            PermissionLevel.FullAccess => userAccount.PermissionLevel == PermissionLevel.FullAccess,
            _ => false
        };
    }
}
```

### 4. Category Real Spending Calculator

**Challenge**: Efficient calculation of category spending across large datasets
**Solution**: Optimized aggregation with caching strategy

```csharp
public interface ICategorySpendingService
{
    Task<CategorySpendingDto> GetCategorySpendingAsync(int categoryId, DateTime month);
    Task<List<CategorySpendingDto>> GetAllCategorySpendingAsync(int userId, DateTime month);
    Task InvalidateCacheAsync(int categoryId, DateTime month);
}

public class CategorySpendingService : ICategorySpendingService
{
    private readonly IMemoryCache _cache;
    
    public async Task<CategorySpendingDto> GetCategorySpendingAsync(int categoryId, DateTime month)
    {
        var cacheKey = $"category_spending_{categoryId}_{month:yyyy-MM}";
        
        if (_cache.TryGetValue(cacheKey, out CategorySpendingDto cached))
            return cached;

        var spending = await CalculateCategorySpendingAsync(categoryId, month);
        
        _cache.Set(cacheKey, spending, TimeSpan.FromMinutes(15));
        return spending;
    }
}
```

## Performance Considerations

### 1. Database Indexing Strategy

```sql
-- Critical indexes for performance
CREATE INDEX idx_transactions_account_date ON Transactions(conta_id, data_inicial);
CREATE INDEX idx_transactions_category_status ON Transactions(category_id, status) WHERE category_id IS NOT NULL;
CREATE INDEX idx_transactions_user_realized ON Transactions(usuario_id, status) WHERE status = 'Realized';
CREATE INDEX idx_recurring_next_occurrence ON RecurringTransactions(next_occurrence_date) WHERE is_active = true;

-- Composite indexes for complex queries
CREATE INDEX idx_monthly_sheets_propagation ON MonthlySheetStatus(account_id, year, month, status);
CREATE INDEX idx_account_relationships_active ON AccountRelationships(source_account_id, relationship_type) WHERE is_active = true;
```

### 2. Query Optimization Patterns

```csharp
// Efficient category spending calculation
public async Task<List<CategorySpendingDto>> GetCategorySpendingOptimizedAsync(int userId, DateTime month)
{
    return await _context.Categories
        .Where(c => c.UserId == userId && c.IsActive)
        .Select(c => new CategorySpendingDto
        {
            CategoryId = c.Id,
            Name = c.Name,
            ProvisionalAmount = c.ProvisionalMonthlyAmount,
            RealSpending = c.Transactions
                .Where(t => t.Status == TransactionStatus.Realized && 
                           t.DataInicial.Year == month.Year && 
                           t.DataInicial.Month == month.Month)
                .Sum(t => t.ValorReal ?? t.ValorProvisionado)
        })
        .ToListAsync();
}
```

### 3. Caching Strategy

**Level 1**: In-memory caching for frequently accessed calculations
- Category spending calculations (15-minute TTL)
- User permissions (5-minute TTL)
- Business day calculations (1-hour TTL)

**Level 2**: Distributed caching for shared data
- Account balances (30-second TTL)
- Monthly sheet status (1-minute TTL)

**Level 3**: Database-level caching
- Materialized views for complex aggregations
- Partial indexes for filtered queries

## Security Architecture

### 1. Data Access Security

```csharp
// Row-level security for multi-tenant data
public class SecureDbContext : ApplicationDbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Ensure users can only access their own data
        modelBuilder.Entity<Transaction>()
            .HasQueryFilter(t => t.UsuarioId == _currentUserService.UserId);
            
        modelBuilder.Entity<Account>()
            .HasQueryFilter(a => a.ContaUsuarios.Any(cu => cu.UsuarioId == _currentUserService.UserId && cu.Ativo));
    }
}
```

### 2. API Security Patterns

```csharp
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    [HttpPost]
    [RequireAccountAccess(MinimumPermission = PermissionLevel.FullAccess)]
    public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionDto dto)
    {
        // Validate user has access to specified account
        if (!await _accountAccessService.HasAccessAsync(GetUserId(), dto.AccountId, PermissionLevel.FullAccess))
            return Forbid();
            
        // Implementation...
    }
}
```

## Scalability Considerations

### 1. Database Scaling Strategy

**Vertical Scaling**: Initial approach for MVP
- PostgreSQL with optimized configuration
- Connection pooling (min: 10, max: 100)
- Read replicas for reporting queries

**Horizontal Scaling**: Future consideration
- Partition transactions by user_id
- Separate read/write databases
- Event sourcing for audit trail

### 2. Application Scaling

**Stateless Design**: All services designed to be stateless
- JWT tokens for authentication
- Database-backed sessions
- No server-side state storage

**Microservices Preparation**: Modular design for future splitting
- Clear service boundaries
- Interface-based dependencies
- Event-driven communication patterns

## Monitoring and Observability

### 1. Application Metrics

```csharp
public class MetricsService : IMetricsService
{
    private readonly IMetricsCollector _metrics;
    
    public async Task TrackTransactionCreation(string transactionType)
    {
        _metrics.Counter("transactions_created")
            .WithTag("type", transactionType)
            .Increment();
    }
    
    public async Task TrackSheetPropagation(int sheetsAffected, TimeSpan duration)
    {
        _metrics.Histogram("sheet_propagation_duration")
            .Record(duration.TotalMilliseconds);
            
        _metrics.Gauge("sheets_propagated")
            .Set(sheetsAffected);
    }
}
```

### 2. Database Monitoring

- Query performance tracking
- Connection pool utilization
- Index usage statistics
- Slow query identification

### 3. Business Metrics

- Transaction creation rates by type
- Category usage patterns
- Joint account adoption rates
- Monthly sheet closure patterns

## Migration Strategy

### 1. Database Migrations

**Phase 1**: Core schema changes
- Add new columns with default values
- Create new tables for enhanced features
- Migrate existing data to new structures

**Phase 2**: Data transformation
- Convert existing transactions to new hierarchy
- Initialize monthly sheet statuses
- Set up default categories for existing users

**Phase 3**: Cleanup
- Remove deprecated columns
- Optimize indexes
- Update constraints

### 2. Application Migration

**Blue-Green Deployment**: Zero-downtime updates
- Parallel environment deployment
- Database migration during maintenance window
- Traffic switching after validation

**Feature Flags**: Gradual feature rollout
- New features behind flags initially
- Gradual user base exposure
- Quick rollback capability

## Risk Mitigation

### 1. Data Consistency Risks

**Risk**: Sheet propagation failures causing inconsistent balances
**Mitigation**: 
- Atomic transactions for all propagation operations
- Comprehensive rollback procedures
- Balance reconciliation jobs
- Audit logging for all changes

### 2. Performance Risks

**Risk**: Category calculations becoming slow with large datasets
**Mitigation**:
- Efficient indexing strategy
- Caching at multiple levels
- Background calculation jobs
- Query optimization monitoring

### 3. Security Risks

**Risk**: Complex permission system creating access control vulnerabilities
**Mitigation**:
- Comprehensive authorization testing
- Regular security audits
- Principle of least privilege
- Input validation at all levels

---

*Document Version: 1.0*  
*Last Updated: September 7, 2025*  
*Next Review: Start of Phase 1 Implementation*
