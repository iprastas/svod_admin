using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using NpgsqlTypes;

namespace svod_admin.Pages.Territory
{
    [Authorize]
    public class CreateTerFormModel : PageModel
    {
        [BindProperty] public int TerritoryID { get; set; }
        [BindProperty] public string Login { get; set; } = "";
        [BindProperty] public int Permission { get; set; } = 0;
        [BindProperty] public FormsPermission Forms { get; set; }

        string? ConnectionString { get; } = string.Empty;

        public ICollection<SelectListItem> Items = new List<SelectListItem>();
        readonly IConfiguration? configuration;

        public CreateTerFormModel(IConfiguration? _configuration)
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
            using (Npgsql.NpgsqlConnection conn = new Npgsql.NpgsqlConnection(ConnectionString))
            {
                conn.Open();
                Npgsql.NpgsqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = $"select form,substring(coalesce(name, short) from 0 for 150) from svod2.form where form>0 and "
                + " coalesce(upto,current_date)>=current_date and form not in(select form from svod2.territoryfinegrained  "
                + $" where territoryusers='{RouteData.Values["login"]}' and territory='{RouteData.Values["territoryid"]}') order by form;";
                Npgsql.NpgsqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    SelectListItem item = new SelectListItem();
                    item.Value = reader.GetInt32(0).ToString();
                    if (!reader.IsDBNull(1)) item.Text = reader.GetString(1);
                    Items.Add(item);
                }
                cmd.Dispose();
                conn.Close();
            }
        }
        public IActionResult OnPostCancel()
        {
            return new RedirectToPageResult("/Territory/TerritoryFinegrained", new { Login, TerritoryID });
        }

        public IActionResult OnPostCreate()
        {
            using (Npgsql.NpgsqlConnection conn = new Npgsql.NpgsqlConnection(ConnectionString))
            {
                conn.Open();
                Npgsql.NpgsqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "insert into svod2.territoryfinegrained(form,territory,territoryusers,permission) values(:f,:t,:u,:p)";
                cmd.Parameters.Add(":f", NpgsqlDbType.Integer);
                cmd.Parameters.Add(":t", NpgsqlDbType.Integer).Value = Convert.ToInt32(RouteData.Values["territoryid"] as string);
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
            return new RedirectToPageResult("/Territory/TerritoryFinegrained", new { Login, TerritoryID });
        }
    }
}
