using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Npgsql;
namespace svod_admin.Pages.Subject
{
    [Authorize]
    public class SubjectUsersModel : PageModel
    {
        public List<User> list = new();
        readonly IConfiguration configuration;
        public SubjectUsersModel(IConfiguration _configuration)
        {
            configuration = _configuration;
        }
        public void OnGet()
        {
            string? connectionString  = configuration.GetConnectionString("DefaultConnection");
            using NpgsqlConnection conn = new (connectionString);
            conn.Open();
            NpgsqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = "select s.subject,s.inn login,u.password,u.passwordupto,ty.name,s.short,u.note,u.myformkinds,u.myforms,u.icanflags,s.changer,s.username,s.changedate "
            + " from svod2.subject s " +
            " left outer join svod2.subjectusers u on u.subject = s.subject and coalesce(s.upto,current_date)>=current_date " +
            " left outer join svod2.territory ty on s.territorywork = ty.territory " +
            " order by ty.name;";
            NpgsqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                User user = new();
                if (!reader.IsDBNull(0))
                    user.id = reader.GetInt32(0);
                if (!reader.IsDBNull(1))
                    user.login = reader.GetString(1);

                if (!reader.IsDBNull(2))
                    user.password = reader.GetString(2);
                if (!reader.IsDBNull(3))
                    user.passwordupto = reader.GetDateTime(3);
                if (!reader.IsDBNull(4))
                    user.territory = reader.GetString(4);
                if (!reader.IsDBNull(5))
                    user.subjectname = reader.GetString(5);
                if (!reader.IsDBNull(6))
                    user.note = reader.GetString(6);
                if (!reader.IsDBNull(7))
                    user.myformkinds = reader.GetString(7);
                if (!reader.IsDBNull(8))
                    user.myforms = reader.GetString(8);
                if (!reader.IsDBNull(9))
                    user.icanflags = (ulong)reader.GetInt64(9);
                if (!reader.IsDBNull(10))
                    user.changer = reader.GetString(10);
                if (!reader.IsDBNull(11))
                    user.username = reader.GetString(11);
                if (!reader.IsDBNull(12))
                    user.changedate = reader.GetDateTime(12);

                list.Add(user);
            }
            conn.Close();
        }
        public IActionResult OnPostEdit(string login, int id)
        {
            return new RedirectToPageResult("/Subject/EditSubUser", new { login, id });
        }

        public IActionResult OnGetDelete(string login)
        {
            string? connectionString = configuration.GetConnectionString("DefaultConnection");
            using NpgsqlConnection conn = new(connectionString);
            conn.Open();
            using NpgsqlTransaction transaction = conn.BeginTransaction();
            try
            {
                string delFinegrained = $"delete from svod2.subjectfinegrained where subjectuser = \'{login}\'";
                using NpgsqlCommand DelFinegrained = new(delFinegrained, conn);
                DelFinegrained.Transaction = transaction;
                DelFinegrained.ExecuteNonQuery();

                string delUser = $"delete from svod2.subjectusers where login = \'{login}\'";
                using NpgsqlCommand DelUser = new(delUser, conn);
                DelUser.Transaction = transaction;
                DelUser.ExecuteNonQuery();

                transaction.Commit();

                string mess = "������������ ������� ������.";
                return new JsonResult(new { result = true, message = mess });
            }
            catch (NpgsqlException e)
            {
                transaction.Rollback();

                string mess = $"�������� ��������.\n��������� ������ {e.Message} errcode = {e.ErrorCode}.";
                return new JsonResult(new { result = false, message = mess });
            }
        }

        public IActionResult OnPostForm(string login, int subjectid)
        {
            TempData["Login"] = login;
            TempData["SubjectID"] = subjectid;
            return RedirectToPage("/Subject/SubjectFinegrained");
        }
    }
}
