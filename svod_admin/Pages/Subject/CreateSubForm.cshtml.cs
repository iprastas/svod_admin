using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using NpgsqlTypes;
using Npgsql;
using static svod_admin.Pages.Target.CreateTarFormModel;

namespace svod_admin.Pages.Subject
{
    [Authorize]
    public class CreateSubFormModel : PageModel
    {
        [BindProperty] public int SubjectID { get; set; }
        [BindProperty] public string Login { get; set; } = "";
        [BindProperty] public int Permission { get; set; } = 0;
        [BindProperty] public FormsPermission Forms { get; set; }

        string? ConnectionString { get; } = string.Empty;

        public ICollection<SelectListItem> Items = [];
        readonly IConfiguration? configuration;

        public CreateSubFormModel(IConfiguration? _configuration)
        {
            configuration = _configuration;
            ConnectionString = configuration == null ? string.Empty : configuration.GetConnectionString("DefaultConnection");
            Forms = new FormsPermission();
        }

        public class FormsPermission
        {
            public IEnumerable<string> forms { get; set; } = [];
        }

        public void OnGet()
        {
            using NpgsqlConnection conn = new (ConnectionString);
            conn.Open();
            NpgsqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = $"select form,substring(coalesce(name, short) from 0 for 150) from svod2.form where form>0 and "
            + " coalesce(upto,current_date)>=current_date and form not in(select form from svod2.subjectfinegrained  "
            + $" where subjectuser='{RouteData.Values["login"]}' and subject='{RouteData.Values["subjectid"]}') order by form;";
            NpgsqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                SelectListItem item = new();
                item.Value = reader.GetInt32(0).ToString();
                if (!reader.IsDBNull(1)) item.Text = reader.GetString(1);
                Items.Add(item);
            }
            cmd.Dispose();
            conn.Close();
        }
        public IActionResult OnPostCancel()
        {
            return new RedirectToPageResult("/Subject/SubjectFinegrained", new { Login, SubjectID });
        }

        public IActionResult OnPostCreate()
        {
            using (NpgsqlConnection conn = new(ConnectionString))
            {
                conn.Open();
                NpgsqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "insert into svod2.subjectfinegrained(form,subject,subjectuser,permission) values(:f,:s,:u,:p)";
                cmd.Parameters.Add(":f", NpgsqlDbType.Integer);
                cmd.Parameters.Add(":s", NpgsqlDbType.Integer).Value = Convert.ToInt32(RouteData.Values["subjectid"] as string);
                cmd.Parameters.Add(":u", NpgsqlDbType.Varchar).Value = Login;
                cmd.Parameters.Add(":p", NpgsqlDbType.Integer).Value = Permission;
                foreach (var item in Forms.forms)
                {
                    cmd.Parameters[":f"].Value = int.Parse(item);
                    cmd.ExecuteNonQuery();
                }
                cmd.Dispose();
                conn.Close();
            }
            return new RedirectToPageResult("/Subject/SubjectFinegrained", new { Login, SubjectID });
        }
    }
}
