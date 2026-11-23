
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Npgsql;
using portfolio.Models;
using portfolio.Repositories;

namespace portfolio.Controllers
{
    public class CoursesController : Controller
    {
        private readonly CourseRepository _courseRepository;

        public CoursesController(CourseRepository courseRepository)
        {
            _courseRepository = courseRepository;
        }

        // VIEW COURSES - PUBLIC
        public async Task<IActionResult> Index()
        {
            return View(await _courseRepository.GetAllAsync());
        }

        // CREATE - EDITOR ONLY
        [Authorize(Roles = "Editor")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Editor")]
        public async Task<IActionResult> Create(Course course)
        {
            if (!ModelState.IsValid)
                return View(course);

            try
            {
                await _courseRepository.AddAsync(course);
                return RedirectToAction(nameof(Index));
            }
            catch (PostgresException ex) when (ex.SqlState == "23505")
            {
                ModelState.AddModelError("CourseCode",
                    "This course code already exists.");

                return View(course);
            }
        }

        // EDIT - EDITOR ONLY
        [Authorize(Roles = "Editor")]
        public async Task<IActionResult> Edit(long id)
        {
            var course = await _courseRepository.GetByIdAsync(id);

            if (course == null)
                return NotFound();

            return View(course);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Editor")]
        public async Task<IActionResult> Edit(long id, Course course)
        {
            if (id != course.CourseId)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(course);

            try
            {
                await _courseRepository.UpdateAsync(course);
                return RedirectToAction(nameof(Index));
            }
            catch (PostgresException ex) when (ex.SqlState == "23505")
            {
                ModelState.AddModelError("CourseCode",
                    "This course code already exists.");

                return View(course);
            }
        }

        // DELETE - EDITOR ONLY
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Editor")]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                await _courseRepository.DeleteAsync(id);
            }
            catch (PostgresException ex) when (ex.SqlState == "23503")
            {
                TempData["Error"] =
                    "Cannot delete this course because it still has projects.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
