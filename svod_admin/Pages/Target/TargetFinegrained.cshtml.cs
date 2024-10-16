using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using NpgsqlTypes;
using Npgsql;

namespace svod_admin.Pages.Target
{
    [Authorize]
    public class TargetFinegrainedModel : PageModel
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
        [BindProperty] public string Login { get; set; } = "";
        [BindProperty] public int Permission { get; set; }
        [BindProperty] public string Username { get; set; } = "";
        [BindProperty] public DateTime? ChangeDate { get; set; }
        [BindProperty] public string? Message { get; set; } = "";
        [BindProperty] public bool Success { get; set; }

        string? connectionString;
        public List<TargetFinegrainedModel> list = new();
        readonly IConfiguration? configuration;

        public TargetFinegrainedModel(IConfiguration? _configuration)
        {
            configuration = _configuration;
            connectionString = configuration==null?string.Empty : configuration.GetConnectionString("DefaultConnection");
        }
        public void OnGet()
        {
            Message = Convert.ToString(RouteData.Values["Message"]);
            if (Message != "")
            {
                var parts = Message.Split(' ');
                Message = parts[0];
                FormID = Convert.ToInt32(parts[1]);

                switch (Message)
                {
                    case "success":
                        Message = $"Успешно! Форма {FormID} изменена.";
                        Success = true;
                        break;
                    case "error":
                        Message = $"Ошибка! Форма {FormID} не была изменена.";
                        Success = false;
                        break;
                }
            }

            using NpgsqlConnection conn = new (connectionString);
            conn.Open();
            NpgsqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = "select t.form,coalesce(f.name,f.short),t.targetuser,t.permission,t.username,t.changedate "
                + "from svod2.targetfinegrained t left outer join svod2.form f on t.form = f.form "
                + $"where t.targetuser='{RouteData.Values["login"]}' order by f.name ";
            NpgsqlDataReader reader = cmd.ExecuteReader();
            int i = 1;
            while (reader.Read())
            {
                TargetFinegrainedModel user = new(null);
                user.FormID = reader.GetInt32(0);
                if (!reader.IsDBNull(1))
                    user.FormName = reader.GetString(1);
                if (!reader.IsDBNull(2))
                    user.Login = reader.GetString(2);
                if (!reader.IsDBNull(3))
                    user.Permission = reader.GetInt32(3);
                if (!reader.IsDBNull(4))
                    user.Username = reader.GetString(4);
                if (!reader.IsDBNull(5))
                    user.ChangeDate = reader.GetDateTime(5);

                user.Num = i;
                i += 1;
                list.Add(user);
            }
            conn.Close();
        }

        public IActionResult OnPostCreate(string login)
        {
            return new RedirectToPageResult("/Target/CreateTarForm" ,new {login});
        }

        public IActionResult OnPostDelete(string login, int formid)
        {
            using (NpgsqlConnection conn = new (connectionString))
            {
                conn.Open();
                NpgsqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "delete from svod2.targetfinegrained where form=:formid and targetuser=:login";
                cmd.Parameters.Add(":formid",NpgsqlDbType.Integer).Value= formid;
                cmd.Parameters.Add(":login",NpgsqlDbType.Varchar).Value=login;
                int res = cmd.ExecuteNonQuery();
                conn.Close();
            }
            return new RedirectToPageResult("/Target/TargetFinegrained", new { login });
        }

        public IActionResult OnPostEdit(string login, int formid, int number)
        {
            var permission = Request.Form["Permission"];
            using (NpgsqlConnection conn = new (connectionString))
            {
                conn.Open();
                NpgsqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "update svod2.targetfinegrained set permission=:perm where form=:formid and targetuser=:login";
                cmd.Parameters.Add(":perm", NpgsqlDbType.Smallint).Value = Convert.ToInt16(permission[number - 1]);
                cmd.Parameters.Add(":formid", NpgsqlDbType.Integer).Value = formid;
                cmd.Parameters.Add(":login", NpgsqlDbType.Varchar).Value = login;
                int res = cmd.ExecuteNonQuery();
                conn.Close();
            }
            string message = $"success {FormID}";
            return new RedirectToPageResult("/Target/TargetFinegrained", new { login, message });
        }
    }
}
