using EMart.Models;
using EMart.ViewModels;

namespace EMart.Repositories.Interfaces
{
    public interface ISubCategoriesRepository
    {
        PagedResult<SubCategories> GetAll(
        string searchText,
        int pageNumber,
        int pageSize);

        int GetTotalCount(string searchText);

        SubCategories? GetById(int id);
        void Insert(SubCategories subCategories);
        void Update(SubCategories subCategories);
        void Delete(int id);
        List<Categories> GetCategories();
        //public List<SubCategories> GetByCategory(int? categoryId);

        PagedResult<SubCategoryVM> GetByCategory(int? categoryId);
    }
}