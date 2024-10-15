using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Npgsql;
using NpgsqlTypes;

namespace svod_admin.Pages.Target
{
    [Authorize]
    public class CreateTarUserModel : PageModel
    {
        [BindProperty] public string Login { get; set; } = "";
        [BindProperty] public string Password { get; set; } = "";
        [BindProperty] public DateTime? Passwordupto { get; set; }
        [BindProperty] public string? Department { get; set; } = "";
        [BindProperty] public string? Name { get; set; } = "";
        [BindProperty] public string? Note { get; set; } = "";
        [BindProperty] public List<int> Fkinds { get; set; }
        [BindProperty] public string? Myforms { get; set; } = "";
        [BindProperty] public List<ulong> Cflags { get; set; }
        [BindProperty] public string? Changer { get; set; } = "";
        [BindProperty] public string? Username { get; set; } = "";
        [BindProperty] public string? ConnectionString {get;}
        private readonly IConfiguration Configuration;
        public CreateTarUserModel(IConfiguration configuration)
        {
            Fkinds = new List<int>();
            Cflags = new List<ulong>();
            Configuration = configuration;
            ConnectionString = Configuration.GetConnectionString("DefaultConnection");
        }

        public IActionResult OnPostCancel()
        {
            return Redirect("/Target/TargetUser");
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

            using (NpgsqlConnection ins = new (ConnectionString))
            {
                ins.Open();
                string connINS = "INSERT INTO svod2.targetusers VALUES (@login, @password, @passwordupto, @territory, @department, @name," +
                    $" @note, @myformkinds, @myforms, @icanflags, @changer, @username, @changedate)";
                NpgsqlCommand plcom = new (connINS, ins);
                plcom.Parameters.Add("@login", NpgsqlDbType.Varchar).Value = Login;
                plcom.Parameters.Add("@password", NpgsqlDbType.Varchar).Value = Password;
                plcom.Parameters.Add("@passwordupto", NpgsqlDbType.Timestamp).Value = Passwordupto != null ? Passwordupto : DBNull.Value;
                plcom.Parameters.Add("@territory", NpgsqlDbType.Integer).Value = 3;
                plcom.Parameters.Add("@department", NpgsqlDbType.Varchar).Value = Department != null ? Department : DBNull.Value;
                plcom.Parameters.Add("@name", NpgsqlDbType.Varchar).Value = Name != null ? Name : DBNull.Value;
                plcom.Parameters.Add("@note", NpgsqlDbType.Varchar).Value = Note != null ? Note : DBNull.Value;
                plcom.Parameters.Add("@myformkinds", NpgsqlDbType.Varchar).Value = myFormkinds;
                plcom.Parameters.Add("@myforms", NpgsqlDbType.Varchar).Value = DBNull.Value;
                plcom.Parameters.Add("@icanflags", NpgsqlDbType.Bigint).Value = (Int64)IcanFlags;
                plcom.Parameters.Add("@changer", NpgsqlDbType.Varchar).Value = "admin";
                plcom.Parameters.Add("@username", NpgsqlDbType.Varchar).Value = Username;
                plcom.Parameters.Add("@changedate", NpgsqlDbType.Timestamp).Value = DateTime.Now;

                plcom.ExecuteNonQuery();

                ins.Close();
            }

            return Redirect("/Target/TargetUser");
        }
    }
}
