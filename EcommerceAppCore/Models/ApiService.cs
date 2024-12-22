using Newtonsoft.Json;
using System.Text;

namespace EcommerceAppCore.Models
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<Product>> GetProducts(string apiUrl)
        {
            var response = await _httpClient.GetStringAsync(apiUrl);
            var products = JsonConvert.DeserializeObject<List<Product>>(response);
            return products;
        }

        public async Task<Product> GetProductById(string apiUrl,int id)
        {
            var response = await _httpClient.GetStringAsync($"{apiUrl}/{id}");
            var product = JsonConvert.DeserializeObject<Product>(response);
            return product;
        }

        public async Task<bool> AddProduct(string apiUrl, Product product)
        {
            var content = new StringContent(JsonConvert.SerializeObject(product) , Encoding.UTF8 , "application/json");
            var response = await _httpClient.PostAsync(apiUrl, content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateProduct(string apiUrl,int id, Product product)
        {
            var content = new StringContent(JsonConvert.SerializeObject(product), Encoding.UTF8 , "application/json");
            var response = await _httpClient.PutAsync($"{apiUrl}/{id}", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteProduct(string apiUrl , int id)
        {
            var response = await _httpClient.DeleteAsync($"{apiUrl}/{id}");
            return response.IsSuccessStatusCode;
        }
    }

}
