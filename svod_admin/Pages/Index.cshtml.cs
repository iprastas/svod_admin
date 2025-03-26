using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Security.Claims;

namespace svod_admin.Pages
{
	[IgnoreAntiforgeryToken]
	public class IndexModel : PageModel
	{
		private readonly ILogger<IndexModel> _logger;
		readonly IConfiguration configuration;

		public IndexModel(ILogger<IndexModel> logger, IConfiguration _configuration)
		{
			_logger = logger;
            configuration = _configuration;

        }

        [BindProperty] public string Username { get; set; } = "";
        [BindProperty] public string Password { get; set; } = "";
        public string Message { get; set; } = "";

        public void OnGet()
        {
            Message = Convert.ToString(RouteData.Values["Message"]);
        }
        public IActionResult OnGetExit()
		{
            string? connectionString = configuration.GetConnectionString("DefaultConnection");
            using NpgsqlConnection con = new (connectionString);
            NpgsqlConnection.ClearPool(con);
            
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
			return Redirect("/Index");
		}

		public IActionResult OnPostEnter()
		{
			if (ModelState.IsValid && Authenticate(Username, Password))
			{
				var claims = new List<Claim>
				{
					new Claim(ClaimsIdentity.DefaultNameClaimType, Username)
				};

				var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

				var authProperties = new AuthenticationProperties { AllowRefresh = true };
				HttpContext.SignInAsync(
					CookieAuthenticationDefaults.AuthenticationScheme,
					new ClaimsPrincipal(claimsIdentity),
					authProperties);
				return Redirect("/Target/TargetUser");
            }
            Message = "Ошибка! Неверно введен логин или пароль.";
            return new RedirectToPageResult("/Index", new { Message });
        }
		private bool Authenticate(string user, string pwd)
		{
            string? connectionString = configuration.GetConnectionString("DefaultConnection");
            User admin = new();
            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
			{
                conn.Open();
                
                Npgsql.NpgsqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT * FROM svod2.targetusers WHERE login='admin'";

                Npgsql.NpgsqlDataReader reader = cmd.ExecuteReader();
                
                while (reader.Read())
                {

                    admin.login = reader.GetString(0);
                    if (!reader.IsDBNull(1))
                        admin.password = reader.GetString(1);
                    if (!reader.IsDBNull(2))
                        admin.passwordupto = reader.GetDateTime(2);
                }

                conn.Close();
            }
            if (user == admin.login && pwd == admin.password)
                return true;
            return false;
		}
	}
}