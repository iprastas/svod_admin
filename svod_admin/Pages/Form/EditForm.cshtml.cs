using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Npgsql;
using NpgsqlTypes;
using System.Reflection;

namespace svod_admin.Pages.Form
{
    [Authorize]
    public class EditFormModel : PageModel
    {
        [BindProperty] public int FormID { get; set; }
        [BindProperty] public int MotherID { get; set; }
        [BindProperty] public string MotherName { get; set; } = "";
        [BindProperty] public string FormName { get; set; } = "";
        [BindProperty] public int Periodic { get; set; }
        [BindProperty] public DateTime? FormDate { get; set; }
        [BindProperty] public DateTime? UptoDate { get; set; }
        [BindProperty] public int Regulations { get; set; }
        [BindProperty] public List<SelectListItem> ListOfRegulations { get; set; } = new List<SelectListItem>();
        [BindProperty] public string? connectionString { get; }

        readonly IConfiguration configuration;
        public EditFormModel(IConfiguration _configuration)
        {
            configuration = _configuration;
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        public void OnGet()
        {
            using (Npgsql.NpgsqlConnection conn = new(connectionString))
            {
                conn.Open();
                Npgsql.NpgsqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "select form,master,(select coalesce(short,name) from svod2.form where form=f.master),"
                + "coalesce(short,name),formdate,upto,periodic,formregulations from svod2.form f "
                + $"where form='{RouteData.Values["formid"]}'";
                Npgsql.NpgsqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    if (!reader.IsDBNull(0))
                        FormID = reader.GetInt32(0);
                    if (!reader.IsDBNull(1))
                        MotherID = reader.GetInt32(1);
                    if (!reader.IsDBNull(2))
                        MotherName = reader.GetString(2);
                    if (!reader.IsDBNull(3))
                        FormName = reader.GetString(3);
                    if (!reader.IsDBNull(4))
                        FormDate = reader.GetDateTime(4);
                    if (!reader.IsDBNull(5))
                        UptoDate = reader.GetDateTime(5);
                    if (!reader.IsDBNull(6))
                        Periodic = reader.GetInt32(6);
                    if (!reader.IsDBNull(7))
                        Regulations = reader.GetInt32(7);
                }
                conn.Close();
            }

            SelectListItem item0 = new("Отсутствует", "0");
            ListOfRegulations.Add(item0);
            using (Npgsql.NpgsqlConnection conn = new(connectionString))
            {
                conn.Open();
                using Npgsql.NpgsqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "select regulation,note from svod2.formregulations where periodic=:p and period is null";
                cmd.Parameters.Add(":p", NpgsqlDbType.Integer).Value = Periodic;

                Npgsql.NpgsqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    SelectListItem item = new();
                    if (!reader.IsDBNull(0))
                    {
                        item.Value = reader[0].ToString();
                        item.Text = reader[1].ToString();
                        ListOfRegulations.Add((SelectListItem)item);
                    }
                }
                reader.Close();
            }
        }

        public IActionResult OnPostCancel()
        {
            return Redirect("/Form/FormList");
        }

        public IActionResult OnPostEdit()
        {
            using (NpgsqlConnection update = new NpgsqlConnection(connectionString))
            {
                update.Open();
                using (NpgsqlCommand cmd = update.CreateCommand())
                {
                    cmd.CommandText = $"update svod2.form SET upto=:date,formregulations=:reg where form=:id";
                    cmd.Parameters.Add(":date", NpgsqlDbType.Date).Value = UptoDate != null ? UptoDate : DBNull.Value;
                    cmd.Parameters.Add(":id", NpgsqlDbType.Integer).Value = FormID;
                    cmd.Parameters.Add(":reg", NpgsqlDbType.Integer).Value = Regulations != 0 ? Regulations : DBNull.Value;
                    cmd.ExecuteNonQuery();
                }
                update.Close();
            }
            return Redirect("/Form/FormList");
        }
        
    }
}
