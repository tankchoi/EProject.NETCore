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
            return View("upload_recipe");
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
        public IActionResult UploadRecipe(Recipe recipe, string guid)
        {
            using (var db = new EProjectNetcoreContext())
            {
                recipe.Type = 0;
                recipe.CreatedDate = DateTime.Now;
                db.Recipes.Add(recipe);
                db.SaveChanges();
                string newFolderName = recipe.Id.ToString();
                string imageDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/postedImage", newFolderName);
                Directory.CreateDirectory(imageDirectory);

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
}
