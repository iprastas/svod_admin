using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using NpgsqlTypes;
using Npgsql;

namespace svod_admin.Pages.Target
{
    [Authorize]
    public class CreateTarFormModel : PageModel
    {
        [BindProperty] public string Login { get; set; } = "";
        [BindProperty] public int Permission { get; set; } = 0;
        [BindProperty] public FormsPermission Forms { get; set; }

        string? ConnectionString { get; } = string.Empty;

        public ICollection<SelectListItem> Items = new List<SelectListItem>();
        readonly IConfiguration? configuration;

        public CreateTarFormModel(IConfiguration? _configuration)
        {
            configuration = _configuration;
            ConnectionString = configuration == null ? string.Empty : configuration.GetConnectionString("DefaultConnection");
            Forms = new FormsPermission();
        }

        public class FormsPermission
        {
            public IEnumerable<string> forms { get; set; } = new List<string>();
        }

        public void OnGet()
        {
            using NpgsqlConnection conn = new(ConnectionString);
            conn.Open();
            NpgsqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = $"select form,substring(coalesce(name, short) from 0 for 150) from svod2.form where form>0 and "
            + " coalesce(upto,current_date)>=current_date and form not in(select form from svod2.targetfinegrained  "
            + $" where targetuser='{RouteData.Values["login"]}') order by form;";
            NpgsqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                SelectListItem item = new();
                item.Value = reader.GetInt32(0).ToString();
                if (!reader.IsDBNull(1)) item.Text = "¹" + item.Value + " - " + reader.GetString(1);
                Items.Add(item);
            }
            cmd.Dispose();
            conn.Close();
        }
        public IActionResult OnPostCancel()
        {
            return new RedirectToPageResult("/Target/TargetFinegrained", new { Login });
        }

        public IActionResult OnPostCreate()
        {
            using (NpgsqlConnection conn = new (ConnectionString))
            {
                conn.Open();
                NpgsqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "insert into svod2.targetfinegrained(form,targetuser,permission,changedate) values(:f,:u,:p,:cd)";
                cmd.Parameters.Add(":f",NpgsqlDbType.Integer);
                cmd.Parameters.Add(":u",NpgsqlDbType.Varchar).Value = Login;
                cmd.Parameters.Add(":p",NpgsqlDbType.Integer).Value = Permission;
                cmd.Parameters.Add(":cd",NpgsqlDbType.Date).Value = DateTime.Now;
                foreach (var item in Forms.forms)
                {
                    cmd.Parameters[":f"].Value = int.Parse(item);
                    cmd.ExecuteNonQuery();
                }
                cmd.Dispose();
                conn.Close();
            }
            return new RedirectToPageResult("/Target/TargetFinegrained", new { Login });
        }
        
    }
}
