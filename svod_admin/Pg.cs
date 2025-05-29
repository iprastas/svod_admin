using System.Data.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Npgsql;
using svod_admin.Pages.RegisterSubject;

namespace svod_admin
{
    public class Pg
    {
        IConfiguration config;
        public static string? connStr;
        public static readonly Dictionary<int, string> forms =new();

        public static List<SelectListItem> TerritoryWork { get; set; } = [];
        public static List<SelectListItem> Okved { get; set; } = [];


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
            cmd.CommandText = "select b.branch, b.code, coalesce(b.name, b.short) from svod2.branch b " +
                "order by b.name;";
            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                SelectListItem item = new()
                {
                    Value = reader.GetInt32(0).ToString()
                };
                if (!reader.IsDBNull(1) && !reader.IsDBNull(2))
                    item.Text = reader.GetString(1) + " - " + reader.GetString(2);
                Okved.Add(item);
            }
            reader.Close();
            cmd.Dispose();
            conn.Close();

        }

    }
}
