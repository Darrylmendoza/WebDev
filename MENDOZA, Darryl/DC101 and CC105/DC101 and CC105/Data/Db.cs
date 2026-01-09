using MySql.Data.MySqlClient;

namespace DC101_and_CC105.Data
{
    public class Db
    {
        private string connectionString = "server=localhost;database=studentinfo;user=root;password=Finals;";
        public MySqlConnection GetConnection()
        {
            return new MySqlConnection(connectionString);
        }
    }
}
