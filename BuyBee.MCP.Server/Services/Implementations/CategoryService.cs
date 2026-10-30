using EMart.MCP.Server.Models;
using EMart.MCP.Server.Repositories.Interfaces;
using EMart.MCP.Server.Services.Interfaces;

namespace EMart.MCP.Server.Services.Implementations
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repository;

        public CategoryService(ICategoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<Category>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<Category?> GetByIdAsync(int categoryId)
        {
            return await _repository.GetByIdAsync(categoryId);
        }

        public async Task<List<Category>> SearchAsync(
            string searchText)
        {
            return await _repository.SearchAsync(searchText);
        }
    }
}