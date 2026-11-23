
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using portfolio.Models;

namespace portfolio.Controllers
{
    public class EditorController : Controller
    {
        private readonly IConfiguration _configuration;

        public EditorController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // SHOW PASSCODE PAGE
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Unlock(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(new EditorPasscodeViewModel());
        }

        // VERIFY PASSCODE
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unlock(
            EditorPasscodeViewModel model,
            string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }

            string? correctPasscode =
                _configuration["Editor:Passcode"];

            if (string.IsNullOrEmpty(correctPasscode))
            {
                ModelState.AddModelError(
                    "",
                    "Editor passcode is not configured.");

                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }

            byte[] enteredBytes =
                Encoding.UTF8.GetBytes(model.Passcode);

            byte[] correctBytes =
                Encoding.UTF8.GetBytes(correctPasscode);

            bool isCorrect =
                enteredBytes.Length == correctBytes.Length &&
                CryptographicOperations.FixedTimeEquals(
                    enteredBytes,
                    correctBytes);

            if (!isCorrect)
            {
                ModelState.AddModelError(
                    "",
                    "Incorrect passcode.");

                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "Editor"),
                new Claim(ClaimTypes.Role, "Editor")
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal);

            if (!string.IsNullOrEmpty(returnUrl) &&
                Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        // SWITCH BACK TO VIEW MODE
        [Authorize(Roles = "Editor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SwitchToView()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Home");
        }
    }
}
    