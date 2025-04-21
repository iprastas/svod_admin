using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using NpgsqlTypes;
using Npgsql;
using System.Security;


namespace svod_admin.Pages.Subject
{
    [Authorize]
    public class SubjectFinegrainedModel : PageModel
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
        [BindProperty] public int SubjectID { get; set; }
        [BindProperty] public string Subject { get; set; } = "";
        [BindProperty] public string Login { get; set; } = "";
        [BindProperty] public int Permission { get; set; }
        [BindProperty] public string Username { get; set; } = "";
        [BindProperty] public DateTime? ChangeDate { get; set; }
        [BindProperty] public string? Message { get; set; } = "";
        [BindProperty] public bool Success { get; set; }

        string? connectionString;
        public List<SubjectFinegrainedModel> list = new();
        readonly IConfiguration? configuration;

        public SubjectFinegrainedModel(IConfiguration? _configuration)
        {
            configuration = _configuration;
            connectionString = configuration==null?string.Empty : configuration.GetConnectionString("DefaultConnection");
        }
        public void OnGet()
        {
            Login = TempData["Login"]?.ToString();
            SubjectID = Convert.ToInt32(TempData["SubjectID"]);
            TempData.Keep();

            using NpgsqlConnection conn = new(connectionString);
            conn.Open();
            NpgsqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = "select sf.form,coalesce(f.name,f.short),s.short,sf.subjectuser,sf.permission,sf.username,sf.changedate,sf.subject "
                + "from svod2.subjectfinegrained sf"
                + " left outer join svod2.form f on sf.form = f.form "
                + " left outer join svod2.subject s on sf.subject = s.subject "
                + $"where sf.subjectuser='{Login}' order by f.name ";
            NpgsqlDataReader reader = cmd.ExecuteReader();
            int i = 1;
            while (reader.Read())
            {
                SubjectFinegrainedModel user = new(null);
                if (!reader.IsDBNull(0))
                    user.FormID = reader.GetInt32(0);
                if (!reader.IsDBNull(1))
                    user.FormName = reader.GetString(1);
                if (!reader.IsDBNull(2))
                    user.Subject = reader.GetString(2);
                if (!reader.IsDBNull(3))
                    user.Login = reader.GetString(3);
                if (!reader.IsDBNull(4))
                    user.Permission = reader.GetInt32(4);
                if (!reader.IsDBNull(5))
                    user.Username = reader.GetString(5);
                if (!reader.IsDBNull(6))
                    user.ChangeDate = reader.GetDateTime(6);
                if (!reader.IsDBNull(7))
                    user.SubjectID = reader.GetInt32(7);

                user.Num = i;
                i += 1;
                list.Add(user);
            }
            conn.Close();
        }

        public IActionResult OnPostCreate(string login, int subjectid)
        {
            return new RedirectToPageResult("/Subject/CreateSubForm", new { login, subjectid});
        }

        public IActionResult OnPostDelete(string login, int formid, int subjectid)
        {
            using (NpgsqlConnection conn = new (connectionString))
            {
                conn.Open();
                NpgsqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "delete from svod2.subjectfinegrained where form=:formid and subjectuser=:login and subject=:subjectid";
                cmd.Parameters.Add(":formid", NpgsqlDbType.Integer).Value = formid;
                cmd.Parameters.Add(":subjectid", NpgsqlDbType.Integer).Value = subjectid;
                cmd.Parameters.Add(":login", NpgsqlDbType.Varchar).Value = login;
                int res = cmd.ExecuteNonQuery();
                conn.Close();
            }
            return new RedirectToPageResult("/Subject/SubjectFinegrained", new { login, subjectid });
        }

        public IActionResult OnGetEdit(string login, int formid, int subjectid, int permission, int num) 
        {
            try
            {
                using NpgsqlConnection conn = new(connectionString);
                conn.Open();
                NpgsqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "update svod2.subjectfinegrained set permission=:perm where form=:formid and subjectuser=:login and subject=:subjectid";
                cmd.Parameters.Add(":perm", NpgsqlDbType.Smallint).Value = permission;
                cmd.Parameters.Add(":formid", NpgsqlDbType.Integer).Value = formid;
                cmd.Parameters.Add(":login", NpgsqlDbType.Varchar).Value = login;
                cmd.Parameters.Add(":subjectid", NpgsqlDbType.Integer).Value = subjectid;
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
