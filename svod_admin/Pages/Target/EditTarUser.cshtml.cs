using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Npgsql;
using NpgsqlTypes;
using System.Reflection;

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

        public Dictionary<ulong, string> Icanflags = Pg.icanflags;
        public Dictionary<ulong, bool> Icanflagsbool = new();
        public Dictionary<int, string> forms = Pg.forms;
        public Dictionary<int, bool> formsbool = new();

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
            foreach (var f in forms)
            {
                formsbool.Add(f.Key, false);
            }
            int[] myFormsKinds;
            using NpgsqlConnection conn = new(connectionString);
            conn.Open();
            NpgsqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT u.login,u.password,u.passwordupto,t.name,u.department,u.name,u.note, u.myformkinds, "
            + "u.myforms,u.icanflags,u.changer,u.username,u.changedate FROM svod2.targetusers u left outer join svod2.territory t "
            + $"on u.territory = t.territory WHERE login='{RouteData.Values["login"]}'";
            NpgsqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Login = reader.GetString(0);
                if (!reader.IsDBNull(1))
                    Password = reader.GetString(1);
                if (!reader.IsDBNull(2))
                    Passwordupto = reader.GetDateTime(2);
                if (!reader.IsDBNull(3))
                    Territory = reader.GetString(3);
                if (!reader.IsDBNull(4))
                    Department = reader.GetString(4);
                if (!reader.IsDBNull(5))
                    Name = reader.GetString(5);
                if (!reader.IsDBNull(6))
                    Note = reader.GetString(6);
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
