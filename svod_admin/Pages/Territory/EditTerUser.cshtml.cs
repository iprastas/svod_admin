using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using svod_admin;
using Npgsql;
using NpgsqlTypes;
using System.Xml.Linq;
using System.Reflection;

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

        public Dictionary<ulong, string> Icanflags = Pg.icanflags;
        public Dictionary<ulong, bool> Icanflagsbool = new();
        public Dictionary<int, string> forms = Pg.forms;
        public Dictionary<int, bool> formsbool = new();
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
            foreach (var f in forms)
            {
                formsbool.Add(f.Key, false);
            }

            int[] myFormsKinds;
            using NpgsqlConnection conn = new(ConnectionString);
            conn.Open();
            NpgsqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = "select t.territory,k.short||t.name,tu.* from territory t "
            + " left outer join territoryusers tu on t.territory=tu.territory"
            + " left outer join territorykind k on t.territorykind=k.territorykind"
            + $" where tu.territory='{RouteData.Values["id"]}' and tu.login='{RouteData.Values["login"]}'";
            Npgsql.NpgsqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                if (!reader.IsDBNull(0))
                    Id = reader.GetInt32(0);
                if (!reader.IsDBNull(1))
                    SubjectName = reader.GetString(1);
                if (!reader.IsDBNull(3))
                    Login = reader.GetString(3);
                if (!reader.IsDBNull(4))
                    Note = reader.GetString(4);
                if (!reader.IsDBNull(5))
                    Password = reader.GetString(5);
                if (!reader.IsDBNull(6))
                    Passwordupto = reader.GetDateTime(6);
                if (!reader.IsDBNull(7))
                    Myformkinds = reader.GetString(7);
                if (!reader.IsDBNull(8))
                    Myforms = reader.GetString(8);
                if (!reader.IsDBNull(9))
                    icanFlags = (ulong)reader.GetInt64(9);
                if (!reader.IsDBNull(10))
                    Changer = reader.GetString(10);
                if (!reader.IsDBNull(11))
                    Username = reader.GetString(11);

                if (!string.IsNullOrEmpty(Myformkinds))
                {
                    myFormsKinds = Myformkinds.Split(',').Select(snum => int.Parse(snum)).ToArray();
                    foreach (var i in myFormsKinds)
                    {
                        if (i < 5 && i > 0)
                        {
                            formsbool[i] = true;
                        }
                    }
                }
            }
            conn.Close();

            foreach (var flag in Icanflags)
            {
                Icanflagsbool.Add(flag.Key, (icanFlags & flag.Key) != 0);
            }
        }

        public IActionResult OnPostCancel()
        {
            return Redirect("/Territory/TerritoryUsers");
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

