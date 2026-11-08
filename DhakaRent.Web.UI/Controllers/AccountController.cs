
using DhakaRent.DataAccess.Identity;
using DhakaRent.Web.UI.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DhakaRent.Web.UI.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }


        // GET: /Account/Register
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            RegisterViewModel model)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check whether email already exists
            var existingUser = await _userManager
                .FindByEmailAsync(model.Email);

            if (existingUser != null)
            {
                ModelState.AddModelError(
                    nameof(model.Email),
                    "This email address is already registered.");

                return View(model);
            }

            // Create new user
            var user = new ApplicationUser
            {
                FullName = model.FullName.Trim(),
                UserName = model.Email.Trim(),
                Email = model.Email.Trim()
            };

            // Identity securely hashes the password
            var result = await _userManager
                .CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }

                return View(model);
            }

            // Assign role based on selected account type
            var selectedRole = model.AccountType;

            var roleResult = await _userManager
                .AddToRoleAsync(user, selectedRole);

            if (!roleResult.Succeeded)
            {
                // Delete user if role assignment fails
                await _userManager.DeleteAsync(user);

                foreach (var error in roleResult.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }

                return View(model);
            }

            if (!roleResult.Succeeded)
            {
                // Do not leave an account without its required role
                await _userManager.DeleteAsync(user);

                foreach (var error in roleResult.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }

                return View(model);
            }

            // Sign in after successful registration
            await _signInManager.SignInAsync(
                user,
                isPersistent: false);

            TempData["SuccessMessage"] =
                "Your account has been created successfully!";

            return RedirectToAction("Index", "Home");
        }
        // GET: /Account/Login
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                model.Email.Trim(),
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false
            );


            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Login successful!";

                // Get the logged-in user
                var user = await _userManager.FindByEmailAsync(
                    model.Email.Trim()
                );

                if (user == null)
                {
                    await _signInManager.SignOutAsync();

                    ModelState.AddModelError(
                        string.Empty,
                        "User account could not be found."
                    );

                    return View(model);
                }

                // Get user's assigned roles
                var roles = await _userManager.GetRolesAsync(user);

                // Redirect according to role
                if (roles.Contains("Admin"))
                {
                    return RedirectToAction(
                        "Dashboard", "Admin"
                    );
                }

                if (roles.Contains("Tenant"))
                {
                    return RedirectToAction(
                        "Dashboard", "Tenant"
                    );
                }

                if (roles.Contains("PropertyOwner"))
                {
                    return RedirectToAction(
                        "Dashboard", "PropertyOwner"
                    );
                }

                // Sign out users without an assigned role
                await _signInManager.SignOutAsync();

                ModelState.AddModelError(
                    string.Empty,
                    "Your account does not have an assigned role."
                );

                return View(model);
            }


            ModelState.AddModelError(
                string.Empty,
                "Invalid email or password."
            );

            return View(model);
        }
        // POST: /Account/Logout
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            TempData["SuccessMessage"] = "You have been logged out.";

            return RedirectToAction("Index", "Home");
        }
    }
}
