using Microsoft.AspNetCore.Mvc;
using EProjet.NETCore.Models;
using System;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
namespace EProjet.NETCore.Controllers
{
    public class RecipeController : Controller
    {
        [HttpGet("recipe")]
        public async Task<IActionResult> Recipe(int? page, int? pageSize)
        {
            // Check if page or pageSize is not provided, set default values
            if (page == null)
            {
                page = 1;
            }
            if (pageSize == null)
            {
                pageSize = 8;
            }
            // Use context to retrieve the list of recipes with pagination
            using (var db = new EProjectNetcoreContext())
            {
                var list = await db.Recipes
                                    .OrderByDescending(b => b.Id) // Sort recipes by Id in descending order
                                    .Skip((page.Value - 1) * pageSize.Value) // Skip recipes before the current page
                                    .Take(pageSize.Value) // Take the number of recipes according to pageSize
                                    .ToListAsync(); // Convert to list asynchronously
                var totalCount = await db.Recipes.CountAsync();
                var pagedList = new StaticPagedList<Recipe>(list, page.Value, pageSize.Value, totalCount); // Create static paged list
                return View(pagedList);
            }
        }
        [HttpGet("recipe/search")]
        public async Task<IActionResult> Search(string input_search, string input_free, string input_premium, int? page, int? pageSize)
        {
            // Check if page or pageSize is not provided, set default values
            if (page == null)
            {
                page = 1;
            }
            if (pageSize == null)
            {
                pageSize = 8;
            }
            // Use context to retrieve the list of recipes with pagination and search criteria
            using (var db = new EProjectNetcoreContext())
            {
                var query = db.Recipes.AsQueryable();
                // If input_search is not empty, add search condition by Title
                if (!string.IsNullOrEmpty(input_search))
                {
                    query = query.Where(r => r.Title.Contains(input_search));
                }
                // Add search condition by tip type (Free, Premium)
                if (input_free == "1" && input_premium == "2")
                {
                    query = query.Where(r => r.Type == 1 || r.Type == 2);
                }
                else if (input_free == "1")
                {
                    query = query.Where(r => r.Type == 1);
                }
                else if (input_premium == "2")
                {
                    query = query.Where(r => r.Type == 2);
                }
                // Retrieve the list of tips with pagination and applied conditions
                var recipes = await query
                                    .OrderByDescending(b => b.Id)
                                    .Skip((page.Value - 1) * pageSize.Value)
                                    .Take(pageSize.Value)
                                    .ToListAsync();

                var totalCount = await query.CountAsync();
                var pagedList = new StaticPagedList<Recipe>(recipes, page.Value, pageSize.Value, totalCount);
                // Store value in ViewBag
                ViewBag.InputSearch = input_search;
                ViewBag.InputFree = input_free;
                ViewBag.InputPremium = input_premium;

                return View("Recipe", pagedList);
            }
        }


        [HttpGet]
        public IActionResult UploadRecipe()
        {
            string guid = Guid.NewGuid().ToString();
            ViewData["Guid"] = guid;
            return View("upload_recipe");
        }
        [HttpPost]
        public async Task<JsonResult> UploadFile(IFormFile uploadedFiles, string guid)
        {
            string returnImagePath = string.Empty;
            string fileName;
            string extension;
            string imageName;
            string imageSavePath;

            if (uploadedFiles != null && uploadedFiles.Length > 0)
            {
                // Tạo thư mục tạm theo GUID
                string tempFolderName = guid;
                string imageDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/postedImage", tempFolderName);
                Directory.CreateDirectory(imageDirectory);

                fileName = Path.GetFileNameWithoutExtension(uploadedFiles.FileName);
                extension = Path.GetExtension(uploadedFiles.FileName);
                imageName = fileName + DateTime.Now.ToString("yyyyMMddHHmmss");
                imageSavePath = Path.Combine(imageDirectory, imageName + extension);

                using (var stream = new FileStream(imageSavePath, FileMode.Create))
                {
                    await uploadedFiles.CopyToAsync(stream);
                }

                returnImagePath = "/postedImage/" + tempFolderName + "/" + imageName + extension;
            }

            return Json(new { path = returnImagePath });
        }
        public List<string> ExtractImageUrlsFromHtml(string htmlContent)
        {
            List<string> imageUrls = new List<string>();

            // Regex pattern để tìm các đường dẫn ảnh
            string pattern = @"<img.*?src=(['""])(.*?)\1.*?>";

            MatchCollection matches = Regex.Matches(htmlContent, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);

            foreach (Match match in matches)
            {
                string imageUrl = match.Groups[2].Value;
                imageUrls.Add(imageUrl);
            }

            return imageUrls;
        }
        [HttpPost]
        public IActionResult DeleteTempImages([FromBody] DeleteImagesRequest request)
        {
            try
            {
                string guid = request.Guid;
                string imageDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "postedImage", guid);

                if (Directory.Exists(imageDirectoryPath))
                {
                    Directory.Delete(imageDirectoryPath, true);
                    return Ok(new { message = "Images deleted successfully." });
                }
                else
                {
                    return NotFound(new { message = "Image directory not found." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error deleting images: {ex.Message}" });
            }
        }

        public class DeleteImagesRequest
        {
            public string Guid { get; set; }
        }
        [HttpPost]
        public async Task<IActionResult> UploadRecipe(Recipe recipe, string guid, IFormFile recipeImg)
        {

            ModelState.Remove("recipeImg");
            ModelState.Remove("Img");
            if (recipeImg == null)
            {
                ModelState.AddModelError("recipeImg", "Vui lòng tải lên một hình ảnh.");
            }
            if (recipe.Content == "<p><br></p>")
            {
                ModelState.AddModelError("content", "Vui lòng không để trống nội dung.");
            }

            // Nếu có lỗi trong ModelState, trả về trang hiện tại với các lỗi
            if (!ModelState.IsValid)
            {
                ViewData["Guid"] = guid;
                ViewData["Fullname"] = recipe.Fullname;
                ViewData["Email"] = recipe.Email;
                ViewData["Title"] = recipe.Title;
                ViewData["Content"] = recipe.Content;

                return View("upload_recipe");
            }
            using (var db = new EProjectNetcoreContext())
            {
                recipe.Type = 0;
                recipe.CreatedDate = DateTime.Now;
                recipe.Img = "1";
                db.Recipes.Add(recipe);
                db.SaveChanges();
                // Tạo thư mục chứa ảnh recipe
                string newFolderName = recipe.Id.ToString();
                string imageDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/postedImage", newFolderName);
                Directory.CreateDirectory(imageDirectory);
                // Tải ảnh bìa của recipe lên server và lưu đường dẫn vào csdl
                string fileName;
                string extension;
                string recipeImgName;
                string imageSavePath;
                fileName = Path.GetFileNameWithoutExtension(recipeImg.FileName);
                extension = Path.GetExtension(recipeImg.FileName);
                recipeImgName = fileName + DateTime.Now.ToString("yyyyMMddHHmmss");
             
                imageSavePath = Path.Combine(imageDirectory, recipeImgName + extension);
                recipe.Img = "/postedImage/" + recipe.Id.ToString() + "/" + recipeImgName + extension;
                
                using (var stream = new FileStream(imageSavePath, FileMode.Create))
                {
                    await recipeImg.CopyToAsync(stream);
                }
         
          
                // Trích xuất đường dẫn ảnh từ nội dung bài đăng
                List<string> imageUrls = ExtractImageUrlsFromHtml(recipe.Content);

                // Di chuyển ảnh từ thư mục tạm sang thư mục mới
                string tempImageDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/postedImage", guid);
                if (Directory.Exists(tempImageDirectory))
                {
                    string[] tempImages = Directory.GetFiles(tempImageDirectory);
                    foreach (var tempImage in tempImages)
                    {
                        string imageName = Path.GetFileName(tempImage);
                        string newImagePath = Path.Combine(imageDirectory, imageName);

                        // Kiểm tra xem ảnh có trong danh sách imageUrls không
                        if (imageUrls.Any(url => url.Contains(imageName)))
                        {
                            // Di chuyển ảnh
                            System.IO.File.Move(tempImage, newImagePath);

                            // Cập nhật đường dẫn trong nội dung bài đăng
                            recipe.Content = recipe.Content.Replace($"/postedImage/{guid}/{imageName}", $"/postedImage/{newFolderName}/{imageName}");
                        }
                        else
                        {
                            // Nếu không có trong danh sách, xóa tệp tin tạm
                            System.IO.File.Delete(tempImage);
                        }
                    }

                    // Xóa thư mục tạm
                    Directory.Delete(tempImageDirectory, true);
                }

                // Lưu thay đổi vào cơ sở dữ liệu
                db.SaveChanges();
            }

            return RedirectToAction("Recipe");
        }
    }
}

