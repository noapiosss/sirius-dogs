using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Api.Models;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Threading;
using System.Security.Cryptography;
using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System;

namespace Api.Controllers;

public class SessionController : Controller
{
    private readonly ILogger<SessionController> _logger;

    public SessionController(ILogger<SessionController> logger)
    {
        _logger = logger;
    }

    public IActionResult Signin()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Signin([FromForm] string password, CancellationToken cancellationToken = default)
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
            StringBuilder builder = new StringBuilder();  
            for (int i = 0; i < bytes.Length; i++)  
            {  
                builder.Append(bytes[i].ToString("x2"));  
            }  
            passwordSHA256 = builder.ToString();  
        }

        if (passwordSHA256 == "68e3262d9d6c110ccdd18e51efc90923fca0d38262095e129b538bafc367981f")
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, "volunteer")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            var authProperties = new AuthenticationProperties();

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                claimsPrincipal,
                authProperties);

            if (Request.Headers["Referer"].ToString().Split('?').Length == 1)
            {
                return RedirectToAction("Index", "Dogs");
            }

            return Redirect($"{Request.Headers["Origin"].ToString()}{Request.Headers["Referer"].ToString().Split('?')[1]}");
        }

        return View();
    }

    public async Task<IActionResult> Signout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Dogs");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
