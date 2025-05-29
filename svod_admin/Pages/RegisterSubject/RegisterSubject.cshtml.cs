using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using svod_admin.Pages.Form;
using Npgsql;
using NpgsqlTypes;
using System.Reflection;

namespace svod_admin.Pages.RegisterSubject
{
    [Authorize]
    public class RegisterSubjectModel : PageModel
    {
        [BindProperty] public int SubjectID { get; set; }
        [BindProperty] public int MasterID { get; set; }
        [BindProperty] public string MasterName { get; set; } = "";
        [BindProperty] public string SubjectName { get; set; } = "";
        [BindProperty] public string Ogrn { get; set; } = "";
        [BindProperty] public string Kpp { get; set; } = "";
        [BindProperty] public string Inn { get; set; } = "";
        [BindProperty] public string Okpo { get; set; } = "";
        [BindProperty] public string TerritoryWork { get; set; } = "";
        [BindProperty] public int TerritoryWorkID { get; set; }
        [BindProperty] public string Okved { get; set; } = "";
        [BindProperty] public int OkvedID { get; set; }
        [BindProperty] public DateTime? SinceDate { get; set; }
        [BindProperty] public DateTime? UptoDate { get; set; }
        [BindProperty] public string Username { get; set; } = "";
        [BindProperty] public DateTime? ChangeDate { get; set; }
        [BindProperty] public string PhoneNum { get; set; } = "";

        public List<RegisterSubjectModel> SubjectList = new();
        public List<RegisterSubjectModel> CloseSubjectList = new();

        public void OnGet()
        {
            OnGetShowOpenSub();
        }

        public IActionResult OnPostCreate()
        {
            return Redirect("/RegisterSubject/CreateSubject");
        }

        public IActionResult OnPostEdit(int id)
        {
            return new RedirectToPageResult("/RegisterSubject/EditSubject", new { id });
        }

        public IActionResult OnGetShowOpenSub()
        {
            using NpgsqlConnection conn = new(Pg.connStr);
            conn.Open();
            NpgsqlCommand commOpen = conn.CreateCommand();
            commOpen.CommandText = "select s.subject, s.master, " +
                " (select coalesce(short,name) from svod2.subject where subject=s.master) mastername, " +
                " coalesce(s.short,s.name), s.ogrn, s.kpp, s.inn, s.okpo, s.territorywork, t.name, " +
                " s.okved, coalesce(b.short,b.name), s.since, s.upto, s.username, s.changedate, s.phone " +
                " from svod2.subject s " +
                " left join svod2.territory t on s.territorywork = t.territory " +
                " left join svod2.branch b on s.okved = b.branch " +
                " where coalesce(s.upto, current_date)>= current_date " +
                " order by s.subject;";
            NpgsqlDataReader reader = commOpen.ExecuteReader();
            while (reader.Read())
            {
                RegisterSubjectModel user = new();
                if (!reader.IsDBNull(0))
                    user.SubjectID = reader.GetInt32(0);
                if (!reader.IsDBNull(1))
                    user.MasterID = reader.GetInt32(1);
                if (!reader.IsDBNull(2))
                    user.MasterName = reader.GetString(2);
                if (!reader.IsDBNull(3))
                    user.SubjectName = reader.GetString(3);
                if (!reader.IsDBNull(4))
                    user.Ogrn = reader.GetString(4);
                if (!reader.IsDBNull(5))
                    user.Kpp = reader.GetString(5);
                if (!reader.IsDBNull(6))
                    user.Inn = reader.GetString(6);
                if (!reader.IsDBNull(7))
                    user.Okpo = reader.GetString(7);
                if (!reader.IsDBNull(8))
                    user.TerritoryWorkID = reader.GetInt32(8);
                if (!reader.IsDBNull(9))
                    user.TerritoryWork = reader.GetString(9);
                if (!reader.IsDBNull(10))
                    user.OkvedID = reader.GetInt32(10);
                if (!reader.IsDBNull(11))
                    user.Okved = reader.GetString(11);
                if (!reader.IsDBNull(12))
                    user.SinceDate = reader.GetDateTime(12);
                if (!reader.IsDBNull(13))
                    user.UptoDate = reader.GetDateTime(13);
                if (!reader.IsDBNull(14))
                    user.Username = reader.GetString(14);
                if (!reader.IsDBNull(15))
                    user.ChangeDate = reader.GetDateTime(15);
                if (!reader.IsDBNull(16))
                    user.PhoneNum = reader.GetString(16);

                SubjectList.Add(user);
            }
            conn.Close();

            return new JsonResult(new { list = SubjectList });
        }

        public IActionResult OnGetShowCloseSub()
        {
            using NpgsqlConnection conn = new(Pg.connStr);
            conn.Open();
            NpgsqlCommand commClose = conn.CreateCommand();
            commClose.CommandText = "select s.subject, s.master, " +
                " (select coalesce(short,name) from svod2.subject where subject=s.master) mastername, " +
                " coalesce(s.short,s.name), s.ogrn, s.kpp, s.inn, s.okpo, s.territorywork, t.name, " +
                " s.okved, coalesce(b.short,b.name), s.since, s.upto, s.username, s.changedate, s.phone " +
                " from svod2.subject s " +
                " left join svod2.territory t on s.territorywork = t.territory " +
                " left join svod2.branch b on s.okved = b.branch " +
                " where coalesce(s.upto, current_date) < current_date " +
                " order by s.subject;";
            NpgsqlDataReader reader = commClose.ExecuteReader();
            while (reader.Read())
            {
                RegisterSubjectModel user = new();
                if (!reader.IsDBNull(0))
                    user.SubjectID = reader.GetInt32(0);
                if (!reader.IsDBNull(1))
                    user.MasterID = reader.GetInt32(1);
                if (!reader.IsDBNull(2))
                    user.MasterName = reader.GetString(2);
                if (!reader.IsDBNull(3))
                    user.SubjectName = reader.GetString(3);
                if (!reader.IsDBNull(4))
                    user.Ogrn = reader.GetString(4);
                if (!reader.IsDBNull(5))
                    user.Kpp = reader.GetString(5);
                if (!reader.IsDBNull(6))
                    user.Inn = reader.GetString(6);
                if (!reader.IsDBNull(7))
                    user.Okpo = reader.GetString(7);
                if (!reader.IsDBNull(8))
                    user.TerritoryWorkID = reader.GetInt32(8);
                if (!reader.IsDBNull(9))
                    user.TerritoryWork = reader.GetString(9);
                if (!reader.IsDBNull(10))
                    user.OkvedID = reader.GetInt32(10);
                if (!reader.IsDBNull(11))
                    user.Okved = reader.GetString(11);
                if (!reader.IsDBNull(12))
                    user.SinceDate = reader.GetDateTime(12);
                if (!reader.IsDBNull(13))
                    user.UptoDate = reader.GetDateTime(13);
                if (!reader.IsDBNull(14))
                    user.Username = reader.GetString(14);
                if (!reader.IsDBNull(15))
                    user.ChangeDate = reader.GetDateTime(15);
                if (!reader.IsDBNull(16))
                    user.PhoneNum = reader.GetString(16);

                CloseSubjectList.Add(user);
            }
            conn.Close();

            return new JsonResult(new { list = CloseSubjectList });
        }
    }
}
