using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Npgsql;
using NpgsqlTypes;

namespace svod_admin.Pages.Target
{
    [Authorize]
    public class EditTarUserModel : PageModel
    {
        [BindProperty] public string Login { get; set; } = "";
        [BindProperty] public string Password { get; set; } = "";
        [BindProperty] public DateTime? Passwordupto { get; set; }
        [BindProperty] public string? Territory { get; set; } = "";
        [BindProperty] public string? Department { get; set; } = "";
        [BindProperty] public string? Name { get; set; } = "";
        [BindProperty] public string? Note { get; set; } = "";
        [BindProperty] public string Myformkinds { get; set; } = "";
        [BindProperty] public List<int> Fkinds { get; set; }
        [BindProperty] public string? Myforms { get; set; } = "";
        [BindProperty] public ulong icanFlags { get; set; }
        [BindProperty] public List<ulong> Cflags { get; set; }
        [BindProperty] public string? Changer { get; set; } = "";
        [BindProperty] public string? Username { get; set; } = "";
        [BindProperty] public string? connectionString { get; }


        readonly IConfiguration configuration;
        public EditTarUserModel(IConfiguration _configuration)
        {
            Fkinds = new List<int>();
            Cflags = new List<ulong>();
            configuration = _configuration;
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public void OnGet()
        {
        }

        public IActionResult OnPostCancel()
        {
            return Redirect("/Target/TargetUser");
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
                    cmd.CommandText = "update svod2.targetusers SET password=:pwd,passwordupto=:upto,department=:dpt,name=:nm,note=:nt,"
                        + "myformkinds=:fk,icanflags=:cf,changedate=:chdt,changer=:ch where login=:lg";
                    cmd.Parameters.Add(":pwd", NpgsqlDbType.Varchar).Value = Password;
                    cmd.Parameters.Add(":upto", NpgsqlDbType.Date).Value = Passwordupto != null ? Passwordupto : DBNull.Value;
                    cmd.Parameters.Add(":chdt", NpgsqlDbType.Date).Value = DateTime.Now;
                    cmd.Parameters.Add(":dpt", NpgsqlDbType.Varchar).Value = Department != null ? Department : DBNull.Value;
                    cmd.Parameters.Add(":nm", NpgsqlDbType.Varchar).Value = Name != null ? Name : DBNull.Value;
                    cmd.Parameters.Add(":nt", NpgsqlDbType.Varchar).Value = Note != null ? Note : DBNull.Value;
                    cmd.Parameters.Add(":fk", NpgsqlDbType.Varchar).Value = string.IsNullOrEmpty(myFormkinds) ? DBNull.Value : myFormkinds;
                    cmd.Parameters.Add(":cf", NpgsqlDbType.Bigint).Value = (long)IcanFlags;
                    cmd.Parameters.Add(":lg", NpgsqlDbType.Varchar).Value = Login;
                    cmd.Parameters.Add(":ch", NpgsqlDbType.Varchar).Value = "admin";
                    cmd.ExecuteNonQuery();
                }
                update.Close();
            }

            return Redirect("/Target/TargetUser");
        }
    }
}
