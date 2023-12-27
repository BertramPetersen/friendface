using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using FriendFace.Models;
using FriendFace.ViewModels;
using Microsoft.AspNetCore.Identity.UI.Services; // Assuming this is where your User model is located

namespace FriendFace.Controllers
{
    public class LoginController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IEmailSender _emailSender;

        public LoginController(UserManager<User> userManager, SignInManager<User> signInManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        // GET: Login/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: Login/Register
        [HttpPost]
        public async Task<IActionResult> Register(string fname, string lname,string uname, string email, string psw)
        {
            var user = new User {FirstName = fname, LastName = lname, UserName = uname, Email = email };
            try
            {
                var result = await _userManager.CreateAsync(user, psw);
                // Handle result...
                if (result.Succeeded)
                {
                    var token = await _userManager.GenerateTwoFactorTokenAsync(user, TokenOptions.DefaultEmailProvider);
                    // Send the token via email
                    await _emailSender.SendEmailAsync(user.Email, "Your 2FA Code", $"Your 2FA code is: {token}");

                    // Redirect to a view where they can enter the 2FA token
                    return RedirectToAction("VerifyTwoFactor", new { userId = user.Id });
                }

                // Handle errors
                foreach (var error in result.Errors)
                {
                    throw new Exception(error.Description);
                }
            }
            catch (Exception ex)
            {
                // Log exception
                throw new InvalidOperationException($"Unexpected error occurred in {nameof(Register)}", ex);
            }

            

            return View();
        }
        // GET: Verify 2FA
        public IActionResult VerifyTwoFactor(string userId)
        {
            // Return a view where the user can input their 2FA token
            return View(new VerifyTwoFactorViewModel { UserId = userId });
        }
        
        // POST: Verify 2FA
        [HttpPost]
        public async Task<IActionResult> VerifyTwoFactor(VerifyTwoFactorViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);

            if (user != null)
            {
                var result =
                    await _userManager.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultEmailProvider, model.Token);

                if (result)
                {
                    // Sign in the user
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    user.TwoFactorEnabled = true;
                    await _userManager.UpdateAsync(user);
                    return RedirectToAction("Index", "Home"); 
                }
            }

            // Handle failure...
            return View(model);
        }

        // GET: Login/Login
        public IActionResult Login()
        {
            // return to home page if user is already logged in
            return View("Index");
        }

        // POST: Login/Login
        [HttpPost]
        public async Task<IActionResult> Login(string uname, string psw)
        {
            var result = await _signInManager.PasswordSignInAsync(uname, psw, isPersistent: false, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home"); // Modify as needed
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View("Index");
        }

        // POST: Login/Logout
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home"); // Modify as needed
        }
    }
}
