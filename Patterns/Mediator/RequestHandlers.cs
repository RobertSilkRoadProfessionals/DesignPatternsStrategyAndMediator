using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DesignPatternsDemo.Models;
using DesignPatternsDemo.Patterns.Strategy;

namespace DesignPatternsDemo.Patterns.Mediator
{
    /// <summary>
    /// Handler for order validation requests
    /// Demonstrates how the Mediator pattern allows us to separate validation logic
    /// into its own focused component
    /// </summary>
    public class OrderValidationHandler : IRequestHandler<ValidateOrderRequest, OrderValidationResult>
    {
        public Task<OrderValidationResult> HandleAsync(ValidateOrderRequest request)
        {
            var result = new OrderValidationResult();
            var order = request.Order;

            // Basic order validation
            if (string.IsNullOrEmpty(order.OrderId))
            {
                result.ValidationErrors.Add("Order ID is required");
            }

            if (string.IsNullOrEmpty(order.CustomerId))
            {
                result.ValidationErrors.Add("Customer ID is required");
            }

            if (order.OrderDate == default(DateTime))
            {
                result.ValidationErrors.Add("Order date is required");
            }

            if (order.OrderDate > DateTime.Now)
            {
                result.ValidationErrors.Add("Order date cannot be in the future");
            }

            // Product validation
            if (!order.IndividualProducts.Any() && !order.ProductEnsembles.Any() && !order.ProductKits.Any())
            {
                result.ValidationErrors.Add("Order must contain at least one product, ensemble, or kit");
            }

            // Validate individual products
            foreach (var product in order.IndividualProducts)
            {
                ValidateProduct(product, "Individual Product", result);
            }

            // Validate ensemble products
            foreach (var ensemble in order.ProductEnsembles)
            {
                if (string.IsNullOrEmpty(ensemble.Id))
                {
                    result.ValidationErrors.Add($"Ensemble ID is required");
                }

                if (ensemble.Quantity <= 0)
                {
                    result.Warnings.Add($"Ensemble {ensemble.Name} has zero or negative quantity");
                }

                if (ensemble.EnsembleDiscount < 0 || ensemble.EnsembleDiscount > 1)
                {
                    result.ValidationErrors.Add($"Ensemble {ensemble.Name} discount must be between 0 and 1");
                }

                foreach (var product in ensemble.Products)
                {
                    ValidateProduct(product, $"Ensemble Product ({ensemble.Name})", result);
                }
            }

            // Validate kit products
            foreach (var kit in order.ProductKits)
            {
                if (string.IsNullOrEmpty(kit.Id))
                {
                    result.ValidationErrors.Add($"Kit ID is required");
                }

                if (kit.Quantity <= 0)
                {
                    result.Warnings.Add($"Kit {kit.Name} has zero or negative quantity");
                }

                if (kit.KitPrice < 0)
                {
                    result.ValidationErrors.Add($"Kit {kit.Name} price cannot be negative");
                }

                if (!kit.MandatoryProducts.Any())
                {
                    result.Warnings.Add($"Kit {kit.Name} has no mandatory products");
                }

                foreach (var product in kit.MandatoryProducts)
                {
                    ValidateProduct(product, $"Kit Mandatory Product ({kit.Name})", result);
                }

                foreach (var product in kit.OptionalProducts)
                {
                    ValidateProduct(product, $"Kit Optional Product ({kit.Name})", result);
                }
            }

            // Validate payment information
            if (order.PaymentMethod == null)
            {
                result.ValidationErrors.Add("Payment method is required");
            }
            else
            {
                if (string.IsNullOrEmpty(order.PaymentMethod.Type))
                {
                    result.ValidationErrors.Add("Payment method type is required");
                }

                if (string.IsNullOrEmpty(order.PaymentMethod.TransactionId))
                {
                    result.Warnings.Add("Payment transaction ID is missing");
                }

                if (order.PaymentMethod.ProcessingFee < 0)
                {
                    result.ValidationErrors.Add("Payment processing fee cannot be negative");
                }
            }

            // Validate discount codes
            foreach (var discount in order.AppliedDiscountCodes)
            {
                if (string.IsNullOrEmpty(discount.Code))
                {
                    result.ValidationErrors.Add("Discount code cannot be empty");
                }

                if (discount.DiscountPercentage < 0 || discount.DiscountPercentage > 1)
                {
                    result.ValidationErrors.Add($"Discount code {discount.Code} percentage must be between 0 and 1");
                }

                if (discount.ValidFrom > discount.ValidTo)
                {
                    result.ValidationErrors.Add($"Discount code {discount.Code} has invalid date range");
                }

                if (discount.CurrentUsages > discount.MaxUsages)
                {
                    result.ValidationErrors.Add($"Discount code {discount.Code} has exceeded maximum usages");
                }
            }

            // Financial validation
            if (order.ActualPricePaid < 0)
            {
                result.ValidationErrors.Add("Actual price paid cannot be negative");
            }

            if (order.ShippingCost < 0)
            {
                result.ValidationErrors.Add("Shipping cost cannot be negative");
            }

            // Status validation
            var validStatuses = new[] { "Pending", "Processing", "Shipped", "Delivered", "Cancelled" };
            if (!validStatuses.Contains(order.Status))
            {
                result.Warnings.Add($"Order status '{order.Status}' is not a standard status");
            }

            result.IsValid = !result.ValidationErrors.Any();

            return Task.FromResult(result);
        }

        private void ValidateProduct(Product product, string productContext, OrderValidationResult result)
        {
            if (string.IsNullOrEmpty(product.Id))
            {
                result.ValidationErrors.Add($"{productContext}: Product ID is required");
            }

            if (string.IsNullOrEmpty(product.Name))
            {
                result.ValidationErrors.Add($"{productContext}: Product name is required");
            }

            if (product.RRP < 0)
            {
                result.ValidationErrors.Add($"{productContext} {product.Name}: RRP cannot be negative");
            }

            if (product.VATRate < 0 || product.VATRate > 1)
            {
                result.ValidationErrors.Add($"{productContext} {product.Name}: VAT rate must be between 0 and 1");
            }

            if (product.Quantity <= 0)
            {
                result.Warnings.Add($"{productContext} {product.Name}: Quantity is zero or negative");
            }

            if (string.IsNullOrEmpty(product.Category))
            {
                result.Warnings.Add($"{productContext} {product.Name}: Category is not specified");
            }

            if (string.IsNullOrEmpty(product.Supplier))
            {
                result.Warnings.Add($"{productContext} {product.Name}: Supplier is not specified");
            }
        }
    }

    /// <summary>
    /// Handler for sending notifications
    /// In a real system, this might integrate with email services, SMS, push notifications, etc.
    /// </summary>
    public class NotificationHandler : IRequestHandler<SendNotificationRequest, NotificationResult>
    {
        public Task<NotificationResult> HandleAsync(SendNotificationRequest request)
        {
            var result = new NotificationResult();

            try
            {
                // Simulate notification logic
                switch (request.NotificationType.ToUpper())
                {
                    case "ORDER_PROCESSED":
                        result = SendOrderProcessedNotification(request.Order, request.ProcessingResult);
                        break;
                    case "ORDER_FAILED":
                        result = SendOrderFailedNotification(request.Order, request.ProcessingResult);
                        break;
                    case "ORDER_SHIPPED":
                        result = SendOrderShippedNotification(request.Order);
                        break;
                    default:
                        result.Success = false;
                        result.Message = $"Unknown notification type: {request.NotificationType}";
                        break;
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Failed to send notification: {ex.Message}";
            }

            return Task.FromResult(result);
        }

        private NotificationResult SendOrderProcessedNotification(Order order, OrderProcessingResult processingResult)
        {
            // In a real system, this would integrate with actual notification services
            var recipients = new List<string>();

            if (!string.IsNullOrEmpty(order.CustomerEmail))
            {
                recipients.Add(order.CustomerEmail);
            }

            // Add system administrators or customer service
            recipients.Add("admin@company.com");
            recipients.Add("audit@company.com");

            return new NotificationResult
            {
                Success = true,
                Message = $"Order {order.OrderId} processing notification sent to {recipients.Count} recipients",
                Recipients = recipients
            };
        }

        private NotificationResult SendOrderFailedNotification(Order order, OrderProcessingResult processingResult)
        {
            var recipients = new List<string> { "admin@company.com", "support@company.com" };

            return new NotificationResult
            {
                Success = true,
                Message = $"Order {order.OrderId} failure notification sent to {recipients.Count} recipients",
                Recipients = recipients
            };
        }

        private NotificationResult SendOrderShippedNotification(Order order)
        {
            var recipients = new List<string>();

            if (!string.IsNullOrEmpty(order.CustomerEmail))
            {
                recipients.Add(order.CustomerEmail);
            }

            return new NotificationResult
            {
                Success = true,
                Message = $"Order {order.OrderId} shipping notification sent to {recipients.Count} recipients",
                Recipients = recipients
            };
        }
    }

    /// <summary>
    /// Handler for audit logging
    /// Demonstrates how cross-cutting concerns like logging can be handled cleanly with the Mediator pattern
    /// </summary>
    public class AuditLogHandler : IRequestHandler<LogAuditRequest, AuditLogResult>
    {
        public Task<AuditLogResult> HandleAsync(LogAuditRequest request)
        {
            var result = new AuditLogResult
            {
                LogEntryId = Guid.NewGuid().ToString(),
                Timestamp = DateTime.Now,
                Success = true
            };

            try
            {
                // In a real system, this would write to a database, log file, or audit service
                Console.WriteLine($"AUDIT LOG [{result.LogEntryId}] - {request.Action}");
                Console.WriteLine($"  Order ID: {request.Order.OrderId}");
                Console.WriteLine($"  Customer ID: {request.Order.CustomerId}");
                Console.WriteLine($"  Timestamp: {result.Timestamp:yyyy-MM-dd HH:mm:ss}");
                Console.WriteLine($"  Success: {request.ProcessingResult.Success}");

                if (!string.IsNullOrEmpty(request.ProcessingResult.ErrorMessage))
                {
                    Console.WriteLine($"  Error: {request.ProcessingResult.ErrorMessage}");
                }

                foreach (var data in request.AdditionalData)
                {
                    Console.WriteLine($"  {data.Key}: {data.Value}");
                }

                Console.WriteLine($"--- End Audit Log [{result.LogEntryId}] ---");
            }
            catch (Exception ex)
            {
                result.Success = false;
                Console.WriteLine($"Failed to write audit log: {ex.Message}");
            }

            return Task.FromResult(result);
        }
    }
}