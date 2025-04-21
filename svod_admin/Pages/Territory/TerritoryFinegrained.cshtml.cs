using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using NpgsqlTypes;
using Npgsql;

namespace svod_admin.Pages.Territory
{
    [Authorize]
    public class TerritoryFinegrainedModel : PageModel
    {
        static List<TypeOFermissions> permiss { get; } = new()
        {
            new TypeOFermissions(0, "Запрет"),
            new TypeOFermissions(1, "Просмотр"),
            new TypeOFermissions(2, "Изменение"),
            new TypeOFermissions(3, "Безусловное изменение")
        };
        public record class TypeOFermissions(int Id, string Name);
        public SelectList ListOfPermiss { get; } = new SelectList(permiss, "Id", "Name");
        [BindProperty] public int Num { get; set; }
        [BindProperty] public int FormID { get; set; }
        [BindProperty] public string FormName { get; set; } = "";
        [BindProperty] public int TerritoryID { get; set; }
        [BindProperty] public string Territory { get; set; } = "";
        [BindProperty] public string Login { get; set; } = "";
        [BindProperty] public int Permission { get; set; }
        [BindProperty] public string Username { get; set; } = "";
        [BindProperty] public DateTime? ChangeDate { get; set; }
        [BindProperty] public string? Message { get; set; } = "";
        [BindProperty] public bool Success { get; set; }

        string? connectionString;
        public List<TerritoryFinegrainedModel> list = new();
        readonly IConfiguration? configuration;

        public TerritoryFinegrainedModel(IConfiguration? _configuration)
        {
            configuration = _configuration;
            connectionString = configuration==null?string.Empty : configuration.GetConnectionString("DefaultConnection");
        }
        public void OnGet()
        {
            Login = TempData["Login"]?.ToString();
            TerritoryID = Convert.ToInt32(TempData["TerritoryID"]);

            using NpgsqlConnection conn = new(connectionString);
            conn.Open();
            NpgsqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = "select tf.form,coalesce(f.name,f.short),t.name,tf.territoryusers,tf.permission,tf.username,tf.changedate, t.territory "
                + "from svod2.territoryfinegrained tf"
                + " left outer join svod2.form f on tf.form = f.form "
                + " left outer join svod2.territory t on tf.territory = t.territory "
                + $"where tf.territoryusers='{Login}' order by f.name ";
            NpgsqlDataReader reader = cmd.ExecuteReader();
            int i = 1;
            while (reader.Read())
            {
                TerritoryFinegrainedModel user = new(null);
                if (!reader.IsDBNull(0))
                    user.FormID = reader.GetInt32(0);
                if (!reader.IsDBNull(1))
                    user.FormName = reader.GetString(1);
                if (!reader.IsDBNull(2))
                    user.Territory = reader.GetString(2);
                if (!reader.IsDBNull(3))
                    user.Login = reader.GetString(3);
                if (!reader.IsDBNull(4))
                    user.Permission = reader.GetInt32(4);
                if (!reader.IsDBNull(5))
                    user.Username = reader.GetString(5);
                if (!reader.IsDBNull(6))
                    user.ChangeDate = reader.GetDateTime(6);
                if (!reader.IsDBNull(7))
                    user.TerritoryID = reader.GetInt32(7);

                user.Num = i;
                i += 1;
                list.Add(user);
            }
            conn.Close();
        }

        public IActionResult OnPostCreate(string login, int territoryid)
        {
            return new RedirectToPageResult("/Territory/CreateTerForm", new { login, territoryid });
        }

        public IActionResult OnPostDelete(string login, int territoryid, int formid)
        {
            using (NpgsqlConnection conn = new (connectionString))
            {
                conn.Open();
                NpgsqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "delete from svod2.territoryfinegrained where form=:formid and territoryusers=:login and territory=:territoryid";
                cmd.Parameters.Add(":formid", NpgsqlDbType.Integer).Value = formid;
                cmd.Parameters.Add(":territoryid", NpgsqlDbType.Integer).Value = territoryid;
                cmd.Parameters.Add(":login", NpgsqlDbType.Varchar).Value = login;
                int res = cmd.ExecuteNonQuery();
                conn.Close();
            }
            TempData["Login"] = Login;
            TempData["TerritoryID"] = TerritoryID;
            return RedirectToPage("/Territory/TerritoryFinegrained");
        }

        public IActionResult OnGetEdit(string login, int formid, int territoryid, int permission, int number)
        {
            try
            {
                using NpgsqlConnection conn = new(connectionString);
                conn.Open();
                NpgsqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "update svod2.territoryfinegrained set permission=:perm where form=:formid and territoryusers=:login and territory=:territoryid";
                cmd.Parameters.Add(":perm", NpgsqlDbType.Smallint).Value = permission;
                cmd.Parameters.Add(":formid", NpgsqlDbType.Integer).Value = formid;
                cmd.Parameters.Add(":login", NpgsqlDbType.Varchar).Value = login; 
                cmd.Parameters.Add(":territoryid", NpgsqlDbType.Integer).Value = territoryid;
                cmd.ExecuteNonQuery();
                conn.Close();

                string mess = $"Форма {formid} успешно обновлена.";
                return new JsonResult(new { result = true, message = mess });
            }
            catch (NpgsqlException e)
            {
                string mess = $"Действие отменено.\nПроизошла ошибка {e.Message} errcode = {e.ErrorCode}.";
                return new JsonResult(new { result = false, message = mess });
            }
        }
    }
}
