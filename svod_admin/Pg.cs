using System.Data.Common;
using Npgsql;

namespace svod_admin
{
    public class Pg
    {
        IConfiguration config;
        public static string? connStr;
        public static readonly Dictionary<int, string> forms =new();

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
        }

    }
}
