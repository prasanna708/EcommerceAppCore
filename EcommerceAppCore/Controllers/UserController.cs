using EcommerceAppCore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace EcommerceAppCore.Controllers
{
    public class UserController : Controller
    {
        private readonly EcommerceDbContext _context;
        private readonly HttpClient _httpClient;
        private const string apiUrl = "http://localhost:5136/api/products";
        public UserController(EcommerceDbContext context , HttpClient httpClient)
        {
            _context = context;
            _httpClient = httpClient;
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


        //Data log Details Method
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


        //Action method for user home page
        public IActionResult UserHomePage()
        {
            var name = HttpContext.Session.GetString("Name");
            ViewData["name"] = name;
            return View();
        }

        //User Profile Action Method
        public IActionResult UserProfile(string? id)
        {
            try
            {
                id = HttpContext.Session.GetString("uid");
                var user = _context.Users.Find(id);
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

        //Displaying All Products From API
        public async Task<IActionResult> DisplayProducts(bool status)
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
        public async Task<IActionResult> DisplayProductByID(int id, bool status)
        {
            try
            {
                var response = await _httpClient.GetAsync(apiUrl + $"/{id}?status={status}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var productlist = JsonConvert.DeserializeObject<Product>(content);
                    string details = "Product" + productlist.Name + " viewed.";
                    string? uid = HttpContext.Session.GetString("Name");
                    ActivityLogDetails(uid, "Viewed", "Products", productlist.ProductId, details);
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


        //Adding Product to Cart(GET)
        public async Task<IActionResult> AddToCart(int id, bool status)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                Order order = new Order
                {
                    ProductId = id,
                    Name = product.Name,
                    Photo = product.Photo,
                    UserId = HttpContext.Session.GetString("uid")
                };
                return View(order);
            }
            catch (Exception ex)
            {
                ErrorLogDetails(ex, "AddToCart");
                throw;
            }
        }

        //Adding Product to Cart(POST)
        [HttpPost]
        public async Task<IActionResult> AddToCart(Product prod,Order order,string? id)
        {
            try
            {
                var product = _context.Products.FirstOrDefault(p => p.ProductId == prod.ProductId);
                id = HttpContext.Session.GetString("uid");
                if (product != null)
                {
                    order.ProductId = product.ProductId;
                    order.Name = product.Name;
                    order.OrderDateAndTime = DateTime.Now;
                    order.UserId = id;
                    order.TotalPrice = order.Quantity * product.Price;
                    order.Photo = product.Photo;
                    _context.Orders.Add(order);
                    _context.SaveChanges();
                    TempData["success"] = "Product added to Cart Successfully....";
                    string details = $"{order.Name} Added to Cart.";
                    ActivityLogDetails(id, "AddToCart", "Orders", order.OrderId, details);
                    return RedirectToAction("DisplayProducts" , "User" , new { status = true });
                }
                else
                    TempData["error"] = "Product not found....";
                return View(order);
            }
            catch (Exception ex)
            {
                ErrorLogDetails(ex, "AddToCart");
                throw ex;
            }
        }

        //Viewing Products that are added to Cart
        public async Task<IActionResult> ViewCart(string? id,bool status)
        {
            try
            {
                id = HttpContext.Session.GetString("uid");
                ViewData["uid"] = id;
                var orders = await _context.Orders.Where(p => p.UserId == id && p.IsActive == status).ToListAsync();
                string details = "Cart Products viewed.";
                string? uid = HttpContext.Session.GetString("Name");
                ActivityLogDetails(uid, "Viewed", "Products", 0 , details);
                return View(orders);
            }
            catch (Exception ex)
            {
                ErrorLogDetails(ex, "ViewCart");
                throw ex;
            }
        }

        //Deleting Orders that are added to Cart
        public async Task<IActionResult> DeleteOrders(int id)
        {
            try
            {
                var order = await _context.Orders.Where(o => o.OrderId == id).FirstOrDefaultAsync();
                order.IsActive = false;
                _context.SaveChanges();
                string? uid = HttpContext.Session.GetString("uid");
                TempData["success"] = "Order Cancelled....";
                string details = $"{order.Name} deleted from Cart.";
                ActivityLogDetails(uid, "DeleteCartItem", "Orders", id, details);
                return RedirectToAction("ViewCart" , "User" , new { status = true } );
            }
            catch (Exception ex)
            {
                ErrorLogDetails(ex, "DeleteOrders");
                throw;
            }
        }

        //Placing Order of Products that are added to cart
        public async Task<IActionResult> PlaceOrder(string? id)
        {
            try
            {
                id = HttpContext.Session.GetString("uid");
                var order = await _context.Orders.Where(o => o.UserId == id && o.IsActive == true).ToListAsync();
                foreach (var item in order)
                {
                    PurchaseAuditLog logs = new PurchaseAuditLog()
                    {
                        OrderId = item.OrderId,
                        ProductId = item.ProductId,
                        UserId = id,
                        ProductName = item.Name,
                        OrderDateAndTime = DateTime.Now,
                        Quantity = item.Quantity,
                        TotalPrice = item.TotalPrice,
                        Photo = item.Photo,
                    };
                    _context.PurchaseAuditLogs.Add(logs);

                    var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == item.ProductId);
                    if (product != null)
                    {
                        product.Quantity = product.Quantity - item.Quantity;
                        if (product.Quantity < 0)
                        {
                            product.Quantity = 0;
                        }
                    }
                    item.IsActive = false;
                    _context.Products.Update(product);
                    //_context.Entry(product).State = EntityState.Modified;
                    _context.SaveChanges();
                    string details = "Order Placed.";
                    ActivityLogDetails(id, "PlacingOrder", "PurchaseAudit", logs.OrderNumber, details);

                }
                //_context.Orders.RemoveRange(order);
                
                //_context.SaveChanges();
                TempData["success"] = "Order Placed Successfully....";
                return RedirectToAction("ViewCart" , "User" , new { status = true } );
            }
            catch (Exception ex)
            {
                ErrorLogDetails(ex, "PlaceOrder");
                throw ex;
            }
        }

        //Displaying Purchased Orders 
        public async Task<IActionResult> ViewOrders(string? id)
        {
            try
            {
                id = HttpContext.Session.GetString("uid");
                var purchases = await _context.PurchaseAuditLogs.Where(p => p.UserId == id).ToListAsync();
                var sum = purchases.Sum(p => p.TotalPrice);
                ViewBag.TotalPrice = Convert.ToInt32(sum);
                string details = "Purchased Products viewed.";
                string? uid = HttpContext.Session.GetString("Name");
                ActivityLogDetails(uid, "Viewed", "Products", 0 , details);
                return View(purchases);
            }
            catch (Exception ex)
            {
                ErrorLogDetails(ex, "ViewOrders");
                throw ex;
            }
        }

    }
}
