using EcommerceAppCore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcommerceAppCore.Controllers
{
    public class FinanceController : Controller
    {
        private readonly EcommerceDbContext _context;

        public FinanceController(EcommerceDbContext context)
        {
            _context = context;
        }

        //Activity Log Details Method for Tracking Activity Data
        public void ActivityLogDetails(string userId, string action, string table, int? tid, string details)
        {
            ActivityLog log = new ActivityLog()
            {
                UserId = userId,
                ActivityDateAndTime = DateTime.Now,
                Action = action,
                TableEffected = table,
                TableId = tid,
                Details = details
            };
            _context.ActivityLogs.Add(log);
            _context.SaveChanges();
        }

        //Error log Details method for storing error logs
        public void ErrorLogDetails(Exception ex, string ActionMethodName)
        {
            ErrorLog logs = new ErrorLog()
            {
                UserId = HttpContext.Session.GetString("uid"),
                ErrorActionMethod = ActionMethodName,
                ErrorCode = ex.HResult,
                ErrorMessage = ex.Message,
                StackTrace = ex.StackTrace,
                DateAndTime = DateTime.Now
            };
            _context.ErrorLogs.Add(logs);
            _context.SaveChanges();
        }

        //Finance home page action method
        public IActionResult FinaceHomePage()
        {
            return View();
        }

        //Action method for displaying Sales between choosen dates
        public async Task<IActionResult> ViewSales(SalesViewModel sales)
        {
            try
            {
                if (sales.StartDate != null && sales.EndDate != null)
                {
                    sales.FilteredSales = await _context.PurchaseAuditLogs.Where(s => s.OrderDateAndTime >= sales.StartDate.Value && s.OrderDateAndTime <= sales.EndDate.Value).ToListAsync();
                }
                return View(sales);
            }
            catch (Exception ex)
            {
                ErrorLogDetails(ex, "ViewSales");
                throw; 
            }
        }

        //Action method for displaying user details
        public async Task<IActionResult> FinanceProfile(string? id)
        {
            try
            {
                id = HttpContext.Session.GetString("uid");
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound();
                }
                return View(user);
            }
            catch (Exception ex)
            {
                ErrorLogDetails(ex, "FinanceProfile");
                throw;
            }
        }
    }
}
