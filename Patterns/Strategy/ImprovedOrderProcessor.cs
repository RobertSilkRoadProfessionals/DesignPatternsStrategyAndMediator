using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DesignPatternsDemo.Models;

namespace DesignPatternsDemo.Patterns.Strategy
{
    /// <summary>
    /// Improved order processor using the Strategy pattern for CSV reporting
    /// 
    /// This refactored version demonstrates several improvements over the legacy code:
    /// 1. Separation of Concerns: Business logic, calculation, and reporting are separated
    /// 2. Single Responsibility: Each class has one clear purpose
    /// 3. Open/Closed Principle: Can add new CSV formats without modifying existing code
    /// 4. Strategy Pattern: Flexible CSV reporting strategies
    /// 5. No Side Effects: Does not modify the original order object
    /// 6. Better Error Handling: Specific exception types and meaningful messages
    /// 7. Immutable Results: Returns calculation results without modifying input
    /// </summary>
    public class ImprovedOrderProcessor
    {
        private readonly ICsvReportingStrategy _csvStrategy;
        private readonly IOrderCalculator _calculator;
        private readonly IFileWriter _fileWriter;

        public ImprovedOrderProcessor(
            ICsvReportingStrategy csvStrategy, 
            IOrderCalculator calculator,
            IFileWriter fileWriter)
        {
            _csvStrategy = csvStrategy ?? throw new ArgumentNullException(nameof(csvStrategy));
            _calculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
            _fileWriter = fileWriter ?? throw new ArgumentNullException(nameof(fileWriter));
        }

        /// <summary>
        /// Process an order and generate a CSV report using the configured strategy
        /// This method is much cleaner and follows single responsibility principle
        /// </summary>
        public OrderProcessingResult ProcessOrder(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            try
            {
                Console.WriteLine($"=== IMPROVED ORDER PROCESSOR ({_csvStrategy.Description}) ===");
                Console.WriteLine($"Processing order: {order.OrderId}");

                // Step 1: Calculate order totals (separated concern)
                var calculationResults = _calculator.CalculateOrder(order);

                // Step 2: Generate CSV content using the strategy (flexible reporting)
                string csvContent = _csvStrategy.GenerateCsvContent(order, calculationResults);
                string fileName = _csvStrategy.GetFileName(order);

                // Step 3: Save the CSV file (separated I/O concern)
                string fullPath = _fileWriter.WriteFile(fileName, csvContent);

                // Step 4: Return comprehensive results (no side effects)
                var result = new OrderProcessingResult
                {
                    Order = order,
                    CalculationResults = calculationResults,
                    CsvFilePath = fullPath,
                    ProcessedAt = DateTime.Now,
                    StrategyUsed = _csvStrategy.Description,
                    Success = true
                };

                Console.WriteLine($"CSV report saved to: {fullPath}");
                Console.WriteLine($"Strategy used: {_csvStrategy.Description}");
                Console.WriteLine($"Order Total: {calculationResults.GrandTotal:C}");
                Console.WriteLine($"VAT Amount: {calculationResults.TotalVAT:C}");
                Console.WriteLine("=== PROCESSING COMPLETE ===");

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing order {order?.OrderId}: {ex.Message}");
                
                return new OrderProcessingResult
                {
                    Order = order,
                    Success = false,
                    ErrorMessage = ex.Message,
                    ProcessedAt = DateTime.Now,
                    StrategyUsed = _csvStrategy.Description
                };
            }
        }
    }

    /// <summary>
    /// Interface for order calculation logic - allows for different calculation strategies if needed
    /// </summary>
    public interface IOrderCalculator
    {
        OrderCalculationResults CalculateOrder(Order order);
    }

    /// <summary>
    /// Interface for file writing - allows for different storage strategies (local, cloud, etc.)
    /// </summary>
    public interface IFileWriter
    {
        string WriteFile(string fileName, string content);
    }

    /// <summary>
    /// Standard implementation of order calculator
    /// Contains the business logic for calculating order totals
    /// </summary>
    public class StandardOrderCalculator : IOrderCalculator
    {
        public OrderCalculationResults CalculateOrder(Order order)
        {
            var results = new OrderCalculationResults();

            // Calculate individual products
            foreach (var product in order.IndividualProducts)
            {
                var calculation = CalculateProduct(product);
                results.ProductCalculations.Add(calculation);
                results.IndividualProductsTotal += calculation.TotalPrice;
                results.IndividualProductsVAT += calculation.VATAmount;
            }

            // Calculate ensemble products
            foreach (var ensemble in order.ProductEnsembles)
            {
                var calculation = CalculateEnsemble(ensemble);
                results.EnsembleCalculations.Add(calculation);
                results.EnsembleTotal += calculation.FinalTotal;
                results.EnsembleVAT += calculation.VATAmount;
            }

            // Calculate kit products
            foreach (var kit in order.ProductKits)
            {
                var calculation = CalculateKit(kit);
                results.KitCalculations.Add(calculation);
                results.KitTotal += calculation.TotalPrice;
                results.KitVAT += calculation.VATAmount;
            }

            // Calculate discounts
            results.TotalDiscount = CalculateDiscounts(order, results.IndividualProductsTotal + results.EnsembleTotal + results.KitTotal);

            // Calculate final totals
            results.Subtotal = results.IndividualProductsTotal + results.EnsembleTotal + results.KitTotal - results.TotalDiscount;
            results.TotalVAT = results.IndividualProductsVAT + results.EnsembleVAT + results.KitVAT;
            results.GrandTotal = results.Subtotal + results.TotalVAT + order.ShippingCost;

            return results;
        }

        private ProductCalculation CalculateProduct(Product product)
        {
            int quantity = Math.Max(product.Quantity, 1);
            decimal vatRate = Math.Max(product.VATRate, 0);
            decimal unitPrice = product.RRP;
            decimal totalPrice = unitPrice * quantity;
            decimal vatAmount = totalPrice * vatRate;

            return new ProductCalculation
            {
                Product = product,
                UnitPrice = unitPrice,
                TotalPrice = totalPrice,
                VATAmount = vatAmount,
                DiscountApplied = 0 // Individual product discounts would be calculated here
            };
        }

        private EnsembleCalculation CalculateEnsemble(ProductEnsemble ensemble)
        {
            int quantity = Math.Max(ensemble.Quantity, 1);
            decimal baseTotal = 0;
            var productCalculations = new List<ProductCalculation>();

            foreach (var product in ensemble.Products)
            {
                var productCalc = CalculateProduct(product);
                productCalculations.Add(productCalc);
                baseTotal += productCalc.TotalPrice;
            }

            decimal discountAmount = baseTotal * ensemble.EnsembleDiscount;
            decimal finalTotal = (baseTotal - discountAmount) * quantity;
            decimal vatAmount = finalTotal * 0.20m; // Could be made configurable

            return new EnsembleCalculation
            {
                Ensemble = ensemble,
                BaseTotal = baseTotal,
                DiscountAmount = discountAmount,
                FinalTotal = finalTotal,
                VATAmount = vatAmount,
                ProductCalculations = productCalculations
            };
        }

        private KitCalculation CalculateKit(ProductKit kit)
        {
            int quantity = Math.Max(kit.Quantity, 1);
            decimal kitPrice = kit.KitPrice > 0 ? kit.KitPrice : CalculateDefaultKitPrice(kit);
            decimal totalPrice = kitPrice * quantity;
            decimal vatAmount = totalPrice * 0.20m; // Could be made configurable

            var mandatoryCalculations = kit.MandatoryProducts.Select(CalculateProduct).ToList();
            var optionalCalculations = kit.OptionalProducts.Select(CalculateProduct).ToList();

            return new KitCalculation
            {
                Kit = kit,
                KitUnitPrice = kitPrice,
                TotalPrice = totalPrice,
                VATAmount = vatAmount,
                MandatoryProductCalculations = mandatoryCalculations,
                OptionalProductCalculations = optionalCalculations
            };
        }

        private decimal CalculateDefaultKitPrice(ProductKit kit)
        {
            decimal mandatoryTotal = kit.MandatoryProducts.Sum(p => p.RRP * Math.Max(p.Quantity, 1));
            decimal optionalTotal = kit.OptionalProducts.Sum(p => p.RRP * Math.Max(p.Quantity, 1));
            return (mandatoryTotal + optionalTotal) * 0.9m; // 10% kit discount
        }

        private decimal CalculateDiscounts(Order order, decimal subtotal)
        {
            decimal totalDiscount = 0;

            foreach (var discountCode in order.AppliedDiscountCodes)
            {
                if (!IsDiscountValid(discountCode, subtotal))
                    continue;

                decimal discountAmount = Math.Min(
                    subtotal * discountCode.DiscountPercentage,
                    discountCode.MaxDiscountAmount
                );
                totalDiscount += discountAmount;
            }

            return totalDiscount;
        }

        private bool IsDiscountValid(DiscountCode discountCode, decimal orderSubtotal)
        {
            return discountCode.IsActive &&
                   DateTime.Now >= discountCode.ValidFrom &&
                   DateTime.Now <= discountCode.ValidTo &&
                   discountCode.CurrentUsages < discountCode.MaxUsages &&
                   orderSubtotal >= discountCode.MinOrderAmount;
        }
    }

    /// <summary>
    /// Simple file writer implementation
    /// </summary>
    public class LocalFileWriter : IFileWriter
    {
        private readonly string _outputPath;

        public LocalFileWriter(string outputPath)
        {
            _outputPath = outputPath ?? throw new ArgumentNullException(nameof(outputPath));
            
            // Ensure directory exists
            if (!Directory.Exists(_outputPath))
            {
                Directory.CreateDirectory(_outputPath);
            }
        }

        public string WriteFile(string fileName, string content)
        {
            string fullPath = Path.Combine(_outputPath, fileName);
            File.WriteAllText(fullPath, content);
            return fullPath;
        }
    }

    /// <summary>
    /// Result object that contains all processing information
    /// </summary>
    public class OrderProcessingResult
    {
        public Order? Order { get; set; }
        public OrderCalculationResults? CalculationResults { get; set; }
        public string CsvFilePath { get; set; } = string.Empty;
        public DateTime ProcessedAt { get; set; }
        public string StrategyUsed { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }
}