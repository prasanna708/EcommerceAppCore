using Azure;
using EcommerceAppCore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Data;
using System.Text;
using static EcommerceAppCore.Models.ActivityViewModel;

namespace EcommerceAppCore.Controllers
{
    public class AdminController : Controller
    {
        private readonly EcommerceDbContext _context;
        private readonly HttpClient _httpClient;
        //private readonly ApiService _apiService;
        private const string apiUrl = "http://localhost:5136/api/products";
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AdminController(EcommerceDbContext context, HttpClient httpClient, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _httpClient = httpClient;
            //_apiService = apiService;
            _webHostEnvironment = webHostEnvironment;
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


        //Data log Details method
        public void DataLogDetails(string table, int? id, string property, string oldValue, string newValue, string userId)
        {
            DataLog log = new DataLog()
            {
                TableEffected = table,
                PropertyId = id,
                PropertyEffected = property,
                OldValue = oldValue,
                NewValue = newValue,
                UserId = userId,
                ActivityDateAndTime = DateTime.Now
            };
            _context.DataLogs.Add(log);
            _context.SaveChanges();
        }

        //Home Page
        public IActionResult Home()
        {
            return View();
        }

        //Login Action Method(GET)
        public async Task<IActionResult> Login()
        {
            return View(new User());
        }

        //Login Action Method(POST)
        [HttpPost]
        public async Task<IActionResult> Login(User user)
        {
            try
            {
                var loginUser = await _context.Users.Where(u => u.UserId == user.UserId && u.Pwd == user.Pwd).FirstOrDefaultAsync();
                HttpContext.Session.SetString("uid", loginUser.UserId);
                HttpContext.Session.SetString("Name", loginUser.Name);
                string details = "User " + loginUser.Name + " logged in.";
                ActivityLogDetails(loginUser.UserId, "Login", "User", 0, details);
                if (loginUser == null)
                {
                    TempData["error"] = "Login failed....";
                    return View(user);
                }
                else
                {
                    TempData["success"] = "Login successful....";
                    if (loginUser.Role == "Admin")
                    {
                        return RedirectToAction("AdminHomePage" , "Admin");
                    }
                    if (loginUser.Role == "User")
                    {
                        return RedirectToAction("UserHomePage" , "User");
                    }
                    if (loginUser.Role == "Finance")
                    {
                        return RedirectToAction("FinaceHomePage", "Finance");
                    }
                    return View(user);
                }
            }
            catch (Exception ex)
            {
                ErrorLogDetails(ex, "Login");
                ViewBag.message =  ex.Message;
                return NotFound();
            }
        }

        //Registration Action Method(GET)
        public async Task<IActionResult> Registration()
        {
            return View();
        }

        //Registration Action Method(POST)
        [HttpPost]
        public async Task<IActionResult> Registration(User user)
        {
            try
            {
                if (user == null)
                    return BadRequest();
                user.IsActive = true;
                _context.Users.Add(user);   
                await _context.SaveChangesAsync();
                TempData["success"] = "Registration successful.";
                string details = $"{user.Name} Registered successfully..";
                ActivityLogDetails(user.UserId, "Registration", "Admin", 0, details);
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                ErrorLogDetails(ex, "Registration");
                throw ex;
            }
        }

        //Admin Home Page Action Method
        public IActionResult AdminHomePage()
        {
            var name = HttpContext.Session.GetString("Name");
            ViewData["name"] = name;   
            return View(new User());
        }

        //Displaying All Products from API
        public async Task<IActionResult> GetProducts(bool status)
        {
            try
            {
                var response = await _httpClient.GetAsync(apiUrl + $"?status={status}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var productlist = JsonConvert.DeserializeObject<List<Product>>(content);
                    return View(productlist);
                }
                return View();
            }
            catch (Exception ex)
            {
                ErrorLogDetails(ex, "GetProducts");
                throw ex;
            }
        }

        //Displaying Product by ID From API
        public async Task<IActionResult> GetProductByID(int id,bool status)
        {
            try
            {
                var response = await _httpClient.GetAsync(apiUrl + $"/{id}?status={status}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var productlist = JsonConvert.DeserializeObject<Product>(content);
                    string details = "Product" +  productlist.Name + " viewed.";
                    string? uid = HttpContext.Session.GetString("Name");
                    ActivityLogDetails(uid, "Viewed", "Products", productlist.ProductId , details);
                    return View(productlist);
                }

                return NotFound();
            }
            catch (Exception ex)
            {
                ErrorLogDetails(ex, "GetProductByID");
                throw ex;
            }
        }

        //Adding new Product(GET)
        public async Task<IActionResult> AddProduct()
        {
            return View();
        }

        //Adding new Product(POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProduct(Product product, IFormFile file)
        {
            try
            {
                if (ModelState.IsValid) 
                {
                    if (file != null && file.Length > 0)
                    {
                        var folderPath = Path.Combine(_webHostEnvironment.WebRootPath , "Uploads");
                        if (!Directory.Exists(folderPath))
                        {
                            Directory.CreateDirectory(folderPath);
                        }

                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        var filePath = Path.Combine(folderPath, fileName);
                        using (var fileStream = new FileStream(filePath , FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }

                        product.Photo = Path.Combine("Uploads", fileName);                       
                    }
                    var content = new StringContent(JsonConvert.SerializeObject(product), Encoding.UTF8, "application/json");
                    var response = await _httpClient.PostAsync(apiUrl , content);
                    TempData["success"] = "Product Added successfully...";
                    string details = "Product" + product.Name + " added.";
                    string? uid = HttpContext.Session.GetString("Name");
                    ActivityLogDetails(uid, "Insert", "Products", product.ProductId, details);
                    return RedirectToAction("GetProducts", "Admin", new { status = true });
                }
                return View(product);
            }
            catch (Exception ex)
            {
                ErrorLogDetails(ex, "AddProduct");
                throw;
            }
            
        }

        //Updating existing Product(GET)
        public async Task<IActionResult> EditProduct(int id, bool status)
        {
            try
            {
                var response = await _httpClient.GetStringAsync(apiUrl + $"/{id}?status={status}");
                var product = JsonConvert.DeserializeObject<Product>(response);
                return View(product);
            }
            catch (Exception ex)
            {
                ErrorLogDetails(ex, "EditProduct");
                throw ex;
            }
        }

        //Updating existing Product(GET)
        [HttpPost]
        public async Task<IActionResult> EditProduct(int id, bool status,Product product,IFormFile file)
        {
            try
            {
                var oldProduct = _context.Products.Find(id);
                string oldName = oldProduct.Name;
                double? oldPrice = oldProduct.Price;
                int? oldQuantity = oldProduct.Quantity;
                if (file != null && file.Length > 0)
                {
                    var folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "Uploads");
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    var filePath = Path.Combine(folderPath, fileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }
                    product.Photo = Path.Combine("Uploads", fileName);
                }
                var content = new StringContent(JsonConvert.SerializeObject(product), Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync(apiUrl + $"/{id}?status={status}", content);
                if (response.IsSuccessStatusCode)
                {
                    TempData["success"] = "Product updated successfully...";
                    string details;
                    string? uid = HttpContext.Session.GetString("uid");
                    //ActivityLogDetails(uid, "Update", "Products", product.ProductId, details);
                    if (oldName != null && product.Name != null && oldName != product.Name)
                    {
                        details = "Product " + oldName + " Updated.";
                        ActivityLogDetails(uid, "Update", "Products", product.ProductId, details);
                        DataLogDetails("Products", product.ProductId, "Name", oldName, product.Name, uid);
                    }
                    if (oldPrice != null && product.Price != null && oldPrice != product.Price)
                    {
                        details = "Price " + oldPrice + " Updated.";
                        ActivityLogDetails(uid, "Update", "Products", product.ProductId, details);
                        DataLogDetails("Products", product.ProductId, "Price", oldPrice.ToString(), product.Price.ToString(), uid);
                    }
                    if (oldQuantity != null && product.Quantity != null && oldQuantity != product.Quantity)
                    {
                        details = "Quantity " + oldQuantity + " Updated.";
                        ActivityLogDetails(uid, "Update", "Products", product.ProductId, details);
                        DataLogDetails("Products", product.ProductId, "Quantity", oldQuantity.ToString(), product.Quantity.ToString(), uid);
                    }
                    return RedirectToAction("GetProducts","Admin" , new { status = true });
                }
                return View();
            }
            catch (Exception ex)
            {
                ErrorLogDetails(ex, "EditProduct");
                throw ex;
            }
        }

        //Deleting Existing Product
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var product = _context.Products.Find(id);
                var response = await _httpClient.DeleteAsync(apiUrl + $"/{id}");
                if (response.IsSuccessStatusCode)
                {
                    TempData["success"] = "Product deleted successfully...";
                    string details = "Product" + product.Name + " viewed.";
                    string? uid = HttpContext.Session.GetString("Name");
                    ActivityLogDetails(uid, "Delete", "Products", product.ProductId, details);
                    return RedirectToAction("GetProducts", "Admin", new { status = true });
                }
                return View();
            }
            catch (Exception ex)
            {
                ErrorLogDetails(ex, "DeleteProduct");
                throw ex;
            }
        }

        //Displaying all Orders Placed by All Users
        public async Task<IActionResult> ViewAllOrders()
        {
            try
            {
                var orders =await _context.PurchaseAuditLogs.ToListAsync();
                var total = orders.Sum(o => o.TotalPrice);
                ViewBag.Total = Convert.ToInt32(total);
                return View(orders);
            }
            catch (Exception ex)
            {
                ErrorLogDetails(ex, "ViewOrders");
                throw ex;
            }
        }

        //Admin Profile Action Method
        public async Task<IActionResult> AdminProfile(string? id)
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
                ErrorLogDetails(ex, "UserProfile");
                throw ex;
            }
        }

        //Action method for displaying Today's sales
        public async Task<IActionResult> TodaySales()
        {
            try
            {
                DateTime date = DateTime.Now.AddDays(1);
                var obj = await _context.PurchaseAuditLogs.Where(o => o.OrderDateAndTime >= DateTime.Today && o.OrderDateAndTime < date).ToListAsync();
                var sum = obj.Sum(o => o.TotalPrice);
                ViewBag.TotalSales = Convert.ToInt32(sum);
                return View(obj);
            }
            catch (Exception ex)
            {
                ErrorLogDetails(ex, "TodaySales");
                throw;
            }
        } 

        //Action method for displaying products stock left 
        public async Task<IActionResult> StockLeft(bool? status)
        {
            try
            {
                var response = await _httpClient.GetAsync(apiUrl + $"?status={status}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var productlist = JsonConvert.DeserializeObject<List<Product>>(content);
                    return View(productlist);
                }
                return View();
            }
            catch (Exception ex)
            {
                ErrorLogDetails(ex, "StockLeft");
                throw;
            }
        }

        //Displaying all users
        public async Task<IActionResult> ViewUsers()
        {
            try
            {
                var users = await _context.Users.ToListAsync();
                return View(users);
            }
            catch (Exception ex)
            {
                ErrorLogDetails(ex, "ViewUsers");
                throw;
            }
        }

        //Activity log Details Action Method
        public async Task<IActionResult> ActivityDetails(int page = 1, int pageSize = 10)
        {
            try
            {
                var totalRecords = await _context.ActivityLogs.CountAsync();
                var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
                if (page < 1)
                {
                    page = 1;
                }
                if (page > totalPages)
                {
                    page = totalPages;
                }
                var logs = await _context.ActivityLogs.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
                var model = new ActivityViewModel
                {
                    Logs = logs,
                    CurrentPage = page,
                    TotalPages = totalPages,
                    PageSize = pageSize
                };

                return View(model);
            }
            catch (Exception ex)
            {
                ErrorLogDetails(ex, "ActivityDetails");
                throw;
            }
        }

        public async Task<IActionResult> GetDataLogDetails(string? id)
        {
            try
            {
                var log = await _context.DataLogs.FirstOrDefaultAsync(l => l.UserId == id);
                if (log == null)
                {
                    return Content("No DataLog found for this activity.");
                }

                // Render a partial view with the DataLog details
                return PartialView("_DataLogDetails", log);
            }
            catch (Exception ex)
            {
                ErrorLogDetails(ex, "GetDataLogDetails");
                throw;
            }
        }
    }

}
