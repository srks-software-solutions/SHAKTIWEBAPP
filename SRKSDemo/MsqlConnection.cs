using System;
using System.Configuration;
using System.Data.SqlClient;

namespace SRKSDemo
{
    public  class MsqlConnection : IDisposable
    {
       public static string serverName = ConfigurationManager.AppSettings["ServerName"];
       public static string dataBaseName = ConfigurationManager.AppSettings["Database"];
        public static string userName = ConfigurationManager.AppSettings["user"];
        public static string passWord = ConfigurationManager.AppSettings["password"];
        public static string DbName= ConfigurationManager.AppSettings["databasename"];
        ////Server
        static string servername = serverName;//@"rmlabkm12400\ranesqlexp17ifac";
        static string username = userName;
        static string password = passWord;
        static string db = dataBaseName;

        ////Titan
        //static String servername = @"TCLHSRPECDT0142\TITANBOMMASQLSVR";
        //static String username = "i-facilityuser";
        //static String password = "srks4$teal";
        //static String db = "i_facility_teal";


        ////local host
        //static string servername = @"PAVANKUMARV013\SQL2017EXPDELL";
        //static string username = "sa";
        //static string password = "srks4$";
        //static string db = "i_facility_shakti";

        //public sqlconnection msqlconnection = new sqlconnection(@"data source = " + servername + ";user id = " + username + ";password = " + password + ";initial catalog = " + db + ";persist security info=true");

        public SqlConnection msqlConnection = new SqlConnection(@"Data Source = " + servername + ";User ID = " + username + ";Password = " + password + ";Initial Catalog = " + db + ";Persist Security Info=True");

        public void open()
        {
            if (msqlConnection.State != System.Data.ConnectionState.Open)
                msqlConnection.Open();
        }

        public void close()
        {
            msqlConnection.Close();
        }

        public void Dispose()
        { }
    }
}