using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Npgsql;
using NpgsqlTypes;
using System.Net;
using System.Xml.Linq;

namespace svod_admin.Pages.Territory
{
    [Authorize]
    public class CreateTerUserModel : PageModel
    {
        [BindProperty] public int Id { get; set; }
        [BindProperty] public string Login { get; set; } = "";
        [BindProperty] public string Password { get; set; } = "";
        [BindProperty] public DateTime? Passwordupto { get; set; }
        [BindProperty] public string? SubjectName { get; set; } = "";
        [BindProperty] public string? Note { get; set; } = "";
        [BindProperty] public List<int> Fkinds { get; set; }
        [BindProperty] public string? Myforms { get; set; } = "";
        [BindProperty] public List<ulong> Cflags { get; set; }
        [BindProperty] public string? Changer { get; set; } = "";
        [BindProperty] public string? Username { get; set; } = "";
        [BindProperty] public string? connectionString { get; }
        IConfiguration Configuration { get; set; }
        public CreateTerUserModel(IConfiguration _configuration)
        {
            Fkinds = [];
            Cflags = [];
            Configuration = _configuration;
            connectionString = Configuration.GetConnectionString("DefaultConnection");
        }

        public void OnGet()
        {
            object? id = RouteData.Values["id"];
        }

        public IActionResult OnPostCancel()
        {
            return Redirect("/TerritoryUsers");
        }

        public IActionResult OnPostCreate()
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

            using (NpgsqlConnection ins = new (connectionString))
            {
                ins.Open();
                string connINS = "INSERT INTO svod2.territoryusers VALUES ( @territory, @login, @note, @password, @passwordupto, " +
                    $"@myformkinds, @myforms, @icanflags, @changer, @username, @changedate)";
                NpgsqlCommand plcom = new (connINS, ins);
                plcom.Parameters.Add("@territory", NpgsqlDbType.Varchar).Value = Id;
                plcom.Parameters.Add("@login", NpgsqlDbType.Varchar).Value = Login;
                plcom.Parameters.Add("@note", NpgsqlDbType.Varchar).Value = Note != null ? Note : DBNull.Value;
                plcom.Parameters.Add("@password", NpgsqlDbType.Varchar).Value = Password;
                plcom.Parameters.Add("@passwordupto", NpgsqlDbType.Timestamp).Value = Passwordupto != null ? Passwordupto : DBNull.Value;
                plcom.Parameters.Add("@myformkinds", NpgsqlDbType.Varchar).Value = myFormkinds;
                plcom.Parameters.Add("@myforms", NpgsqlDbType.Varchar).Value = DBNull.Value;
                plcom.Parameters.Add("@icanflags", NpgsqlDbType.Bigint).Value = (long)IcanFlags;
                plcom.Parameters.Add("@changer", NpgsqlDbType.Varchar).Value = "admin";
                plcom.Parameters.Add("@username", NpgsqlDbType.Varchar).Value = Username;
                plcom.Parameters.Add("@changedate", NpgsqlDbType.Timestamp).Value = DateTime.Now;

                plcom.ExecuteNonQuery();

                ins.Close();
            }

            return Redirect("/Territory/TerritoryUsers");
        }
    }
}
