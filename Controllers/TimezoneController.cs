using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace SampleUserEmbeddedApp.Controllers
{
    public class TimezoneController : Controller
    {
        private readonly HttpClient _httpClient;

        public TimezoneController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task<IActionResult> Index(string token = "")
        {
            if (!string.IsNullOrEmpty(token))
            {
                HttpContext.Session.SetString("BearerToken2", token); // Save token in session
            }
            else
            {
                token = HttpContext.Session.GetString("BearerToken2"); // Retrieve token from session
            }

            string apiUrl = $"http://localhost:64762/Resource/UserTimeZoneInfo/589"; // Append search query
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode)
            {
                return View("Error", "Failed to retrieve timezone info");
            }

            string jsonResponse = await response.Content.ReadAsStringAsync();
            var user = JsonConvert.DeserializeObject<TimezoneInfo>(jsonResponse);

            return View("Index", user);
        }
    }

    public class TimezoneInfo
    {
        public string Abbreviation { get; set; }
        public string DstAbbreviation { get; set; }
        public string TimeZone { get; set; }
        public string TzDSTName { get; set; }
        public string TzName { get; set; }
    }
}
