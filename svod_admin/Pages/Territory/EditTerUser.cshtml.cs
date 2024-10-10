using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Npgsql;
using NpgsqlTypes;
using System.Xml.Linq;

namespace svod_admin.Pages
{
    [Authorize]
    public class EditTerUserModel : PageModel
    {
        [BindProperty] public int Id { get; set; }
        [BindProperty] public string Login { get; set; } = "";
        [BindProperty] public string Password { get; set; } = "";
        [BindProperty] public DateTime? Passwordupto { get; set; }
        [BindProperty] public string? SubjectName { get; set; } = "";
        [BindProperty] public string? Note { get; set; } = "";
        [BindProperty] public string Myformkinds { get; set; } = "";
        [BindProperty] public List<int> Fkinds { get; set; }
        [BindProperty] public string? Myforms { get; set; } = "";
        [BindProperty] public ulong icanFlags { get; set; }
        [BindProperty] public List<ulong> Cflags { get; set; }
        [BindProperty] public string? Changer { get; set; } = "";
        [BindProperty] public string? Username { get; set; } = "";
        [BindProperty] public string? ConnectionString { get;}
        IConfiguration Configuration { get; set; }
        public EditTerUserModel(IConfiguration _configuration)
        {
            Fkinds = new List<int>();
            Cflags = new List<ulong>();
            Configuration = _configuration;
            ConnectionString = Configuration.GetConnectionString("DefaultConnection");
        }

        public void OnGet()
        {
            //object? login = RouteData.Values["login"];
            //Id = Convert.ToInt32((string)RouteData.Values["id"]);
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

            using (NpgsqlConnection update = new (ConnectionString))
            {
                update.Open();
                using (NpgsqlCommand cmd = update.CreateCommand())
                {
                    cmd.CommandText = "update svod2.territoryusers SET password=:pwd,passwordupto=:upto,note=:nt,myformkinds=:fk,"
                        + "icanflags=:cf,changedate=:chdt,changer=:ch where login=:lg and territory=:ter";
                    cmd.Parameters.Add(":pwd", NpgsqlDbType.Varchar).Value = Password;
                    cmd.Parameters.Add(":upto", NpgsqlDbType.Date).Value = Passwordupto != null ? Passwordupto : DBNull.Value;
                    cmd.Parameters.Add(":chdt", NpgsqlDbType.Date).Value = DateTime.Now;
                    cmd.Parameters.Add(":nt", NpgsqlDbType.Varchar).Value = Note != null ? Note : DBNull.Value;
                    cmd.Parameters.Add(":fk", NpgsqlDbType.Varchar).Value = string.IsNullOrEmpty(myFormkinds) ? DBNull.Value : myFormkinds;
                    cmd.Parameters.Add(":cf", NpgsqlDbType.Bigint).Value = (Int64)IcanFlags;
                    cmd.Parameters.Add(":lg", NpgsqlDbType.Varchar).Value = Login;
                    cmd.Parameters.Add(":ter", NpgsqlDbType.Integer).Value = Convert.ToInt32(RouteData.Values["id"] as string);//Id;
                    cmd.Parameters.Add(":ch", NpgsqlDbType.Varchar).Value = "admin";
                    cmd.ExecuteNonQuery();
                }
                
                update.Close();
            }

            return Redirect("/Territory/TerritoryUsers");
        }
    }
}

