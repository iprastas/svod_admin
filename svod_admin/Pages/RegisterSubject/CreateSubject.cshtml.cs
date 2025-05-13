using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Npgsql;
using NpgsqlTypes;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml.Linq;

namespace svod_admin.Pages.RegisterSubject
{
    [Authorize]
    public class CreateSubjectModel : PageModel
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
        [BindProperty] public string Username { get; set; } = "";
        [BindProperty] public DateTime? ChangeDate { get; set; }

        string? connectionString;
        readonly IConfiguration? configuration;

        public CreateSubjectModel(IConfiguration? _configuration)
        {
            configuration = _configuration;
            connectionString = configuration == null ? string.Empty : configuration.GetConnectionString("DefaultConnection");
        }

        public void OnGet()
        {
            using NpgsqlConnection conn = new(connectionString);
            conn.Open();
            NpgsqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = "select max(subject) from svod2.subject";
            NpgsqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                SubjectID = reader.GetInt32(0) + 1;
            }
            reader.Close();
            cmd.Dispose();
            conn.Close();

            SelectListItem startitem = new()
            {
                Value = "0",
                Text = "Выберите мастер-предприятие"
            };
            Master.Add(startitem);
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

            TerritoryWork = Pg.TerritoryWork;
            Okved = Pg.Okved;
        }

        public IActionResult OnPostCancel()
        {
            return new RedirectToPageResult("/RegisterSubject/RegisterSubject");
        }

        public IActionResult OnPostCreate(int id)
        {
            try
            {
                StringBuilder ins = new("INSERT INTO svod2.subject(subject,");
                StringBuilder val = new($"VALUES({id},");
                ins.Append(MasterID != 0 ? "master," : ""); val.Append(MasterID != 0? $"{MasterID}," : "");
                ins.Append(SubjectShortName != null ? "short," : ""); val.Append(SubjectShortName != null ? "\'" + SubjectShortName + "\'," : "");
                ins.Append(SubjectName != null ? "name," : ""); val.Append(SubjectName != null ? "\'" + SubjectName + "\'," : "");
                ins.Append(Ogrn != null ? "ogrn," : ""); val.Append(Ogrn != null ? "\'" + Ogrn + "\'," : "");
                ins.Append(Kpp != null ? "kpp," : ""); val.Append(Kpp != null ? "\'" + Kpp + "\'," : "");
                ins.Append(Inn != null ? "inn," : ""); val.Append(Inn != null ? "\'" + Inn + "\'," : "");
                ins.Append(Okpo != null ? "okpo," : ""); val.Append(Okpo != null ? "\'" + Okpo + "\'," : "");
                ins.Append("territorywork,"); val.Append($"{TerritoryWorkID},");
                ins.Append(OkvedID != 0 ? "okved," : ""); val.Append(OkvedID != 0? $"{OkvedID}," : "");
                ins.Append(SinceDate != null ? "since," : ""); val.Append(SinceDate != null ? "\'" + SinceDate + "\'," : "");
                ins.Append(UptoDate != null ? "upto," : ""); val.Append(UptoDate != null ? "\'" + UptoDate + "\'," : "");
                ins.Append("username) "); val.Append("\'svod_admin\');");

                string text = ins.ToString() + val.ToString();

                using NpgsqlConnection conn = new(connectionString);
                conn.Open();

                NpgsqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = text;
                cmd.ExecuteNonQuery();
                conn.Close();

                string mess = $"Предприятие {id} успешно создано.";
                return new JsonResult(new { result = true, message = mess });
            }
            catch (NpgsqlException ex) {
                string mess = $"Действие отменено.\nПроизошла ошибка {ex.Message} errcode = {ex.ErrorCode}.";
                return new JsonResult(new { result = false, message = mess });
            }
        }
    }
}
