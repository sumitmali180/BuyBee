using EMart.Models;
using EMart.Repositories.Interfaces;
using EMart.ViewModels;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace EMart.Repositories.Implementation
{
    public class ProductsRepository : IProductsRepository
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public ProductsRepository(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }
  
        private SqlConnection GetConnection()
        {
            return new SqlConnection(_configuration.GetConnectionString("DBCS"));
        }

        #region Get All Products

        public async Task<PagedResult<Products>> GetAll(string? searchText, int pageNumber, int pageSize)
        {
            var products = new List<Products>();
            int totalRecords = 0;

            using (SqlConnection con = GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand("sp_Products2", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Flag", "SELECTALL");
                    cmd.Parameters.AddWithValue("@SearchText", searchText ?? "");
                    cmd.Parameters.AddWithValue("@PageNumber", pageNumber);
                    cmd.Parameters.AddWithValue("@PageSize", pageSize);

                    await con.OpenAsync();

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            products.Add(new Products
                            {
                                ProductId = reader["ProductId"] == DBNull.Value
        ? 0 : Convert.ToInt32(reader["ProductId"]),

                                SKU = reader["SKU"]?.ToString() ?? "",

                                ProductName = reader["ProductName"]?.ToString() ?? "",

                                UnitId = reader["UnitId"] == DBNull.Value
        ? 0 : Convert.ToInt32(reader["UnitId"]),

                                UnitName = reader["UnitName"]?.ToString() ?? "",

                                CategoryId = reader["CategoryId"] == DBNull.Value
        ? 0 : Convert.ToInt32(reader["CategoryId"]),

                                CategoryName = reader["CategoryName"]?.ToString() ?? "",

                                SubCategoryId = reader["SubCategoryId"] == DBNull.Value
        ? 0 : Convert.ToInt32(reader["SubCategoryId"]),

                                SubCategoryName = reader["SubCategoryName"]?.ToString() ?? "",

                                BrandId = reader["BrandId"] == DBNull.Value
        ? 0 : Convert.ToInt32(reader["BrandId"]),

                                BrandName = reader["BrandName"]?.ToString() ?? "",

                                Price = reader["Price"] == DBNull.Value
        ? 0 : Convert.ToDecimal(reader["Price"]),

                                Quantity = reader["Quantity"] == DBNull.Value
        ? 0 : Convert.ToInt32(reader["Quantity"]),

                                Description = reader["Description"]?.ToString() ?? "",

                                IsActive = reader["IsActive"] == DBNull.Value
        ? false : Convert.ToBoolean(reader["IsActive"]),

                                ThumbnailImage = reader["ThumbnailImage"]?.ToString() ?? ""
                            });
                        }

                        if (await reader.NextResultAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                totalRecords = Convert.ToInt32(reader[0]);
                            }
                        }
                    }
                }
            }

            return new PagedResult<Products>
            {
                Items = products,
                TotalRecords = totalRecords,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        #endregion

        #region Get Product By Id

        public async Task<ProductVM> GetById(int productId)
        {
            ProductVM model = new ProductVM();

            using (SqlConnection con = GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand("sp_Products2", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Flag", "SELECTBYID");
                    cmd.Parameters.AddWithValue("@ProductId", productId);

                    await con.OpenAsync();

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            model.Product = new Products
                            {
                                ProductId = reader["ProductId"] == DBNull.Value ? 0 : Convert.ToInt32(reader["ProductId"]),
                                SKU = reader["SKU"] == DBNull.Value ? "" : reader["SKU"].ToString(),
                                ProductName = reader["ProductName"] == DBNull.Value ? "" : reader["ProductName"].ToString(),

                                UnitName = reader["UnitName"] == DBNull.Value ? "" : reader["UnitName"].ToString(),
                                UnitId = reader["UnitId"] == DBNull.Value ? 0 : Convert.ToInt32(reader["UnitId"]),

                                CategoryId = reader["CategoryId"] == DBNull.Value ? 0 : Convert.ToInt32(reader["CategoryId"]),
                                CategoryName = reader["CategoryName"] == DBNull.Value ? "" : reader["CategoryName"].ToString(),

                                SubCategoryId = reader["SubCategoryId"] == DBNull.Value ? 0 : Convert.ToInt32(reader["SubCategoryId"]),
                                SubCategoryName = reader["SubCategoryName"] == DBNull.Value ? "" : reader["SubCategoryName"].ToString(),

                                BrandId = reader["BrandId"] == DBNull.Value ? 0 : Convert.ToInt32(reader["BrandId"]),
                                BrandName = reader["BrandName"] == DBNull.Value ? "" : reader["BrandName"].ToString(),

                                Price = reader["Price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["Price"]),
                                Quantity = reader["Quantity"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Quantity"]),

                                Description = reader["Description"] == DBNull.Value ? "" : reader["Description"].ToString(),

                                IsActive = reader["IsActive"] == DBNull.Value ? false : Convert.ToBoolean(reader["IsActive"]),

                                CreatedOn = reader["CreatedOn"] == DBNull.Value
                                    ? null
                                    : Convert.ToDateTime(reader["CreatedOn"]),

                                UpdatedOn = reader["UpdatedOn"] == DBNull.Value
                                    ? null
                                    : Convert.ToDateTime(reader["UpdatedOn"])
                            };
                        }
                    }
                }

                // Load Product Images
                model.ProductImages = await GetImages(productId);
            }

            return model;
        }

        #endregion

        public async Task<bool> Insert(ProductVM model)
        {
            int productId = 0;

            using (SqlConnection con = GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand("sp_Products2", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Flag", "INSERT");
                    //cmd.Parameters.AddWithValue("@SKU", model.Product.SKU);
                    cmd.Parameters.AddWithValue("@ProductName", model.Product.ProductName);
                    cmd.Parameters.AddWithValue("@UnitId", model.Product.UnitId);
                    cmd.Parameters.AddWithValue("@CategoryId", model.Product.CategoryId);
                    cmd.Parameters.AddWithValue("@SubCategoryId", model.Product.SubCategoryId);
                    cmd.Parameters.AddWithValue("@BrandId", model.Product.BrandId);
                    cmd.Parameters.AddWithValue("@Price", model.Product.Price);
                    cmd.Parameters.AddWithValue("@Quantity", model.Product.Quantity);
                    cmd.Parameters.AddWithValue("@Description", model.Product.Description ?? "");
                    cmd.Parameters.AddWithValue("@IsActive", model.Product.IsActive);
                    cmd.Parameters.AddWithValue("@CreatedBy", model.Product.CreatedBy ?? 1);

                    await con.OpenAsync();

                    object? result = await cmd.ExecuteScalarAsync();

                    if (result != null)
                    {
                        productId = Convert.ToInt32(result);
                    }
                }

                if (productId > 0 && model.Images != null && model.Images.Count > 0)
                {
                    string uploadPath = Path.Combine(_environment.WebRootPath, "uploads", "products");

                    if (!Directory.Exists(uploadPath))
                        Directory.CreateDirectory(uploadPath);

                    foreach (IFormFile file in model.Images)
                    {
                        if (file.Length > 0)
                        {
                            string fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";

                            string filePath = Path.Combine(uploadPath, fileName);

                            using (FileStream stream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }

                            using (SqlCommand imageCmd = new SqlCommand("sp_Products2", con))
                            {
                                imageCmd.CommandType = CommandType.StoredProcedure;

                                imageCmd.Parameters.AddWithValue("@Flag", "INSERTIMAGE");
                                imageCmd.Parameters.AddWithValue("@ProductId", productId);
                                imageCmd.Parameters.AddWithValue("@ImagePath", "/uploads/products/" + fileName);

                                await imageCmd.ExecuteNonQueryAsync();
                            }
                        }
                    }
                }

                con.Close();
            }

            return true;
        }

        public async Task<bool> Update(ProductVM model)
        {
            using (SqlConnection con = GetConnection())
            {
                await con.OpenAsync();

                using (SqlCommand cmd = new SqlCommand("sp_Products2", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Flag", "UPDATE");
                    cmd.Parameters.AddWithValue("@ProductId", model.Product.ProductId);
                    //cmd.Parameters.AddWithValue("@SKU", model.Product.SKU);
                    cmd.Parameters.AddWithValue("@ProductName", model.Product.ProductName);
                    cmd.Parameters.AddWithValue("@UnitId", model.Product.UnitId);
                    cmd.Parameters.AddWithValue("@CategoryId", model.Product.CategoryId);
                    cmd.Parameters.AddWithValue("@SubCategoryId", model.Product.SubCategoryId);
                    cmd.Parameters.AddWithValue("@BrandId", model.Product.BrandId);
                    cmd.Parameters.AddWithValue("@Price", model.Product.Price);
                    cmd.Parameters.AddWithValue("@Quantity", model.Product.Quantity);
                    cmd.Parameters.AddWithValue("@Description", model.Product.Description ?? "");
                    cmd.Parameters.AddWithValue("@IsActive", model.Product.IsActive);
                    cmd.Parameters.AddWithValue("@UpdatedBy", model.Product.UpdatedBy ?? 1);

                    await cmd.ExecuteNonQueryAsync();
                }

                // Upload New Images
                if (model.Images != null && model.Images.Count > 0)
                {
                    string uploadPath = Path.Combine(_environment.WebRootPath, "uploads", "products");

                    if (!Directory.Exists(uploadPath))
                        Directory.CreateDirectory(uploadPath);

                    foreach (IFormFile file in model.Images)
                    {
                        if (file.Length > 0)
                        {
                            string fileName = Guid.NewGuid() + "_" + Path.GetFileName(file.FileName);

                            string fullPath = Path.Combine(uploadPath, fileName);

                            using (FileStream stream = new FileStream(fullPath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }

                            using (SqlCommand imageCmd = new SqlCommand("sp_Products2", con))
                            {
                                imageCmd.CommandType = CommandType.StoredProcedure;

                                imageCmd.Parameters.AddWithValue("@Flag", "INSERTIMAGE");
                                imageCmd.Parameters.AddWithValue("@ProductId", model.Product.ProductId);
                                imageCmd.Parameters.AddWithValue("@ImagePath", "/uploads/products/" + fileName);

                                await imageCmd.ExecuteNonQueryAsync();
                            }
                        }
                    }
                }
            }

            return true;
        }

        public async Task<bool> Delete(int productId)
        {
            List<ProductImages> images = await GetImages(productId);

            foreach (var image in images)
            {
                if (!string.IsNullOrEmpty(image.ImagePath))
                {
                    string file = Path.Combine(
                        _environment.WebRootPath,
                        image.ImagePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));

                    if (File.Exists(file))
                    {
                        File.Delete(file);
                    }
                }
            }

            using (SqlConnection con = GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand("sp_Products2", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Flag", "DELETE");
                    cmd.Parameters.AddWithValue("@ProductId", productId);

                    await con.OpenAsync();

                    await cmd.ExecuteNonQueryAsync();
                }
            }

            return true;
        }

        public async Task<List<ProductImages>> GetImages(int productId)
        {
            List<ProductImages> images = new();

            using (SqlConnection con = GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand("sp_Products2", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Flag", "GETIMAGES");
                    cmd.Parameters.AddWithValue("@ProductId", productId);

                    await con.OpenAsync();

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            images.Add(new ProductImages
                            {
                                ImageId = Convert.ToInt32(reader["ImageId"]),
                                ProductId = Convert.ToInt32(reader["ProductId"]),
                                ImagePath = reader["ImagePath"].ToString()
                            });
                        }
                    }
                }
            }

            return images;
        }

        public async Task<bool> DeleteImage(int imageId)
        {
            string imagePath = string.Empty;

            using (SqlConnection con = GetConnection())
            {
                await con.OpenAsync();

                // Get Image Path
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT ImagePath FROM ProductImages WHERE ImageId=@ImageId", con))
                {
                    cmd.Parameters.AddWithValue("@ImageId", imageId);

                    object? result = await cmd.ExecuteScalarAsync();

                    if (result != null)
                    {
                        imagePath = result.ToString()!;
                    }
                }

                // Delete Database Record
                using (SqlCommand cmd = new SqlCommand("sp_Products2", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Flag", "DELETEIMAGE");
                    cmd.Parameters.AddWithValue("@ImageId", imageId);

                    await cmd.ExecuteNonQueryAsync();
                }
            }

            if (!string.IsNullOrEmpty(imagePath))
            {
                string file = Path.Combine(
                    _environment.WebRootPath,
                    imagePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));

                if (File.Exists(file))
                {
                    File.Delete(file);
                }
            }

            return true;
        }

        public async Task<List<Products>> GetLeatestProducts(int count)
        {
            List<Products> products = new();

            using (SqlConnection con = GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand("SP_Products", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Flag", "LATESTPRODUCTS");
                    cmd.Parameters.AddWithValue("@PageSize", count);

                    await con.OpenAsync();

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            products.Add(new Products
                            {
                                ProductId = Convert.ToInt32(reader["ProductId"]),
                                SKU = reader["SKU"].ToString(),
                                ProductName = reader["ProductName"].ToString(),
                                CategoryName = reader["CategoryName"].ToString(),
                                SubCategoryName = reader["SubCategoryName"].ToString(),
                                BrandName = reader["BrandName"].ToString(),
                                Price = Convert.ToDecimal(reader["Price"]),
                                Quantity = Convert.ToInt32(reader["Quantity"]),
                                Description = reader["Description"].ToString(),
                                IsActive = Convert.ToBoolean(reader["IsActive"]),
                                ThumbnailImage = reader["ThumbnailImage"].ToString()
                            });
                        }
                    }
                }
            }

            return products;

        }

        public async Task<List<Products>> GetFeaturedProducts(int count)
        {
            List<Products> products = new();

            using (SqlConnection con = GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand("SP_Products", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Flag", "FEATUREDPRODUCTS");
                    cmd.Parameters.AddWithValue("@PageSize", count);

                    await con.OpenAsync();

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            products.Add(new Products
                            {
                                ProductId = Convert.ToInt32(reader["ProductId"]),
                                SKU = reader["SKU"].ToString(),
                                ProductName = reader["ProductName"].ToString(),
                                CategoryName = reader["CategoryName"].ToString(),
                                SubCategoryName = reader["SubCategoryName"].ToString(),
                                BrandName = reader["BrandName"].ToString(),
                                Price = Convert.ToDecimal(reader["Price"]),
                                Quantity = Convert.ToInt32(reader["Quantity"]),
                                Description = reader["Description"].ToString(),
                                IsActive = Convert.ToBoolean(reader["IsActive"]),
                                ThumbnailImage = reader["ThumbnailImage"].ToString()
                            });
                        }
                    }
                }
            }

            return products;
        }

        public async Task<PagedResult<Products>> GetCatalogAsync(
            string? searchText,
            int? categoryId,
            int? subCategoryId,
            int? brandId,
            decimal? minPrice,
            decimal? maxPrice,
            string? sortBy,
            int pageNumber,
            int pageSize)
        {
            var products = new List<Products>();
            int totalRecords = 0;

            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 12;

            using (SqlConnection con = GetConnection())
            {
                var conditions = new List<string> { "p.IsActive = 1" };
                using SqlCommand cmd = new SqlCommand();
                cmd.Connection = con;

                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    conditions.Add("(p.ProductName LIKE @Search OR p.Description LIKE @Search OR b.BrandName LIKE @Search OR c.CategoryName LIKE @Search)");
                    cmd.Parameters.AddWithValue("@Search", $"%{searchText.Trim()}%");
                }

                if (categoryId.HasValue && categoryId.Value > 0)
                {
                    conditions.Add("p.CategoryId = @CategoryId");
                    cmd.Parameters.AddWithValue("@CategoryId", categoryId.Value);
                }

                if (subCategoryId.HasValue && subCategoryId.Value > 0)
                {
                    conditions.Add("p.SubCategoryId = @SubCategoryId");
                    cmd.Parameters.AddWithValue("@SubCategoryId", subCategoryId.Value);
                }

                if (brandId.HasValue && brandId.Value > 0)
                {
                    conditions.Add("p.BrandId = @BrandId");
                    cmd.Parameters.AddWithValue("@BrandId", brandId.Value);
                }

                if (minPrice.HasValue && minPrice.Value > 0)
                {
                    conditions.Add("p.Price >= @MinPrice");
                    cmd.Parameters.AddWithValue("@MinPrice", minPrice.Value);
                }

                if (maxPrice.HasValue && maxPrice.Value > 0)
                {
                    conditions.Add("p.Price <= @MaxPrice");
                    cmd.Parameters.AddWithValue("@MaxPrice", maxPrice.Value);
                }

                string whereClause = string.Join(" AND ", conditions);

                string orderByClause = sortBy switch
                {
                    "price_asc" => "p.Price ASC",
                    "price_desc" => "p.Price DESC",
                    "name_asc" => "p.ProductName ASC",
                    "name_desc" => "p.ProductName DESC",
                    _ => "p.ProductId DESC"
                };

                int offset = (pageNumber - 1) * pageSize;
                cmd.Parameters.AddWithValue("@Offset", offset);
                cmd.Parameters.AddWithValue("@PageSize", pageSize);

                string sql = $@"
                    SELECT p.ProductId, p.ProductName, p.SKU, p.Price, p.Quantity, p.Description, p.IsActive,
                           p.CategoryId, ISNULL(c.CategoryName, '') AS CategoryName,
                           p.SubCategoryId, ISNULL(s.SubCategoryName, '') AS SubCategoryName,
                           p.BrandId, ISNULL(b.BrandName, '') AS BrandName,
                           p.UnitId, ISNULL(u.UnitName, '') AS UnitName,
                           ISNULL((SELECT TOP 1 ImagePath FROM ProductImages WHERE ProductId = p.ProductId ORDER BY ImageId ASC), '') AS ThumbnailImage,
                           COUNT(*) OVER() AS TotalCount
                    FROM Products p
                    LEFT JOIN Categories c ON p.CategoryId = c.CategoryId
                    LEFT JOIN SubCategories s ON p.SubCategoryId = s.SubCategoryId
                    LEFT JOIN Brands b ON p.BrandId = b.BrandId
                    LEFT JOIN Units u ON p.UnitId = u.UnitId
                    WHERE {whereClause}
                    ORDER BY {orderByClause}
                    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                cmd.CommandText = sql;
                cmd.CommandType = CommandType.Text;

                await con.OpenAsync();
                using SqlDataReader reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    if (totalRecords == 0 && reader["TotalCount"] != DBNull.Value)
                    {
                        totalRecords = Convert.ToInt32(reader["TotalCount"]);
                    }

                    products.Add(new Products
                    {
                        ProductId = Convert.ToInt32(reader["ProductId"]),
                        SKU = reader["SKU"]?.ToString() ?? "",
                        ProductName = reader["ProductName"]?.ToString() ?? "",
                        Price = reader["Price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["Price"]),
                        Quantity = reader["Quantity"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Quantity"]),
                        Description = reader["Description"]?.ToString() ?? "",
                        IsActive = Convert.ToBoolean(reader["IsActive"]),
                        CategoryId = reader["CategoryId"] == DBNull.Value ? 0 : Convert.ToInt32(reader["CategoryId"]),
                        CategoryName = reader["CategoryName"].ToString(),
                        SubCategoryId = reader["SubCategoryId"] == DBNull.Value ? 0 : Convert.ToInt32(reader["SubCategoryId"]),
                        SubCategoryName = reader["SubCategoryName"].ToString(),
                        BrandId = reader["BrandId"] == DBNull.Value ? 0 : Convert.ToInt32(reader["BrandId"]),
                        BrandName = reader["BrandName"].ToString(),
                        UnitId = reader["UnitId"] == DBNull.Value ? 0 : Convert.ToInt32(reader["UnitId"]),
                        UnitName = reader["UnitName"].ToString(),
                        ThumbnailImage = reader["ThumbnailImage"].ToString()
                    });
                }
            }

            return new PagedResult<Products>
            {
                Items = products,
                TotalRecords = totalRecords,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
}