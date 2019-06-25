using DBControllers.SerieA;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace MSUnitTestCore
{
    public class DBIntegrationTestBase : IDisposable
    {
        public static string ConnectionString
        {
            get;
            set;
        }

        public static string ConnectionStringMaster
        {
            get;
            set;
        }
        public static string TempDBName { get; private set; }

        public static bool ManageError = true;

        [AssemblyInitialize]
        public static void StartUp()
        {

            //DBIntegrationTests.ConnectionString = string.Format("Integrated Security=SSPI;Persist Security Info=False;Initial Catalog={0};Data Source=DESKTOP-393K7QE\\SQLEXPRESS", "SusyLeague");

            /*INIT DB*/
            DBIntegrationTestBase.TempDBName = string.Format("{0}_{1}{2}{3}_{4}{5}",
                "SusyLagueUT",
                DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,
                DateTime.Now.Hour, DateTime.Now.Minute);
            DBIntegrationTestBase.ConnectionStringMaster = "Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=master;Data Source=DESKTOP-393K7QE\\SQLEXPRESS";
            // Create database (drop first, just in case)
            var connection = new SqlConnection(DBIntegrationTestBase.ConnectionStringMaster);
            //var server = new Server(new ServerConnection(connection));
            string dropDBScript = string.Format("IF EXISTS(select * from sys.databases where name='{0}') DROP DATABASE [{0}]", TempDBName);
            //string createDBScript = string.Format("CREATE DATABASE {0}", TempDBName);
            connection.Open();
            using (var command = new SqlCommand(dropDBScript, connection))
            {
                command.ExecuteNonQuery();
            }
            // Run database creation script
            using (var sr = new StreamReader(@"script.sql"))
            {

                string sql = sr.ReadToEnd();
                sql = sql.Replace("[SusyLeague]", string.Format("[{0}]", TempDBName));
                sql = sql.Replace("SusyLeague.mdf", string.Format("{0}.mdf", TempDBName));

                sql = sql.Replace("SusyLeague_log.ldf", string.Format("{0}_log.ldf", TempDBName));


                Regex r = new Regex(@"^(\s|\t)*GO(\s\t)?.*", RegexOptions.Multiline);

                foreach (string s in r.Split(sql))
                {
                    //Skip empty statements, in case of a GO and trailing blanks or something
                    string thisStatement = s.Trim();
                    if (String.IsNullOrEmpty(thisStatement)) continue;

                    using (SqlCommand cmd = new SqlCommand(thisStatement, connection))
                    {
                        cmd.CommandType = CommandType.Text;

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            connection.Close();
            DBIntegrationTestBase.ConnectionString = string.Format("Integrated Security=SSPI;Persist Security Info=False;Initial Catalog={0};Data Source=DESKTOP-393K7QE\\SQLEXPRESS", TempDBName);
            /*FINE INIT DB*/

            ///*INIT DATI*/
            //SquadraDBController c = new SquadraDBController(DBIntegrationTests.ConnectionString);
            //int id1 = c.InsertSquadra("SERIE MINORE");
            //int id2 = c.InsertSquadra("SVINCOLATO");
            //int id3 = c.InsertSquadra("SERIE ESTERA");
            ///*FINE INIT DATI*/
        }

        [AssemblyCleanup]
        public void Dispose()
        {
            string dropDBScript = string.Format("DROP DATABASE [{0}]", TempDBName);
            var connection = new SqlConnection(DBIntegrationTestBase.ConnectionStringMaster);

            connection.Open();
            using (SqlCommand cmd = new SqlCommand(dropDBScript, connection))
            {
                cmd.CommandType = CommandType.Text;

                cmd.ExecuteNonQuery();
            }
            connection.Close();
        }
    }
}
