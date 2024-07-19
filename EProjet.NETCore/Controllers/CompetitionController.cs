using EProjet.NETCore.Models;
using Microsoft.AspNetCore.Mvc;
using X.PagedList;
using Microsoft.EntityFrameworkCore;

namespace EProjet.NETCore.Controllers
{
    public class CompetitionController : Controller
    {
        [HttpGet("competition")]
        public async Task<IActionResult> Competition(int? page, int? pageSize)
        {
            // Check if page or pageSize is not provided, set default values
            if (page == null)
            {
                page = 1;
            }
            if (pageSize == null)
            {
                pageSize = 5;
            }
            // Use context to retrieve the list of tips with pagination
            List<Competition> list;
            using (var db = new EProjectNetcoreContext())
            {
                list = await db.Competitions
                                .OrderByDescending(b => b.Id) // Sort tips by Id in descending order
                                .Skip((page.Value - 1) * pageSize.Value)  // Skip tips before the current page
                                .Take(pageSize.Value) // Take the number of tips according to pageSize
                                .ToListAsync(); // Convert to list asynchronously
                var totalCount = await db.Competitions.CountAsync();
                var pagedList = new StaticPagedList<Competition>(list, page.Value, pageSize.Value, totalCount); // Create static paged list
                return View(pagedList);
            }
        }

        public IActionResult newCompetition()
        {
            return View();
        }
        public IActionResult detailCompetition()
        {
            return View();
        }

        public IActionResult detailSubmission()
        {
            return View();
        }
    }
}
