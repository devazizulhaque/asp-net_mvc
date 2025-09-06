using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyMvcApp.Models.DTOs;
using MyMvcApp.Services.Contracts;

namespace MyMvcApp.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoryController> _logger;
        private readonly string _webRootPath;

        public CategoryController(ICategoryService categoryService, ILogger<CategoryController> logger, IWebHostEnvironment webHostEnvironment)
        {
            _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _webRootPath = webHostEnvironment.WebRootPath;
        }

        [Authorize]
        [Route("/categories")]
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
        [Authorize]
        [Route("categories/list")]
        [HttpPost]
        public async Task<IActionResult> List()
        {
            _logger.LogInformation("Processing DataTables request for categories.");
            _logger.LogInformation("Request form keys: {Keys}", string.Join(", ", Request.Form.Keys));

            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][data]"].FirstOrDefault();
            var sortDirection = Request.Form["order[0][dir]"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault()?.ToLower();

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            var (recordsTotal, recordsFiltered, data) = await _categoryService.GetCategoriesForDataTableAsync(
                skip, pageSize, sortColumn, sortDirection, searchValue);
            _logger.LogInformation("Fetched {RecordCount} categories (Filtered: {FilteredCount})", data.Count(), recordsFiltered);

            return Json(new
            {
                draw = draw,
                recordsTotal = recordsTotal,
                recordsFiltered = recordsTotal,
                data = data.Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.Slug,
                    CreatedAt = c.CreatedAt.ToString("yyyy-MM-dd"),
                    Status = c.IsActive ? "<span class='badge badge-linesuccess'>Active</span>" : "<span class='badge badge-linedanger'>Inactive</span>",
                    Actions = $@"
                            <div class='edit-delete-action'>
                                <a class='me-2 p-2 btn-edit' href='javascript:void(0);' data-bs-toggle='modal' data-bs-target='#edit-category' data-id='{c.Id}'>
                                    <i data-feather='edit' class='feather-edit'></i>
                                </a>
                                <a class='confirm-text p-2' href='javascript:void(0);' data-id='{c.Id}'>
                                    <i data-feather='trash-2' class='feather-trash-2'></i>
                                </a>
                            </div>"
                })
            });
        }


        [Authorize]
        [Route("/categories/{id}")]
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                _logger.LogWarning("Category with ID {CategoryId} not found.", id);
                return NotFound();
            }
            return View(category);
        }
        [Authorize]
        [Route("/categories/create")]
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateCategoryDto dto)
        {
            if (!ModelState.IsValid)
            {
                // Collect validation errors
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors?.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                    );

                return Json(new { success = false, errors });
            }

            try
            {
                var categoryId = await _categoryService.CreateCategoryAsync(dto, _webRootPath);
                _logger.LogInformation("Category created with ID {CategoryId}.", categoryId);

                return Json(new { success = true, message = "Category created successfully!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                return Json(new { success = false, message = "An unexpected error occurred." });
            }
        }

        [Authorize]
        [Route("/categories/edit/{id}")]
        [HttpGet]
        public async Task<IActionResult> GetCategory(int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                _logger.LogWarning("Category with ID {CategoryId} not found.", id);
                return NotFound();
            }

            return Json(new
            {
                id = category.Id,
                parentCategoryId = category.ParentCategoryId,
                name = category.Name,
                slug = category.Slug,
                description = category.Description,
                isActive = category.IsActive,
                imageUrl = category.ImageUrl
            });
        }

        [Authorize]
        [Route("/categories/edit/{id}")]
        [HttpPost]
        public async Task<IActionResult> Update(int id, UpdateCategoryDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            var updatedCategory = await _categoryService.UpdateCategoryAsync(dto, _webRootPath);
            if (updatedCategory == null)
            {
                _logger.LogWarning("Category with ID {CategoryId} not found.", id);
                return NotFound();
            }

            _logger.LogInformation("Category with ID {CategoryId} updated.", id);
            return RedirectToAction("Index");
        }
        [Authorize]
        [Route("/categories/delete/{id}")]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _categoryService.DeleteCategoryAsync(id);
            if (!success)
            {
                _logger.LogWarning("Category with ID {CategoryId} not found.", id);
                return NotFound();
            }

            _logger.LogInformation("Category with ID {CategoryId} deleted.", id);
            return RedirectToAction("Index");
        }
    }
}