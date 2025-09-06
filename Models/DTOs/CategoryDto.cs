using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using MyMvcApp.Data;

namespace MyMvcApp.Models.DTOs
{

    // Custom validation attribute for file size
    public class MaxFileSizeAttribute : ValidationAttribute
    {
        private readonly int _maxFileSize;
        public MaxFileSizeAttribute(int maxFileSize)
        {
            _maxFileSize = maxFileSize;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is IFormFile file)
            {
                if (file.Length > _maxFileSize)
                {
                    return new ValidationResult($"Maximum allowed file size is {_maxFileSize / (1024 * 1024)} MB.");
                }
            }
            return ValidationResult.Success;
        }
    }


    public class UniqueCategoryNameAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is string name && !string.IsNullOrWhiteSpace(name))
            {
                var dbContextObj = validationContext.GetService(typeof(AppDbContext));
                if (dbContextObj is not AppDbContext dbContext)
                {
                    return new ValidationResult("Database context is not available.");
                }

                int? currentId = null;

                // Detect which DTO type is being validated
                if (validationContext.ObjectInstance is UpdateCategoryDto updateDto)
                {
                    currentId = updateDto.Id; // existing category being updated
                }
                // If it's CreateCategoryDto, currentId stays null

                var exists = dbContext.Categories
                    .FirstOrDefault(c => c.Name == name);

                if (exists != null && (currentId == null || exists.Id != currentId.Value))
                {
                    return new ValidationResult("Category name must be unique.");
                }
            }

            return ValidationResult.Success;
        }
    }

    
    // For creating a new category
    public class CreateCategoryDto
    {
        public int? ParentCategoryId { get; set; }
        [Required(ErrorMessage = "Category name is required.")]
        [MaxLength(100, ErrorMessage = "Category name cannot exceed 100 characters.")]
        [UniqueCategoryName]
        public string Name { get; set; } = string.Empty;
        [MaxLength(100, ErrorMessage = "Category slug cannot exceed 100 characters.")]
        public string Slug { get; set; } = string.Empty;
        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }
        [MaxFileSize(2 * 1024 * 1024)]
        public IFormFile? ImageFile { get; set; }
        [Required(ErrorMessage = "Please specify if the category is active.")]
        public bool IsActive { get; set; } = true;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    }
    // For updating an existing category
    public class UpdateCategoryDto
    {
        [Required(ErrorMessage = "Category ID is required.")]
        public int Id { get; set; }
        public int? ParentCategoryId { get; set; }
        [Required(ErrorMessage = "Category name is required.")]
        [MaxLength(100, ErrorMessage = "Category name cannot exceed 100 characters.")]
        [UniqueCategoryName]
        public string Name { get; set; } = string.Empty;
        [MaxLength(100, ErrorMessage = "Category slug cannot exceed 100 characters.")]
        public string Slug { get; set; } = string.Empty;
        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }
        [MaxFileSize(2 * 1024 * 1024)]
        public IFormFile? ImageFile { get; set; } // for new uploads
        public string? ExistingImageUrl { get; set; } // for displaying current image
        [Required(ErrorMessage = "Please specify if the category is active.")]
        public bool IsActive { get; set; }
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}
 