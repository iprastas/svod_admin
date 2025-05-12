using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Npgsql;
using NpgsqlTypes;
using System.Text;
using System.Xml.Linq;

namespace svod_admin.Pages.RegisterSubject
{
    [Authorize]
    public class EditSubjectModel : PageModel
    {
        [BindProperty] public int SubjectID { get; set; }
        [BindProperty] public List<SelectListItem> Master { get; set; } = [];
        [BindProperty] public int MasterID { get; set; }
        [BindProperty] public string SubjectShortName { get; set; } = "";
        [BindProperty] public string SubjectName { get; set; } = "";
        [BindProperty] public string Ogrn { get; set; } = "";
        [BindProperty] public string Kpp { get; set; } = "";
        [BindProperty] public string Inn { get; set; } = "";
        [BindProperty] public string Okpo { get; set; } = "";
        [BindProperty] public List<SelectListItem> TerritoryWork { get; set; } = [];
        [BindProperty] public int TerritoryWorkID { get; set; }
        [BindProperty] public List<SelectListItem> Okved { get; set; } = [];
        [BindProperty] public int OkvedID { get; set; }
        [BindProperty] public DateTime? SinceDate { get; set; }
        [BindProperty] public DateTime? UptoDate { get; set; }

        string? connectionString;
        readonly IConfiguration? configuration;

        public EditSubjectModel(IConfiguration? _configuration)
        {
            configuration = _configuration;
            connectionString = configuration == null ? string.Empty : configuration.GetConnectionString("DefaultConnection");
        }

        public void OnGet()
        {
            SubjectID = Convert.ToInt32(RouteData.Values["id"]);

            using NpgsqlConnection conn = new(connectionString);
            conn.Open();
            NpgsqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = $"select master,short,name,ogrn,kpp,inn,okpo,territorywork,okved,since,upto " +
                $" from svod2.subject where subject={SubjectID}";
            NpgsqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                if (!reader.IsDBNull(0))
                    MasterID = reader.GetInt32(0);
                if (!reader.IsDBNull(1))
                    SubjectShortName = reader.GetString(1);
                if (!reader.IsDBNull(2))
                    SubjectName = reader.GetString(2);
                if (!reader.IsDBNull(3))
                    Ogrn = reader.GetString(3);
                if (!reader.IsDBNull(4))
                    Kpp = reader.GetString(4);
                if (!reader.IsDBNull(5))
                    Inn = reader.GetString(5);
                if (!reader.IsDBNull(6))
                    Okpo = reader.GetString(6);
                if (!reader.IsDBNull(7))
                    TerritoryWorkID = reader.GetInt32(7);
                if (!reader.IsDBNull(8))
                    OkvedID = reader.GetInt32(8);
                if (!reader.IsDBNull(9))
                    SinceDate = reader.GetDateTime(9);
                if (!reader.IsDBNull(10))
                    UptoDate = reader.GetDateTime(10);
            }
            reader.Close();
            cmd.Dispose();
            conn.Close();

            conn.Open();
            cmd = conn.CreateCommand();
            cmd.CommandText = "select s.subject, coalesce(s.name, s.short) from svod2.subject s where " +
                "coalesce(upto,current_date)>=current_date order by s.subject;";
            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                SelectListItem item = new()
                {
                    Value = reader.GetInt32(0).ToString()
                };
                if (!reader.IsDBNull(1))
                    item.Text = reader.GetInt32(0) + " - " + reader.GetString(1);
                Master.Add(item);
            }
            reader.Close();
            cmd.Dispose();
            conn.Close();

            conn.Open();
            cmd = conn.CreateCommand();
            cmd.CommandText = "select t.territory, t.name from svod2.territory t " +
                " order by t.name;";
            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                SelectListItem item = new()
                {
                    Value = reader.GetInt32(0).ToString()
                };
                if (!reader.IsDBNull(1))
                    item.Text = reader.GetString(1);
                TerritoryWork.Add(item);
            }
            reader.Close();
            cmd.Dispose();
            conn.Close();

            conn.Open();
            cmd = conn.CreateCommand();
            cmd.CommandText = "select b.branch, coalesce(b.name, b.short) from svod2.branch b " +
                "order by b.name;";
            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                SelectListItem item = new()
                {
                    Value = reader.GetInt32(0).ToString()
                };
                if (!reader.IsDBNull(1))
                    item.Text = reader.GetString(1);
                Okved.Add(item);
            }
            reader.Close();
            cmd.Dispose();
            conn.Close();
        }

        public IActionResult OnPostCancel()
        {
            return new RedirectToPageResult("/RegisterSubject/RegisterSubject");
        }

        public IActionResult OnPostSave(int id)
        {
            try
            {
                using NpgsqlConnection conn = new(connectionString);
                conn.Open();

                NpgsqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "update svod2.subject SET short=:shn,name=:n,ogrn=:ogrn,kpp=:kpp,inn=:inn,okpo=:okpo,"
                        + "territorywork=:terw,okved=:okved,since=:since,upto=:upto,changer=:ch where subject=:s";
                cmd.Parameters.Add(":shn", NpgsqlDbType.Varchar).Value = SubjectShortName;
                cmd.Parameters.Add(":n", NpgsqlDbType.Varchar).Value = SubjectName;
                cmd.Parameters.Add(":ogrn", NpgsqlDbType.Varchar).Value = Ogrn != null ? Ogrn : DBNull.Value;
                cmd.Parameters.Add(":kpp", NpgsqlDbType.Varchar).Value = Kpp != null ? Kpp : DBNull.Value;
                cmd.Parameters.Add(":inn", NpgsqlDbType.Varchar).Value = Inn != null ? Inn : DBNull.Value;
                cmd.Parameters.Add(":okpo", NpgsqlDbType.Varchar).Value = Okpo == null ? DBNull.Value : Okpo;
                cmd.Parameters.Add(":terw", NpgsqlDbType.Integer).Value = TerritoryWorkID;
                cmd.Parameters.Add(":okved", NpgsqlDbType.Integer).Value = OkvedID == 0? DBNull.Value : OkvedID;
                cmd.Parameters.Add(":since", NpgsqlDbType.Date).Value = SinceDate != null ? SinceDate : DBNull.Value;
                cmd.Parameters.Add(":upto", NpgsqlDbType.Date).Value = UptoDate != null ? UptoDate : DBNull.Value;
                cmd.Parameters.Add(":ch", NpgsqlDbType.Varchar).Value = "svod_admin";
                cmd.Parameters.Add(":s", NpgsqlDbType.Integer).Value = id;
                cmd.ExecuteNonQuery();
                conn.Close();

                string mess = $"Предприятие {id} - {SubjectShortName} успешно изменено.";
                return new JsonResult(new { result = true, message = mess });
            }
            catch (NpgsqlException ex)
            {
                string mess = $"Действие отменено.\nПроизошла ошибка {ex.Message} errcode = {ex.ErrorCode}.";
                return new JsonResult(new { result = false, message = mess });
            }
        }
    }
}
