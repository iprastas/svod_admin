using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Npgsql;
using NpgsqlTypes;
using System.Text;
using System;
using System.Xml.Linq;
using System.Reflection;
using System.ComponentModel.DataAnnotations;

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

        public Dictionary<ulong, string> Icanflags = Pg.icanflags;
        public Dictionary<ulong, bool> Icanflagsbool = new();
        public Dictionary<int, string> forms = Pg.forms;
        public Dictionary<int, bool> formsbool = new();
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

            foreach (var f in forms)
            {
                formsbool.Add(f.Key, false);
            }

            int[] myFormsKinds;
            using NpgsqlConnection conn = new(connectionString);
            conn.Open();
            NpgsqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = "select s.subject,s.inn login,u.password,u.passwordupto,ty.name,s.short,u.note,u.myformkinds,u.myforms,u.icanflags,s.changer,s.username,s.changedate "
            + " from svod2.subject s "
            + " left outer join svod2.subjectusers u on u.subject = s.subject and coalesce(s.upto,current_date)>=current_date"
            + " left outer join svod2.territory ty on s.territorywork = ty.territory "
            + " where s.subject=:sid and s.inn=:l";
            cmd.Parameters.Add(":sid", NpgsqlDbType.Integer).Value = Convert.ToInt32(id);
            cmd.Parameters.Add(":l", NpgsqlDbType.Varchar).Value = login;
            NpgsqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                if (!reader.IsDBNull(0))
                    Id = reader.GetInt32(0);
                if (!reader.IsDBNull(1))
                    Login = reader.GetString(1);
                if (!reader.IsDBNull(2))
                    Password = reader.GetString(2);
                if (!reader.IsDBNull(3))
                    Passwordupto = reader.GetDateTime(3);
                if (!reader.IsDBNull(4))
                    Territory = reader.GetString(4);
                if (!reader.IsDBNull(5))
                    SubjectName = reader.GetString(5);
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
            return Redirect("/Subject/SubjectUsers");
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

            using NpgsqlConnection conn = new(connectionString);
            conn.Open();
            using NpgsqlCommand cnt = conn.CreateCommand();
            cnt.CommandText = $"select count(*) from svod2.subjectusers where subject={Id}";

            int countUsers = 0;
            NpgsqlDataReader reader = cnt.ExecuteReader();
            while (reader.Read())
            {
                if (!reader.IsDBNull(0))
                    countUsers = reader.GetInt32(0);
            }
            conn.Close();

            using NpgsqlConnection update = new(connectionString);
            update.Open();
            using NpgsqlCommand cmd = update.CreateCommand();

            if (countUsers == 0)
            {
                cmd.CommandText = "insert into svod2.subjectusers(password, passwordupto, note, myformkinds, " +
                    "icanflags, changedate, changer, login, subject) "
                  + "values(:pwd,:upto,:nt,:fk,:cf,:chdt,:ch,:lg,:sub)";
            }
            else
            {
                cmd.CommandText = "update svod2.subjectusers SET password=:pwd,passwordupto=:upto,note=:nt,myformkinds=:fk,"
                        + "icanflags=:cf,changedate=:chdt,changer=:ch where login=:lg and subject=:sub";
            }
                
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

            update.Close();

            return Redirect("/Subject/SubjectUsers");
        }
    }
}
