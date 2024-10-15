using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace svod_admin.Controller
{
    public class PwdController : ControllerBase
    {
        [HttpGet ("/Pwd/Index")]
        public IActionResult Index()
        {
            string Password;
            StringBuilder password = new();
            password.Append(Pg.UppercaseLetters[Pg.random.Next(Pg.UppercaseLetters.Length)]);
            password.Append(Pg.LowercaseLetters[Pg.random.Next(Pg.LowercaseLetters.Length)]);
            password.Append(Pg.Numbers[Pg.random.Next(Pg.Numbers.Length)]);
            password.Append(Pg.SpecialCharacters[Pg.random.Next(Pg.SpecialCharacters.Length)]);

            // Fill the remaining length of the password with random characters from all categories.
            string allCharacters = Pg.UppercaseLetters + Pg.LowercaseLetters + Pg.Numbers + Pg.SpecialCharacters;
            for (int i = 4; i < 12; i++)
            {
                password.Append(allCharacters[Pg.random.Next(allCharacters.Length)]);
            }

            char[] passwordArray = password.ToString().ToCharArray();
            for (int i = 0; i < passwordArray.Length; i++)
            {
                int j = Pg.random.Next(i, passwordArray.Length);
                // Swap characters at positions i and j.
                (passwordArray[j], passwordArray[i]) = (passwordArray[i], passwordArray[j]);
            }

            Password = password.ToString();
            return new JsonResult(new { password = Password });

        }
    }
}
