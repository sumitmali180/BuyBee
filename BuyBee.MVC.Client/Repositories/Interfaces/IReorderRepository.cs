using EMart.ViewModels;

namespace EMart.Repositories.Interfaces
{
    public interface IReorderRepository
    {
        Task<ReorderHubVM> GetReorderHubData(int customerId);
        Task<List<ReorderItemVM>> GetTopReorderSuggestions(int customerId, int take = 6);
    }
}
