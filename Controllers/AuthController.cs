using Microsoft.AspNetCore.Mvc;
using MyMvcApp.Models.DTOs;
using MyMvcApp.Services.Contracts;
using MyMvcApp.Filters;
using Microsoft.AspNetCore.Identity;
using MyMvcApp.Models.Entities;
using Microsoft.AspNetCore.Authorization;

namespace MyMvcApp.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AuthController(IAuthService authService, ILogger<AuthController> logger, SignInManager<ApplicationUser> signInManager)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
        }

        // Register Page
        [RedirectIfAuthenticated]
        [Route("/register")]
        [HttpGet]
        public IActionResult Register()
        {
            return View(); // Views/Auth/Register.cshtml
        }

        [RedirectIfAuthenticated]
        [Route("/register")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (dto == null || !ModelState.IsValid)
            {
                _logger.LogWarning("Invalid registration attempt: Model state invalid or DTO is null.");
                return View(dto);
            }

            try
            {
                var user = await _authService.RegisterAsync(dto);
                _logger.LogInformation("User registered successfully: {Email}", dto.Email);
                
                // Sign in the new user
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Dashboard");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration failed for {Email}", dto.Email);
                ModelState.AddModelError(string.Empty, $"Registration failed: {ex.Message}");
                return View(dto);
            }
        }

        // Login Page
        [RedirectIfAuthenticated]
        [Route("/")]
        [HttpGet]
        public IActionResult Login()
        {
            return View(); // Views/Auth/Login.cshtml
        }

        [RedirectIfAuthenticated]
        [Route("/login")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            if (dto == null || !ModelState.IsValid)
            {
                _logger.LogWarning("Invalid login attempt: Model state invalid or DTO is null.");
                return View(dto);
            }

            try
            {
                var user = await _authService.LoginAsync(dto);
                _logger.LogInformation("User logged in successfully: {Email}", dto.Email);
                
                // Sign in the user
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Dashboard");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed for {Email}", dto.Email);
                ModelState.AddModelError(string.Empty, $"Login failed: {ex.Message}");
                return View(dto);
            }
        }

        // Logout
        [Authorize]
        [Route("/logout")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            _logger.LogInformation("User logged out.");
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Auth");
        }
    }
}