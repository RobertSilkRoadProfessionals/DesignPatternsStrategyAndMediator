using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DesignPatternsDemo.Models;
using DesignPatternsDemo.Patterns.Strategy;

namespace DesignPatternsDemo.Patterns.Mediator
{
    /// <summary>
    /// Concrete implementation of the Order Processing Mediator
    /// 
    /// The Mediator pattern provides several benefits over the Strategy pattern alone:
    /// 1. Centralized Control: All order processing logic flows through one place
    /// 2. Loose Coupling: Components don't need to know about each other directly
    /// 3. Extensibility: Easy to add new processing steps without changing existing code
    /// 4. Orchestration: Coordinates complex workflows involving multiple components
    /// 5. Cross-cutting Concerns: Handles validation, logging, notifications uniformly
    /// </summary>
    public class OrderProcessingMediator : IOrderProcessingMediator
    {
        private readonly ConcurrentDictionary<Type, object> _handlers;
        private readonly Dictionary<string, ICsvReportingStrategy> _strategies;
        private readonly IOrderCalculator _calculator;

        public OrderProcessingMediator(IOrderCalculator calculator)
        {
            _handlers = new ConcurrentDictionary<Type, object>();
            _strategies = new Dictionary<string, ICsvReportingStrategy>();
            _calculator = calculator ?? throw new ArgumentNullException(nameof(calculator));

            // Register default strategies
            RegisterStrategy("Standard", new StandardRetailAuditStrategy());
            RegisterStrategy("Enhanced", new EnhancedRetailAuditStrategy());
            RegisterStrategy("Financial", new FinancialSummaryStrategy());

            // Register default handlers
            RegisterHandler<ValidateOrderRequest, OrderValidationResult>(new OrderValidationHandler());
            RegisterHandler<SendNotificationRequest, NotificationResult>(new NotificationHandler());
            RegisterHandler<LogAuditRequest, AuditLogResult>(new AuditLogHandler());
        }

        public async Task<OrderProcessingResult> ProcessOrderAsync(ProcessOrderRequest request)
        {
            if (request?.Order == null)
                throw new ArgumentNullException(nameof(request));

            try
            {
                Console.WriteLine($"=== MEDIATOR-COORDINATED ORDER PROCESSING ===");
                Console.WriteLine($"Processing order: {request.Order.OrderId}");
                Console.WriteLine($"Strategy: {request.StrategyName}");

                // Step 1: Validate the order if requested
                if (request.ValidateOrder)
                {
                    var validationResult = await SendAsync<OrderValidationResult>(
                        new ValidateOrderRequest { Order = request.Order });

                    if (!validationResult.IsValid)
                    {
                        return new OrderProcessingResult
                        {
                            Order = request.Order,
                            Success = false,
                            ErrorMessage = $"Order validation failed: {string.Join(", ", validationResult.ValidationErrors)}",
                            ProcessedAt = DateTime.Now
                        };
                    }

                    if (validationResult.Warnings.Any())
                    {
                        Console.WriteLine($"Validation warnings: {string.Join(", ", validationResult.Warnings)}");
                    }
                }

                // Step 2: Get the appropriate CSV strategy
                if (!_strategies.TryGetValue(request.StrategyName, out var strategy))
                {
                    strategy = _strategies.Values.First(); // Default to first available strategy
                    Console.WriteLine($"Strategy '{request.StrategyName}' not found, using default: {strategy.Description}");
                }

                // Step 3: Create the order processor with the selected strategy
                var fileWriter = new LocalFileWriter(request.OutputPath);
                var processor = new ImprovedOrderProcessor(strategy, _calculator, fileWriter);

                // Step 4: Process the order
                var processingResult = processor.ProcessOrder(request.Order);

                // Step 5: Log the audit trail if requested
                if (request.LogAuditTrail)
                {
                    var auditResult = await SendAsync<AuditLogResult>(
                        new LogAuditRequest
                        {
                            Order = request.Order,
                            ProcessingResult = processingResult,
                            Action = "ORDER_PROCESSED",
                            AdditionalData = new Dictionary<string, object>
                            {
                                { "Strategy", strategy.Description },
                                { "OutputPath", request.OutputPath },
                                { "ProcessingTime", DateTime.Now }
                            }
                        });

                    Console.WriteLine($"Audit logged: {auditResult.LogEntryId}");
                }

                // Step 6: Send notifications if requested and processing was successful
                if (request.SendNotifications && processingResult.Success)
                {
                    var notificationResult = await SendAsync<NotificationResult>(
                        new SendNotificationRequest
                        {
                            Order = request.Order,
                            ProcessingResult = processingResult,
                            NotificationType = "ORDER_PROCESSED"
                        });

                    Console.WriteLine($"Notification sent: {notificationResult.Message}");
                }

                Console.WriteLine("=== MEDIATOR PROCESSING COMPLETE ===");
                return processingResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in mediator processing: {ex.Message}");
                
                // Even in case of error, try to log it
                if (request.LogAuditTrail)
                {
                    try
                    {
                        await SendAsync<AuditLogResult>(
                            new LogAuditRequest
                            {
                                Order = request.Order,
                                ProcessingResult = new OrderProcessingResult { Success = false, ErrorMessage = ex.Message },
                                Action = "ORDER_PROCESSING_ERROR",
                                AdditionalData = new Dictionary<string, object> { { "Error", ex.Message } }
                            });
                    }
                    catch
                    {
                        // If logging fails, we don't want to throw another exception
                    }
                }

                return new OrderProcessingResult
                {
                    Order = request.Order,
                    Success = false,
                    ErrorMessage = ex.Message,
                    ProcessedAt = DateTime.Now
                };
            }
        }

        public IEnumerable<ICsvReportingStrategy> GetAvailableStrategies()
        {
            return _strategies.Values;
        }

        public void RegisterHandler<TRequest, TResponse>(IRequestHandler<TRequest, TResponse> handler)
            where TRequest : class
            where TResponse : class
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            _handlers.TryAdd(typeof(TRequest), handler);
        }

        public async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request) where TResponse : class
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var requestType = request.GetType();
            if (!_handlers.TryGetValue(requestType, out var handlerObj))
            {
                throw new InvalidOperationException($"No handler registered for request type {requestType.Name}");
            }

            // Use dynamic dispatch to handle the specific request type
            try
            {
                dynamic handler = handlerObj;
                dynamic dynamicRequest = request;
                var result = await handler.HandleAsync(dynamicRequest);
                return (TResponse)result;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error invoking handler for {requestType.Name}: {ex.Message}", ex);
            }
        }

        public void RegisterStrategy(string name, ICsvReportingStrategy strategy)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Strategy name cannot be null or empty", nameof(name));
            if (strategy == null)
                throw new ArgumentNullException(nameof(strategy));

            _strategies[name] = strategy;
        }

        public ICsvReportingStrategy? GetStrategy(string name)
        {
            return _strategies.TryGetValue(name, out var strategy) ? strategy : null;
        }
    }
}