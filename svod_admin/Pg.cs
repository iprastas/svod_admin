using System.Data.Common;

namespace svod_admin
{
    public class Pg
    {
        IConfiguration config;
        public static string? connStr;
        public static readonly Dictionary<int, string> forms = new Dictionary<int, string>();
        public static Dictionary<int, bool> formsbool = new Dictionary<int, bool>();
        public  Pg(IConfiguration configuration)
        {
            config = configuration;
            connStr = configuration.GetConnectionString("DefaultConnection");

            using (Npgsql.NpgsqlConnection conn = new Npgsql.NpgsqlConnection(Pg.connStr))
            {
                conn.Open();
                Npgsql.NpgsqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT formkind, short FROM svod2.formkind Where formkind > 0 and formkind < 5 ORDER BY formkind ASC";
                Npgsql.NpgsqlDataReader reader = cmd.ExecuteReader();
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
}
