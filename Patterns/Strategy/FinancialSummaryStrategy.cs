using System;
using System.Linq;
using System.Text;
using DesignPatternsDemo.Models;

namespace DesignPatternsDemo.Patterns.Strategy
{
    /// <summary>
    /// Financial summary CSV reporting strategy for high-level financial reporting
    /// This demonstrates a completely different CSV format using the same calculation results
    /// </summary>
    public class FinancialSummaryStrategy : ICsvReportingStrategy
    {
        public string Description => "Financial Summary CSV Format - Aggregated data for financial reporting";

        public string GenerateCsvContent(Order order, OrderCalculationResults results)
        {
            var csvContent = new StringBuilder();
            
            // Financial summary header
            csvContent.AppendLine("OrderID,CustomerID,OrderDate,OrderType,ItemCount,SubtotalExVAT,VATAmount,DiscountAmount,ShippingCost,TotalAmount,PaymentMethod,PaymentProvider,ProcessingFee,OrderStatus");

            // Calculate summary statistics
            int totalItemCount = order.IndividualProducts.Sum(p => p.Quantity) + 
                               order.ProductEnsembles.Sum(e => e.Quantity * e.Products.Count) +
                               order.ProductKits.Sum(k => k.Quantity * (k.MandatoryProducts.Count + k.OptionalProducts.Count));

            string orderType = DetermineOrderType(order);
            string escapedPaymentMethod = EscapeCsvField(order.PaymentMethod.Type);
            string escapedPaymentProvider = EscapeCsvField(order.PaymentMethod.Provider);

            // Single summary line per order
            csvContent.AppendLine($"{order.OrderId},{order.CustomerId},{order.OrderDate:yyyy-MM-dd},{orderType},{totalItemCount},{results.Subtotal:F2},{results.TotalVAT:F2},{results.TotalDiscount:F2},{order.ShippingCost:F2},{results.GrandTotal:F2},{escapedPaymentMethod},{escapedPaymentProvider},{order.PaymentMethod.ProcessingFee:F2},{order.Status}");

            return csvContent.ToString();
        }

        public string GetFileName(Order order)
        {
            return $"FinancialSummary_{order.OrderId}_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid()}.csv";
        }

        private string DetermineOrderType(Order order)
        {
            bool hasIndividual = order.IndividualProducts.Any();
            bool hasEnsembles = order.ProductEnsembles.Any();
            bool hasKits = order.ProductKits.Any();

            if (hasIndividual && hasEnsembles && hasKits)
                return "Mixed";
            else if (hasKits)
                return "Kit-Only";
            else if (hasEnsembles)
                return "Ensemble-Only";
            else if (hasIndividual)
                return "Individual-Only";
            else
                return "Empty";
        }

        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field))
                return "";
                
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
            {
                return "\"" + field.Replace("\"", "\"\"") + "\"";
            }
            
            return field;
        }
    }
}