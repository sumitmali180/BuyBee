using EMart.MCP.Server.Models;

namespace EMart.MCP.Server.Repositories.Interfaces
{
    public interface ICategoryRepository
    {
        Task<List<Category>> GetAllAsync();

        Task<Category?> GetByIdAsync(int categoryId);

        Task<List<Category>> SearchAsync(string searchText);
    }
}
