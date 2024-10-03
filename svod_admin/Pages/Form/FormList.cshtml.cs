using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Npgsql;
using NpgsqlTypes;

namespace svod_admin.Pages.Form
{
    [Authorize]
    public class FormListModel : PageModel
    {
        [BindProperty] public int FormID { get; set; }
        [BindProperty] public int MotherID { get; set; }
        [BindProperty] public string MotherName { get; set; } = "";
        [BindProperty] public string FormName { get; set; } = "";
        [BindProperty] public DateTime? FormDate { get; set; }
        [BindProperty] public DateTime? UptoDate { get; set; }

        string? connectionString;
        public List<FormListModel> list = new();
        readonly IConfiguration? configuration;

        public FormListModel(IConfiguration? _configuration)
        {
            configuration = _configuration;
            connectionString = configuration == null ? string.Empty : configuration.GetConnectionString("DefaultConnection");
        }
        public void OnGet()
        {
            using (Npgsql.NpgsqlConnection conn = new Npgsql.NpgsqlConnection(connectionString))
            {
                conn.Open();
                Npgsql.NpgsqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "select form,master,(select coalesce(short,name) from svod2.form where form=f.master),coalesce(short,name),formdate,upto from svod2.form f "
                +"where coalesce(upto, current_date)>= current_date and(select count(*) from svod2.form f1 "
                +"where f1.master = f.form) = 0 order by form";
                Npgsql.NpgsqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    FormListModel user = new(null);
                    if (!reader.IsDBNull(0))
                        user.FormID = reader.GetInt32(0);
                    if (!reader.IsDBNull(1))
                        user.MotherID = reader.GetInt32(1);
                    if (!reader.IsDBNull(2))
                        user.MotherName = reader.GetString(2);
                    if (!reader.IsDBNull(3))
                        user.FormName = reader.GetString(3);
                    if (!reader.IsDBNull(4))
                        user.FormDate = reader.GetDateTime(4); 
                    if (!reader.IsDBNull(5))
                        user.UptoDate = reader.GetDateTime(5);

                    list.Add(user);
                }
                conn.Close();
            }
        }
        public IActionResult OnPostEdit(int formid)
        {
            return new RedirectToPageResult("/Form/EditForm", new {formid});
        }
    }
}
