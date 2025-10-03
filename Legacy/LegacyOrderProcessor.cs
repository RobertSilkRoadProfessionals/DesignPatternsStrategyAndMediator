using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DesignPatternsDemo.Models;

namespace DesignPatternsDemo.Legacy
{
    /// <summary>
    /// LEGACY CODE - DO NOT USE IN PRODUCTION!
    /// 
    /// This is an intentionally badly written "big ball of mud" class that demonstrates
    /// poor software engineering practices. It violates multiple SOLID principles:
    /// - Single Responsibility: Does order processing, validation, calculations, AND CSV generation
    /// - Open/Closed: Hard to extend without modifying existing code
    /// - Dependency Inversion: Tightly coupled to file system and specific CSV format
    /// 
    /// Problems with this approach:
    /// 1. Massive method with complex nested logic
    /// 2. Mixed responsibilities (business logic + file I/O + formatting)
    /// 3. Hard-coded CSV format makes it impossible to support other formats
    /// 4. Difficult to test individual pieces of functionality
    /// 5. Violates separation of concerns
    /// 6. Modifies the Order object during processing (side effects)
    /// 7. No proper error handling
    /// 8. String concatenation for CSV (performance issues)
    /// 9. Magic numbers and hard-coded values scattered throughout
    /// </summary>
    public class LegacyOrderProcessor
    {
        private string _csvOutputPath;
        private List<string> _auditLog;

        public LegacyOrderProcessor(string csvOutputPath)
        {
            _csvOutputPath = csvOutputPath;
            _auditLog = new List<string>();
        }

        /// <summary>
        /// This monolithic method does EVERYTHING - a perfect example of what NOT to do!
        /// It processes orders, calculates totals, validates data, and generates CSV reports
        /// all in one massive, unmaintainable method.
        /// </summary>
        public void ProcessOrderAndGenerateRetailAuditReport(Order order)
        {
            // WARNING: This method modifies the order object - major side effect!
            try
            {
                Console.WriteLine("=== LEGACY ORDER PROCESSOR ===");
                Console.WriteLine("Processing order: " + order.OrderId);

                // Validation logic mixed with processing logic (BAD!)
                if (string.IsNullOrEmpty(order.OrderId))
                {
                    order.OrderId = "ORD-" + DateTime.Now.Ticks.ToString();
                    Console.WriteLine("Generated new order ID: " + order.OrderId);
                }

                // Calculate totals for individual products (buried in the middle of the method)
                decimal individualProductsTotal = 0;
                decimal individualProductsVAT = 0;
                foreach (var product in order.IndividualProducts)
                {
                    if (product.Quantity <= 0) product.Quantity = 1; // Side effect!
                    if (product.VATRate < 0) product.VATRate = 0.20m; // Another side effect!
                    
                    decimal productTotal = product.RRP * product.Quantity;
                    decimal productVAT = productTotal * product.VATRate;
                    individualProductsTotal += productTotal;
                    individualProductsVAT += productVAT;
                    
                    // Logging mixed with business logic (BAD!)
                    _auditLog.Add($"Product {product.Name}: Total={productTotal}, VAT={productVAT}");
                }

                // Calculate ensemble totals (duplicated logic with slight variations)
                decimal ensembleTotal = 0;
                decimal ensembleVAT = 0;
                foreach (var ensemble in order.ProductEnsembles)
                {
                    if (ensemble.Quantity <= 0) ensemble.Quantity = 1; // More side effects!
                    
                    decimal baseEnsembleTotal = 0;
                    foreach (var product in ensemble.Products)
                    {
                        if (product.Quantity <= 0) product.Quantity = 1; // Even more side effects!
                        baseEnsembleTotal += product.RRP * product.Quantity;
                    }
                    
                    // Apply ensemble discount (hard-coded business rules)
                    decimal discountAmount = baseEnsembleTotal * ensemble.EnsembleDiscount;
                    decimal ensembleSubtotal = (baseEnsembleTotal - discountAmount) * ensemble.Quantity;
                    
                    // Hard-coded VAT calculation (should be configurable)
                    decimal ensembleVATAmount = ensembleSubtotal * 0.20m;
                    
                    ensembleTotal += ensembleSubtotal;
                    ensembleVAT += ensembleVATAmount;
                    
                    _auditLog.Add($"Ensemble {ensemble.Name}: Subtotal={ensembleSubtotal}, VAT={ensembleVATAmount}");
                }

                // Calculate kit totals (yet another variation of similar logic)
                decimal kitTotal = 0;
                decimal kitVAT = 0;
                foreach (var kit in order.ProductKits)
                {
                    if (kit.Quantity <= 0) kit.Quantity = 1; // Consistent side effects!
                    
                    // Kits have fixed pricing, but we still need to validate
                    if (kit.KitPrice <= 0)
                    {
                        // Fallback calculation if kit price is not set (more business logic)
                        decimal mandatoryTotal = 0;
                        foreach (var product in kit.MandatoryProducts)
                        {
                            mandatoryTotal += product.RRP * product.Quantity;
                        }
                        decimal optionalTotal = 0;
                        foreach (var product in kit.OptionalProducts)
                        {
                            optionalTotal += product.RRP * product.Quantity;
                        }
                        kit.KitPrice = (mandatoryTotal + optionalTotal) * 0.9m; // 10% kit discount
                    }
                    
                    decimal kitSubtotal = kit.KitPrice * kit.Quantity;
                    decimal kitVATAmount = kitSubtotal * 0.20m; // More hard-coded VAT
                    
                    kitTotal += kitSubtotal;
                    kitVAT += kitVATAmount;
                    
                    _auditLog.Add($"Kit {kit.Name}: Subtotal={kitSubtotal}, VAT={kitVATAmount}");
                }

                // Apply discount codes (complex business logic mixed with everything else)
                decimal totalDiscount = 0;
                foreach (var discountCode in order.AppliedDiscountCodes)
                {
                    if (!discountCode.IsActive) continue;
                    if (DateTime.Now < discountCode.ValidFrom || DateTime.Now > discountCode.ValidTo) continue;
                    if (discountCode.CurrentUsages >= discountCode.MaxUsages) continue;
                    
                    decimal orderSubtotal = individualProductsTotal + ensembleTotal + kitTotal;
                    if (orderSubtotal < discountCode.MinOrderAmount) continue;
                    
                    decimal discountAmount = Math.Min(
                        orderSubtotal * discountCode.DiscountPercentage,
                        discountCode.MaxDiscountAmount
                    );
                    totalDiscount += discountAmount;
                    
                    // Side effect: updating usage count
                    discountCode.CurrentUsages++;
                    
                    _auditLog.Add($"Applied discount {discountCode.Code}: Amount={discountAmount}");
                }

                // Calculate final totals (more business logic)
                decimal subtotal = individualProductsTotal + ensembleTotal + kitTotal - totalDiscount;
                decimal totalVATAmount = individualProductsVAT + ensembleVAT + kitVAT;
                decimal grandTotal = subtotal + totalVATAmount + order.ShippingCost;

                // More side effects: updating the order object
                order.TotalVAT = totalVATAmount;
                order.OrderTotal = grandTotal;

                // Validate payment (yet more business logic)
                if (Math.Abs(order.ActualPricePaid - grandTotal) > 0.01m)
                {
                    Console.WriteLine($"WARNING: Payment mismatch! Expected: {grandTotal}, Paid: {order.ActualPricePaid}");
                }

                // NOW we get to the CSV generation part (completely mixed with business logic)
                Console.WriteLine("Generating Retail Audit CSV Report...");
                
                // Hard-coded CSV structure (inflexible!)
                var csvContent = new StringBuilder();
                
                // Header (what if we need different headers for different audit systems?)
                csvContent.AppendLine("OrderID,CustomerID,OrderDate,ProductType,ProductID,ProductName,Quantity,UnitPrice,TotalPrice,VATAmount,DiscountApplied");

                // Individual products section (repetitive CSV building code)
                foreach (var product in order.IndividualProducts)
                {
                    decimal unitPrice = product.RRP;
                    decimal totalPrice = unitPrice * product.Quantity;
                    decimal vatAmount = totalPrice * product.VATRate;
                    
                    // String concatenation (performance issue for large orders)
                    csvContent.AppendLine($"{order.OrderId},{order.CustomerId},{order.OrderDate:yyyy-MM-dd},Individual,{product.Id},{product.Name},{product.Quantity},{unitPrice:F2},{totalPrice:F2},{vatAmount:F2},0.00");
                }

                // Ensemble products section (duplicated CSV logic)
                foreach (var ensemble in order.ProductEnsembles)
                {
                    decimal baseTotal = 0;
                    foreach (var product in ensemble.Products)
                    {
                        baseTotal += product.RRP * product.Quantity;
                    }
                    
                    decimal discountAmount = baseTotal * ensemble.EnsembleDiscount;
                    decimal ensemblePrice = (baseTotal - discountAmount) / ensemble.Products.Count; // Weird calculation
                    decimal vatAmount = ensemblePrice * 0.20m;
                    
                    foreach (var product in ensemble.Products)
                    {
                        // More string building (inefficient)
                        csvContent.AppendLine($"{order.OrderId},{order.CustomerId},{order.OrderDate:yyyy-MM-dd},Ensemble,{product.Id},{product.Name},{product.Quantity * ensemble.Quantity},{ensemblePrice:F2},{ensemblePrice * product.Quantity:F2},{vatAmount:F2},{discountAmount / ensemble.Products.Count:F2}");
                    }
                }

                // Kit products section (more duplicated logic)
                foreach (var kit in order.ProductKits)
                {
                    decimal kitUnitPrice = kit.KitPrice / (kit.MandatoryProducts.Count + kit.OptionalProducts.Count);
                    decimal vatAmount = kitUnitPrice * 0.20m;
                    
                    // Mandatory products
                    foreach (var product in kit.MandatoryProducts)
                    {
                        csvContent.AppendLine($"{order.OrderId},{order.CustomerId},{order.OrderDate:yyyy-MM-dd},Kit-Mandatory,{product.Id},{product.Name},{product.Quantity * kit.Quantity},{kitUnitPrice:F2},{kitUnitPrice * product.Quantity:F2},{vatAmount:F2},0.00");
                    }
                    
                    // Optional products
                    foreach (var product in kit.OptionalProducts)
                    {
                        csvContent.AppendLine($"{order.OrderId},{order.CustomerId},{order.OrderDate:yyyy-MM-dd},Kit-Optional,{product.Id},{product.Name},{product.Quantity * kit.Quantity},{kitUnitPrice:F2},{kitUnitPrice * product.Quantity:F2},{vatAmount:F2},0.00");
                    }
                }

                // File I/O mixed with everything else (violation of separation of concerns)
                string fileName = $"RetailAudit_{order.OrderId}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                string fullPath = Path.Combine(_csvOutputPath, fileName);
                
                // What if the directory doesn't exist? What if there are permission issues?
                File.WriteAllText(fullPath, csvContent.ToString());
                
                Console.WriteLine($"CSV report saved to: {fullPath}");
                Console.WriteLine($"Total records: {csvContent.ToString().Split('\n').Length - 2}"); // Off-by-one errors waiting to happen
                
                // Logging at the end (should be throughout the process)
                Console.WriteLine("=== PROCESSING COMPLETE ===");
                Console.WriteLine($"Order Total: {order.OrderTotal:C}");
                Console.WriteLine($"VAT Amount: {order.TotalVAT:C}");
                Console.WriteLine($"Audit Log Entries: {_auditLog.Count}");
                
                // Print audit log (more mixed concerns)
                foreach (var logEntry in _auditLog)
                {
                    Console.WriteLine($"AUDIT: {logEntry}");
                }
            }
            catch (Exception ex)
            {
                // Generic error handling (doesn't help with debugging)
                Console.WriteLine($"Error processing order: {ex.Message}");
                throw; // Just re-throw without adding value
            }
        }

        /// <summary>
        /// Another problematic method that tries to do too much
        /// What if we need different validation rules for different scenarios?
        /// </summary>
        private bool ValidateOrder(Order order)
        {
            // Hard-coded validation rules (not configurable or extensible)
            if (string.IsNullOrEmpty(order.OrderId)) return false;
            if (string.IsNullOrEmpty(order.CustomerId)) return false;
            if (order.OrderDate == default(DateTime)) return false;
            
            // What about validating products, ensembles, kits?
            // This method is incomplete and would need to grow into another monster!
            
            return true;
        }

        /// <summary>
        /// Clear the audit log (why is this the processor's responsibility?)
        /// </summary>
        public void ClearAuditLog()
        {
            _auditLog.Clear();
        }

        /// <summary>
        /// Get audit log (mixed concerns - why is the processor a logger?)
        /// </summary>
        public List<string> GetAuditLog()
        {
            return new List<string>(_auditLog); // At least it returns a copy
        }
    }
}