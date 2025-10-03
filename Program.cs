using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DesignPatternsDemo.Models;
using DesignPatternsDemo.Legacy;
using DesignPatternsDemo.Patterns.Strategy;
using DesignPatternsDemo.Patterns.Mediator;

namespace DesignPatternsDemo
{
    /// <summary>
    /// Demonstration program showing the evolution from legacy "big ball of mud" code
    /// to clean, maintainable code using Strategy and Mediator design patterns
    /// </summary>
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=======================================================");
            Console.WriteLine("DESIGN PATTERNS DEMONSTRATION");
            Console.WriteLine("Transforming Legacy Code with Strategy & Mediator Patterns");
            Console.WriteLine("=======================================================\n");

            // Create output directory for CSV files
            string outputPath = Path.Combine(Environment.CurrentDirectory, "Reports");
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            // Create a sample complex order for demonstration
            var sampleOrder = CreateComplexSampleOrder();

            Console.WriteLine("1. LEGACY APPROACH (Big Ball of Mud)");
            Console.WriteLine("=====================================");
            await DemonstrateLegacyApproach(sampleOrder, outputPath);

            Console.WriteLine("\n2. STRATEGY PATTERN IMPROVEMENT");
            Console.WriteLine("=================================");
            await DemonstrateStrategyPattern(sampleOrder, outputPath);

            Console.WriteLine("\n3. MEDIATOR PATTERN ENHANCEMENT");
            Console.WriteLine("================================");
            await DemonstrateMediatorPattern(sampleOrder, outputPath);

            Console.WriteLine("\n4. PATTERN COMPARISON SUMMARY");
            Console.WriteLine("==============================");
            DisplayPatternComparison();

            Console.WriteLine($"\nGenerated CSV files can be found in: {outputPath}");
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }

        private static Order CreateComplexSampleOrder()
        {
            Console.WriteLine("Creating complex sample order with products, ensembles, and kits...\n");

            var order = new Order
            {
                OrderId = "ORD-2024-001",
                OrderDate = DateTime.Now,
                CustomerId = "CUST-12345",
                CustomerEmail = "customer@example.com",
                Status = "Processing",
                ShippingCost = 15.99m,
                ActualPricePaid = 299.97m,
                ShippingAddress = "123 Main St, City, State 12345",
                BillingAddress = "123 Main St, City, State 12345",
                PaymentMethod = new PaymentMethod
                {
                    Type = "Credit Card",
                    Provider = "Visa",
                    LastFourDigits = "1234",
                    TransactionDate = DateTime.Now,
                    TransactionId = "TXN-789456",
                    ProcessingFee = 2.99m,
                    IsVerified = true
                }
            };

            // Add individual products
            order.IndividualProducts.AddRange(
            [
                new Product
                {
                    Id = "PROD-001",
                    Name = "Wireless Bluetooth Headphones",
                    RRP = 79.99m,
                    VATRate = 0.20m,
                    Quantity = 1,
                    Category = "Electronics",
                    Supplier = "TechCorp Ltd",
                    CreatedDate = DateTime.Now.AddDays(-30)
                },
                new Product
                {
                    Id = "PROD-002",
                    Name = "USB-C Cable 2m",
                    RRP = 12.99m,
                    VATRate = 0.20m,
                    Quantity = 2,
                    Category = "Accessories",
                    Supplier = "CableCo Inc",
                    CreatedDate = DateTime.Now.AddDays(-15)
                }
            ]);

            // Add product ensemble
            var gamingEnsemble = new ProductEnsemble
            {
                Id = "ENS-001",
                Name = "Gaming Setup Bundle",
                Theme = "Gaming Essentials",
                EnsembleDiscount = 0.10m, // 10% discount
                Quantity = 1,
                CreatedDate = DateTime.Now.AddDays(-7)
            };

            gamingEnsemble.Products.AddRange(
            [
                new Product
                {
                    Id = "PROD-003",
                    Name = "Gaming Mouse",
                    RRP = 45.99m,
                    VATRate = 0.20m,
                    Quantity = 1,
                    Category = "Gaming",
                    Supplier = "GameGear Ltd"
                },
                new Product
                {
                    Id = "PROD-004",
                    Name = "Mechanical Keyboard",
                    RRP = 89.99m,
                    VATRate = 0.20m,
                    Quantity = 1,
                    Category = "Gaming",
                    Supplier = "GameGear Ltd"
                }
            ]);

            order.ProductEnsembles.Add(gamingEnsemble);

            // Add product kit
            var starterKit = new ProductKit
            {
                Id = "KIT-001",
                Name = "Home Office Starter Kit",
                KitType = "Starter",
                KitPrice = 149.99m, // Fixed kit price
                Quantity = 1,
                IsCustomizable = true,
                CreatedDate = DateTime.Now.AddDays(-3)
            };

            starterKit.MandatoryProducts.AddRange(
            [
                new Product
                {
                    Id = "PROD-005",
                    Name = "Wireless Mouse",
                    RRP = 25.99m,
                    VATRate = 0.20m,
                    Quantity = 1,
                    Category = "Office",
                    Supplier = "OfficeTech Inc"
                }
            ]);

            starterKit.OptionalProducts.AddRange(
            [
                new Product
                {
                    Id = "PROD-006",
                    Name = "Laptop Stand",
                    RRP = 35.99m,
                    VATRate = 0.20m,
                    Quantity = 1,
                    Category = "Office",
                    Supplier = "OfficeTech Inc"
                },
                new Product
                {
                    Id = "PROD-007",
                    Name = "Desk Organizer",
                    RRP = 19.99m,
                    VATRate = 0.20m,
                    Quantity = 1,
                    Category = "Office",
                    Supplier = "OfficeTech Inc"
                }
            ]);

            order.ProductKits.Add(starterKit);

            // Add discount codes
            order.AppliedDiscountCodes.Add(new DiscountCode
            {
                Code = "NEWCUSTOMER10",
                DiscountPercentage = 0.10m,
                MaxDiscountAmount = 50.00m,
                ValidFrom = DateTime.Now.AddDays(-30),
                ValidTo = DateTime.Now.AddDays(30),
                MaxUsages = 100,
                CurrentUsages = 15,
                MinOrderAmount = 100.00m,
                IsActive = true,
                ApplicableCategories = new List<string> { "Electronics", "Gaming", "Office" }
            });

            return order;
        }

        private static Task DemonstrateLegacyApproach(Order order, string outputPath)
        {
            try
            {
                Console.WriteLine("Problems with the legacy approach:");
                Console.WriteLine("- Monolithic method doing everything");
                Console.WriteLine("- Mixed responsibilities (business logic + I/O + formatting)");
                Console.WriteLine("- Hard to test individual components");
                Console.WriteLine("- Violates SOLID principles");
                Console.WriteLine("- Side effects: modifies the original order object");
                Console.WriteLine("- Hard-coded CSV format\n");

                var legacyProcessor = new LegacyOrderProcessor(outputPath);
                legacyProcessor.ProcessOrderAndGenerateRetailAuditReport(order);

                Console.WriteLine($"Audit log entries: {legacyProcessor.GetAuditLog().Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Legacy processing failed: {ex.Message}");
            }
            return Task.CompletedTask;
        }

        private static Task DemonstrateStrategyPattern(Order order, string outputPath)
        {
            Console.WriteLine("Improvements with Strategy Pattern:");
            Console.WriteLine("- Separation of concerns: calculation vs reporting");
            Console.WriteLine("- Multiple CSV formats supported");
            Console.WriteLine("- Open/Closed principle: easy to add new formats");
            Console.WriteLine("- No side effects on original order");
            Console.WriteLine("- Better error handling\n");

            var calculator = new StandardOrderCalculator();

            // Demonstrate different CSV strategies
            var strategies = new ICsvReportingStrategy[]
            {
                new StandardRetailAuditStrategy(),
                new EnhancedRetailAuditStrategy(),
                new FinancialSummaryStrategy()
            };

            foreach (var strategy in strategies)
            {
                try
                {
                    var fileWriter = new LocalFileWriter(outputPath);
                    var processor = new ImprovedOrderProcessor(strategy, calculator, fileWriter);
                    
                    var result = processor.ProcessOrder(order);
                    
                    if (result.Success)
                    {
                        Console.WriteLine($"✓ Successfully generated: {Path.GetFileName(result.CsvFilePath)}");
                    }
                    else
                    {
                        Console.WriteLine($"✗ Failed to generate report: {result.ErrorMessage}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"✗ Strategy {strategy.Description} failed: {ex.Message}");
                }
            }
            return Task.CompletedTask;
        }

        private static async Task DemonstrateMediatorPattern(Order order, string outputPath)
        {
            Console.WriteLine("Additional benefits with Mediator Pattern:");
            Console.WriteLine("- Centralized orchestration of complex workflows");
            Console.WriteLine("- Cross-cutting concerns (validation, logging, notifications)");
            Console.WriteLine("- Loose coupling between components");
            Console.WriteLine("- Easy to add new processing steps");
            Console.WriteLine("- Consistent error handling and logging\n");

            var calculator = new StandardOrderCalculator();
            var mediator = new OrderProcessingMediator(calculator);

            // Demonstrate mediator with different strategies
            var strategyNames = new[] { "Standard", "Enhanced", "Financial", "test" };

            foreach (var strategyName in strategyNames)
            {
                try
                {
                    var request = new ProcessOrderRequest
                    {
                        Order = order,
                        StrategyName = strategyName,
                        OutputPath = outputPath,
                        ValidateOrder = true,
                        SendNotifications = true,
                        LogAuditTrail = true
                    };

                    var result = await mediator.ProcessOrderAsync(request);

                    if (result.Success)
                    {
                        Console.WriteLine($"✓ Mediator successfully processed order with {strategyName} strategy");
                        Console.WriteLine($"  File: {Path.GetFileName(result.CsvFilePath)}");
                        Console.WriteLine($"  Total: {result.CalculationResults?.GrandTotal:C}");
                    }
                    else
                    {
                        Console.WriteLine($"✗ Mediator processing failed: {result.ErrorMessage}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"✗ Mediator with {strategyName} strategy failed: {ex.Message}");
                }

                Console.WriteLine(); // Add spacing between strategies
            }
        }

        private static void DisplayPatternComparison()
        {
            Console.WriteLine("COMPARISON SUMMARY:");
            Console.WriteLine("==================");
            Console.WriteLine();
            
            Console.WriteLine("LEGACY CODE PROBLEMS:");
            Console.WriteLine("❌ Monolithic 'god' method");
            Console.WriteLine("❌ Mixed responsibilities");
            Console.WriteLine("❌ Hard-coded dependencies");
            Console.WriteLine("❌ Difficult to test");
            Console.WriteLine("❌ Side effects");
            Console.WriteLine("❌ Violates SOLID principles");
            Console.WriteLine("❌ Poor error handling");
            Console.WriteLine();

            Console.WriteLine("STRATEGY PATTERN BENEFITS:");
            Console.WriteLine("✅ Flexible algorithm selection");
            Console.WriteLine("✅ Open/Closed principle");
            Console.WriteLine("✅ Separation of concerns");
            Console.WriteLine("✅ Easy to unit test");
            Console.WriteLine("✅ No side effects");
            Console.WriteLine("✅ Multiple output formats");
            Console.WriteLine();

            Console.WriteLine("MEDIATOR PATTERN ADDITIONAL BENEFITS:");
            Console.WriteLine("✅ Centralized workflow orchestration");
            Console.WriteLine("✅ Loose coupling between components");
            Console.WriteLine("✅ Cross-cutting concerns handling");
            Console.WriteLine("✅ Consistent error handling");
            Console.WriteLine("✅ Easy to extend with new features");
            Console.WriteLine("✅ Request/response pattern");
            Console.WriteLine();

            Console.WriteLine("MAINTAINABILITY IMPROVEMENTS:");
            Console.WriteLine("📈 Code readability: Significantly improved");
            Console.WriteLine("📈 Testability: Each component can be unit tested");
            Console.WriteLine("📈 Extensibility: New formats and features easy to add");
            Console.WriteLine("📈 Debugging: Clear separation of concerns");
            Console.WriteLine("📈 Team collaboration: Different developers can work on different handlers");
        }
    }
}
