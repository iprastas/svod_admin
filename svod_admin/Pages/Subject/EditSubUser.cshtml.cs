using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Npgsql;
using NpgsqlTypes;
using System.Text;
using System;
using System.Xml.Linq;

namespace svod_admin.Pages.Subject
{
    [Authorize]
    public class EditSubUserModel : PageModel
    {
        [BindProperty] public int Id { get; set; }
        [BindProperty] public string Login { get; set; } = "";
        [BindProperty] public string Password { get; set; } = "";
        [BindProperty] public DateTime? Passwordupto { get; set; }
        [BindProperty] public string? Territory { get; set; } = "";
        [BindProperty] public string? SubjectName { get; set; } = "";
        [BindProperty] public string? Note { get; set; } = "";
        [BindProperty] public string Myformkinds { get; set; } = "";
        [BindProperty] public List<int> Fkinds { get; set; }
        [BindProperty] public string? Myforms { get; set; } = "";
        [BindProperty] public ulong icanFlags { get; set; }
        [BindProperty] public List<ulong> Cflags { get; set; }
        [BindProperty] public string? Changer { get; set; } = "";
        [BindProperty] public string? Username { get; set; } = "";
        [BindProperty] public string? connectionString { get; }
        IConfiguration Configuration { get; set; }
        public EditSubUserModel(IConfiguration _configuration)
        {
            Fkinds = new List<int>();
            Cflags = new List<ulong>();
            Configuration = _configuration;
            connectionString = Configuration.GetConnectionString("DefaultConnection");
        }

        public void OnGet()
        {
            object? login = RouteData.Values["login"];
            object? id = RouteData.Values["id"];
        }

        public IActionResult OnPostCancel()
        {
            return Redirect("/Subject/SubjectUsers");
        }

        public void OnPostGenerate()
        {
            StringBuilder password = new ();
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
        }

        public IActionResult OnPostEdit()
        {
            string myFormkinds = "";
            foreach (int c in Fkinds)
            {
                myFormkinds = myFormkinds + c + ",";
            }
            myFormkinds = myFormkinds.TrimEnd(',');

            ulong IcanFlags;
            if (Cflags.Count > 0)
            {
                IcanFlags = Cflags[0];
                Cflags.RemoveAt(0);
                foreach (ulong el in Cflags)
                {
                    IcanFlags |= el;
                }
            }
            else
            {
                IcanFlags = 0;
            }

            using (NpgsqlConnection update = new (connectionString))
            {
                update.Open();
                using (NpgsqlCommand cmd = update.CreateCommand())
                {
                    cmd.CommandText = "update svod2.subjectusers SET password=:pwd,passwordupto=:upto,note=:nt,myformkinds=:fk,"
                        + "icanflags=:cf,changedate=:chdt,changer=:ch where login=:lg and subject=:sub";
                    cmd.Parameters.Add(":pwd", NpgsqlDbType.Varchar).Value = Password;
                    cmd.Parameters.Add(":upto", NpgsqlDbType.Date).Value = Passwordupto != null ? Passwordupto : DBNull.Value;
                    cmd.Parameters.Add(":chdt", NpgsqlDbType.Date).Value = DateTime.Now;
                    cmd.Parameters.Add(":nt", NpgsqlDbType.Varchar).Value = Note != null ? Note : DBNull.Value;
                    cmd.Parameters.Add(":fk", NpgsqlDbType.Varchar).Value = string.IsNullOrEmpty(myFormkinds) ? DBNull.Value : myFormkinds;
                    cmd.Parameters.Add(":cf", NpgsqlDbType.Bigint).Value = (long)IcanFlags;
                    cmd.Parameters.Add(":lg", NpgsqlDbType.Varchar).Value = Login;
                    cmd.Parameters.Add(":sub", NpgsqlDbType.Integer).Value = Id;
                    cmd.Parameters.Add(":ch", NpgsqlDbType.Varchar).Value = "admin";
                    cmd.ExecuteNonQuery();
                }

                update.Close();
            }

            return Redirect("/Subject/SubjectUsers");
        }
    }
}
