using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NpgsqlTypes;
using Npgsql;

namespace svod_admin.Pages.Territory
{
    [Authorize]
    public class TerritoryUsersModel : PageModel
    {
        public List<User> list = new();
        readonly IConfiguration configuration;
        public TerritoryUsersModel(IConfiguration _configuration)
        {
            configuration = _configuration;
        }
        public void OnGet()
        {
            string? connectionString = configuration.GetConnectionString("DefaultConnection");
            using NpgsqlConnection conn = new(connectionString);
            conn.Open();
            NpgsqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = "select t.territory,k.short||t.name,tu.* from svod2.territory t "
            + " left outer join svod2.territoryusers tu on t.territory=tu.territory"
            + " left outer join svod2.territorykind k on t.territorykind=k.territorykind"
            + " where t.territory > 3 and k.territorykind in(10,12,15,16,17,28,29,35,37,38) order by 1;";
            NpgsqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                User user = new();
                if (!reader.IsDBNull(0))
                    user.id = reader.GetInt32(0);
                if (!reader.IsDBNull(1))
                    user.subjectname = reader.GetString(1);
                if (!reader.IsDBNull(3))
                    user.login = reader.GetString(3);
                if (!reader.IsDBNull(4))
                    user.note = reader.GetString(4);
                if (!reader.IsDBNull(5))
                    user.password = reader.GetString(5);
                if (!reader.IsDBNull(6))
                    user.passwordupto = reader.GetDateTime(6);
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
            return new RedirectToPageResult("/Territory/EditTerUser", new { login, id });
        }

        public IActionResult OnGetDelete(string login, int territory)
        {
            string? connectionString = configuration.GetConnectionString("DefaultConnection");
            using NpgsqlConnection conn = new(connectionString);
            conn.Open();
            using NpgsqlTransaction transaction = conn.BeginTransaction();
            try
            {
                string delFinegrained = $"delete from svod2.territoryfinegrained where territory = :ter";
                using NpgsqlCommand DelFinegrained = new(delFinegrained, conn);
                DelFinegrained.Parameters.Add(":ter", NpgsqlDbType.Integer).Value = territory;
                DelFinegrained.Transaction = transaction;
                DelFinegrained.ExecuteNonQuery();

                string delUser = $"delete from svod2.territoryusers where login = :lg and territory = :ter";
                using NpgsqlCommand DelUser = new(delUser, conn);
                DelUser.Parameters.Add(":ter", NpgsqlDbType.Integer).Value = territory;
                DelUser.Parameters.Add(":lg", NpgsqlDbType.Varchar).Value = login != null ? login : DBNull.Value; ;
                DelUser.Transaction = transaction;
                DelUser.ExecuteNonQuery(); 

                transaction.Commit();

                string mess = "Пользователь успешно удален.";
                return new JsonResult(new { result = true, message = mess });
            }
            catch (NpgsqlException e)
            {
                transaction.Rollback();

                string mess = $"Действие отменено.\nПроизошла ошибка {e.Message} errcode = {e.ErrorCode}.";
                return new JsonResult(new { result = false, message = mess });
            }
        }

        public IActionResult OnPostForm(string login, int territoryid)
        {
            TempData["Login"] = login;
            TempData["TerritoryID"] = territoryid;
            return RedirectToPage("/Territory/TerritoryFinegrained");
        }

        public IActionResult OnPostCreate(int id)
        {
            return new RedirectToPageResult("/Territory/CreateTerUser", new { id });
        }
    }
}
