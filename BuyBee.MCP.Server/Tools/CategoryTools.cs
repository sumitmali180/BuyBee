using EMart.MCP.Server.Services.Interfaces;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace EMart.MCP.Server.Tools
{
    [McpServerToolType]
    public class CategoryTools
    {
        private readonly ICategoryService _categoryService;

        public CategoryTools(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [McpServerTool]
        [Description("Gets all product categories from EMart.")]
        public async Task<object> GetCategories()
        {
            return await _categoryService.GetAllAsync();
        }

        [McpServerTool]
        [Description("Gets a specific EMart category using its category ID.")]
        public async Task<object> GetCategoryById(
            [Description("The category ID.")]
            int categoryId)
        {
            var category =
                await _categoryService.GetByIdAsync(categoryId);

            if (category == null)
            {
                return new
                {
                    success = false,
                    message = $"Category {categoryId} was not found."
                };
            }

            return category;
        }

        [McpServerTool]
        [Description("Searches EMart categories by category name.")]
        public async Task<object> SearchCategories(
            [Description("Text to search in the category name.")]
            string searchText)
        {
            return await _categoryService.SearchAsync(searchText);
        }
    }
}