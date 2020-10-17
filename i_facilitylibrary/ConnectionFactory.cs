using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i_facilitylibrary
{
    public class ConnectionFactory : IConnectionFactory
    {
      //  //static String ServerName = @"TCP:TSAL-DAS\TSALSQLEXPDAS"; // Server
      //  static String ServerName = @"TCP:10.20.10.65,1433"; // Server
      //  //static String ServerName = @"SRKSDEV001-PC\SQLSERVER17";
      //  //static String ServerName = @"PAVANKUMARV013\SQL2017EXPDELL"; //Localhost
      //  static String username = "sa";
      //static String password = "srks4$tsal";//server
      ////  static String password = "srks4$";
      //  static String port = "3306";
      //  static String DB = "i_facility_tsal"; //Common


        public static String ServerName = @"" + ConfigurationManager.AppSettings["ServerName"]; //SIEMENS\SQLEXPRESS
        public static String username = ConfigurationManager.AppSettings["user"]; //sa
                                                                                      //static String password = "srks4$";//server
        public static String password = ConfigurationManager.AppSettings["password"];
        public static String port = "3306";
        public static String DB = ConfigurationManager.AppSettings["Database"];// i_facility_tsal //Common
        public static String Schema = ConfigurationManager.AppSettings["Schema"];  //Schema Name
        public static String DbName = ConfigurationManager.AppSettings["databasename"];

        public static String DBConfig = "i_facility_configuration";
        public static String DBLive = "i_facility_live";
        public static string DBDashboard = "i_facility_dashboard";
        public static string DBHistory = "i_facility_history";

        public readonly string connectionString = @"Data Source = " + ServerName + "; User ID = " + username + "; Password = " + password + ";Initial Catalog = " + DB + "; Persist Security Info=True;Connection TimeOut=60;";

        public readonly string connectionStringConfig = @"Data Source = " + ServerName + "; User ID = " + username + "; Password = " + password + ";Initial Catalog = " + DBConfig + "; Persist Security Info=True";
        public readonly string connectionStringLive = @"Data Source = " + ServerName + "; User ID = " + username + "; Password = " + password + ";Initial Catalog = " + DBLive + "; Persist Security Info=True";
        public readonly string connectionStringDashboard = @"Data Source = " + ServerName + "; User ID = " + username + "; Password = " + password + ";Initial Catalog = " + DBDashboard + "; Persist Security Info=True";
        public readonly string connectionStringHistory = @"Data Source = " + ServerName + "; User ID = " + username + "; Password = " + password + ";Initial Catalog = " + DBHistory + ";Persist Security Info=True";

        public IDbConnection GetConnection
        {

            get
            {
                var factory = DbProviderFactories.GetFactory("System.Data.SqlClient");
                var conn = factory.CreateConnection();
                conn.ConnectionString = connectionString;
                conn.Open();
                return conn;
            }
        }

        public IDbConnection GetConnectionConfig
        {

            get
            {
                var factory = DbProviderFactories.GetFactory("System.Data.SqlClient");
                var conn = factory.CreateConnection();
                conn.ConnectionString = connectionStringConfig;
                conn.Open();
                return conn;
            }
        }

        public IDbConnection GetConnectionLive
        {
            get
            {
                var factory = DbProviderFactories.GetFactory("System.Data.SqlClient");
                var conn = factory.CreateConnection();
                conn.ConnectionString = connectionStringLive;
                conn.Open();
                return conn;
            }
        }

        public IDbConnection GetConnectionHistory
        {
            get
            {
                var factory = DbProviderFactories.GetFactory("System.Data.SqlClient");
                var conn = factory.CreateConnection();
                conn.ConnectionString = connectionStringHistory;
                conn.Open();
                return conn;
            }
        }

        public IDbConnection GetConnectionDashboard
        {
            get
            {
                var factory = DbProviderFactories.GetFactory("System.Data.SqlClient");
                var conn = factory.CreateConnection();
                conn.ConnectionString = connectionStringDashboard;
                conn.Open();
                return conn;
            }
        }
    }
}
