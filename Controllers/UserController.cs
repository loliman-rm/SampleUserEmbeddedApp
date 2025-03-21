using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SampleUserEmbeddedApp.Models;
using System.Net.Http.Headers;

namespace SampleUserEmbeddedApp.Controllers
{
    public class UserController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly UserInfo _userInfo;

        public UserController(IHttpClientFactory httpClientFactory, UserInfo userInfo)
        {
            _httpClient = httpClientFactory.CreateClient();
            _userInfo = userInfo;
        }

        public async Task<IActionResult> Index(string search = "", string token = "")
        {
            if (!string.IsNullOrEmpty(token))
            {
                HttpContext.Session.SetString("BearerToken1", token); // Save token in session

                // Redirect to the same action with an empty string
                return RedirectToAction("Index", new { search = search });
            }
            else
            {
                token = HttpContext.Session.GetString("BearerToken1"); // Retrieve token from session
            }

            var model = _userInfo;

            if (string.IsNullOrEmpty(search) || string.IsNullOrEmpty(token))
            {

                return View(model);
            }

            string apiUrl = $"http://localhost:64762/Resource/Users/{search}"; // Append search query
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode)
            {
                return View(model); // Return an empty view
            }

            string jsonResponse = await response.Content.ReadAsStringAsync();
            var user = JsonConvert.DeserializeObject<UserInfo>(jsonResponse);
            user.AllowedOrigin = _userInfo.AllowedOrigin;

            return View("Index", user);
        }
    }
}
