using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace svod_admin.Pages.Territory
{
    [Authorize]
    public class TerritoryUsersModel : PageModel
    {
        public List<User> list = new();
        readonly IConfiguration configuration;
        public TerritoryUsersModel(IConfiguration _configuration)
        {
            configuration = _configuration;
        }
        public void OnGet()
        {
            string? connectionString = configuration.GetConnectionString("DefaultConnection");
            using (Npgsql.NpgsqlConnection conn = new Npgsql.NpgsqlConnection(connectionString))
            {
                conn.Open();
                Npgsql.NpgsqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "select t.territory,k.short||t.name,tu.* from territory t "
                + " left outer join territoryusers tu on t.territory=tu.territory"
                + " left outer join territorykind k on t.territorykind=k.territorykind"
                + " where t.territory > 3 and k.territorykind in(10,12,15,16,17,28,29,35,37,38) order by 1;";
                Npgsql.NpgsqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    User user = new();
                    if (!reader.IsDBNull(0))
                        user.id = reader.GetInt32(0);
                    if (!reader.IsDBNull(1))
                        user.subjectname = reader.GetString(1);
                    if (!reader.IsDBNull(3))
                        user.login = reader.GetString(3);
                    if (!reader.IsDBNull(4))
                        user.note = reader.GetString(4);
                    if (!reader.IsDBNull(5))
                        user.password = reader.GetString(5);
                    if (!reader.IsDBNull(6))
                        user.passwordupto = reader.GetDateTime(6);
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
        }
        public IActionResult OnPostEdit(string login, int id)
        {
            return new RedirectToPageResult("/Territory/EditTerUser", new { login, id });
        }

        public IActionResult OnPostForm(string login, int territoryid)
        {
            return new RedirectToPageResult("/Territory/TerritoryFinegrained", new { login, territoryid });
        }

        public IActionResult OnPostCreate(int id)
        {
            return new RedirectToPageResult("/Territory/CreateTerUser", new { id });
        }
    }
}
