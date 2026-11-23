
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Npgsql;
using portfolio.Models;
using portfolio.Repositories;
using System.Text.RegularExpressions;

namespace portfolio.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly ProjectRepository _projectRepository;
        private readonly ProjectImageRepository _imageRepository;
        private readonly CourseRepository _courseRepository;

        public ProjectsController(
            ProjectRepository projectRepository,
            ProjectImageRepository imageRepository,
            CourseRepository courseRepository)
        {
            _projectRepository = projectRepository;
            _imageRepository = imageRepository;
            _courseRepository = courseRepository;
        }

        // VIEW PROJECTS - PUBLIC
        public async Task<IActionResult> Index()
        {
            var projects = await _projectRepository.GetAllAsync();

            foreach (var project in projects)
            {
                project.Images =
                    await _imageRepository.GetByProjectIdAsync(project.ProjectId);
            }

            return View(projects);
        }

        // VIEW PROJECT DETAILS - PUBLIC
        public async Task<IActionResult> Details(long id)
        {
            var project = await _projectRepository.GetByIdAsync(id);

            if (project == null)
                return NotFound();

            project.Images =
                await _imageRepository.GetByProjectIdAsync(id);

            return View(project);
        }

        private async Task LoadCoursesAsync()
        {
            var courses = await _courseRepository.GetAllAsync();

            ViewBag.Courses = new SelectList(
                courses,
                "CourseId",
                "CourseCode");
        }

        private List<string> GetImageUrls(string? imageUrls)
        {
            return (imageUrls ?? string.Empty)
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(url => url.Trim())
                .Where(url => !string.IsNullOrWhiteSpace(url))
                .ToList();
        }

        private void ValidateImageUrls(List<string> imageUrls)
        {
            if (imageUrls.Count == 0)
            {
                ModelState.AddModelError(
                    "ImageUrls",
                    "Enter at least one image URL.");
            }

            for (int i = 0; i < imageUrls.Count; i++)
            {
                if (!Regex.IsMatch(
                    imageUrls[i],
                    @"^https?://[^\s]+$",
                    RegexOptions.IgnoreCase))
                {
                    ModelState.AddModelError(
                        "ImageUrls",
                        $"Image URL on line {i + 1} is invalid.");
                }
            }
        }

        // CONVERT YOUTUBE URL TO EMBED URL
        private string? NormalizeVideoUrl(string? videoUrl)
        {
            if (string.IsNullOrWhiteSpace(videoUrl))
                return null;

            if (!Uri.TryCreate(
                videoUrl.Trim(),
                UriKind.Absolute,
                out Uri? uri))
            {
                ModelState.AddModelError(
                    "VideoUrl",
                    "Enter a valid YouTube URL.");

                return videoUrl;
            }

            string host = uri.Host.ToLowerInvariant();

            bool isYouTube =
                host == "youtube.com" ||
                host.EndsWith(".youtube.com") ||
                host == "youtu.be" ||
                host == "youtube-nocookie.com" ||
                host.EndsWith(".youtube-nocookie.com");

            if (!isYouTube)
            {
                ModelState.AddModelError(
                    "VideoUrl",
                    "Please enter a YouTube URL.");

                return videoUrl;
            }

            string[] parts = uri.AbsolutePath
                .Trim('/')
                .Split('/', StringSplitOptions.RemoveEmptyEntries);

            string? videoId = null;

            if (host == "youtu.be")
            {
                if (parts.Length > 0)
                    videoId = parts[0];
            }
            else if (parts.Length > 0 &&
                     parts[0].Equals(
                         "watch",
                         StringComparison.OrdinalIgnoreCase))
            {
                foreach (string item in uri.Query.TrimStart('?').Split('&'))
                {
                    string[] pair = item.Split('=', 2);

                    if (pair.Length == 2 &&
                        pair[0].Equals(
                            "v",
                            StringComparison.OrdinalIgnoreCase))
                    {
                        videoId = Uri.UnescapeDataString(pair[1]);
                        break;
                    }
                }
            }
            else if (parts.Length > 1 &&
                     (parts[0].Equals("shorts", StringComparison.OrdinalIgnoreCase) ||
                      parts[0].Equals("live", StringComparison.OrdinalIgnoreCase) ||
                      parts[0].Equals("embed", StringComparison.OrdinalIgnoreCase) ||
                      parts[0].Equals("v", StringComparison.OrdinalIgnoreCase)))
            {
                videoId = parts[1];
            }

            if (string.IsNullOrWhiteSpace(videoId) ||
                !Regex.IsMatch(videoId, @"^[A-Za-z0-9_-]{11}$"))
            {
                ModelState.AddModelError(
                    "VideoUrl",
                    "Invalid or unsupported YouTube video URL.");

                return videoUrl;
            }

            return $"https://www.youtube-nocookie.com/embed/{videoId}";
        }

        // CREATE PROJECT - EDITOR ONLY
        [Authorize(Roles = "Editor")]
        public async Task<IActionResult> Create()
        {
            await LoadCoursesAsync();

            return View(new ProjectCreateViewModel());
        }

        [Authorize(Roles = "Editor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProjectCreateViewModel model)
        {
            var imageUrls = GetImageUrls(model.ImageUrls);

            ValidateImageUrls(imageUrls);

            model.VideoUrl = NormalizeVideoUrl(model.VideoUrl);

            if (!ModelState.IsValid)
            {
                await LoadCoursesAsync();
                return View(model);
            }

            try
            {
                await _projectRepository.AddWithImagesAsync(
                    model,
                    imageUrls);

                TempData["Success"] = "Project added successfully!";

                return RedirectToAction(nameof(Index));
            }
            catch (PostgresException ex) when (ex.SqlState == "23505")
            {
                ModelState.AddModelError(
                    "ImageUrls",
                    "A duplicate image URL was detected. No project or images were saved.");
            }
            catch (PostgresException ex) when (ex.SqlState == "23503")
            {
                ModelState.AddModelError(
                    "CourseId",
                    "The selected course does not exist.");
            }

            await LoadCoursesAsync();

            return View(model);
        }

        // EDIT PROJECT - EDITOR ONLY
        [Authorize(Roles = "Editor")]
        public async Task<IActionResult> Edit(long id)
        {
            var project = await _projectRepository.GetByIdAsync(id);

            if (project == null)
                return NotFound();

            var images = await _imageRepository.GetByProjectIdAsync(id);

            var model = new ProjectEditViewModel
            {
                ProjectId = project.ProjectId,
                CourseId = project.CourseId,
                ProjectTitle = project.ProjectTitle,
                Description = project.Description,
                VideoUrl = project.VideoUrl,
                ImageUrls = string.Join(
                    Environment.NewLine,
                    images.Select(image => image.ImageUrl))
            };

            await LoadCoursesAsync();

            return View(model);
        }

        [Authorize(Roles = "Editor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            long id,
            ProjectEditViewModel model)
        {
            if (id != model.ProjectId)
                return BadRequest();

            var imageUrls = GetImageUrls(model.ImageUrls);

            ValidateImageUrls(imageUrls);

            model.VideoUrl = NormalizeVideoUrl(model.VideoUrl);

            if (!ModelState.IsValid)
            {
                await LoadCoursesAsync();
                return View(model);
            }

            try
            {
                await _projectRepository.UpdateWithImagesAsync(
                    model,
                    imageUrls);

                TempData["Success"] = "Project updated successfully!";

                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }
            catch (PostgresException ex) when (ex.SqlState == "23505")
            {
                ModelState.AddModelError(
                    "ImageUrls",
                    "A duplicate image URL was detected. No changes were saved.");
            }
            catch (PostgresException ex) when (ex.SqlState == "23503")
            {
                ModelState.AddModelError(
                    "CourseId",
                    "The selected course does not exist.");
            }

            await LoadCoursesAsync();

            return View(model);
        }

        // DELETE PROJECT - EDITOR ONLY
        [Authorize(Roles = "Editor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                await _projectRepository.DeleteAsync(id);

                TempData["Success"] = "Project deleted successfully!";
            }
            catch (PostgresException)
            {
                TempData["Error"] =
                    "Unable to delete this project.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
    