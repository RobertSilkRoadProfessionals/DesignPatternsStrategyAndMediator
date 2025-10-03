using System;
using System.Text;
using DesignPatternsDemo.Models;

namespace DesignPatternsDemo.Patterns.Strategy
{
    /// <summary>
    /// Standard retail audit CSV reporting strategy
    /// This implements the original CSV format but in a clean, maintainable way
    /// </summary>
    public class StandardRetailAuditStrategy : ICsvReportingStrategy
    {
        public string Description => "Standard Retail Audit CSV Format - Compatible with legacy systems";

        public string GenerateCsvContent(Order order, OrderCalculationResults results)
        {
            var csvContent = new StringBuilder();
            
            // Add header
            csvContent.AppendLine("OrderID,CustomerID,OrderDate,ProductType,ProductID,ProductName,Quantity,UnitPrice,TotalPrice,VATAmount,DiscountApplied");

            // Process individual products
            foreach (var productCalc in results.ProductCalculations)
            {
                csvContent.AppendLine(FormatProductLine(
                    order, 
                    "Individual", 
                    productCalc.Product.Id, 
                    productCalc.Product.Name,
                    productCalc.Product.Quantity,
                    productCalc.UnitPrice,
                    productCalc.TotalPrice,
                    productCalc.VATAmount,
                    productCalc.DiscountApplied
                ));
            }

            // Process ensemble products
            foreach (var ensembleCalc in results.EnsembleCalculations)
            {
                foreach (var productCalc in ensembleCalc.ProductCalculations)
                {
                    csvContent.AppendLine(FormatProductLine(
                        order,
                        "Ensemble",
                        productCalc.Product.Id,
                        productCalc.Product.Name,
                        productCalc.Product.Quantity,
                        productCalc.UnitPrice,
                        productCalc.TotalPrice,
                        productCalc.VATAmount,
                        productCalc.DiscountApplied
                    ));
                }
            }

            // Process kit products
            foreach (var kitCalc in results.KitCalculations)
            {
                // Mandatory products
                foreach (var productCalc in kitCalc.MandatoryProductCalculations)
                {
                    csvContent.AppendLine(FormatProductLine(
                        order,
                        "Kit-Mandatory",
                        productCalc.Product.Id,
                        productCalc.Product.Name,
                        productCalc.Product.Quantity,
                        productCalc.UnitPrice,
                        productCalc.TotalPrice,
                        productCalc.VATAmount,
                        productCalc.DiscountApplied
                    ));
                }

                // Optional products
                foreach (var productCalc in kitCalc.OptionalProductCalculations)
                {
                    csvContent.AppendLine(FormatProductLine(
                        order,
                        "Kit-Optional",
                        productCalc.Product.Id,
                        productCalc.Product.Name,
                        productCalc.Product.Quantity,
                        productCalc.UnitPrice,
                        productCalc.TotalPrice,
                        productCalc.VATAmount,
                        productCalc.DiscountApplied
                    ));
                }
            }

            return csvContent.ToString();
        }

        public string GetFileName(Order order)
        {
            return $"RetailAudit_{order.OrderId}_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid()}.csv";
        }

        private string FormatProductLine(Order order, string productType, string productId, string productName, 
            int quantity, decimal unitPrice, decimal totalPrice, decimal vatAmount, decimal discountApplied)
        {
            // Escape any commas in product names to prevent CSV corruption
            string escapedProductName = productName.Replace(",", ";");
            
            return $"{order.OrderId},{order.CustomerId},{order.OrderDate:yyyy-MM-dd},{productType},{productId},{escapedProductName},{quantity},{unitPrice:F2},{totalPrice:F2},{vatAmount:F2},{discountApplied:F2}";
        }
    }
}