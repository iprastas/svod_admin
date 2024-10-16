using System.Data.Common;
using Npgsql;

namespace svod_admin
{
    public class Pg
    {
        IConfiguration config;
        public static string? connStr;
        public static readonly Dictionary<int, string> forms = [];
        public static Dictionary<int, bool> formsbool = [];

        public static readonly Random random = new Random();
        public const string UppercaseLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public const string LowercaseLetters = "abcdefghijklmnopqrstuvwxyz";
        public const string Numbers = "0123456789";
        public const string SpecialCharacters = "!@#$%^&*()_-+=<>?";

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
                bool fl = false;

                forms.Add(i, s);
                formsbool.Add(i, fl);
            }
            conn.Close();
        }

    }
}
