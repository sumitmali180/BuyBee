using EMart.MCP.Server.Models;

namespace EMart.MCP.Server.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<List<Category>> GetAllAsync();

        Task<Category?> GetByIdAsync(int categoryId);

        Task<List<Category>> SearchAsync(string searchText);
    }
}