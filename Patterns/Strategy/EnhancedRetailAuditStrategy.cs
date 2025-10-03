using System;
using System.Linq;
using System.Text;
using DesignPatternsDemo.Models;

namespace DesignPatternsDemo.Patterns.Strategy
{
    /// <summary>
    /// Enhanced retail audit CSV reporting strategy with additional compliance fields
    /// This demonstrates how easy it is to add new reporting formats using the Strategy pattern
    /// </summary>
    public class EnhancedRetailAuditStrategy : ICsvReportingStrategy
    {
        public string Description => "Enhanced Retail Audit CSV Format - Includes compliance and tracking data";

        public string GenerateCsvContent(Order order, OrderCalculationResults results)
        {
            var csvContent = new StringBuilder();

            // Enhanced header with additional compliance fields
            csvContent.AppendLine("OrderID,CustomerID,CustomerEmail,OrderDate,ProductType,ProductID,ProductName,Category,Supplier,Quantity,UnitPrice,TotalPrice,VATRate,VATAmount,DiscountApplied,PaymentMethod,TrackingNumber,ComplianceStatus");

            // Process individual products with enhanced data
            foreach (var productCalc in results.ProductCalculations)
            {
                csvContent.AppendLine(FormatEnhancedProductLine(
                    order,
                    "Individual",
                    productCalc.Product,
                    productCalc.Product.Quantity,
                    productCalc.UnitPrice,
                    productCalc.TotalPrice,
                    productCalc.Product.VATRate,
                    productCalc.VATAmount,
                    productCalc.DiscountApplied
                ));
            }

            // Process ensemble products
            foreach (var ensembleCalc in results.EnsembleCalculations)
            {
                foreach (var productCalc in ensembleCalc.ProductCalculations)
                {
                    csvContent.AppendLine(FormatEnhancedProductLine(
                        order,
                        $"Ensemble-{ensembleCalc.Ensemble.Theme}",
                        productCalc.Product,
                        productCalc.Product.Quantity,
                        productCalc.UnitPrice,
                        productCalc.TotalPrice,
                        productCalc.Product.VATRate,
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
                    csvContent.AppendLine(FormatEnhancedProductLine(
                        order,
                        $"Kit-{kitCalc.Kit.KitType}-Mandatory",
                        productCalc.Product,
                        productCalc.Product.Quantity,
                        productCalc.UnitPrice,
                        productCalc.TotalPrice,
                        productCalc.Product.VATRate,
                        productCalc.VATAmount,
                        productCalc.DiscountApplied
                    ));
                }

                // Optional products
                foreach (var productCalc in kitCalc.OptionalProductCalculations)
                {
                    csvContent.AppendLine(FormatEnhancedProductLine(
                        order,
                        $"Kit-{kitCalc.Kit.KitType}-Optional",
                        productCalc.Product,
                        productCalc.Product.Quantity,
                        productCalc.UnitPrice,
                        productCalc.TotalPrice,
                        productCalc.Product.VATRate,
                        productCalc.VATAmount,
                        productCalc.DiscountApplied
                    ));
                }
            }

            return csvContent.ToString();
        }

        public string GetFileName(Order order)
        {
            return $"EnhancedRetailAudit_{order.OrderId}_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid()}.csv";
        }

        private string FormatEnhancedProductLine(Order order, string productType, Product product,
            int quantity, decimal unitPrice, decimal totalPrice, decimal vatRate, decimal vatAmount, decimal discountApplied)
        {
            // Escape any commas in text fields
            string escapedProductName = EscapeCsvField(product.Name);
            string escapedCategory = EscapeCsvField(product.Category);
            string escapedSupplier = EscapeCsvField(product.Supplier);
            string escapedEmail = EscapeCsvField(order.CustomerEmail);
            string escapedPaymentMethod = EscapeCsvField(order.PaymentMethod.Type);
            string trackingNumber = string.IsNullOrEmpty(order.TrackingNumber) ? "N/A" : order.TrackingNumber;

            // Calculate compliance status based on various factors
            string complianceStatus = CalculateComplianceStatus(order, product);

            return $"{order.OrderId},{order.CustomerId},{escapedEmail},{order.OrderDate:yyyy-MM-dd},{productType},{product.Id},{escapedProductName},{escapedCategory},{escapedSupplier},{quantity},{unitPrice:F2},{totalPrice:F2},{vatRate:P2},{vatAmount:F2},{discountApplied:F2},{escapedPaymentMethod},{trackingNumber},{complianceStatus}";
        }

        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field))
                return "";

            // If the field contains commas, quotes, or newlines, wrap it in quotes and escape internal quotes
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
            {
                return "\"" + field.Replace("\"", "\"\"") + "\"";
            }

            return field;
        }

        private string CalculateComplianceStatus(Order order, Product product)
        {
            var issues = new List<string>();

            // Check for various compliance issues
            if (string.IsNullOrEmpty(product.Supplier))
                issues.Add("NO_SUPPLIER");

            if (product.VATRate <= 0)
                issues.Add("INVALID_VAT");

            if (string.IsNullOrEmpty(order.TrackingNumber) && order.Status == "Shipped")
                issues.Add("NO_TRACKING");

            if (!order.PaymentMethod.IsVerified)
                issues.Add("UNVERIFIED_PAYMENT");

            if (order.AppliedDiscountCodes.Any(dc => !dc.IsActive))
                issues.Add("INACTIVE_DISCOUNT");

            return issues.Any() ? string.Join(";", issues) : "COMPLIANT";
        }
    }
}