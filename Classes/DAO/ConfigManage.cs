using System.Configuration;

namespace SneakerIcon.Classes.DAO
{
    public static class ConfigManage
    {
        //public static string LogFile { get; private set; }
        public static string Server { get; private set; }
        public static string Database { get; private set; }
        public static string Username { get; private set; }
        public static string Password { get; private set; }
        public static bool ResetDatabase { get; private set; }


        public static void GetConfig()
        {
            var appSettings = ConfigurationManager.AppSettings;
            Server = appSettings["server"];
            Database = appSettings["database"];
            Username = appSettings["username"];
            Password = appSettings["password"];
            ResetDatabase = appSettings["resetDatabase"].Equals("1");

        }
    }
}
