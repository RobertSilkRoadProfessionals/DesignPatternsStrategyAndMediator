using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DesignPatternsDemo.Models;
using DesignPatternsDemo.Patterns.Strategy;

namespace DesignPatternsDemo.Patterns.Mediator
{
    /// <summary>
    /// Mediator interface for handling order processing requests
    /// The Mediator pattern helps reduce coupling between components by centralizing
    /// complex communications and control logic between related objects
    /// </summary>
    public interface IOrderProcessingMediator
    {
        /// <summary>
        /// Process an order with the specified reporting strategy
        /// </summary>
        Task<OrderProcessingResult> ProcessOrderAsync(ProcessOrderRequest request);

        /// <summary>
        /// Get available CSV reporting strategies
        /// </summary>
        IEnumerable<ICsvReportingStrategy> GetAvailableStrategies();

        /// <summary>
        /// Register a new processing event handler
        /// </summary>
        void RegisterHandler<TRequest, TResponse>(IRequestHandler<TRequest, TResponse> handler)
            where TRequest : class
            where TResponse : class;

        /// <summary>
        /// Send a request through the mediator
        /// </summary>
        Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request) where TResponse : class;
    }

    /// <summary>
    /// Request to process an order
    /// </summary>
    public class ProcessOrderRequest : IRequest<OrderProcessingResult>
    {
        public Order Order { get; set; } = new Order();
        public string StrategyName { get; set; } = string.Empty;
        public string OutputPath { get; set; } = string.Empty;
        public bool ValidateOrder { get; set; } = true;
        public bool SendNotifications { get; set; } = true;
        public bool LogAuditTrail { get; set; } = true;
    }

    /// <summary>
    /// Base interface for requests
    /// </summary>
    public interface IRequest<TResponse> where TResponse : class
    {
    }

    /// <summary>
    /// Interface for request handlers
    /// </summary>
    public interface IRequestHandler<in TRequest, TResponse>
        where TRequest : class
        where TResponse : class
    {
        Task<TResponse> HandleAsync(TRequest request);
    }

    /// <summary>
    /// Validation request for orders
    /// </summary>
    public class ValidateOrderRequest : IRequest<OrderValidationResult>
    {
        public Order Order { get; set; } = new Order();
    }

    /// <summary>
    /// Order validation result
    /// </summary>
    public class OrderValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> ValidationErrors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
    }

    /// <summary>
    /// Notification request for order processing events
    /// </summary>
    public class SendNotificationRequest : IRequest<NotificationResult>
    {
        public Order Order { get; set; } = new Order();
        public OrderProcessingResult ProcessingResult { get; set; } = new OrderProcessingResult();
        public string NotificationType { get; set; } = string.Empty;
    }

    /// <summary>
    /// Notification result
    /// </summary>
    public class NotificationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> Recipients { get; set; } = new List<string>();
    }

    /// <summary>
    /// Audit logging request
    /// </summary>
    public class LogAuditRequest : IRequest<AuditLogResult>
    {
        public Order Order { get; set; } = new Order();
        public OrderProcessingResult ProcessingResult { get; set; } = new OrderProcessingResult();
        public string Action { get; set; } = string.Empty;
        public Dictionary<string, object> AdditionalData { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Audit log result
    /// </summary>
    public class AuditLogResult
    {
        public bool Success { get; set; }
        public string LogEntryId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}