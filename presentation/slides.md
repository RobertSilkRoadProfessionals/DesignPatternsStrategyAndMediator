---
# Behavioral Design Patterns
## Strategy & Mediator Patterns

theme: default
background: https://source.unsplash.com/1920x1080/?software,code
class: text-center
highlighter: shiki
lineNumbers: false
info: |
  ## Behavioral Design Patterns
  
  A practical guide to Strategy and Mediator patterns
  for software engineers.

drawings:
  persist: false
transition: slide-left
title: Behavioral Design Patterns - Strategy & Mediator
mdc: true
---

# Behavioral Design Patterns
## Strategy & Mediator Patterns

<div class="pt-12">
  <span @click="$slidev.nav.next" class="px-2 py-1 rounded cursor-pointer" hover="bg-white bg-opacity-10">
    Building Better Software Through Smart Object Interactions 🚀
  </span>
</div>

<div class="abs-br m-6 flex gap-2">
  <a href="https://github.com/slidevjs/slidev" target="_blank" alt="GitHub"
    class="text-xl slidev-icon-btn opacity-50 !border-none !hover:text-white">
    <carbon-logo-github />
  </a>
</div>

<!--
Welcome everyone! Today we're going to explore behavioral design patterns, specifically Strategy and Mediator patterns. These patterns help us manage complex object interactions and make our code more maintainable.
-->

---
transition: fade-out
---

# What Are Behavioral Patterns?

Behavioral patterns focus on **communication between objects** and the **assignment of responsibilities**.

<br>

## Key Characteristics:
- 🎯 **How objects interact** and communicate
- 📋 **Distribution of responsibilities** between objects  
- 🔄 **Algorithms and flow control** management
- 🎭 **Runtime behavior** modification

<br>

## Common Problems They Solve:
- Complex conditional logic (if/else chains)
- Tight coupling between objects
- Difficulty adding new behaviors
- Hard-to-test algorithms

<!--
Behavioral patterns are all about managing how objects talk to each other and who does what. Think of them as the "social rules" for your objects.

Discussion: What kind of complex interactions do you deal with in your current projects?
-->

---

# Today's Focus: Two Essential Patterns

<div grid="~ cols-2 gap-12" class="mt-12">
<div>

## 🎯 Strategy Pattern
**"Choose your algorithm at runtime"**

- Multiple ways to do the same thing
- Switch between algorithms dynamically
- Clean separation of concerns

</div>
<div>

## 🎬 Mediator Pattern  
**"Central coordinator for complex interactions"**

- Manages communication between multiple objects
- Reduces coupling between components
- Orchestrates complex workflows

</div>
</div>

<br>

### Real-world analogy:
- **Strategy**: Different payment methods (credit card, PayPal, bank transfer)
- **Mediator**: Air traffic control tower coordinating multiple planes

<!--
We'll focus on these two patterns because they're extremely practical and you'll likely encounter scenarios where they're useful in most projects.

Think about this: How many times have you written long if/else chains to handle different scenarios? That's where Strategy pattern shines.
-->

---

# The Problem: "Big Ball of Mud" Code

Let's look at a typical legacy order processing system:

```csharp {1-3|5-15|17-25|27-30}
public class LegacyOrderProcessor
{
    // 700+ line method that does EVERYTHING!
    
    public void ProcessOrderAndGenerateReport(Order order)
    {
        // Validation mixed with business logic
        if (string.IsNullOrEmpty(order.OrderId))
            order.OrderId = "ORD-" + DateTime.Now.Ticks; // Side effect!
        
        // Calculate totals (hundreds of lines...)
        foreach (var product in order.IndividualProducts) { /*...*/ }
        foreach (var ensemble in order.ProductEnsembles) { /*...*/ }
        foreach (var kit in order.ProductKits) { /*...*/ }
        // Apply discounts, calculate VAT, etc.
        
        // CSV generation hardcoded inside business logic
        csvContent.AppendLine("OrderID,CustomerID,ProductName...");
        foreach (var product in order.IndividualProducts)
        {
            csvContent.AppendLine($"{order.OrderId},{product.Name}...");
        }
        
        // File I/O mixed with everything else
        File.WriteAllText(fullPath, csvContent.ToString());
    }
}
```

<!--
This is what we call a "big ball of mud" - everything is mixed together. Notice how:
- Business logic is mixed with file I/O
- CSV format is hardcoded
- Method has multiple responsibilities
- Hard to test individual parts
- Violates Single Responsibility Principle

Discussion: What problems do you see here? How would you add a new report format?
-->

---

# Problems with Legacy Approach

<div grid="~ cols-2 gap-8" class="mt-8">
<div>

## 🚫 What's Wrong?

- **Mixed Responsibilities**: Business logic + formatting + I/O
- **Hard-coded Dependencies**: Only one CSV format
- **Side Effects**: Modifies original order object
- **Difficult Testing**: Can't test pieces independently
- **Violates SOLID**: Especially SRP and OCP

</div>
<div>

## 💡 What We Need:

- **Separation of Concerns**: Each class has one job
- **Flexible Output**: Support multiple report formats
- **No Side Effects**: Don't modify input data
- **Easy Testing**: Test each component separately
- **Open for Extension**: Add new features without changing existing code

</div>
</div>

<br>

### The Question:
**"How do we add a new CSV format without changing existing code?"**

<br>

This is where the **Strategy Pattern** comes to the rescue! 🎯

<!--
The key insight here is that we need to separate WHAT we're doing (business logic) from HOW we're presenting the results (formatting/output).

Discussion: In your projects, where do you see this kind of mixed responsibility? What would happen if you needed to add a new output format?
-->

---
layout: center
class: text-center
---

# Strategy Pattern
## "Define a family of algorithms, encapsulate each one, and make them interchangeable"

<div class="text-6xl mt-8">
🎯
</div>

---

# Strategy Pattern: Core Concept

The Strategy pattern lets you **change algorithms at runtime** without modifying the client code.

<br>

## Key Components:

```csharp {1-6|8-12|14-18}
// 1. Strategy Interface - defines the contract
public interface ICsvReportingStrategy
{
    string GenerateCsvContent(Order order, OrderCalculationResults results);
    string GetFileName(Order order);
}

// 2. Concrete Strategies - different implementations
public class StandardRetailAuditStrategy : ICsvReportingStrategy { }
public class EnhancedRetailAuditStrategy : ICsvReportingStrategy { }
public class FinancialSummaryStrategy : ICsvReportingStrategy { }

// 3. Context - uses the strategy
public class ImprovedOrderProcessor
{
    private readonly ICsvReportingStrategy _strategy;
    // Uses _strategy to generate reports
}
```

<!--
The beauty of Strategy pattern is in its simplicity:
1. Define what all algorithms should do (interface)
2. Create different implementations (concrete strategies)
3. Use them interchangeably in your context

This follows the Open/Closed Principle - open for extension (new strategies), closed for modification (existing code doesn't change).
-->

---

# Strategy Pattern Example: CSV Strategies

<div grid="~ cols-2 gap-6">
<div>

## Standard Format
```csharp
public class StandardRetailAuditStrategy 
  : ICsvReportingStrategy
{
    public string GenerateCsvContent(Order order, 
        OrderCalculationResults results)
    {
        var csv = new StringBuilder();
        csv.AppendLine("OrderID,ProductName,Price");
        
        foreach (var product in results.Products)
        {
            csv.AppendLine($"{order.Id}," +
                          $"{product.Name}," +
                          $"{product.Price:F2}");
        }
        return csv.ToString();
    }
}
```

</div>
<div>

## Enhanced Format
```csharp
public class EnhancedRetailAuditStrategy 
  : ICsvReportingStrategy
{
    public string GenerateCsvContent(Order order, 
        OrderCalculationResults results)
    {
        var csv = new StringBuilder();
        csv.AppendLine("OrderID,ProductName,Price," +
                      "VAT,Compliance,Tracking");
        
        foreach (var product in results.Products)
        {
            csv.AppendLine($"{order.Id}," +
                          $"{product.Name}," +
                          $"{product.Price:F2}," +
                          $"{product.VAT:F2}," +
                          $"{GetCompliance(product)}," +
                          $"{order.TrackingNumber}");
        }
        return csv.ToString();
    }
}
```

</div>
</div>

<!--
Notice how both strategies implement the same interface but produce completely different outputs. The client code doesn't need to know which strategy is being used - it just calls GenerateCsvContent().

This makes it easy to:
- Add new formats (Financial summary, XML, JSON, etc.)
- Switch formats based on customer requirements
- Test each format independently
-->

---

# Using the Strategy Pattern

```csharp {1-8|10-18|20-25}
// Clean processor that uses strategies
public class ImprovedOrderProcessor
{
    private readonly ICsvReportingStrategy _strategy;
    
    public ImprovedOrderProcessor(ICsvReportingStrategy strategy)
    {
        _strategy = strategy;
    }
    
    public OrderProcessingResult ProcessOrder(Order order)
    {
        // 1. Calculate business logic (separated concern)
        var results = _calculator.CalculateOrder(order);
        
        // 2. Generate report using strategy (flexible)
        string csvContent = _strategy.GenerateCsvContent(order, results);
        string fileName = _strategy.GetFileName(order);
        
        // 3. Save file (separated I/O concern)
        _fileWriter.WriteFile(fileName, csvContent);
        
        return new OrderProcessingResult 
        { 
            Success = true, CsvFilePath = fileName 
        };
    }
}
```

<!--
Now our processor is much cleaner:
- Single responsibility: process orders
- No knowledge of CSV formats
- Easy to test with mock strategies
- No side effects on the original order

The strategy is injected, following Dependency Inversion Principle.

Discussion: Where in your current projects could you apply Strategy pattern? Think about places where you have if/else chains or switch statements.
-->

---

# When to Use Strategy Pattern

## ✅ **Perfect For:**
- Multiple ways to perform the same task
- Runtime algorithm selection
- Eliminating complex if/else chains
- Supporting different data formats (CSV, JSON, XML)
- Payment processing (credit card, PayPal, bank transfer)
- Sorting algorithms (quick sort, merge sort, bubble sort)

## 🤔 **Consider When:**
- You have 3+ similar algorithms
- Client code needs to choose behavior
- You want to add new algorithms without changing existing code

## ❌ **Avoid When:**
- Only one or two simple algorithms
- Algorithms never change
- The overhead isn't worth the flexibility

<!--
The key question is: "Do I need to switch between different ways of doing the same thing?"

If you find yourself writing code like:
if (format == "standard") { ... }
else if (format == "enhanced") { ... }
else if (format == "financial") { ... }

That's a perfect candidate for Strategy pattern!

Discussion: Can you think of examples in your code where you have similar if/else chains?
-->

---
layout: center
class: text-center
---

# Mediator Pattern
## "Define how a set of objects interact with each other"

<div class="text-6xl mt-8">
🎬
</div>

---

# The Problem: Complex Object Interactions

Imagine our order processing needs additional features:

<div grid="~ cols-2 gap-8" class="mt-8">
<div>

## Without Mediator:
- Order processor calls validator
- Order processor calls notification service  
- Order processor calls audit logger
- Order processor calls payment service
- Validator might call notification service
- **Tight coupling everywhere!** 😵

```csharp
// Processor knows about everything!
public class OrderProcessor
{
    private IValidator _validator;
    private INotifier _notifier;
    private IAuditor _auditor;
    private IPayment _payment;
    
    // Complex orchestration logic...
}
```

</div>
<div>

## With Mediator:
- All components talk through mediator
- Mediator orchestrates the workflow
- Components don't know about each other
- **Loose coupling!** 😎

```csharp
// Processor only knows about mediator
public class OrderProcessor
{
    private IOrderMediator _mediator;
    
    public async Task ProcessAsync(ProcessOrderRequest request)
    {
        return await _mediator.HandleAsync(request);
    }
}
```

</div>
</div>

<!--
Think of the mediator like an air traffic control tower:
- Planes (objects) don't talk directly to each other
- Tower (mediator) coordinates all communication
- Adding a new plane doesn't require changing others
- Tower handles complex routing and timing

This is especially powerful for complex workflows with multiple steps and cross-cutting concerns.
-->

---

# Mediator Pattern: Core Concept

The Mediator pattern centralizes complex communications and control logic between related objects.

```csharp {1-6|8-12|14-22|24-30}
// 1. Mediator Interface
public interface IOrderProcessingMediator
{
    Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request);
    Task<OrderProcessingResult> ProcessOrderAsync(ProcessOrderRequest request);
}

// 2. Request/Response objects
public class ProcessOrderRequest : IRequest<OrderProcessingResult>
{
    public Order Order { get; set; }
    public string StrategyName { get; set; }
}

// 3. Request Handlers (individual concerns)
public class OrderValidationHandler : IRequestHandler<ValidateOrderRequest, ValidationResult>
{
    public async Task<ValidationResult> HandleAsync(ValidateOrderRequest request)
    {
        // Pure validation logic
        return new ValidationResult { IsValid = true };
    }
}

// 4. Mediator orchestrates everything
public class OrderProcessingMediator : IOrderProcessingMediator
{
    public async Task<OrderProcessingResult> ProcessOrderAsync(ProcessOrderRequest request)
    {
        // 1. Validate → 2. Process → 3. Log → 4. Notify
        // Centralized workflow management
    }
}
```

<!--
The Mediator pattern uses a request/response approach:
1. Client sends a request to the mediator
2. Mediator finds the right handler
3. Handler processes the request
4. Mediator coordinates multiple handlers if needed

This creates a clean separation where each handler has a single responsibility, and the mediator manages the complex workflow.
-->

---

# Mediator Pattern in Action

```csharp {1-10|12-18|20-26|28-32}
public async Task<OrderProcessingResult> ProcessOrderAsync(ProcessOrderRequest request)
{
    try
    {
        // Step 1: Validate the order
        var validationResult = await SendAsync<OrderValidationResult>(
            new ValidateOrderRequest { Order = request.Order });
            
        if (!validationResult.IsValid)
            return new OrderProcessingResult { Success = false, /* ... */ };

        // Step 2: Process with selected strategy (Strategy + Mediator!)
        var strategy = GetStrategy(request.StrategyName);
        var processor = new ImprovedOrderProcessor(strategy, calculator, fileWriter);
        var processingResult = processor.ProcessOrder(request.Order);

        // Step 3: Log audit trail
        await SendAsync<AuditLogResult>(new LogAuditRequest 
        { 
            Order = request.Order, 
            ProcessingResult = processingResult,
            Action = "ORDER_PROCESSED" 
        });

        // Step 4: Send notifications
        await SendAsync<NotificationResult>(new SendNotificationRequest 
        { 
            Order = request.Order, 
            NotificationType = "ORDER_PROCESSED" 
        });

        return processingResult;
    }
    catch (Exception ex) { /* Centralized error handling */ }
}
```

<!--
Notice how the mediator orchestrates a complex workflow:
1. Validation (business rule)
2. Processing with Strategy pattern (core logic)
3. Audit logging (compliance requirement)
4. Notifications (user experience)

Each step is handled by a specialized component, but the mediator coordinates the entire flow. This makes it easy to:
- Add new steps (e.g., payment processing)
- Change the workflow order
- Handle errors consistently
- Test each step independently
-->

---

# Individual Request Handlers

<div grid="~ cols-2 gap-6">
<div>

## Validation Handler
```csharp
public class OrderValidationHandler 
  : IRequestHandler<ValidateOrderRequest, 
                    OrderValidationResult>
{
    public async Task<OrderValidationResult> 
        HandleAsync(ValidateOrderRequest request)
    {
        var result = new OrderValidationResult();
        var order = request.Order;
        
        if (string.IsNullOrEmpty(order.OrderId))
            result.ValidationErrors.Add(
                "Order ID is required");
                
        if (order.Products.Count == 0)
            result.ValidationErrors.Add(
                "Order must have products");
                
        result.IsValid = !result.ValidationErrors.Any();
        return result;
    }
}
```

</div>
<div>

## Notification Handler
```csharp
public class NotificationHandler 
  : IRequestHandler<SendNotificationRequest, 
                    NotificationResult>
{
    public async Task<NotificationResult> 
        HandleAsync(SendNotificationRequest request)
    {
        var recipients = new List<string>();
        
        if (!string.IsNullOrEmpty(request.Order.CustomerEmail))
            recipients.Add(request.Order.CustomerEmail);
            
        recipients.Add("admin@company.com");
        
        // Send notifications (email, SMS, etc.)
        await SendEmailNotifications(recipients, request);
        
        return new NotificationResult 
        { 
            Success = true,
            Recipients = recipients 
        };
    }
}
```

</div>
</div>

<!--
Each handler is focused on one specific concern:
- Validation handler only validates
- Notification handler only sends notifications
- Audit handler only logs events

This makes them:
- Easy to test in isolation
- Easy to modify without affecting others
- Easy to reuse in different workflows
- Easy to understand (single responsibility)

Discussion: What cross-cutting concerns do you handle in your applications? (logging, validation, notifications, caching, etc.)
-->

---

# When to Use Mediator Pattern

## ✅ **Perfect For:**
- Complex workflows with multiple steps
- Cross-cutting concerns (logging, validation, notifications)
- Reducing coupling between many objects
- Request/response processing pipelines
- Command handling systems (CQRS)

## 🤔 **Consider When:**
- You have 3+ objects that interact frequently
- Adding new behavior requires changing multiple classes
- You need centralized workflow control
- You want consistent error handling across operations

## ❌ **Avoid When:**
- Simple, direct object interactions
- Only 2 objects communicating
- Performance is critical (adds indirection)
- The mediator becomes too complex (god object)

<!--
The key question is: "Do I have complex interactions between multiple objects that are hard to manage?"

Signs you might need Mediator:
- Objects calling each other in complex patterns
- Hard to add new features without touching many classes
- Inconsistent error handling or logging
- Difficult to test complex workflows

Discussion: Where in your applications do you have complex object interactions that might benefit from a mediator?
-->

---

# Combining Strategy + Mediator

Our order processing system uses **both patterns together**:

```csharp {1-8|10-16|18-24}
// Mediator orchestrates the workflow
public async Task<OrderProcessingResult> ProcessOrderAsync(ProcessOrderRequest request)
{
    // 1. Validate order
    var validation = await SendAsync<OrderValidationResult>(/* ... */);
    
    // 2. Use Strategy pattern for CSV generation
    var strategy = GetStrategy(request.StrategyName); // Strategy selection
    var processor = new ImprovedOrderProcessor(strategy, calculator, fileWriter);
    var result = processor.ProcessOrder(request.Order);
    
    // 3. Cross-cutting concerns via Mediator
    await SendAsync<AuditLogResult>(/* audit logging */);
    await SendAsync<NotificationResult>(/* notifications */);
    
    return result;
}

// Client code is simple and clean
var request = new ProcessOrderRequest 
{ 
    Order = complexOrder, 
    StrategyName = "Enhanced"  // Strategy selection
};
var result = await mediator.ProcessOrderAsync(request);
```

<!--
This combination is powerful:
- Strategy pattern handles algorithm selection (CSV formats)
- Mediator pattern handles workflow orchestration
- Clean separation of concerns
- Easy to extend both patterns independently

You can add new CSV strategies without touching the mediator, and add new workflow steps without touching the strategies.

This is a real-world example of how patterns work together to solve complex problems.
-->

---

# Benefits: Before vs After

<div grid="~ cols-2 gap-8" class="mt-4">
<div>

## 🚫 Before (Legacy)
```csharp
public void ProcessOrder(Order order)
{
    // 700+ line method
    
    // Validation mixed with logic
    if (order.Id == null) order.Id = GenerateId();
    
    // Business calculations
    foreach(var product in order.Products) {
        // Hundreds of lines...
    }
    
    // Hard-coded CSV format
    if (format == "standard") {
        csv = "OrderID,Product,Price\n";
    } else if (format == "enhanced") {
        csv = "OrderID,Product,Price,VAT\n";
    }
    
    // File I/O mixed in
    File.WriteAllText(path, csv);
    
    // Logging mixed in
    LogAudit(order);
    
    // Notifications mixed in  
    SendEmail(order.CustomerEmail);
}
```

**Problems:** Mixed responsibilities, hard-coded logic, difficult to test, violates SOLID principles

</div>
<div>

## ✅ After (Patterns)
```csharp
// Clean mediator orchestration
public async Task<Result> ProcessOrderAsync(
    ProcessOrderRequest request)
{
    var validation = await SendAsync<ValidationResult>(
        new ValidateOrderRequest { Order = request.Order });
        
    var strategy = GetStrategy(request.StrategyName);
    var processor = new ImprovedOrderProcessor(
        strategy, calculator, fileWriter);
    var result = processor.ProcessOrder(request.Order);
    
    await SendAsync<AuditLogResult>(
        new LogAuditRequest { /* ... */ });
    await SendAsync<NotificationResult>(
        new SendNotificationRequest { /* ... */ });
        
    return result;
}

// Usage
var result = await mediator.ProcessOrderAsync(
    new ProcessOrderRequest 
    { 
        Order = order, 
        StrategyName = "Enhanced" 
    });
```

**Benefits:** Separated concerns, flexible algorithms, easy testing, follows SOLID principles

</div>
</div>

<!--
The transformation is dramatic:
- From 700+ line method to focused, single-purpose components
- From hard-coded logic to flexible, configurable behavior
- From difficult testing to easy unit testing
- From violating SOLID to exemplifying good design

This is the power of design patterns - they help us write code that's maintainable, testable, and extensible.
-->

---

# Key Takeaways

<div class="text-lg mt-8">

## 🎯 **Strategy Pattern - "When to Use"**
- Multiple ways to do the same thing
- Need to switch algorithms at runtime  
- Want to eliminate if/else chains
- **Ask yourself:** *"Do I need different ways to accomplish the same goal?"*

## 🎬 **Mediator Pattern - "When to Use"**
- Complex interactions between multiple objects
- Cross-cutting concerns (logging, validation, notifications)
- Need centralized workflow control
- **Ask yourself:** *"Are my objects too tightly coupled with complex interactions?"*

## 🚀 **Both Patterns Help You:**
- Follow SOLID principles (especially SRP and OCP)
- Write more testable code
- Make code easier to extend and maintain
- Separate concerns properly

</div>

<!--
Remember, patterns are tools to solve specific problems. Don't use them just because you can - use them when they solve a real problem you're facing.

The key is recognizing the problems these patterns solve:
- Strategy: Complex conditional logic, need for algorithm flexibility
- Mediator: Complex object interactions, tight coupling

When you see these problems in your code, that's when these patterns become valuable.
-->

---

# Discussion & Questions

<div class="text-xl mt-12">

## 🤔 **Think About Your Current Projects:**

1. **Where do you have complex if/else chains that could benefit from Strategy pattern?**

2. **Where do you have tight coupling between objects that Mediator could help with?**

3. **What cross-cutting concerns (logging, validation, etc.) do you handle repeatedly?**

4. **How could these patterns help make your code more testable?**

<br>

## 💡 **Remember:**
- Patterns solve problems - don't use them just because you can
- Start simple, refactor to patterns when you need them
- Focus on **when to use** rather than **how to implement**
- The internet has great implementation examples when you need them!

</div>

<div class="abs-br m-6">
  <span class="text-sm opacity-50">Questions?</span>
</div>

<!--
This is the most important part - applying what we've learned to real problems.

Encourage discussion about:
- Specific examples from their projects
- Challenges they're facing that these patterns might help with
- Questions about when NOT to use these patterns

The goal is for them to recognize these problems in their own code and know that these patterns exist as solutions.
-->

---
layout: center
class: text-center
---

# Thank You!

<div class="text-6xl mt-8 mb-8">
🎯 🎬
</div>

## Strategy & Mediator Patterns
### Building Better Software Through Smart Object Interactions

<div class="mt-12 text-lg opacity-75">
Remember: Patterns are tools to solve problems.<br>
Use them when they help, not just because you can! 
</div>

<!--
Thank you for your attention! Remember that design patterns are practical tools - they should make your code better, not more complex.

The key is recognizing when you have the problems these patterns solve, and then applying them appropriately.

Any final questions?
-->