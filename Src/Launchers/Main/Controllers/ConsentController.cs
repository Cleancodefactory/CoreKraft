using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MySqlX.XDevAPI.Common;

namespace Ccf.Ck.Launchers.Main.Controllers
{
    public class ConsentController : Controller
    {
        public async Task<IActionResult> Index(string code, string scope)
        {
            //code={code}&redirect_uri=https://localhost:5001/consent&client_id=322656278343-38jp2n9oes6k0mkd2dibc4fsbj7elc3j.apps.googleusercontent.com&client_secret=MWabN_LLhLIEUwWpJV16nQor&scope=&grant_type=authorization_code
            var dict = new Dictionary<string, string>();
            var url = "https://oauth2.googleapis.com/token";
            dict.Add("code", $"{code}");
            dict.Add("redirect_uri", "https://localhost:5001/consent");
            dict.Add("client_id", "322656278343-38jp2n9oes6k0mkd2dibc4fsbj7elc3j.apps.googleusercontent.com");
            dict.Add("client_secret", "MWabN_LLhLIEUwWpJV16nQor");
            dict.Add("scope", $"{scope}");
            dict.Add("grant_type", "authorization_code");

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.PostAsync(url, new FormUrlEncodedContent(dict));
                ViewData["Data"] = response.Content.ReadAsStringAsync().Result;
            }
            return View();
        }

        public IActionResult Drive()
        {
            return View();
        }
    }
}
