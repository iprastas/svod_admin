using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Npgsql;

namespace svod_admin.Pages.Target
{
    [Authorize]
    public class TargetUserModel : PageModel
    {
        public List<User> list = new();
        readonly IConfiguration configuration;
        public TargetUserModel(IConfiguration _configuration)
        {
            configuration = _configuration;
        }
        public void OnGet()
        {
            string? connectionString = configuration.GetConnectionString("DefaultConnection");
            using NpgsqlConnection conn = new(connectionString);
            conn.Open();
            NpgsqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = "select u.login,u.password,u.passwordupto,t.name,u.department,u.name,u.note,"
                + " u.myformkinds,u.myforms,u.icanflags,u.changer,u.username,u.changedate from"
                + " svod2.targetusers u left outer join svod2.territory t on u.territory = t.territory order by u.name";
            NpgsqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                User user = new();
                user.login = reader.GetString(0);
                if (!reader.IsDBNull(1))
                    user.password = reader.GetString(1);
                if (!reader.IsDBNull(2))
                    user.passwordupto = reader.GetDateTime(2);
                if (!reader.IsDBNull(3))
                    user.territory = reader.GetString(3);
                if (!reader.IsDBNull(4))
                    user.department = reader.GetString(4);
                if (!reader.IsDBNull(5))
                    user.name = reader.GetString(5);
                if (!reader.IsDBNull(6))
                    user.note = reader.GetString(6);
                if (!reader.IsDBNull(7))
                    user.myformkinds = reader.GetString(7);
                if (!reader.IsDBNull(8))
                    user.myforms = reader.GetString(8);
                if (!reader.IsDBNull(9))
                    user.icanflags = (ulong)reader.GetInt64(9);
                if (!reader.IsDBNull(10))
                    user.changer = reader.GetString(10);
                if (!reader.IsDBNull(11))
                    user.username = reader.GetString(11);
                if (!reader.IsDBNull(12))
                    user.changedate = reader.GetDateTime(12);

                list.Add(user);
            }
            conn.Close();
        }
        public IActionResult OnPostEdit(string login)
        {
            return new RedirectToPageResult("/Target/EditTarUser", new { login });
        }

        public IActionResult OnPostForm(string login)
        {
            return new RedirectToPageResult("/Target/TargetFinegrained", new { login });
        }

        public IActionResult OnPostCreate()
        {
            return Redirect("/Target/CreateTarUser");
        }
    }
}
