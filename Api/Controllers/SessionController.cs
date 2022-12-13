using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Api.Models;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    public class SessionController : Controller
    {

        public SessionController()
        {
        }

        public IActionResult Signin()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Signin([FromForm] string username, string password, CancellationToken cancellationToken = default)
        {
            if (password == "")
            {
                return View();
            }

            string passwordSHA256;
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

                // Convert byte array to a string   
                StringBuilder builder = new();
                for (int i = 0; i < bytes.Length; i++)
                {
                    _ = builder.Append(bytes[i].ToString("x2"));
                }
                passwordSHA256 = builder.ToString();
            }

            if (passwordSHA256 == "68e3262d9d6c110ccdd18e51efc90923fca0d38262095e129b538bafc367981f")
            {
                Claim[] claims = new[]
                {
                new Claim(ClaimTypes.Name, username)
            };

                ClaimsIdentity claimsIdentity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                ClaimsPrincipal claimsPrincipal = new(claimsIdentity);
                AuthenticationProperties authProperties = new();

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    claimsPrincipal,
                    authProperties);

                return Request.Headers["Referer"].ToString().Split('?').Length == 1
                    ? RedirectToAction("Shelter", "Dogs")
                    : Redirect($"{Request.Headers["Origin"]}{Request.Headers["Referer"].ToString().Split('?')[1]}");
            }

            return View();
        }

        public async Task<IActionResult> Signout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Shelter", "Dogs");
        }

        [HttpGet("username")]
        public string GetSessionUsername(CancellationToken cancellationToken)
        {
            return HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}