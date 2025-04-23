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
            SubjectList = Pg.SubjectList;
            return new JsonResult(new { list = SubjectList });
        }

        public IActionResult OnGetShowCloseSub()
        {
            CloseSubjectList = Pg.CloseSubjectList;
            return new JsonResult(new { list = CloseSubjectList });
        }
    }
}
