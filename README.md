# Design Patterns Demonstration: Strategy and Mediator Patterns

This project demonstrates the transformative power of design patterns in Object-Oriented programming by showing how to refactor badly written legacy code into maintainable, extensible solutions using the **Strategy** and **Mediator** patterns.

## 🎯 Project Overview

The project illustrates the evolution of an order processing system from a "big ball of mud" legacy implementation to clean, maintainable code. It focuses on:

- **Complex Order Processing**: Orders containing products, product ensembles, and product kits
- **CSV Reporting**: Generating audit reports for a Retail Audit system
- **Design Pattern Application**: Practical implementation of Strategy and Mediator patterns

## 📁 Project Structure

```
DesignPatternsStrategyAndMediator/
├── Models/
│   └── OrderModels.cs              # Complex domain models (Order, Product, Ensemble, Kit)
├── Legacy/
│   └── LegacyOrderProcessor.cs     # "Big ball of mud" legacy code (intentionally bad)
├── Patterns/
│   ├── Strategy/
│   │   ├── ICsvReportingStrategy.cs        # Strategy pattern interface
│   │   ├── StandardRetailAuditStrategy.cs  # Standard CSV format
│   │   ├── EnhancedRetailAuditStrategy.cs  # Enhanced CSV with compliance data
│   │   ├── FinancialSummaryStrategy.cs     # Financial summary format
│   │   └── ImprovedOrderProcessor.cs       # Clean processor using Strategy pattern
│   └── Mediator/
│       ├── IOrderProcessingMediator.cs     # Mediator pattern interface
│       ├── OrderProcessingMediator.cs      # Concrete mediator implementation
│       └── RequestHandlers.cs              # Individual request handlers
├── Program.cs                      # Demonstration program
└── README.md                       # This file
```

## 🚫 Legacy Code Problems

The `LegacyOrderProcessor` demonstrates common anti-patterns and code smells:

### Major Issues:
1. **Monolithic Method**: Single method doing everything (700+ lines)
2. **Mixed Responsibilities**: Business logic + I/O + formatting + validation
3. **Side Effects**: Modifies the original Order object during processing
4. **Hard-coded Dependencies**: Fixed CSV format, direct file I/O
5. **Violates SOLID Principles**: 
   - **SRP**: Multiple responsibilities in one class
   - **OCP**: Cannot extend without modifying existing code
   - **DIP**: Depends on concrete implementations
6. **Poor Error Handling**: Generic try-catch with limited context
7. **Difficult Testing**: Cannot test individual components in isolation
8. **Code Duplication**: Similar logic repeated for different product types

### Example of Legacy Problems:
```csharp
// 700+ line method that does EVERYTHING
public void ProcessOrderAndGenerateRetailAuditReport(Order order)
{
    // Validation mixed with business logic
    if (string.IsNullOrEmpty(order.OrderId))
    {
        order.OrderId = "ORD-" + DateTime.Now.Ticks; // Side effect!
    }
    
    // Business calculations mixed with formatting
    foreach (var product in order.IndividualProducts)
    {
        // Calculation logic...
        // Then immediately followed by CSV formatting...
        csvContent.AppendLine($"{order.OrderId},{product.Name}..."); // Hard-coded format!
    }
    
    // File I/O mixed with everything else
    File.WriteAllText(fullPath, csvContent.ToString()); // No abstraction!
}
```

## ✅ Strategy Pattern Improvements

The Strategy pattern addresses the CSV formatting concerns by:

### Benefits:
1. **Separation of Concerns**: Business logic separated from formatting
2. **Multiple Formats**: Support different CSV formats without changing core logic
3. **Open/Closed Principle**: Add new formats without modifying existing code
4. **No Side Effects**: Original order object remains unchanged
5. **Better Testing**: Each strategy can be unit tested independently

### Strategy Pattern Implementation:

```csharp
// Strategy interface allows multiple CSV formats
public interface ICsvReportingStrategy
{
    string GenerateCsvContent(Order order, OrderCalculationResults results);
    string GetFileName(Order order);
    string Description { get; }
}

// Multiple concrete strategies
public class StandardRetailAuditStrategy : ICsvReportingStrategy { ... }
public class EnhancedRetailAuditStrategy : ICsvReportingStrategy { ... }
public class FinancialSummaryStrategy : ICsvReportingStrategy { ... }

// Clean processor using strategy
public class ImprovedOrderProcessor
{
    private readonly ICsvReportingStrategy _csvStrategy;
    
    public OrderProcessingResult ProcessOrder(Order order)
    {
        var calculations = _calculator.CalculateOrder(order);
        string csvContent = _csvStrategy.GenerateCsvContent(order, calculations);
        // Clean, focused logic...
    }
}
```

### Available CSV Strategies:
- **StandardRetailAuditStrategy**: Compatible with legacy systems
- **EnhancedRetailAuditStrategy**: Includes compliance and tracking data  
- **FinancialSummaryStrategy**: High-level financial reporting format

## 🎯 Mediator Pattern Enhancements

The Mediator pattern provides additional architectural benefits:

### Additional Benefits:
1. **Centralized Orchestration**: Complex workflows managed in one place
2. **Loose Coupling**: Components don't need to know about each other
3. **Cross-cutting Concerns**: Validation, logging, notifications handled consistently
4. **Request/Response Pattern**: Structured communication between components
5. **Easy Extension**: Add new processing steps without changing existing code

### Mediator Pattern Implementation:

```csharp
public interface IOrderProcessingMediator
{
    Task<OrderProcessingResult> ProcessOrderAsync(ProcessOrderRequest request);
    Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request);
}

// Centralized processing with cross-cutting concerns
public async Task<OrderProcessingResult> ProcessOrderAsync(ProcessOrderRequest request)
{
    // 1. Validate order
    var validationResult = await SendAsync<OrderValidationResult>(
        new ValidateOrderRequest { Order = request.Order });
    
    // 2. Process with selected strategy
    var processor = new ImprovedOrderProcessor(strategy, calculator, fileWriter);
    var result = processor.ProcessOrder(request.Order);
    
    // 3. Log audit trail
    await SendAsync<AuditLogResult>(new LogAuditRequest { ... });
    
    // 4. Send notifications
    await SendAsync<NotificationResult>(new SendNotificationRequest { ... });
    
    return result;
}
```

### Mediator Handlers:
- **OrderValidationHandler**: Comprehensive order validation
- **NotificationHandler**: Email/SMS notifications  
- **AuditLogHandler**: Audit trail logging

## 🏃‍♂️ Running the Demonstration

### Prerequisites:
- .NET 9.0 SDK or later
- Visual Studio 2022 / VS Code / JetBrains Rider

### Steps:
1. Clone the repository
2. Open the solution in your IDE
3. Build the project: `dotnet build`
4. Run the demonstration: `dotnet run`

### What the Demo Shows:
1. **Legacy Approach**: Demonstrates the problems with monolithic code
2. **Strategy Pattern**: Shows flexible CSV generation with multiple formats
3. **Mediator Pattern**: Demonstrates orchestrated processing with cross-cutting concerns
4. **Comparison**: Side-by-side comparison of all approaches

### Sample Output:
```
=======================================================
DESIGN PATTERNS DEMONSTRATION
Transforming Legacy Code with Strategy & Mediator Patterns
=======================================================

1. LEGACY APPROACH (Big Ball of Mud)
=====================================
Problems with the legacy approach:
- Monolithic method doing everything
- Mixed responsibilities (business logic + I/O + formatting)
- Hard to test individual components
- Violates SOLID principles
- Side effects: modifies the original order object
- Hard-coded CSV format

=== LEGACY ORDER PROCESSOR ===
Processing order: ORD-2024-001
[... processing details ...]

2. STRATEGY PATTERN IMPROVEMENT
=================================
Improvements with Strategy Pattern:
- Separation of concerns: calculation vs reporting
- Multiple CSV formats supported
- Open/Closed principle: easy to add new formats
- No side effects on original order
- Better error handling

✓ Successfully generated: RetailAudit_ORD-2024-001_20241002_141530.csv
✓ Successfully generated: EnhancedRetailAudit_ORD-2024-001_20241002_141530.csv
✓ Successfully generated: FinancialSummary_ORD-2024-001_20241002_141530.csv

3. MEDIATOR PATTERN ENHANCEMENT
================================
Additional benefits with Mediator Pattern:
- Centralized orchestration of complex workflows
- Cross-cutting concerns (validation, logging, notifications)
- Loose coupling between components
- Easy to add new processing steps
- Consistent error handling and logging

✓ Mediator successfully processed order with Standard strategy
✓ Mediator successfully processed order with Enhanced strategy  
✓ Mediator successfully processed order with Financial strategy
```

## 📊 Pattern Comparison

| Aspect | Legacy Code | Strategy Pattern | Mediator Pattern |
|--------|-------------|------------------|------------------|
| **Maintainability** | ❌ Very Poor | ✅ Good | ✅ Excellent |
| **Testability** | ❌ Difficult | ✅ Good | ✅ Excellent |
| **Extensibility** | ❌ Hard | ✅ Easy | ✅ Very Easy |
| **Coupling** | ❌ Tight | ✅ Loose | ✅ Very Loose |
| **SOLID Compliance** | ❌ Violates Most | ✅ Good | ✅ Excellent |
| **Error Handling** | ❌ Poor | ✅ Better | ✅ Comprehensive |
| **Code Reusability** | ❌ None | ✅ Good | ✅ Excellent |

## 🎓 Learning Outcomes

After studying this project, you should understand:

1. **Why Legacy Code is Problematic**: Real examples of anti-patterns and code smells
2. **Strategy Pattern Benefits**: How to make algorithms interchangeable and extensible
3. **Mediator Pattern Benefits**: How to orchestrate complex workflows cleanly
4. **SOLID Principles Application**: Practical implementation of good design principles
5. **Refactoring Strategies**: Step-by-step improvement of existing code
6. **Testing Implications**: How design patterns improve testability

## 🔧 Extending the Project

### Adding New CSV Formats:
```csharp
public class CustomAuditStrategy : ICsvReportingStrategy
{
    public string Description => "Custom Audit Format";
    
    public string GenerateCsvContent(Order order, OrderCalculationResults results)
    {
        // Implement your custom CSV format
        return customCsvContent;
    }
    
    public string GetFileName(Order order) => $"Custom_{order.OrderId}.csv";
}

// Register with mediator
mediator.RegisterStrategy("Custom", new CustomAuditStrategy());
```

### Adding New Request Handlers:
```csharp
public class EmailValidationRequest : IRequest<EmailValidationResult> { ... }

public class EmailValidationHandler : IRequestHandler<EmailValidationRequest, EmailValidationResult>
{
    public Task<EmailValidationResult> HandleAsync(EmailValidationRequest request)
    {
        // Implement email validation logic
    }
}

// Register with mediator
mediator.RegisterHandler<EmailValidationRequest, EmailValidationResult>(new EmailValidationHandler());
```

## 🏗️ Architecture Principles Demonstrated

1. **Single Responsibility Principle (SRP)**: Each class has one reason to change
2. **Open/Closed Principle (OCP)**: Open for extension, closed for modification
3. **Dependency Inversion Principle (DIP)**: Depend on abstractions, not concretions
4. **Separation of Concerns**: Business logic, formatting, and I/O are separated
5. **Command Query Separation**: Commands and queries are clearly separated
6. **Don't Repeat Yourself (DRY)**: Common functionality is abstracted

## 📝 Key Takeaways

1. **Design Patterns Solve Real Problems**: Not just academic concepts, but practical solutions
2. **Incremental Improvement**: You can refactor legacy code step by step
3. **Testing Benefits**: Well-designed code is much easier to test
4. **Team Collaboration**: Clean architecture enables better team collaboration
5. **Maintenance Costs**: Good design pays off in reduced maintenance costs
6. **Business Agility**: Clean code enables faster feature delivery

## 🤝 Contributing

This is a demonstration project, but contributions are welcome:
- Add new CSV strategies
- Improve error handling
- Add more comprehensive validation
- Enhance the documentation
- Add unit tests

## 📄 License

This project is for educational purposes. Feel free to use it for learning and teaching design patterns.

---

*This project demonstrates that with proper application of design patterns, even the worst legacy code can be transformed into maintainable, testable, and extensible solutions.*