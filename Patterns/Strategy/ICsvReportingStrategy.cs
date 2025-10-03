using System.Collections.Generic;
using DesignPatternsDemo.Models;

namespace DesignPatternsDemo.Patterns.Strategy
{
    /// <summary>
    /// Strategy pattern interface for different CSV reporting formats
    /// This allows us to support multiple audit systems with different CSV requirements
    /// without modifying existing code (Open/Closed Principle)
    /// </summary>
    public interface ICsvReportingStrategy
    {
        /// <summary>
        /// Generate CSV content for the given order
        /// </summary>
        /// <param name="order">The order to generate the report for</param>
        /// <param name="calculationResults">Pre-calculated financial results</param>
        /// <returns>CSV content as string</returns>
        string GenerateCsvContent(Order order, OrderCalculationResults calculationResults);

        /// <summary>
        /// Get the file name pattern for this reporting strategy
        /// </summary>
        /// <param name="order">The order being processed</param>
        /// <returns>File name for the CSV report</returns>
        string GetFileName(Order order);

        /// <summary>
        /// Get a description of this reporting strategy
        /// </summary>
        string Description { get; }
    }

    /// <summary>
    /// Data structure to hold pre-calculated order results
    /// This separates calculation logic from reporting logic
    /// </summary>
    public class OrderCalculationResults
    {
        public decimal IndividualProductsTotal { get; set; }
        public decimal IndividualProductsVAT { get; set; }
        public decimal EnsembleTotal { get; set; }
        public decimal EnsembleVAT { get; set; }
        public decimal KitTotal { get; set; }
        public decimal KitVAT { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal Subtotal { get; set; }
        public decimal TotalVAT { get; set; }
        public decimal GrandTotal { get; set; }
        public List<ProductCalculation> ProductCalculations { get; set; } = new List<ProductCalculation>();
        public List<EnsembleCalculation> EnsembleCalculations { get; set; } = new List<EnsembleCalculation>();
        public List<KitCalculation> KitCalculations { get; set; } = new List<KitCalculation>();
    }

    /// <summary>
    /// Calculation details for individual products
    /// </summary>
    public class ProductCalculation
    {
        public Product Product { get; set; } = new Product();
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal VATAmount { get; set; }
        public decimal DiscountApplied { get; set; }
    }

    /// <summary>
    /// Calculation details for product ensembles
    /// </summary>
    public class EnsembleCalculation
    {
        public ProductEnsemble Ensemble { get; set; } = new ProductEnsemble();
        public decimal BaseTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalTotal { get; set; }
        public decimal VATAmount { get; set; }
        public List<ProductCalculation> ProductCalculations { get; set; } = new List<ProductCalculation>();
    }

    /// <summary>
    /// Calculation details for product kits
    /// </summary>
    public class KitCalculation
    {
        public ProductKit Kit { get; set; } = new ProductKit();
        public decimal KitUnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal VATAmount { get; set; }
        public List<ProductCalculation> MandatoryProductCalculations { get; set; } = new List<ProductCalculation>();
        public List<ProductCalculation> OptionalProductCalculations { get; set; } = new List<ProductCalculation>();
    }
}