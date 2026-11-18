
using Microsoft.AspNetCore.Mvc;
using portfolio.Models;
using portfolio.Data;
using System.Diagnostics;

namespace portfolio.Controllers
{
    public class HomeController : Controller
    {
        private readonly DatabaseTest _databaseTest;

        public HomeController(DatabaseTest databaseTest)
        {
            _databaseTest = databaseTest;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> TestConnection()
        {
            try
            {
                bool connected = await _databaseTest.TestConnectionAsync();

                if (connected)
                {
                    return Content("SUCCESS: Connected to Supabase!");
                }

                return Content("FAILED: Database connection unsuccessful.");
            }
            catch
            {
                return Content("FAILED: Could not connect to Supabase. Check your connection settings.");
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
