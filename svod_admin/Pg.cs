using System.Data.Common;
using Npgsql;
using svod_admin.Pages.RegisterSubject;

namespace svod_admin
{
    public class Pg
    {
        IConfiguration config;
        public static string? connStr;
        public static readonly Dictionary<int, string> forms =new();
        public static List<RegisterSubjectModel> SubjectList = new();
        public static List<RegisterSubjectModel> CloseSubjectList = new();

        public static readonly Random random = new Random();
        public const string UppercaseLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public const string LowercaseLetters = "abcdefghijklmnopqrstuvwxyz";
        public const string Numbers = "0123456789";
        public const string SpecialCharacters = "!@#$%^&*()_-+=<>?";

        public static Dictionary<ulong, string> icanflags = new ()
        {
            { 0x00000001U, "я могу изменить введённые данные моего субъекта"},
            { 0x00000002U, "я могу изменить данные моих подчинённых субъектов"},
            { 0x00000004U, "я могу изменить данные субъектов моей территории"},
            { 0x00000008U, "я могу изменить данные любых субъектов"},
            { 0x00000010U, "я могу смотреть вводные формы данных"},
            { 0x00000020U, "я могу смотреть аналитические формы данных"},
            { 0x00000040U, "я могу смотреть первичные данные"},
            { 0x00000100U, "я могу редактировать реестр территорий"},
            { 0x00000200U, "я могу редактировать реестр субъектов"},
            { 0x00000400U, "я могу редактировать реестр объектов"},
            { 0x10000000U, "я могу копировать данные"},
            { 0x20000000U, "я могу редактировать пользователей и их права"},
            { 0x40000000U, "я могу редактировать параметры"},
            { 0x80000000U, "я могу смотреть устаревшие формы"},
            { 0x00001000U, "я могу редактировать исторические данные"}
        };

        public Pg(IConfiguration configuration)
        {
            config = configuration;
            connStr = configuration.GetConnectionString("DefaultConnection");

            using NpgsqlConnection conn = new(Pg.connStr);
            conn.Open();
            NpgsqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT formkind, short FROM svod2.formkind Where formkind > 0 and formkind < 5 ORDER BY formkind ASC";
            NpgsqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                int i = reader.GetInt32(0);
                string s = reader.GetString(1);

                forms.Add(i, s);
            }
            conn.Close();

            conn.Open();
            NpgsqlCommand commOpen = conn.CreateCommand();
            commOpen.CommandText = "select s.subject, s.master, " +
                " (select coalesce(s.short,s.name) from svod2.subject where subject=s.master) mastername, " +
                " coalesce(s.short,s.name), s.ogrn, s.kpp, s.inn, s.okpo, s.territorywork, t.name, " +
                " s.okved, coalesce(b.short,b.name), s.since, s.upto, s.username, s.changedate " +
                " from svod2.subject s " +
                " left join svod2.territory t on s.territorywork = t.territory " +
                " left join svod2.branch b on s.okved = b.branch " +
                " where coalesce(s.upto, current_date)>= current_date " +
                " order by s.subject;";
            reader = commOpen.ExecuteReader();
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

                SubjectList.Add(user);
            }
            conn.Close();

            conn.Open();
            NpgsqlCommand commClose = conn.CreateCommand();
            commClose.CommandText = "select s.subject, s.master, " +
                " (select coalesce(s.short,s.name) from svod2.subject where subject=s.master) mastername, " +
                " coalesce(s.short,s.name), s.ogrn, s.kpp, s.inn, s.okpo, s.territorywork, t.name, " +
                " s.okved, coalesce(b.short,b.name), s.since, s.upto, s.username, s.changedate " +
                " from svod2.subject s " +
                " left join svod2.territory t on s.territorywork = t.territory " +
                " left join svod2.branch b on s.okved = b.branch " +
                " where coalesce(s.upto, current_date) < current_date " +
                " order by s.subject;";
            reader = commClose.ExecuteReader();
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

                CloseSubjectList.Add(user);
            }
            conn.Close();
        }

    }
}
