using EMart.Repositories.Interfaces;
using EMart.ViewModels;
using Microsoft.Data.SqlClient;
using System.Data;

namespace EMart.Repositories.Implementation
{
    public class ReorderRepository : IReorderRepository
    {
        private readonly IConfiguration _configuration;

        public ReorderRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private SqlConnection GetConnection()
        {
            string? connectionString = _configuration.GetConnectionString("DBCS");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Connection string 'DBCS' was not found.");
            }
            return new SqlConnection(connectionString);
        }

        public async Task<ReorderHubVM> GetReorderHubData(int customerId)
        {
            var hub = new ReorderHubVM();
            var allItems = new List<ReorderItemVM>();

            using (var con = GetConnection())
            {
                await con.OpenAsync();

                const string sql = @"
                    SELECT 
                        P.ProductId,
                        P.ProductName,
                        P.Price,
                        P.Quantity AS StockQuantity,
                        P.CategoryId,
                        ISNULL(C.CategoryName, 'General') AS CategoryName,
                        ISNULL(B.BrandName, 'Buybee') AS BrandName,
                        (SELECT TOP 1 ImagePath FROM ProductImages WHERE ProductId = P.ProductId) AS ThumbnailImage,
                        COUNT(OD.OrderDetailId) AS TimesOrdered,
                        MAX(O.OrderDate) AS LastOrderedDate,
                        MIN(O.OrderDate) AS FirstOrderedDate
                    FROM OrderDetails OD
                    INNER JOIN Orders O ON O.OrderId = OD.OrderId
                    INNER JOIN Products P ON P.ProductId = OD.ProductId
                    LEFT JOIN Categories C ON C.CategoryId = P.CategoryId
                    LEFT JOIN Brands B ON B.BrandId = P.BrandId
                    WHERE O.CustomerId = @CustomerId AND O.OrderStatus != 'Cancelled'
                    GROUP BY P.ProductId, P.ProductName, P.Price, P.Quantity, P.CategoryId, C.CategoryName, B.BrandName
                    ORDER BY MAX(O.OrderDate) DESC;";

                using (var cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@CustomerId", customerId);
                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        var lastDate = Convert.ToDateTime(reader["LastOrderedDate"]);
                        int daysSince = Math.Max(0, (DateTime.Now - lastDate).Days);
                        int categoryId = Convert.ToInt32(reader["CategoryId"]);
                        int timesOrdered = Convert.ToInt32(reader["TimesOrdered"]);

                        // Estimated cadence based on product category:
                        // Grocery: 14 days, Personal Care: 25 days, Home/Kitchen: 40 days, others: 60 days
                        int cadenceDays = categoryId switch
                        {
                            2 => 14, // Grocery & Gourmet
                            3 => 25, // Personal Care & Beauty
                            4 => 40, // Home & Kitchen
                            _ => 60
                        };

                        bool isDue = daysSince >= cadenceDays;
                        string urgency = "Regular";
                        string reason;

                        if (isDue)
                        {
                            if (daysSince >= cadenceDays + 10)
                            {
                                urgency = "Urgent";
                                reason = $"⚡ Running low! Last ordered {daysSince} days ago (Typical cycle: {cadenceDays}d)";
                            }
                            else
                            {
                                urgency = "DueSoon";
                                reason = $"📦 Replenishment recommended (Last ordered {daysSince} days ago)";
                            }
                        }
                        else if (timesOrdered >= 2)
                        {
                            urgency = "Favorite";
                            reason = $"⭐ Ordered {timesOrdered} times • Your frequent staple";
                        }
                        else
                        {
                            reason = $"Purchased {daysSince} days ago";
                        }

                        allItems.Add(new ReorderItemVM
                        {
                            ProductId = Convert.ToInt32(reader["ProductId"]),
                            ProductName = reader["ProductName"]?.ToString() ?? "",
                            Price = Convert.ToDecimal(reader["Price"]),
                            StockQuantity = Convert.ToInt32(reader["StockQuantity"]),
                            CategoryId = categoryId,
                            CategoryName = reader["CategoryName"]?.ToString() ?? "",
                            BrandName = reader["BrandName"]?.ToString() ?? "",
                            ThumbnailImage = reader["ThumbnailImage"]?.ToString(),
                            TimesOrdered = timesOrdered,
                            LastOrderedDate = lastDate,
                            DaysSinceLastOrder = daysSince,
                            CadenceDays = cadenceDays,
                            IsDueForReorder = isDue,
                            UrgencyLevel = urgency,
                            AiReason = reason,
                            DefaultReorderQuantity = 1
                        });
                    }
                }

                // If customer has no past orders, seed intelligent discovery recommendations from Grocery & Personal Care
                if (allItems.Count == 0)
                {
                    const string fallbackSql = @"
                        SELECT TOP 8
                            P.ProductId,
                            P.ProductName,
                            P.Price,
                            P.Quantity AS StockQuantity,
                            P.CategoryId,
                            ISNULL(C.CategoryName, 'Pantry Staples') AS CategoryName,
                            ISNULL(B.BrandName, 'Buybee') AS BrandName,
                            (SELECT TOP 1 ImagePath FROM ProductImages WHERE ProductId = P.ProductId) AS ThumbnailImage
                        FROM Products P
                        LEFT JOIN Categories C ON C.CategoryId = P.CategoryId
                        LEFT JOIN Brands B ON B.BrandId = P.BrandId
                        WHERE P.CategoryId IN (2, 3) AND P.IsActive = 1
                        ORDER BY P.ProductId ASC;";

                    using (var fbCmd = new SqlCommand(fallbackSql, con))
                    {
                        using var reader = await fbCmd.ExecuteReaderAsync();
                        while (await reader.ReadAsync())
                        {
                            allItems.Add(new ReorderItemVM
                            {
                                ProductId = Convert.ToInt32(reader["ProductId"]),
                                ProductName = reader["ProductName"]?.ToString() ?? "",
                                Price = Convert.ToDecimal(reader["Price"]),
                                StockQuantity = Convert.ToInt32(reader["StockQuantity"]),
                                CategoryId = Convert.ToInt32(reader["CategoryId"]),
                                CategoryName = reader["CategoryName"]?.ToString() ?? "",
                                BrandName = reader["BrandName"]?.ToString() ?? "",
                                ThumbnailImage = reader["ThumbnailImage"]?.ToString(),
                                TimesOrdered = 1,
                                LastOrderedDate = DateTime.Now.AddDays(-15),
                                DaysSinceLastOrder = 15,
                                CadenceDays = 14,
                                IsDueForReorder = true,
                                UrgencyLevel = "DueSoon",
                                AiReason = "Popular daily household essential",
                                DefaultReorderQuantity = 1
                            });
                        }
                    }
                }
            }

            hub.AllPastItems = allItems;
            hub.DueItems = allItems.Where(x => x.IsDueForReorder).ToList();
            hub.FrequentItems = allItems.Where(x => x.TimesOrdered >= 2 || x.UrgencyLevel == "Favorite").ToList();

            return hub;
        }

        public async Task<List<ReorderItemVM>> GetTopReorderSuggestions(int customerId, int take = 6)
        {
            var hub = await GetReorderHubData(customerId);
            // Prioritize due items first, then frequent items, then latest
            return hub.DueItems
                .Concat(hub.FrequentItems)
                .Concat(hub.AllPastItems)
                .GroupBy(p => p.ProductId)
                .Select(g => g.First())
                .Take(take)
                .ToList();
        }
    }
}
