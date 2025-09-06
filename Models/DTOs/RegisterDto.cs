using System.ComponentModel.DataAnnotations;

using System.Text.RegularExpressions;
using MyMvcApp.Data;

namespace MyMvcApp.Models.DTOs
{
    public class PasswordStrengthAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var password = value as string;
            if (string.IsNullOrEmpty(password))
                return ValidationResult.Success;

            if (!Regex.IsMatch(password, @"[A-Z]"))
                return new ValidationResult("Password must have at least one uppercase letter.");

            if (!Regex.IsMatch(password, @"[a-z]"))
                return new ValidationResult("Password must have at least one lowercase letter.");

            if (!Regex.IsMatch(password, @"[0-9]"))
                return new ValidationResult("Password must have at least one number.");

            return ValidationResult.Success;
        }
    }

    public class UniqueEmailAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var dbContextObj = validationContext.GetService(typeof(AppDbContext));
            if (dbContextObj is not AppDbContext dbContext)
            {
                throw new InvalidOperationException("AppDbContext is not available in ValidationContext.");
            }
            var email = value as string;

            if (dbContext.Users.Any(u => u.Email == email))
            {
                return new ValidationResult("This email is already registered.");
            }

            return ValidationResult.Success;
        }
    }
    public class RegisterDto
    {
        [Required(ErrorMessage = "First name is required")]
        public required string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        public required string LastName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [UniqueEmail]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [PasswordStrength(ErrorMessage = "Passwords must have at least one uppercase ('A'-'Z')")]
        public required string Password { get; set; }

        [Required(ErrorMessage = "Confirm Password is required")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public required string ConfirmPassword { get; set; }

        [Range(typeof(bool), "true", "true", ErrorMessage = "You must accept the terms")]
        public bool TermsAccepted { get; set; }
    }
}
