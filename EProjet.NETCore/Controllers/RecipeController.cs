using Microsoft.AspNetCore.Mvc;
using EProjet.NETCore.Models;
using System;
using System.Text.RegularExpressions;
namespace EProjet.NETCore.Controllers
{
    public class RecipeController : Controller
    {
        [HttpGet("recipe")]
        public IActionResult Recipe()
        {
            return View();
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

            ModelState.Clear();
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

