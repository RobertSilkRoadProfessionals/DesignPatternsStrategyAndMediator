using System;
using System.Collections.Generic;

namespace DesignPatternsDemo.Models
{
    /// <summary>
    /// Represents a basic product in the system
    /// </summary>
    public class Product
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal RRP { get; set; } // Recommended Retail Price
        public decimal VATRate { get; set; } // VAT percentage (e.g., 0.20 for 20%)
        public int Quantity { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Supplier { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }

    /// <summary>
    /// Represents a product ensemble - a curated set of products sold together
    /// </summary>
    public class ProductEnsemble
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public List<Product> Products { get; set; } = new List<Product>();
        public decimal EnsembleDiscount { get; set; } // Discount applied to the ensemble
        public string Theme { get; set; } = string.Empty; // e.g., "Summer Collection", "Wedding Set"
        public int Quantity { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    /// <summary>
    /// Represents a product kit - a new type of product grouping with different rules
    /// </summary>
    public class ProductKit
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public List<Product> MandatoryProducts { get; set; } = new List<Product>();
        public List<Product> OptionalProducts { get; set; } = new List<Product>();
        public decimal KitPrice { get; set; } // Fixed kit price regardless of individual product prices
        public string KitType { get; set; } = string.Empty; // e.g., "Starter", "Professional", "Premium"
        public int Quantity { get; set; }
        public bool IsCustomizable { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    /// <summary>
    /// Represents a discount code with various rules and conditions
    /// </summary>
    public class DiscountCode
    {
        public string Code { get; set; } = string.Empty;
        public decimal DiscountPercentage { get; set; }
        public decimal MaxDiscountAmount { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public int MaxUsages { get; set; }
        public int CurrentUsages { get; set; }
        public List<string> ApplicableCategories { get; set; } = new List<string>();
        public decimal MinOrderAmount { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Represents payment method information
    /// </summary>
    public class PaymentMethod
    {
        public string Type { get; set; } = string.Empty; // Credit Card, Debit Card, PayPal, etc.
        public string Provider { get; set; } = string.Empty; // Visa, MasterCard, PayPal, etc.
        public string LastFourDigits { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public decimal ProcessingFee { get; set; }
        public bool IsVerified { get; set; }
    }

    /// <summary>
    /// Complex Order object containing all types of products and order details
    /// This represents the complex domain that our legacy code will struggle to handle properly
    /// </summary>
    public class Order
    {
        public string OrderId { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;

        // Different types of products in the order
        public List<Product> IndividualProducts { get; set; } = new List<Product>();
        public List<ProductEnsemble> ProductEnsembles { get; set; } = new List<ProductEnsemble>();
        public List<ProductKit> ProductKits { get; set; } = new List<ProductKit>();

        // Order financial details
        public List<DiscountCode> AppliedDiscountCodes { get; set; } = new List<DiscountCode>();
        public PaymentMethod PaymentMethod { get; set; } = new PaymentMethod();
        public decimal ActualPricePaid { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal TotalVAT { get; set; }
        public decimal OrderTotal { get; set; }

        // Order processing status
        public string Status { get; set; } = string.Empty; // Pending, Processing, Shipped, Delivered, Cancelled
        public string ShippingAddress { get; set; } = string.Empty;
        public string BillingAddress { get; set; } = string.Empty;
        public DateTime? ShippedDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public string TrackingNumber { get; set; } = string.Empty;

        // Additional properties that make the object even more complex
        public string PromotionalCampaign { get; set; } = string.Empty;
        public bool IsGiftOrder { get; set; }
        public string GiftMessage { get; set; } = string.Empty;
        public bool RequiresSignature { get; set; }
        public string OrderNotes { get; set; } = string.Empty;
    }
}