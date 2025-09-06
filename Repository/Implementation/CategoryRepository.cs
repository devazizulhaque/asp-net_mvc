using MyMvcApp.Data;
using MyMvcApp.Models.Entities;
using MyMvcApp.Repository.Contracts;

namespace MyMvcApp.Repository.Implementation
{
    public class CategoryRepository : BaseRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(AppDbContext context) : base(context)
        {
        }

        // Additional methods specific to Category can be added here
    }
}
