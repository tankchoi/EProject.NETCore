using EProjet.NETCore.Models;
using Microsoft.AspNetCore.Mvc;
using X.PagedList;
using Microsoft.EntityFrameworkCore;

namespace EProjet.NETCore.Controllers
{
    public class TipController : Controller
    {
        [HttpGet("tip")]
        public async Task<IActionResult> Tip(int? page, int? pageSize)
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

            // Use context to retrieve the list of tips with pagination
            using (var db = new EProjectNetcoreContext())
            {
                var list = await db.Tips
                                    .OrderByDescending(b => b.Id) // Sort tips by Id in descending order
                                    .Skip((page.Value - 1) * pageSize.Value) // Skip tips before the current page
                                    .Take(pageSize.Value) // Take the number of tips according to pageSize
                                    .ToListAsync(); // Convert to list asynchronously
                var totalCount = await db.Tips.CountAsync();
                var pagedList = new StaticPagedList<Tip>(list, page.Value, pageSize.Value, totalCount); // Create static paged list
                return View(pagedList);
            }
        }

        [HttpGet("tip/search")]
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

            // Use context to retrieve the list of tips with pagination and search criteria
            using (var db = new EProjectNetcoreContext())
            {
                var query = db.Tips.AsQueryable();
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
                var tips = await query
                                    .OrderByDescending(b => b.Id)
                                    .Skip((page.Value - 1) * pageSize.Value)
                                    .Take(pageSize.Value)
                                    .ToListAsync();

                var totalCount = await query.CountAsync();
                var pagedList = new StaticPagedList<Tip>(tips, page.Value, pageSize.Value, totalCount);

                // Store value in ViewBag
                ViewBag.InputSearch = input_search;
                ViewBag.InputFree = input_free;
                ViewBag.InputPremium = input_premium;

                return View("Tip", pagedList);
            }
        }
    } 
}
