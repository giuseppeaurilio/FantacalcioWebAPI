using DBControllers.SerieA;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CustomExceptions;
using System.Diagnostics;
using System;
using System.Data.SqlClient;
using System.IO;
using Dapper;
using System.Text.RegularExpressions;
using System.Data;
using Models.SerieA;
using System.Collections.Generic;
using System.Linq;

namespace MSUnitTestCore
{
    [TestClass]
    public class DBIntegrationTests
    {

        private static string ConnectionString
        {
            get;
            set;
        }

        private static string ConnectionStringMaster
        {
            get;
            set;
        }
        public static string TempDBName { get; private set; }

        [ClassInitialize]
        public static void SetUp(TestContext testContext)
        {

            //DBIntegrationTests.ConnectionString = string.Format("Integrated Security=SSPI;Persist Security Info=False;Initial Catalog={0};Data Source=DESKTOP-393K7QE\\SQLEXPRESS", "SusyLeague");

            /*INIT DB*/
            DBIntegrationTests.TempDBName = DataMock.FakeData.GetString(8, false, true, false, false);
            DBIntegrationTests.ConnectionStringMaster = "Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=master;Data Source=DESKTOP-393K7QE\\SQLEXPRESS";
            // Create database (drop first, just in case)
            var connection = new SqlConnection(DBIntegrationTests.ConnectionStringMaster);
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
            DBIntegrationTests.ConnectionString = string.Format("Integrated Security=SSPI;Persist Security Info=False;Initial Catalog={0};Data Source=DESKTOP-393K7QE\\SQLEXPRESS", TempDBName);
            /*FINE INIT DB*/
            
            /*INIT DATI*/
            SquadraDBController c = new SquadraDBController(DBIntegrationTests.ConnectionString);
            int id1 = c.InserisciSquadra("SERIE MINORE");
            int id2 = c.InserisciSquadra("SVINCOLATO");
            int id3 = c.InserisciSquadra("SERIE ESTERA");
            /*FINE INIT DATI*/
        }

        //[ClassCleanup]
        //public static void CleanUp()
        //{
        //    string dropDBScript = string.Format("DROP DATABASE [{0}]", TempDBName);
        //    var connection = new SqlConnection(DBIntegrationTests.ConnectionStringMaster);

        //    connection.Open();
        //    connection.Execute(dropDBScript);
        //    connection.Close();
        //}


        [TestMethod]
        public void InsertStagione()
        {
            StagioneDBController c = new StagioneDBController(DBIntegrationTests.ConnectionString);
            int id = c.InserisciStagione(DataMock.FakeData.GetString(8, false, true, false, false));

            Assert.AreNotEqual(0, id);
        }
        [TestMethod]
        public void InsertStagioneDuplicata()
        {
            try
            {
                StagioneDBController c = new StagioneDBController(DBIntegrationTests.ConnectionString);
                int id = 0;
                string nomeStagione = DataMock.FakeData.GetString(8, false, true, false, false);
                id = c.InserisciStagione(nomeStagione);
                id = c.InserisciStagione(nomeStagione);

                Assert.AreNotEqual(0, id);
            }
            catch (SusyLeagueDBException ex)
            {

                Assert.AreEqual(50007, ex.Code);
            }

        }

        [TestMethod]
        public void InsertGiornataInStagione()
        {
            try
            {
                ////se voglio utilizzare una stagione gia inserita in DB
                //StagioneControllerGet scg = new StagioneControllerGet(DBIntegrationTests.ConnectionString);
                //List<Stagione> stagioniLista = scg.GetAll();
                //Stagione current = stagioniLista.First();
                //int idStagione = current.Id;

                //se voglio inserire una nuova stagione 
                StagioneDBController c = new StagioneDBController(DBIntegrationTests.ConnectionString);
                int id = c.InserisciStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, id);


                DateTime giornataStartDate = DataMock.FakeData.GetDate(new DateTime(2019, 8, 21, 15, 00, 00), 0, 0);
                DateTime giornataEndDate = DataMock.FakeData.GetDate(giornataStartDate, 0, 72);
                int idGiornata = c.InserisciGiornataInStagione(DataMock.FakeData.GetString(8, false, true, false, false),
                    giornataStartDate,
                    giornataEndDate,
                    id);

                Assert.AreNotEqual(0, idGiornata);

            }
            catch (Exception)
            {

                throw;
            }
        }

        [TestMethod]
        public void InsertGiornataMultiplaInStagione()
        {
            try
            {
                StagioneDBController c = new StagioneDBController(DBIntegrationTests.ConnectionString);
                int id = c.InserisciStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, id);

                int numGiornateDaInserire = DataMock.FakeData.GetIntegerNumber(1, 38);
                DateTime giornataStartDate = DataMock.FakeData.GetDate(new DateTime(2019, 8, 21, 15, 00, 00), 0, 0);
                DateTime giornataEndDate = DataMock.FakeData.GetDate(giornataStartDate, 0, 72);
                for (int i = 1; i <= numGiornateDaInserire; i++)
                {
                    if (i > 1)
                    {
                        giornataStartDate = DataMock.FakeData.GetDate(giornataEndDate, 72, 120);
                        giornataEndDate = DataMock.FakeData.GetDate(giornataStartDate, 0, 72);
                    }
                    int idGiornata = c.InserisciGiornataInStagione(DataMock.FakeData.GetString(8, false, true, false, false),
                    giornataStartDate,
                    giornataEndDate,
                    id);
                    Assert.AreNotEqual(0, idGiornata);
                }

            }
            catch (Exception)
            {

                throw;
            }
        }

        [TestMethod]
        public void UpdateGiornata()
        {
            try
            {

                //se voglio inserire una nuova stagione 
                StagioneDBController c = new StagioneDBController(DBIntegrationTests.ConnectionString);
                int id = c.InserisciStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, id);


                DateTime giornataStartDate = DataMock.FakeData.GetDate(new DateTime(2019, 8, 21, 15, 00, 00), 0, 0);
                DateTime giornataEndDate = DataMock.FakeData.GetDate(giornataStartDate, 0, 72);
                int idGiornata = c.InserisciGiornataInStagione(DataMock.FakeData.GetString(8, false, true, false, false),
                    giornataStartDate,
                    giornataEndDate,
                    id);

                Assert.AreNotEqual(0, idGiornata);

                giornataStartDate = DataMock.FakeData.GetDate(new DateTime(2019, 8, 21, 15, 00, 00), 0, 0);
                giornataEndDate = DataMock.FakeData.GetDate(giornataStartDate, 0, 72);
                c.UpdateGiornata(idGiornata, DataMock.FakeData.GetString(8, false, true, false, false), giornataStartDate,
                    giornataEndDate);

            }
            catch (Exception)
            {

                throw;
            }
        }
        [TestMethod]
        public void Error_InsertGiornataDuplicata()
        {
            try
            {
                StagioneDBController c = new StagioneDBController(DBIntegrationTests.ConnectionString);
                int id = c.InserisciStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, id);


                DateTime giornataStartDate = DataMock.FakeData.GetDate(new DateTime(2019, 8, 21, 15, 00, 00), 0, 0);
                DateTime giornataEndDate = DataMock.FakeData.GetDate(giornataStartDate, 0, 72);
                string descrizioneGiornata = DataMock.FakeData.GetString(8, false, true, false, false);
                int idGiornata = c.InserisciGiornataInStagione(descrizioneGiornata,
                    giornataStartDate,
                    giornataEndDate,
                    id);

                idGiornata = c.InserisciGiornataInStagione(descrizioneGiornata,
                   giornataStartDate,
                   giornataEndDate,
                   id);


            }
            catch (SusyLeagueDBException ex)
            {

                Assert.AreEqual(50004, ex.Code);
            }

        }

        [TestMethod]
        public void Error_UpdateGiornataNonEsistente()
        {
            try
            {
                StagioneDBController c = new StagioneDBController(DBIntegrationTests.ConnectionString);
                int id = 0;
                DateTime giornataStartDate = DataMock.FakeData.GetDate(new DateTime(2019, 8, 21, 15, 00, 00), 0, 0);
                DateTime giornataEndDate = DataMock.FakeData.GetDate(giornataStartDate, 0, 72);
                c.UpdateGiornata(id, DataMock.FakeData.GetString(8, false, true, false, false), giornataStartDate,
                    giornataEndDate);

            }
            catch (SusyLeagueDBException ex)
            {

                Assert.AreEqual(50005, ex.Code);
            }

        }

        [TestMethod]
        public void InsertSquadra()
        {
            try
            {
                SquadraDBController c = new SquadraDBController(DBIntegrationTests.ConnectionString);
                int id = c.InserisciSquadra(DataMock.FakeData.GetString(8, false, true, false, false));

                Assert.AreNotEqual(0, id);
            }
            catch (Exception)
            {

                throw;
            }
        }
        [TestMethod]
        public void Error_InsertSquadraDuplicata()
        {
            try
            {
                SquadraDBController c = new SquadraDBController(DBIntegrationTests.ConnectionString);
                int id = 0;
                string nome = DataMock.FakeData.GetString(8, false, true, false, false);
                id = c.InserisciSquadra(nome);
                id = c.InserisciSquadra(nome);

                Assert.AreNotEqual(0, id);
            }
            catch (SusyLeagueDBException ex)
            {

                Assert.AreEqual(50006, ex.Code);
            }
        }
        [TestMethod]
        public void InsertGiocatore()
        {
            try
            {
                GiocatoreDBController c = new GiocatoreDBController(DBIntegrationTests.ConnectionString);
                int id = c.InserisciGiocatore(DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(4, true, false, true, false));

                Assert.AreNotEqual(0, id);
            }
            catch (Exception)
            {

                throw;
            }
        }

        [TestMethod]
        public void Error_InsertGiocatoreDuplicato()
        {
            try
            {
                GiocatoreDBController c = new GiocatoreDBController(DBIntegrationTests.ConnectionString);
                int id = 0;
                string nome = DataMock.FakeData.GetString(8, false, true, false, false);
                string cognome = DataMock.FakeData.GetString(8, false, true, false, false);
                string gazzaId = DataMock.FakeData.GetString(4, true, false, true, false);
                id = c.InserisciGiocatore(nome, cognome, gazzaId);
                id = c.InserisciGiocatore(nome, cognome, gazzaId);


            }
            catch (SusyLeagueDBException ex)
            {

                Assert.AreEqual(50001, ex.Code);
            }
        }

        [TestMethod]
        public void InsertGiocatoreLiberoInSquadra()
        {
            try
            {

                SquadraDBController cs = new SquadraDBController(DBIntegrationTests.ConnectionString);
                int idSquadra = cs.InserisciSquadra(DataMock.FakeData.GetString(8, false, true, false, false));

                GiocatoreDBController cg = new GiocatoreDBController(DBIntegrationTests.ConnectionString);
                int idGiocatore = cg.InserisciGiocatore(DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(4, true, false, true, false));

                SquadraDBController csq = new SquadraDBController(DBIntegrationTests.ConnectionString);
                csq.InserisciGiocatoreInSquadra(idGiocatore, idSquadra,
                     DataMock.FakeData.GetDate(DateTime.Now, 0, 0));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [TestMethod]
        public void DeleteGiocatoreDaSquadra()
        {
            try
            {

                SquadraDBController cs = new SquadraDBController(DBIntegrationTests.ConnectionString);
                int idSquadra = cs.InserisciSquadra(DataMock.FakeData.GetString(8, false, true, false, false));

                GiocatoreDBController cg = new GiocatoreDBController(DBIntegrationTests.ConnectionString);
                int idGiocatore = cg.InserisciGiocatore(DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(4, true, false, true, false));

                SquadraDBController csq = new SquadraDBController(DBIntegrationTests.ConnectionString);
                csq.InserisciGiocatoreInSquadra(idGiocatore, idSquadra,
                     DataMock.FakeData.GetDate(DateTime.Now, 0, 0));

                csq.CancellaGiocatoreDaSquadra(idGiocatore, idSquadra,
                     DataMock.FakeData.GetDate(DateTime.Now, 0, 0));

            }
            catch (Exception)
            {

                throw;
            }
        }

        [TestMethod]
        public void Error_InsertGiocatoreNonLiberoInSquadra()
        {
            try
            {

                SquadraDBController cs = new SquadraDBController(DBIntegrationTests.ConnectionString);
                int idSquadra1 = cs.InserisciSquadra(DataMock.FakeData.GetString(8, false, true, false, false));

                GiocatoreDBController cg = new GiocatoreDBController(DBIntegrationTests.ConnectionString);
                int idGiocatore = cg.InserisciGiocatore(DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(4, true, false, true, false));

                SquadraDBController csq = new SquadraDBController(DBIntegrationTests.ConnectionString);
                csq.InserisciGiocatoreInSquadra(idGiocatore, idSquadra1,
                     DataMock.FakeData.GetDate(DateTime.Now, 0, 0));

                int idSquadra2 = cs.InserisciSquadra(DataMock.FakeData.GetString(8, false, true, false, false));
                csq.InserisciGiocatoreInSquadra(idGiocatore, idSquadra2,
                     DataMock.FakeData.GetDate(DateTime.Now, 0, 0));

            }
            catch (SusyLeagueDBException ex)
            {

                Assert.AreEqual(50014, ex.Code);
            }
        }

        [TestMethod]
        public void InsertGiocatoreInSquadraDaSerieMinore()
        { }
        [TestMethod]
        public void InsertGiocatoreInSquadraDaSvincolato()
        { }

        [TestMethod]
        public void InsertGiocatoreInSquadraDaSerieEstera()
        { }
        [TestMethod]
        public void CambiaGiocatoreInSquadra()
        { }

        [TestMethod]
        public void DeleteGiocatoreDaSquadraDoveNonEsiste()
        { }

        [TestMethod]
        public void InsertVotoDelGiocatore()
        { }

        [TestMethod]
        public void UpdateVotoDelGiocatore()
        { }

        [TestMethod]
        public void InsertVotoDelGiocatoreError()
        { }

        [TestMethod]
        public void UpdateVotoDelGiocatoreError()
        { }

        [TestMethod]
        public void InsertStatisticaDelGiocatore()
        { }

        [TestMethod]
        public void UpdateStatisticaDelGiocatore()
        { }

        [TestMethod]
        public void InsertStatisticaDelGiocatoreError()
        { }

        [TestMethod]
        public void UpdateStatisticaDelGiocatoreError()
        { }

    }
}
