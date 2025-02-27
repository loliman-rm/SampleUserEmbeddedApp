using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace SampleWebApplication.Controllers
{
    public class UserController : Controller
    {
        private readonly HttpClient _httpClient;

        public UserController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task<IActionResult> Index(string search = "", string token = "")
        {
            if (!string.IsNullOrEmpty(token))
            {
                HttpContext.Session.SetString("BearerToken", token); // Save token in session
            }
            else
            {
                token = HttpContext.Session.GetString("BearerToken"); // Retrieve token from session
            }

            if (string.IsNullOrEmpty(search) || string.IsNullOrEmpty(token))
            {
                return View(); // Return an empty view if parameters are missing
            }

            string apiUrl = $"http://localhost:64762/Resource/Users/{search}"; // Append search query
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode)
            {
                return View("Error", "Failed to retrieve user info");
            }

            string jsonResponse = await response.Content.ReadAsStringAsync();
            var user = JsonConvert.DeserializeObject<UserInfo>(jsonResponse);

            return View("Index", user);
        }
    }

    public class UserInfo
    {
        public int UserID { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string CostCenter { get; set; }
        public string Organization { get; set; }
    }
}
