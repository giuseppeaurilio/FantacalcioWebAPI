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
using System.Collections.Generic;
using System.Linq;
using DBControllers.Membership;


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

        public static bool ManageError = false;
        public static bool TestError = false;

        public static int iCounter { get; private set; }

        public static string ServerName { get; private set; }
        [ClassInitialize]
        public static void SetUp(TestContext testContext)
        {

            //DBIntegrationTests.ConnectionString = string.Format("Integrated Security=SSPI;Persist Security Info=False;Initial Catalog={0};Data Source=DESKTOP-393K7QE\\SQLEXPRESS", "SusyLeague");

            /*INIT DB*/
            DBIntegrationTests.TempDBName = string.Format("{0}_{1}{2}{3}_{4}{5}",
                "SusyLeagueUT",
                DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,
                DateTime.Now.Hour, DateTime.Now.Minute);
            //DBIntegrationTests.ServerName = "DESKTOP-393K7QE\\SQLEXPRESS";
            DBIntegrationTests.ServerName = "DESKTOP-7LGLNDB\\SQLEXPRESS";
            DBIntegrationTests.ConnectionStringMaster = string.Format("Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=master;Data Source={0}", DBIntegrationTests.ServerName);
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
            DBIntegrationTests.ConnectionString = string.Format("Integrated Security=SSPI;Persist Security Info=False;Initial Catalog={0};Data Source={1}",
                DBIntegrationTests.TempDBName, DBIntegrationTests.ServerName);
            /*FINE INIT DB*/

            /*INIT DATI*/
            SquadraDBController c = new SquadraDBController(DBIntegrationTests.ConnectionString);
            int id1 = c.InsertSquadra("SERIE MINORE");
            int id2 = c.InsertSquadra("SVINCOLATO");
            int id3 = c.InsertSquadra("SERIE ESTERA");
            /*FINE INIT DATI*/

            /*init variabili globali di util*/
            iCounter = 0;
            /*FINE init variabili globali di util*/
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
        #region DBControllers.SERIEA
        [TestMethod]
        public void InsertStagione()
        {
            DBControllers.SerieA.StagioneDBController c = new DBControllers.SerieA.StagioneDBController(DBIntegrationTests.ConnectionString);
            int id = c.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));

            Assert.AreNotEqual(0, id);
        }

        [TestMethod]
        [Description("controlla che in caso di inserimento doppio della stagione, venga restituito l'errore 50007 dal DB")]
        public void InsertStagione_Error_Duplicata()
        {
            try
            {
                DBControllers.SerieA.StagioneDBController c = new DBControllers.SerieA.StagioneDBController(DBIntegrationTests.ConnectionString);
                int id = 0;
                string nomeStagione = DataMock.FakeData.GetString(8, false, true, false, false);
                id = c.InsertStagione(nomeStagione);
                //questa istruzione deve generare l'errore
                id = c.InsertStagione(nomeStagione);

                Assert.AreNotEqual(0, id);
            }
            catch (SusyLeagueDBException ex)
            {
                if (ManageError)
                    Assert.AreEqual(50007, ex.Code);
                else if (!TestError) { }
                else
                    throw;
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
                DBControllers.SerieA.StagioneDBController c = new DBControllers.SerieA.StagioneDBController(DBIntegrationTests.ConnectionString);
                int id = c.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, id);


                DateTime giornataStartDate = DataMock.FakeData.GetDate(new DateTime(2019, 8, 21, 15, 00, 00), 0, 0);
                DateTime giornataEndDate = DataMock.FakeData.GetDate(giornataStartDate, 0, 72);
                int idGiornata = c.InsertGiornataInStagione(DataMock.FakeData.GetString(8, false, true, false, false),
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
                DBControllers.SerieA.StagioneDBController c = new DBControllers.SerieA.StagioneDBController(DBIntegrationTests.ConnectionString);
                int id = c.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, id);

                int numGiornateDaInserire = DataMock.FakeData.GetInteger(1, 38);
                DateTime giornataStartDate = DataMock.FakeData.GetDate(new DateTime(2019, 8, 21, 15, 00, 00), 0, 0);
                DateTime giornataEndDate = DataMock.FakeData.GetDate(giornataStartDate, 0, 72);
                for (int i = 1; i <= numGiornateDaInserire; i++)
                {
                    if (i > 1)
                    {
                        giornataStartDate = DataMock.FakeData.GetDate(giornataEndDate, 72, 120);
                        giornataEndDate = DataMock.FakeData.GetDate(giornataStartDate, 0, 72);
                    }
                    int idGiornata = c.InsertGiornataInStagione(DataMock.FakeData.GetString(8, false, true, false, false),
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
                DBControllers.SerieA.StagioneDBController c = new DBControllers.SerieA.StagioneDBController(DBIntegrationTests.ConnectionString);
                int id = c.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, id);


                DateTime giornataStartDate = DataMock.FakeData.GetDate(new DateTime(2019, 8, 21, 15, 00, 00), 0, 0);
                DateTime giornataEndDate = DataMock.FakeData.GetDate(giornataStartDate, 0, 72);
                int idGiornata = c.InsertGiornataInStagione(DataMock.FakeData.GetString(8, false, true, false, false),
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
        [Description("controlla che in caso di inserimento doppio della giornata nella stagione, venga restituito l'errore 50004 dal DB")]
        public void InsertGiornata_Error_Duplicata()
        {
            try
            {
                DBControllers.SerieA.StagioneDBController c = new DBControllers.SerieA.StagioneDBController(DBIntegrationTests.ConnectionString);
                int id = c.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, id);


                DateTime giornataStartDate = DataMock.FakeData.GetDate(new DateTime(2019, 8, 21, 15, 00, 00), 0, 0);
                DateTime giornataEndDate = DataMock.FakeData.GetDate(giornataStartDate, 0, 72);
                string descrizioneGiornata = DataMock.FakeData.GetString(8, false, true, false, false);
                int idGiornata = c.InsertGiornataInStagione(descrizioneGiornata,
                    giornataStartDate,
                    giornataEndDate,
                    id);
                //questa istruzione deve generare l'errore
                idGiornata = c.InsertGiornataInStagione(descrizioneGiornata,
                   giornataStartDate,
                   giornataEndDate,
                   id);

                Assert.AreEqual(0, idGiornata);
            }
            catch (SusyLeagueDBException ex)
            {
                if (ManageError)
                    Assert.AreEqual(50004, ex.Code);
                else if (!TestError) { }
                else
                    throw;
            }

        }

        [TestMethod]
        [Description("controlla che in caso di update di giornata non esistente, venga restituito l'errore 50005 dal DB")]
        public void UpdateGiornata_Error_NOTFOUND()
        {
            try
            {
                DBControllers.SerieA.StagioneDBController c = new DBControllers.SerieA.StagioneDBController(DBIntegrationTests.ConnectionString);
                int id = 0;
                DateTime giornataStartDate = DataMock.FakeData.GetDate(new DateTime(2019, 8, 21, 15, 00, 00), 0, 0);
                DateTime giornataEndDate = DataMock.FakeData.GetDate(giornataStartDate, 0, 72);
                //questa istruzione deve generare l'errore
                c.UpdateGiornata(id, DataMock.FakeData.GetString(8, false, true, false, false), giornataStartDate,
                    giornataEndDate);

            }
            catch (SusyLeagueDBException ex)
            {
                if (ManageError)
                    Assert.AreEqual(50005, ex.Code);
                else if (!TestError) { }
                else
                    throw;
            }

        }

        [TestMethod]
        public void InsertSquadra()
        {
            try
            {
                SquadraDBController c = new SquadraDBController(DBIntegrationTests.ConnectionString);
                int id = c.InsertSquadra(DataMock.FakeData.GetString(8, false, true, false, false));

                Assert.AreNotEqual(0, id);
            }
            catch (Exception)
            {

                throw;
            }
        }

        [TestMethod]
        [Description("controlla che in caso di inserimento doppio di squadra, venga restituito l'errore 50006 dal DB")]
        public void InsertSquadra_Error_Duplicata()
        {
            try
            {
                SquadraDBController c = new SquadraDBController(DBIntegrationTests.ConnectionString);
                int id = 0;
                string nome = DataMock.FakeData.GetString(8, false, true, false, false);
                id = c.InsertSquadra(nome);
                //questa istruzione deve generare l'errore
                id = c.InsertSquadra(nome);

                Assert.AreNotEqual(0, id);
            }
            catch (SusyLeagueDBException ex)
            {
                if (ManageError)
                    Assert.AreEqual(50006, ex.Code);
                else if (!TestError) { }
                else
                    throw;
            }
        }

        [TestMethod]
        public void InsertSquadraInStagione()
        {
            try
            {

                DBControllers.SerieA.StagioneDBController c = new DBControllers.SerieA.StagioneDBController(DBIntegrationTests.ConnectionString);
                int id = c.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, id);


                SquadraDBController cs = new SquadraDBController(DBIntegrationTests.ConnectionString);
                int ids = cs.InsertSquadra(DataMock.FakeData.GetString(8, false, true, false, false));

                Assert.AreNotEqual(0, ids);

                int ret = c.InsertSquadraInStagione(id, ids);
                Assert.AreNotEqual(0, ret);

            }
            catch (Exception)
            {

                throw;
            }
        }

        [TestMethod]
        [Description("controlla che in caso di inserimento doppio di squadra nella stagione, venga restituito l'errore 50016 dal DB")]
        public void InsertSquadraInStagione_Error_Duplicata()
        {
            try
            {

                DBControllers.SerieA.StagioneDBController c = new DBControllers.SerieA.StagioneDBController(DBIntegrationTests.ConnectionString);
                int id = c.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, id);


                SquadraDBController cs = new SquadraDBController(DBIntegrationTests.ConnectionString);
                int ids = cs.InsertSquadra(DataMock.FakeData.GetString(8, false, true, false, false));

                Assert.AreNotEqual(0, ids);

                int ret = c.InsertSquadraInStagione(id, ids);
                Assert.AreNotEqual(0, ret);

                //questa istruzione deve generare l'errore
                ret = c.InsertSquadraInStagione(id, ids);
                Assert.AreEqual(0, ret);

            }
            catch (SusyLeagueDBException ex)
            {
                if (ManageError)
                    Assert.AreEqual(50016, ex.Code);
                else if (!TestError) { }
                else
                    throw;
            }
        }

        [TestMethod]
        public void InsertGiocatore()
        {
            try
            {
                GiocatoreDBController c = new GiocatoreDBController(DBIntegrationTests.ConnectionString);
                int id = c.InsertGiocatore(DataMock.FakeData.GetString(8, false, true, false, false),
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
        [Description("controlla che in caso di inserimento doppio di giocatore, venga restituito l'errore 50001 dal DB")]
        public void InsertGiocatore_Error_Duplicato()
        {
            try
            {
                GiocatoreDBController c = new GiocatoreDBController(DBIntegrationTests.ConnectionString);
                int id = 0;
                string nome = DataMock.FakeData.GetString(8, false, true, false, false);
                string cognome = DataMock.FakeData.GetString(8, false, true, false, false);
                string gazzaId = DataMock.FakeData.GetString(4, true, false, true, false);
                id = c.InsertGiocatore(nome, cognome, gazzaId);
                //questa istruzione deve generare l'errore
                id = c.InsertGiocatore(nome, cognome, gazzaId);

                Assert.AreEqual(0, id);
            }
            catch (SusyLeagueDBException ex)
            {
                if (ManageError)
                    Assert.AreEqual(50001, ex.Code);
                else if (!TestError) { }
                else
                    throw;
            }
        }

        [TestMethod]
        public void InsertGiocatoreLiberoInSquadra()
        {
            try
            {

                SquadraDBController cs = new SquadraDBController(DBIntegrationTests.ConnectionString);
                int idSquadra = cs.InsertSquadra(DataMock.FakeData.GetString(8, false, true, false, false));

                GiocatoreDBController cg = new GiocatoreDBController(DBIntegrationTests.ConnectionString);
                int idGiocatore = cg.InsertGiocatore(DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(4, true, false, true, false));

                SquadraDBController csq = new SquadraDBController(DBIntegrationTests.ConnectionString);
                int ret = csq.InsertGiocatoreInSquadra(idGiocatore, idSquadra,
                      DataMock.FakeData.GetDate(DateTime.Now, 0, 0));
                Assert.AreNotEqual(0, ret);
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
                int idSquadra = cs.InsertSquadra(DataMock.FakeData.GetString(8, false, true, false, false));

                GiocatoreDBController cg = new GiocatoreDBController(DBIntegrationTests.ConnectionString);
                int idGiocatore = cg.InsertGiocatore(DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(4, true, false, true, false));

                SquadraDBController csq = new SquadraDBController(DBIntegrationTests.ConnectionString);
                int ret = csq.InsertGiocatoreInSquadra(idGiocatore, idSquadra,
                     DataMock.FakeData.GetDate(DateTime.Now, 0, 0));
                Assert.AreNotEqual(0, ret);

                csq.DeleteGiocatoreDaSquadra(idGiocatore, idSquadra,
                     DataMock.FakeData.GetDate(DateTime.Now, 0, 0));

            }
            catch (Exception)
            {
                throw;
            }
        }

        [TestMethod]
        [Description("controlla che in caso di inserimento di un giocatore non libero in un'altra squadra, venga restituito l'errore 50014 dal DB")]
        public void InsertGiocatoreInSquadra_Error_GiocatoreNonLibero()
        {
            try
            {

                SquadraDBController cs = new SquadraDBController(DBIntegrationTests.ConnectionString);
                int idSquadra1 = cs.InsertSquadra(DataMock.FakeData.GetString(8, false, true, false, false));

                GiocatoreDBController cg = new GiocatoreDBController(DBIntegrationTests.ConnectionString);
                int idGiocatore = cg.InsertGiocatore(DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(4, true, false, true, false));

                SquadraDBController csq = new SquadraDBController(DBIntegrationTests.ConnectionString);
                int ret = csq.InsertGiocatoreInSquadra(idGiocatore, idSquadra1,
                     DataMock.FakeData.GetDate(DateTime.Now, 0, 0));
                Assert.AreNotEqual(0, ret);

                int idSquadra2 = cs.InsertSquadra(DataMock.FakeData.GetString(8, false, true, false, false));
                //questa istruzione deve generare l'errore
                ret = csq.InsertGiocatoreInSquadra(idGiocatore, idSquadra2,
                     DataMock.FakeData.GetDate(DateTime.Now, 0, 0));

                //non dovrebbbe mai arrivare qui
                Assert.AreEqual(0, ret);

            }
            catch (SusyLeagueDBException ex)
            {
                if (ManageError)
                    Assert.AreEqual(50014, ex.Code);
                else if (!TestError) { }
                else
                    throw;
            }
        }

        [TestMethod]
        public void InsertGiocatoreInSquadraDaAltraSquadra()
        {
            try
            {

                SquadraDBController cs = new SquadraDBController(DBIntegrationTests.ConnectionString);
                int idSquadra = cs.InsertSquadra(DataMock.FakeData.GetString(8, false, true, false, false));

                GiocatoreDBController cg = new GiocatoreDBController(DBIntegrationTests.ConnectionString);
                int idGiocatore = cg.InsertGiocatore(DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(4, true, false, true, false));

                SquadraDBController csq = new SquadraDBController(DBIntegrationTests.ConnectionString);
                int ret = csq.InsertGiocatoreInSquadra(idGiocatore, idSquadra,
                     DataMock.FakeData.GetDate(DateTime.Now, 0, 0));
                Assert.AreNotEqual(0, ret);
                csq.DeleteGiocatoreDaSquadra(idGiocatore, idSquadra,
                     DataMock.FakeData.GetDate(DateTime.Now, 0, 0));

                int idSquadra2 = cs.InsertSquadra(DataMock.FakeData.GetString(8, false, true, false, false));

                int ret2 = csq.InsertGiocatoreInSquadra(idGiocatore, idSquadra2,
                     DataMock.FakeData.GetDate(DateTime.Now, 24, 24));
                Assert.AreNotEqual(0, ret2);
            }
            catch (Exception)
            {

                throw;
            }
        }

        [TestMethod]
        [Description("controlla che in caso di errore durante la Deletezione di un giocatore dalla squadra(Giocatore non trovato), venga restituito l'errore 50002 dal DB")]
        public void DeleteGiocatoreDaSquadra_Error_NOTFOUND()
        {
            try
            {

                SquadraDBController cs = new SquadraDBController(DBIntegrationTests.ConnectionString);
                int idSquadra = cs.InsertSquadra(DataMock.FakeData.GetString(8, false, true, false, false));

                GiocatoreDBController cg = new GiocatoreDBController(DBIntegrationTests.ConnectionString);
                int idGiocatore = cg.InsertGiocatore(DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(4, true, false, true, false));

                SquadraDBController csq = new SquadraDBController(DBIntegrationTests.ConnectionString);
                int ret = csq.InsertGiocatoreInSquadra(idGiocatore, idSquadra,
                     DataMock.FakeData.GetDate(DateTime.Now, 0, 0));
                Assert.AreNotEqual(0, ret);
                //questa istruzione deve generare l'errore
                csq.DeleteGiocatoreDaSquadra(idGiocatore, 0,
                     DataMock.FakeData.GetDate(DateTime.Now, 0, 0));

            }
            catch (SusyLeagueDBException ex)
            {
                if (ManageError)
                    Assert.AreEqual(50002, ex.Code);
                else if (!TestError) { }
                else
                    throw;
            }
        }

        [TestMethod]
        public void InsertVotoDelGiocatore()
        {
            try
            {
                DBControllers.SerieA.StagioneDBController cStagione = new DBControllers.SerieA.StagioneDBController(DBIntegrationTests.ConnectionString);
                int idStagione = cStagione.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idStagione);

                DateTime giornataStartDate = DataMock.FakeData.GetDate(new DateTime(2019, 8, 21, 15, 00, 00), 0, 0);
                DateTime giornataEndDate = DataMock.FakeData.GetDate(giornataStartDate, 0, 72);
                int idGiornata = cStagione.InsertGiornataInStagione(DataMock.FakeData.GetString(8, false, true, false, false),
                    giornataStartDate,
                    giornataEndDate,
                    idStagione);
                Assert.AreNotEqual(0, idGiornata);

                SquadraDBController cSquadra = new SquadraDBController(DBIntegrationTests.ConnectionString);
                int idSquadra = cSquadra.InsertSquadra(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idSquadra);

                GiocatoreDBController cGiocatore = new GiocatoreDBController(DBIntegrationTests.ConnectionString);
                int idGiocatore = cGiocatore.InsertGiocatore(DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(4, true, false, true, false));
                Assert.AreNotEqual(0, idGiocatore);

                int idSquadraGiocatore = cSquadra.InsertGiocatoreInSquadra(idGiocatore, idSquadra,
                      DataMock.FakeData.GetDate(DateTime.Now, 0, 0));
                Assert.AreNotEqual(0, idSquadraGiocatore);

                int idVoto = cGiocatore.InsertVotoDelGiocatore(idGiocatore, idGiornata
                    , DataMock.FakeData.GetDouble()
                    , DataMock.FakeData.GetDouble()
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetBoolean()
                    , DataMock.FakeData.GetBoolean()
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    );
                Assert.AreNotEqual(0, idVoto);
            }
            catch (Exception)
            {

                throw;
            }

        }

        [TestMethod]
        public void UpdateVotoDelGiocatore()
        {
            try
            {
                DBControllers.SerieA.StagioneDBController cStagione = new DBControllers.SerieA.StagioneDBController(DBIntegrationTests.ConnectionString);
                int idStagione = cStagione.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idStagione);

                DateTime giornataStartDate = DataMock.FakeData.GetDate(new DateTime(2019, 8, 21, 15, 00, 00), 0, 0);
                DateTime giornataEndDate = DataMock.FakeData.GetDate(giornataStartDate, 0, 72);
                int idGiornata = cStagione.InsertGiornataInStagione(DataMock.FakeData.GetString(8, false, true, false, false),
                    giornataStartDate,
                    giornataEndDate,
                    idStagione);
                Assert.AreNotEqual(0, idGiornata);

                SquadraDBController cSquadra = new SquadraDBController(DBIntegrationTests.ConnectionString);
                int idSquadra = cSquadra.InsertSquadra(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idSquadra);

                GiocatoreDBController cGiocatore = new GiocatoreDBController(DBIntegrationTests.ConnectionString);
                int idGiocatore = cGiocatore.InsertGiocatore(DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(4, true, false, true, false));
                Assert.AreNotEqual(0, idGiocatore);

                int idSquadraGiocatore = cSquadra.InsertGiocatoreInSquadra(idGiocatore, idSquadra,
                      DataMock.FakeData.GetDate(DateTime.Now, 0, 0));
                Assert.AreNotEqual(0, idSquadraGiocatore);

                int idVoto = cGiocatore.InsertVotoDelGiocatore(idGiocatore, idGiornata
                    , DataMock.FakeData.GetDouble()
                    , DataMock.FakeData.GetDouble()
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetBoolean()
                    , DataMock.FakeData.GetBoolean()
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    );
                Assert.AreNotEqual(0, idVoto);

                cGiocatore.UpdateVotoDelGiocatore(idVoto
                    , DataMock.FakeData.GetDouble()
                    , DataMock.FakeData.GetDouble()
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetBoolean()
                    , DataMock.FakeData.GetBoolean()
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    );
            }
            catch (Exception)
            {

                throw;
            }
        }

        [TestMethod]
        [Description("controlla che in caso di doppio inserimento di voto su giocatore, venga restituito l'errore 50010 dal DB")]
        public void InsertVotoDelGiocatore_Error_Duplicato()
        {
            try
            {
                DBControllers.SerieA.StagioneDBController cStagione = new DBControllers.SerieA.StagioneDBController(DBIntegrationTests.ConnectionString);
                int idStagione = cStagione.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idStagione);

                DateTime giornataStartDate = DataMock.FakeData.GetDate(new DateTime(2019, 8, 21, 15, 00, 00), 0, 0);
                DateTime giornataEndDate = DataMock.FakeData.GetDate(giornataStartDate, 0, 72);
                int idGiornata = cStagione.InsertGiornataInStagione(DataMock.FakeData.GetString(8, false, true, false, false),
                    giornataStartDate,
                    giornataEndDate,
                    idStagione);
                Assert.AreNotEqual(0, idGiornata);

                SquadraDBController cSquadra = new SquadraDBController(DBIntegrationTests.ConnectionString);
                int idSquadra = cSquadra.InsertSquadra(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idSquadra);

                GiocatoreDBController cGiocatore = new GiocatoreDBController(DBIntegrationTests.ConnectionString);
                int idGiocatore = cGiocatore.InsertGiocatore(DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(4, true, false, true, false));
                Assert.AreNotEqual(0, idGiocatore);

                int idSquadraGiocatore = cSquadra.InsertGiocatoreInSquadra(idGiocatore, idSquadra,
                      DataMock.FakeData.GetDate(DateTime.Now, 0, 0));
                Assert.AreNotEqual(0, idSquadraGiocatore);

                int idVoto = cGiocatore.InsertVotoDelGiocatore(idGiocatore, idGiornata
                    , DataMock.FakeData.GetDouble()
                    , DataMock.FakeData.GetDouble()
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetBoolean()
                    , DataMock.FakeData.GetBoolean()
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    );
                Assert.AreNotEqual(0, idVoto);

                //questa istruzione deve generare l'errore
                int idVoto2 = cGiocatore.InsertVotoDelGiocatore(idGiocatore, idGiornata
                    , DataMock.FakeData.GetDouble()
                    , DataMock.FakeData.GetDouble()
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetBoolean()
                    , DataMock.FakeData.GetBoolean()
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    );
                //qui non dovrebe mai arrivare
                Assert.AreEqual(0, idVoto2);
            }
            catch (SusyLeagueDBException ex)
            {
                if (ManageError)
                    Assert.AreEqual(50010, ex.Code);
                else if (!TestError) { }
                else
                    throw;
            }
        }

        [TestMethod]
        [Description("controlla che in caso di inserimento di voto su giocatore non esistente per una giornata, venga restituito l'errore 547 dal DB")]
        public void InsertVotoDelGiocatore_Error_GiocatoreNonEsistente()
        {
            try
            {
                DBControllers.SerieA.StagioneDBController cStagione = new DBControllers.SerieA.StagioneDBController(DBIntegrationTests.ConnectionString);
                int idStagione = cStagione.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idStagione);

                DateTime giornataStartDate = DataMock.FakeData.GetDate(new DateTime(2019, 8, 21, 15, 00, 00), 0, 0);
                DateTime giornataEndDate = DataMock.FakeData.GetDate(giornataStartDate, 0, 72);
                int idGiornata = cStagione.InsertGiornataInStagione(DataMock.FakeData.GetString(8, false, true, false, false),
                    giornataStartDate,
                    giornataEndDate,
                    idStagione);
                Assert.AreNotEqual(0, idGiornata);

                SquadraDBController cSquadra = new SquadraDBController(DBIntegrationTests.ConnectionString);
                int idSquadra = cSquadra.InsertSquadra(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idSquadra);

                GiocatoreDBController cGiocatore = new GiocatoreDBController(DBIntegrationTests.ConnectionString);
                //int idGiocatore = cGiocatore.InsertGiocatore(DataMock.FakeData.GetString(8, false, true, false, false),
                //    DataMock.FakeData.GetString(8, false, true, false, false),
                //    DataMock.FakeData.GetString(4, true, false, true, false));
                //Assert.AreNotEqual(0, idGiocatore);

                //int idSquadraGiocatore = cSquadra.InsertGiocatoreInSquadra(idGiocatore, idSquadra,
                //      DataMock.FakeData.GetDate(DateTime.Now, 0, 0));
                //Assert.AreNotEqual(0, idSquadraGiocatore);

                //questa istruzione deve generare l'errore
                int idVoto = cGiocatore.InsertVotoDelGiocatore(0, idGiornata
                    , DataMock.FakeData.GetDouble()
                    , DataMock.FakeData.GetDouble()
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetBoolean()
                    , DataMock.FakeData.GetBoolean()
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    );
                Assert.AreNotEqual(0, idVoto);
            }
            catch (SusyLeagueDBException ex)
            {
                if (ManageError)
                    Assert.AreEqual(547, ex.Code);
                else if (!TestError) { }
                else
                    throw;
            }
        }

        [TestMethod]
        [Description("controlla che in caso di inserimento di voto su giocatore per una giornata non esistente, venga restituito l'errore 547 dal DB")]
        public void InsertVotoDelGiocatore_Error_GiornataNonEsistente()
        {
            try
            {
                DBControllers.SerieA.StagioneDBController cStagione = new DBControllers.SerieA.StagioneDBController(DBIntegrationTests.ConnectionString);
                int idStagione = cStagione.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idStagione);

                DateTime giornataStartDate = DataMock.FakeData.GetDate(new DateTime(2019, 8, 21, 15, 00, 00), 0, 0);
                DateTime giornataEndDate = DataMock.FakeData.GetDate(giornataStartDate, 0, 72);
                int idGiornata = cStagione.InsertGiornataInStagione(DataMock.FakeData.GetString(8, false, true, false, false),
                    giornataStartDate,
                    giornataEndDate,
                    idStagione);
                Assert.AreNotEqual(0, idGiornata);

                SquadraDBController cSquadra = new SquadraDBController(DBIntegrationTests.ConnectionString);
                int idSquadra = cSquadra.InsertSquadra(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idSquadra);

                GiocatoreDBController cGiocatore = new GiocatoreDBController(DBIntegrationTests.ConnectionString);
                int idGiocatore = cGiocatore.InsertGiocatore(DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(4, true, false, true, false));
                Assert.AreNotEqual(0, idGiocatore);

                //int idSquadraGiocatore = cSquadra.InsertGiocatoreInSquadra(idGiocatore, idSquadra,
                //      DataMock.FakeData.GetDate(DateTime.Now, 0, 0));
                //Assert.AreNotEqual(0, idSquadraGiocatore);

                //questa istruzione deve generare l'errore
                int idVoto = cGiocatore.InsertVotoDelGiocatore(idGiocatore, 0
                    , DataMock.FakeData.GetDouble()
                    , DataMock.FakeData.GetDouble()
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetBoolean()
                    , DataMock.FakeData.GetBoolean()
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    );
                Assert.AreNotEqual(0, idVoto);
            }
            catch (SusyLeagueDBException ex)
            {
                if (ManageError)
                    Assert.AreEqual(547, ex.Code);
                else if (!TestError) { }
                else
                    throw;
            }
        }

        [TestMethod]
        [Description("controlla che in caso di update di un voto non esistente, venga restituito l'errore 50011 dal DB")]
        public void UpdateVotoDelGiocatore_Error_NOTFOUND()
        {
            try
            {
                GiocatoreDBController cGiocatore = new GiocatoreDBController(DBIntegrationTests.ConnectionString);
                //questa istruzione deve generare l'errore
                cGiocatore.UpdateVotoDelGiocatore(0
                    , DataMock.FakeData.GetDouble()
                    , DataMock.FakeData.GetDouble()
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetBoolean()
                    , DataMock.FakeData.GetBoolean()
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    );
                //qui non dovrebe mai arrivare
            }
            catch (SusyLeagueDBException ex)
            {
                if (ManageError)
                    Assert.AreEqual(50011, ex.Code);
                else if (!TestError) { }
                else
                    throw;
            }
        }

        [TestMethod]
        public void InsertStatisticaDelGiocatore()
        {
            try
            {
                DBControllers.SerieA.StagioneDBController cStagione = new DBControllers.SerieA.StagioneDBController(DBIntegrationTests.ConnectionString);
                int idStagione = cStagione.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idStagione);

                GiocatoreDBController cGiocatore = new GiocatoreDBController(DBIntegrationTests.ConnectionString);
                int idGiocatore = cGiocatore.InsertGiocatore(DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(4, true, false, true, false));
                Assert.AreNotEqual(0, idGiocatore);

                int idStatistica = cGiocatore.InsertStatisticaDelGiocatorePerStagione(idGiocatore, idStagione
                    , DataMock.FakeData.GetInteger(0, 38)
                    , DataMock.FakeData.GetInteger(0, 38)
                    , DataMock.FakeData.GetDouble()
                    , DataMock.FakeData.GetDouble()
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 38)
                    , DataMock.FakeData.GetInteger(0, 38)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    );
                Assert.AreNotEqual(0, idStatistica);


            }
            catch (Exception)
            {

                throw;
            }
        }

        [TestMethod]
        public void UpdateStatisticaDelGiocatore()
        {
            try
            {
                DBControllers.SerieA.StagioneDBController cStagione = new DBControllers.SerieA.StagioneDBController(DBIntegrationTests.ConnectionString);
                int idStagione = cStagione.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idStagione);

                GiocatoreDBController cGiocatore = new GiocatoreDBController(DBIntegrationTests.ConnectionString);
                int idGiocatore = cGiocatore.InsertGiocatore(DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(4, true, false, true, false));
                Assert.AreNotEqual(0, idGiocatore);

                int idStatistica = cGiocatore.InsertStatisticaDelGiocatorePerStagione(idGiocatore, idStagione
                    , DataMock.FakeData.GetInteger(0, 38)
                    , DataMock.FakeData.GetInteger(0, 38)
                    , DataMock.FakeData.GetDouble()
                    , DataMock.FakeData.GetDouble()
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 38)
                    , DataMock.FakeData.GetInteger(0, 38)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    );
                Assert.AreNotEqual(0, idStatistica);

                cGiocatore.UpdateStatisticaDelGiocatorePerStagione(idStatistica
                   , DataMock.FakeData.GetInteger(0, 38)
                   , DataMock.FakeData.GetInteger(0, 38)
                   , DataMock.FakeData.GetDouble()
                   , DataMock.FakeData.GetDouble()
                   , DataMock.FakeData.GetInteger(0, 5)
                   , DataMock.FakeData.GetInteger(0, 5)
                   , DataMock.FakeData.GetInteger(0, 5)
                   , DataMock.FakeData.GetInteger(0, 5)
                   , DataMock.FakeData.GetInteger(0, 38)
                   , DataMock.FakeData.GetInteger(0, 38)
                   , DataMock.FakeData.GetInteger(0, 5)
                   , DataMock.FakeData.GetInteger(0, 5)
                   , DataMock.FakeData.GetInteger(0, 5)
                   , DataMock.FakeData.GetInteger(0, 5)
                   );
                Assert.AreNotEqual(0, idStatistica);


            }
            catch (Exception)
            {

                throw;
            }
        }

        [TestMethod]
        [Description("controlla che in caso di doppio inserimento di statistica su giocatore, venga restituito l'errore 50008 dal DB")]
        public void InsertStatisticaDelGiocatore_Error_Duplicato()
        {
            try
            {
                DBControllers.SerieA.StagioneDBController cStagione = new DBControllers.SerieA.StagioneDBController(DBIntegrationTests.ConnectionString);
                int idStagione = cStagione.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idStagione);

                GiocatoreDBController cGiocatore = new GiocatoreDBController(DBIntegrationTests.ConnectionString);
                int idGiocatore = cGiocatore.InsertGiocatore(DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(4, true, false, true, false));
                Assert.AreNotEqual(0, idGiocatore);

                int idStatistica1 = cGiocatore.InsertStatisticaDelGiocatorePerStagione(idGiocatore, idStagione
                    , DataMock.FakeData.GetInteger(0, 38)
                    , DataMock.FakeData.GetInteger(0, 38)
                    , DataMock.FakeData.GetDouble()
                    , DataMock.FakeData.GetDouble()
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 38)
                    , DataMock.FakeData.GetInteger(0, 38)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    );
                Assert.AreNotEqual(0, idStatistica1);

                //questa istruzione deve generare l'errore
                int idStatistica2 = cGiocatore.InsertStatisticaDelGiocatorePerStagione(idGiocatore, idStagione
                    , DataMock.FakeData.GetInteger(0, 38)
                    , DataMock.FakeData.GetInteger(0, 38)
                    , DataMock.FakeData.GetDouble()
                    , DataMock.FakeData.GetDouble()
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 38)
                    , DataMock.FakeData.GetInteger(0, 38)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    );
                //qui non dovrebe mai arrivare
                Assert.AreEqual(0, idStatistica1);
            }
            catch (SusyLeagueDBException ex)
            {
                if (ManageError)
                    Assert.AreEqual(50008, ex.Code);
                else if (!TestError) { }
                else
                    throw;
            }
        }

        [TestMethod]
        [Description("controlla che in caso di inserimento di statistica su giocatore non esistente, venga restituito l'errore 547 dal DB")]
        public void InsertStatisticaDelGiocatore_Error_NOTFOUND()
        {
            try
            {
                DBControllers.SerieA.StagioneDBController cStagione = new DBControllers.SerieA.StagioneDBController(DBIntegrationTests.ConnectionString);
                int idStagione = cStagione.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idStagione);

                GiocatoreDBController cGiocatore = new GiocatoreDBController(DBIntegrationTests.ConnectionString);
                //int idGiocatore = cGiocatore.InsertGiocatore(DataMock.FakeData.GetString(8, false, true, false, false),
                //    DataMock.FakeData.GetString(8, false, true, false, false),
                //    DataMock.FakeData.GetString(4, true, false, true, false));
                //Assert.AreNotEqual(0, idGiocatore);

                //questa istruzione deve generare l'errore
                int idStatistica1 = cGiocatore.InsertStatisticaDelGiocatorePerStagione(0, idStagione
                    , DataMock.FakeData.GetInteger(0, 38)
                    , DataMock.FakeData.GetInteger(0, 38)
                    , DataMock.FakeData.GetDouble()
                    , DataMock.FakeData.GetDouble()
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 38)
                    , DataMock.FakeData.GetInteger(0, 38)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    );
                //qui non dovrebe mai arrivare
                Assert.AreEqual(0, idStatistica1);
            }
            catch (SusyLeagueDBException ex)
            {
                if (ManageError)
                    Assert.AreEqual(547, ex.Code);
                else if (!TestError) { }
                else
                    throw;
            }
        }
        [TestMethod]
        [Description("controlla che in caso di inserimento di statistica su giocatore per una stagione non esistente, venga restituito l'errore 547 dal DB")]
        public void InsertStatisticaDelGiocatore_Error_StagioneNOTFOUND()
        {
            try
            {
                DBControllers.SerieA.StagioneDBController cStagione = new DBControllers.SerieA.StagioneDBController(DBIntegrationTests.ConnectionString);
                //int idStagione = cStagione.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                //Assert.AreNotEqual(0, idStagione);

                GiocatoreDBController cGiocatore = new GiocatoreDBController(DBIntegrationTests.ConnectionString);
                int idGiocatore = cGiocatore.InsertGiocatore(DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(4, true, false, true, false));
                Assert.AreNotEqual(0, idGiocatore);

                //questa istruzione deve generare l'errore
                int idStatistica1 = cGiocatore.InsertStatisticaDelGiocatorePerStagione(idGiocatore, 0
                    , DataMock.FakeData.GetInteger(0, 38)
                    , DataMock.FakeData.GetInteger(0, 38)
                    , DataMock.FakeData.GetDouble()
                    , DataMock.FakeData.GetDouble()
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 38)
                    , DataMock.FakeData.GetInteger(0, 38)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    , DataMock.FakeData.GetInteger(0, 5)
                    );
                //qui non dovrebe mai arrivare
                Assert.AreEqual(0, idStatistica1);
            }
            catch (SusyLeagueDBException ex)
            {
                if (ManageError)
                    Assert.AreEqual(547, ex.Code);
                else if (!TestError) { }
                else
                    throw;
            }
        }

        [TestMethod]
        [Description("controlla che in caso di update di una statistica non esistente, venga restituito l'errore 50009 dal DB")]
        public void UpdateStatisticaDelGiocatore_Error_NOTFOUND()
        {
            try
            {
                GiocatoreDBController cGiocatore = new GiocatoreDBController(DBIntegrationTests.ConnectionString);
                //questa istruzione deve generare l'errore
                cGiocatore.UpdateStatisticaDelGiocatorePerStagione(0
                   , DataMock.FakeData.GetInteger(0, 38)
                   , DataMock.FakeData.GetInteger(0, 38)
                   , DataMock.FakeData.GetDouble()
                   , DataMock.FakeData.GetDouble()
                   , DataMock.FakeData.GetInteger(0, 5)
                   , DataMock.FakeData.GetInteger(0, 5)
                   , DataMock.FakeData.GetInteger(0, 5)
                   , DataMock.FakeData.GetInteger(0, 5)
                   , DataMock.FakeData.GetInteger(0, 38)
                   , DataMock.FakeData.GetInteger(0, 38)
                   , DataMock.FakeData.GetInteger(0, 5)
                   , DataMock.FakeData.GetInteger(0, 5)
                   , DataMock.FakeData.GetInteger(0, 5)
                   , DataMock.FakeData.GetInteger(0, 5)
                   );
                //qui non dovrebe mai arrivare
            }
            catch (SusyLeagueDBException ex)
            {
                if (ManageError)
                    Assert.AreEqual(50009, ex.Code);
                else if (!TestError) { }
                else
                    throw;
            }
        }

        [TestMethod]
        public void InsertRuolo()
        {
            try
            {
                RuoloDBController c = new RuoloDBController(DBIntegrationTests.ConnectionString);
                int id = c.InsertRuolo(
                    DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(1, true, false, false, false),
                    DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(1, true, false, false, false));

                Assert.AreNotEqual(0, id);
            }
            catch (Exception)
            {

                throw;
            }
        }

        [TestMethod]
        [Description("controlla che in caso di doppio inserimento di statistica su giocatore, venga restituito l'errore 50017 dal DB")]
        public void InsertRuolo_ERROR_Duplicato()
        {
            try
            {
                RuoloDBController c = new RuoloDBController(DBIntegrationTests.ConnectionString);
                string sigla1 = DataMock.FakeData.GetString(1, true, false, false, false);
                string sigla2 = DataMock.FakeData.GetString(1, true, false, false, false);
                int id = c.InsertRuolo(
                    DataMock.FakeData.GetString(8, false, true, false, false),
                    sigla1,
                    DataMock.FakeData.GetString(8, false, true, false, false),
                    sigla2);
                Assert.AreNotEqual(0, id);
                //questa istruzione deve generare l'errore
                id = c.InsertRuolo(
                   DataMock.FakeData.GetString(8, false, true, false, false),
                   sigla1,
                   DataMock.FakeData.GetString(8, false, true, false, false),
                   sigla2);
                //qui non dovrebe mai arrivare
                Assert.AreEqual(0, id);
            }
            catch (SusyLeagueDBException ex)
            {
                if (ManageError)
                    Assert.AreEqual(50017, ex.Code);
                else if (!TestError) { }
                else
                    throw;
            }
        }

        [TestMethod]
        public void InsertRuoloPerGiocatorePerStagione()
        {
            try
            {
                DBControllers.SerieA.StagioneDBController cStagione = new DBControllers.SerieA.StagioneDBController(DBIntegrationTests.ConnectionString);
                int idStagione = cStagione.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idStagione);

                GiocatoreDBController cGiocatore = new GiocatoreDBController(DBIntegrationTests.ConnectionString);
                int idGiocatore = cGiocatore.InsertGiocatore(DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(4, true, false, true, false));
                Assert.AreNotEqual(0, idGiocatore);

                RuoloDBController cRuolo = new RuoloDBController(DBIntegrationTests.ConnectionString);
                int idRuolo = cRuolo.InsertRuolo(
                    DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(1, true, false, false, false),
                    DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(1, true, false, false, false));

                Assert.AreNotEqual(0, idRuolo);

                int id = cGiocatore.InsertRuoloDelGiocatorePerStagione(idGiocatore, idStagione, idRuolo);

                Assert.AreNotEqual(0, id);
            }
            catch (Exception)
            {

                throw;
            }
        }

        [TestMethod]
        [Description("controlla che in caso di doppio inserimento della relazione ruolo giocatore stagione, venga restituito l'errore 50018 dal DB")]
        public void InsertRuoloPerGiocatorePerStagione_ERROR_Duplicato()
        {
            try
            {
                DBControllers.SerieA.StagioneDBController cStagione = new DBControllers.SerieA.StagioneDBController(DBIntegrationTests.ConnectionString);
                int idStagione = cStagione.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idStagione);

                GiocatoreDBController cGiocatore = new GiocatoreDBController(DBIntegrationTests.ConnectionString);
                int idGiocatore = cGiocatore.InsertGiocatore(DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(4, true, false, true, false));
                Assert.AreNotEqual(0, idGiocatore);

                RuoloDBController cRuolo = new RuoloDBController(DBIntegrationTests.ConnectionString);
                int idRuolo = cRuolo.InsertRuolo(
                    DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(1, true, false, false, false),
                    DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(1, true, false, false, false));
                Assert.AreNotEqual(0, idRuolo);

                int id = cGiocatore.InsertRuoloDelGiocatorePerStagione(idGiocatore, idStagione, idRuolo);
                Assert.AreNotEqual(0, id);
                //questa istruzione deve generare l'errore
                id = cGiocatore.InsertRuoloDelGiocatorePerStagione(idGiocatore, idStagione, idRuolo);
                //qui non dovrebe mai arrivare
                Assert.AreEqual(0, id);
            }
            catch (SusyLeagueDBException ex)
            {
                if (ManageError)
                    Assert.AreEqual(50018, ex.Code);
                else if (!TestError) { }
                else
                    throw;
            }
        }

        [TestMethod]
        [Description("controlla che in caso di inserimento della relazione ruolo giocatore stagione per giocatore che non esiste, venga restituito l'errore 547 dal DB")]
        public void InsertRuoloPerGiocatorePerStagione_ERROR_GiocatoreNOTFOUND()
        {
            try
            {
                DBControllers.SerieA.StagioneDBController cStagione = new DBControllers.SerieA.StagioneDBController(DBIntegrationTests.ConnectionString);
                int idStagione = cStagione.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idStagione);

                GiocatoreDBController cGiocatore = new GiocatoreDBController(DBIntegrationTests.ConnectionString);
                int idGiocatore = cGiocatore.InsertGiocatore(DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(4, true, false, true, false));
                Assert.AreNotEqual(0, idGiocatore);

                RuoloDBController cRuolo = new RuoloDBController(DBIntegrationTests.ConnectionString);
                int idRuolo = cRuolo.InsertRuolo(
                    DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(1, true, false, false, false),
                    DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(1, true, false, false, false));
                Assert.AreNotEqual(0, idRuolo);
                //questa istruzione deve generare l'errore
                int id = cGiocatore.InsertRuoloDelGiocatorePerStagione(0, idStagione, idRuolo);
                //qui non dovrebe mai arrivare
                Assert.AreEqual(0, id);
            }
            catch (SusyLeagueDBException ex)
            {
                if (ManageError)
                    Assert.AreEqual(547, ex.Code);
                else if (!TestError) { }
                else
                    throw;
            }
        }

        [TestMethod]
        [Description("controlla che in caso di inserimento della relazione ruolo giocatore stagione per stagione che non esiste, venga restituito l'errore 547 dal DB")]
        public void InsertRuoloPerGiocatorePerStagione_ERROR_StagioneNOTFOUND()
        {
            try
            {
                DBControllers.SerieA.StagioneDBController cStagione = new DBControllers.SerieA.StagioneDBController(DBIntegrationTests.ConnectionString);
                int idStagione = cStagione.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idStagione);

                GiocatoreDBController cGiocatore = new GiocatoreDBController(DBIntegrationTests.ConnectionString);
                int idGiocatore = cGiocatore.InsertGiocatore(DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(4, true, false, true, false));
                Assert.AreNotEqual(0, idGiocatore);

                RuoloDBController cRuolo = new RuoloDBController(DBIntegrationTests.ConnectionString);
                int idRuolo = cRuolo.InsertRuolo(
                    DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(1, true, false, false, false),
                    DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(1, true, false, false, false));
                Assert.AreNotEqual(0, idRuolo);
                //questa istruzione deve generare l'errore
                int id = cGiocatore.InsertRuoloDelGiocatorePerStagione(idGiocatore, 0, idRuolo);
                //qui non dovrebe mai arrivare
                Assert.AreEqual(0, id);
            }
            catch (SusyLeagueDBException ex)
            {
                if (ManageError)
                    Assert.AreEqual(547, ex.Code);
                else if (!TestError) { }
                else
                    throw;
            }
        }

        [TestMethod]
        [Description("controlla che in caso di inserimento della relazione ruolo giocatore stagione per ruolo che non esiste, venga restituito l'errore 547 dal DB")]
        public void InsertRuoloPerGiocatorePerStagione_ERROR_RuoloNOTFOUND()
        {
            try
            {
                DBControllers.SerieA.StagioneDBController cStagione = new DBControllers.SerieA.StagioneDBController(DBIntegrationTests.ConnectionString);
                int idStagione = cStagione.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idStagione);

                GiocatoreDBController cGiocatore = new GiocatoreDBController(DBIntegrationTests.ConnectionString);
                int idGiocatore = cGiocatore.InsertGiocatore(DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(4, true, false, true, false));
                Assert.AreNotEqual(0, idGiocatore);

                RuoloDBController cRuolo = new RuoloDBController(DBIntegrationTests.ConnectionString);
                int idRuolo = cRuolo.InsertRuolo(
                    DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(1, true, false, false, false),
                    DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(1, true, false, false, false));
                Assert.AreNotEqual(0, idRuolo);
                //questa istruzione deve generare l'errore
                int id = cGiocatore.InsertRuoloDelGiocatorePerStagione(idGiocatore, idStagione, 0);
                //qui non dovrebe mai arrivare
                Assert.AreEqual(0, id);
            }
            catch (SusyLeagueDBException ex)
            {
                if (ManageError)
                    Assert.AreEqual(547, ex.Code);
                else if (!TestError) { }
                else
                    throw;
            }
        }
        #endregion


        #region DBControllers.Membership

        [TestMethod]
        public void InsertUser()
        {
            try
            {
                UserDBController c = new UserDBController(DBIntegrationTests.ConnectionString);
                int id = c.InsertUser(DataMock.FakeData.GetUsername());

                Assert.AreNotEqual(0, id);
            }
            catch (Exception)
            {

                throw;
            }
        }
        //[TestMethod]
        //public void SetTemporaryUserPassword()
        //{
        //    try
        //    {
        //        UserDBController c = new UserDBController(DBIntegrationTests.ConnectionString);
        //        int id = c.InsertUser(DataMock.FakeData.GetUsername(), DataMock.FakeData.GetPassword());

        //        Assert.AreNotEqual(0, id);

        //        c.SetPasswordTemporaneaUser(id, DataMock.FakeData.GetPassword());
        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }
        //}
        //[TestMethod]
        //public void UpdateUserPassword()
        //{
        //    try
        //    {
        //        UserDBController c = new UserDBController(DBIntegrationTests.ConnectionString);
        //        int id = c.InsertUser(DataMock.FakeData.GetUsername(), DataMock.FakeData.GetPassword());

        //        Assert.AreNotEqual(0, id);

        //        c.SetPasswordTemporaneaUser(id, DataMock.FakeData.GetPassword());

        //        c.UpdatePasswordUser(id, DataMock.FakeData.GetPassword());
        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }
        //}
        [TestMethod]
        [Description("controlla che in caso di inserimento doppio dellu username, venga restituito l'errore 51002 dal DB")]
        public void InsertUser_Error_Duplicato()
        {
            try
            {
                UserDBController c = new UserDBController(DBIntegrationTests.ConnectionString);
                string username = DataMock.FakeData.GetUsername();
                int id = c.InsertUser(username);

                Assert.AreNotEqual(0, id);
                //questa istruzione deve generare l'errore
                id = c.InsertUser(username);
                //qui non dovrebe mai arrivare
                Assert.AreEqual(0, id);
            }
            catch (SusyLeagueDBException ex)
            {
                if (ManageError)
                    Assert.AreEqual(51002, ex.Code);
                else if (!TestError) { }
                else
                    throw;
            }
        }

        //[TestMethod]
        //[Description("controlla che in caso di set passwrord temporanea per utente non esistente, venga restituito l'errore 51001 dal DB")]
        //public void SetTemporaryUserPassword_Error_NOTFOUND()
        //{
        //    try
        //    {
        //        UserDBController c = new UserDBController(DBIntegrationTests.ConnectionString);
        //        //questa istruzione deve generare l'errore
        //        c.SetPasswordTemporaneaUser(0, DataMock.FakeData.GetPassword());
        //        //qui non dovrebe mai arrivare

        //    }
        //    catch (SusyLeagueDBException ex)
        //    {
        //        if (ManageError)
        //            Assert.AreEqual(51001, ex.Code);
        //        else if (!TestError) { }
        //        else
        //            throw;
        //    }
        //}

        //[TestMethod]
        //[Description("controlla che in caso di set passwrord temporanea per utente non esistente, venga restituito l'errore 51001 dal DB")]
        //public void UpdateUserPassword_Error_NOTFOUND()
        //{
        //    try
        //    {
        //        UserDBController c = new UserDBController(DBIntegrationTests.ConnectionString);
        //        //questa istruzione deve generare l'errore
        //        c.UpdatePasswordUser(0, DataMock.FakeData.GetPassword());
        //        //qui non dovrebe mai arrivare

        //    }
        //    catch (SusyLeagueDBException ex)
        //    {
        //        if (ManageError)
        //            Assert.AreEqual(51001, ex.Code);
        //        else if (!TestError) { }
        //        else
        //            throw;
        //    }
        //}

        #endregion

        #region DBControllers.Web
        //[TestMethod]
        //public void InsertRole()
        //{
        //    try
        //    {
        //        RoleDBController c = new RoleDBController(DBIntegrationTests.ConnectionString);
        //        int id = c.InsertRole(DataMock.FakeData.GetString(8, false, true, false, false));

        //        Assert.AreNotEqual(0, id);
        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }
        //}
        //[TestMethod]
        //public void UpdateRole()
        //{
        //    try
        //    {
        //        RoleDBController c = new RoleDBController(DBIntegrationTests.ConnectionString);
        //        int id = c.InsertRole(DataMock.FakeData.GetString(8, false, true, false, false));

        //        Assert.AreNotEqual(0, id);

        //        c.UpdateRole(id, DataMock.FakeData.GetString(8, false, true, false, false));
        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }
        //}

        //[TestMethod]
        //[Description("controlla che in caso di inserimento doppio ruolo, venga restituito l'errore 52004 dal DB")]
        //public void InsertRole_ERROR_Duplicato()
        //{
        //    try
        //    {
        //        RoleDBController c = new RoleDBController(DBIntegrationTests.ConnectionString);
        //        string description = DataMock.FakeData.GetString(8, false, true, false, false);
        //        int id = c.InsertRole(description);

        //        Assert.AreNotEqual(0, id);

        //        //questa istruzione deve generare l'errore
        //        id = c.InsertRole(description);
        //        //qui non dovrebe mai arrivare
        //        Assert.AreEqual(0, id);
        //    }
        //    catch (SusyLeagueDBException ex)
        //    {
        //        if (ManageError)
        //            Assert.AreEqual(52004, ex.Code);
        //        else if (!TestError) { }
        //        else
        //            throw;
        //    }
        //}
        //[TestMethod]
        //[Description("controlla che in caso di ruolo non esistente, venga restituito l'errore 52005 dal DB")]
        //public void UpdateRole_ERROR_NOTFOUND()
        //{
        //    try
        //    {
        //        RoleDBController c = new RoleDBController(DBIntegrationTests.ConnectionString);
        //        //questa istruzione deve generare l'errore
        //        c.UpdateRole(0, DataMock.FakeData.GetString(8, false, true, false, false));
        //        //qui non dovrebe mai arrivare
        //    }
        //    catch (SusyLeagueDBException ex)
        //    {
        //        if (ManageError)
        //            Assert.AreEqual(52005, ex.Code);
        //        else if (!TestError) { }
        //        else
        //            throw;
        //    }
        //}

        //[TestMethod]
        //public void InsertUserRole()
        //{
        //    try
        //    {

        //        UserDBController cUser = new UserDBController(DBIntegrationTests.ConnectionString);
        //        int idUser = cUser.InsertUser(DataMock.FakeData.GetUsername());

        //        Assert.AreNotEqual(0, idUser);

        //        RoleDBController cRole = new RoleDBController(DBIntegrationTests.ConnectionString);
        //        int idRole = cRole.InsertRole(DataMock.FakeData.GetString(8, false, true, false, false));

        //        Assert.AreNotEqual(0, idRole);

        //        int id = cRole.InsertUserRole(idUser, idRole);

        //        Assert.AreNotEqual(0, id);
        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }
        //}

        //[TestMethod]
        //public void DeleteUserRole()
        //{
        //    try
        //    {
        //        UserDBController cUser = new UserDBController(DBIntegrationTests.ConnectionString);
        //        int idUser = cUser.InsertUser(DataMock.FakeData.GetUsername());
        //        Assert.AreNotEqual(0, idUser);

        //        RoleDBController cRole = new RoleDBController(DBIntegrationTests.ConnectionString);
        //        int idRole = cRole.InsertRole(DataMock.FakeData.GetString(8, false, true, false, false));
        //        Assert.AreNotEqual(0, idRole);

        //        int id = cRole.InsertUserRole(idUser, idRole);
        //        Assert.AreNotEqual(0, id);

        //        cRole.DeleteUserRole(id);
        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }
        //}

        //[TestMethod]
        //[Description("controlla che in caso di inserimento user_role duplicato , venga restituito l'errore 52007 dal DB")]
        //public void InsertUserRole_Error_Duplicato()
        //{
        //    try
        //    {

        //        UserDBController cUser = new UserDBController(DBIntegrationTests.ConnectionString);
        //        int idUser = cUser.InsertUser(DataMock.FakeData.GetUsername());
        //        Assert.AreNotEqual(0, idUser);

        //        RoleDBController cRole = new RoleDBController(DBIntegrationTests.ConnectionString);
        //        int idRole = cRole.InsertRole(DataMock.FakeData.GetString(8, false, true, false, false));
        //        Assert.AreNotEqual(0, idRole);

        //        int id = cRole.InsertUserRole(idUser, idRole);
        //        Assert.AreNotEqual(0, id);

        //        //questa istruzione deve generare l'errore
        //        id = cRole.InsertUserRole(idUser, idRole);
        //        //qui non dovrebe mai arrivare
        //        Assert.AreEqual(0, id);
        //    }
        //    catch (SusyLeagueDBException ex)
        //    {
        //        if (ManageError)
        //            Assert.AreEqual(52007, ex.Code);
        //        else if (!TestError) { }
        //        else
        //            throw;
        //    }
        //}

        //[TestMethod]
        //[Description("controlla che in caso di inserimento user_role con USER NOT FOUND , venga restituito l'errore 547 dal DB")]
        //public void InsertUserRole_Error_UserNOTFOUND()
        //{
        //    try
        //    {

        //        //UserDBController cUser = new UserDBController(DBIntegrationTests.ConnectionString);
        //        //int idUser = cUser.InsertUser(DataMock.FakeData.GetUsername());

        //        //Assert.AreNotEqual(0, idUser);

        //        RoleDBController cRole = new RoleDBController(DBIntegrationTests.ConnectionString);
        //        int idRole = cRole.InsertRole(DataMock.FakeData.GetString(8, false, true, false, false));
        //        Assert.AreNotEqual(0, idRole);

        //        //questa istruzione deve generare l'errore
        //        int id = cRole.InsertUserRole(0, idRole);
        //        //qui non dovrebe mai arrivare
        //        Assert.AreEqual(0, id);
        //    }
        //    catch (SusyLeagueDBException ex)
        //    {
        //        if (ManageError)
        //            Assert.AreEqual(547, ex.Code);
        //        else if (!TestError) { }
        //        else
        //            throw;
        //    }
        //}

        //[TestMethod]
        //[Description("controlla che in caso di inserimento user_role con USER NOT FOUND , venga restituito l'errore 547 dal DB")]
        //public void InsertUserRole_Error_RoleNOTFOUND()
        //{
        //    try
        //    {
        //        UserDBController cUser = new UserDBController(DBIntegrationTests.ConnectionString);
        //        int idUser = cUser.InsertUser(DataMock.FakeData.GetUsername());
        //        Assert.AreNotEqual(0, idUser);

        //        RoleDBController cRole = new RoleDBController(DBIntegrationTests.ConnectionString);
        //        //questa istruzione deve generare l'errore
        //        int id = cRole.InsertUserRole(idUser, 0);
        //        //qui non dovrebe mai arrivare
        //        Assert.AreNotEqual(0, id);
        //    }
        //    catch (SusyLeagueDBException ex)
        //    {
        //        if (ManageError)
        //            Assert.AreEqual(547, ex.Code);
        //        else if (!TestError) { }
        //        else
        //            throw;
        //    }
        //}

        //[TestMethod]
        //public void InsertPage()
        //{
        //    try
        //    {
        //        PageDBController c = new PageDBController(DBIntegrationTests.ConnectionString);
        //        int id = c.InsertPage(DataMock.FakeData.GetString(8, false, true, false, false));
        //        Assert.AreNotEqual(0, id);
        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }
        //}
        //[TestMethod]
        //public void UpdatePage()
        //{
        //    try
        //    {
        //        PageDBController c = new PageDBController(DBIntegrationTests.ConnectionString);
        //        int id = c.InsertPage(DataMock.FakeData.GetString(8, false, true, false, false));
        //        Assert.AreNotEqual(0, id);

        //        c.UpdatePage(id, DataMock.FakeData.GetString(8, false, true, false, false));
        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }
        //}

        //[TestMethod]
        //public void InsertPageFunction()
        //{
        //    try
        //    {
        //        PageDBController cPage = new PageDBController(DBIntegrationTests.ConnectionString);
        //        int idPage = cPage.InsertPage(DataMock.FakeData.GetString(8, false, true, false, false));
        //        Assert.AreNotEqual(0, idPage);

        //        int idFuncion = cPage.InsertPageFunction(DataMock.FakeData.GetString(8, false, true, false, false), idPage);
        //        Assert.AreNotEqual(0, idFuncion);
        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }
        //}

        //[TestMethod]
        //[Description("controlla che in caso di inserimento doppio Page, venga restituito l'errore 52002 dal DB")]
        //public void InsertPage_ERROR_Duplicato()
        //{
        //    try
        //    {
        //        PageDBController c = new PageDBController(DBIntegrationTests.ConnectionString);

        //        string descrizione = DataMock.FakeData.GetString(8, false, true, false, false);
        //        int id = c.InsertPage(descrizione);
        //        Assert.AreNotEqual(0, id);

        //        //questa istruzione deve generare l'errore
        //        id = c.InsertPage(descrizione);
        //        //qui non dovrebe mai arrivare
        //        Assert.AreEqual(0, id);
        //    }
        //    catch (SusyLeagueDBException ex)
        //    {
        //        if (ManageError)
        //            Assert.AreEqual(52002, ex.Code);
        //        else if (!TestError) { }
        //        else
        //            throw;
        //    }
        //}

        //[TestMethod]
        //[Description("controlla che in caso di pagina non esistente, venga restituito l'errore 52003 dal DB")]
        //public void UpdatePage_ERROR_NOTFOUND()
        //{
        //    try
        //    {
        //        PageDBController c = new PageDBController(DBIntegrationTests.ConnectionString);
        //        //questa istruzione deve generare l'errore
        //        c.UpdatePage(0, DataMock.FakeData.GetString(8, false, true, false, false));
        //        //qui non dovrebe mai arrivare
        //    }
        //    catch (SusyLeagueDBException ex)
        //    {
        //        if (ManageError)
        //            Assert.AreEqual(52003, ex.Code);
        //        else if (!TestError) { }
        //        else
        //            throw;
        //    }
        //}

        //[TestMethod]
        //[Description("controlla che in caso di inserimento doppio funzione  di pagina, venga restituito l'errore 52001 dal DB")]
        //public void InsertPageFunction_ERROR_Duplicato()
        //{
        //    try
        //    {
        //        PageDBController cPage = new PageDBController(DBIntegrationTests.ConnectionString);
        //        int idPage = cPage.InsertPage(DataMock.FakeData.GetString(8, false, true, false, false));
        //        Assert.AreNotEqual(0, idPage);

        //        string descrizione = DataMock.FakeData.GetString(8, false, true, false, false);
        //        int idFuncion = cPage.InsertPageFunction(descrizione, idPage);
        //        Assert.AreNotEqual(0, idFuncion);

        //        //questa istruzione deve generare l'errore
        //        idFuncion = cPage.InsertPageFunction(descrizione, idPage);
        //        //qui non dovrebe mai arrivare
        //        Assert.AreEqual(0, idFuncion);
        //    }
        //    catch (SusyLeagueDBException ex)
        //    {
        //        if (ManageError)
        //            Assert.AreEqual(52001, ex.Code);
        //        else if (!TestError) { }
        //        else
        //            throw;
        //    }
        //}

        //[TestMethod]
        //[Description("controlla che in caso di inserimento funzione di pagina con pagina NOT FOUND , venga restituito l'errore 547 dal DB")]
        //public void InsertPageFunction_Error_PageNOTFOUND()
        //{
        //    try
        //    {
        //        PageDBController cPage = new PageDBController(DBIntegrationTests.ConnectionString);
        //        string descrizione = DataMock.FakeData.GetString(8, false, true, false, false);

        //        //questa istruzione deve generare l'errore
        //        int idFuncion = cPage.InsertPageFunction(descrizione, 0);
        //        //qui non dovrebe mai arrivare
        //        Assert.AreEqual(0, idFuncion);
        //    }
        //    catch (SusyLeagueDBException ex)
        //    {
        //        if (ManageError)
        //            Assert.AreEqual(547, ex.Code);
        //        else if (!TestError) { }
        //        else
        //            throw;
        //    }
        //}

        //[TestMethod]
        //public void InsertRolePage()
        //{
        //    try
        //    {

        //        RoleDBController cRole = new RoleDBController(DBIntegrationTests.ConnectionString);
        //        int idRole = cRole.InsertRole(DataMock.FakeData.GetString(8, false, true, false, false));

        //        PageDBController cPage = new PageDBController(DBIntegrationTests.ConnectionString);
        //        int idPage = cPage.InsertPage(DataMock.FakeData.GetString(8, false, true, false, false));
        //        Assert.AreNotEqual(0, idPage);

        //        //int idFuncion = cPage.InsertPageFunction(DataMock.FakeData.GetString(8, false, true, false, false), idPage);
        //        //Assert.AreNotEqual(0, idFuncion);

        //        int id = cRole.InsertRolePageFunction(idRole, idPage);
        //        Assert.AreNotEqual(0, id);
        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }
        //}

        //[TestMethod]
        //public void InsertRolePageFunction()
        //{
        //    try
        //    {
        //        RoleDBController cRole = new RoleDBController(DBIntegrationTests.ConnectionString);
        //        int idRole = cRole.InsertRole(DataMock.FakeData.GetString(8, false, true, false, false));

        //        PageDBController cPage = new PageDBController(DBIntegrationTests.ConnectionString);
        //        int idPage = cPage.InsertPage(DataMock.FakeData.GetString(8, false, true, false, false));
        //        Assert.AreNotEqual(0, idPage);

        //        int idFuncion = cPage.InsertPageFunction(DataMock.FakeData.GetString(8, false, true, false, false), idPage);
        //        Assert.AreNotEqual(0, idFuncion);

        //        int id = cRole.InsertRolePageFunction(idRole, idPage, idFuncion);
        //        Assert.AreNotEqual(0, id);
        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }
        //}

        //[TestMethod]
        //public void DeleteRolePageFunction()
        //{
        //    try
        //    {
        //        RoleDBController cRole = new RoleDBController(DBIntegrationTests.ConnectionString);
        //        int idRole = cRole.InsertRole(DataMock.FakeData.GetString(8, false, true, false, false));

        //        PageDBController cPage = new PageDBController(DBIntegrationTests.ConnectionString);
        //        int idPage = cPage.InsertPage(DataMock.FakeData.GetString(8, false, true, false, false));
        //        Assert.AreNotEqual(0, idPage);

        //        int idFuncion = cPage.InsertPageFunction(DataMock.FakeData.GetString(8, false, true, false, false), idPage);
        //        Assert.AreNotEqual(0, idFuncion);

        //        int id = cRole.InsertRolePageFunction(idRole, idPage, idFuncion);
        //        Assert.AreNotEqual(0, id);

        //        cRole.DeleteRolePageFunction(id);
        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }
        //}

        //[TestMethod]
        //[Description("controlla che in caso di inserimento doppio di ruolo/pagina, venga restituito l'errore 52006 dal DB")]
        //public void InsertRolePage_ERROR_Duplicato()
        //{
        //    try
        //    {
        //        RoleDBController cRole = new RoleDBController(DBIntegrationTests.ConnectionString);
        //        int idRole = cRole.InsertRole(DataMock.FakeData.GetString(8, false, true, false, false));

        //        PageDBController cPage = new PageDBController(DBIntegrationTests.ConnectionString);
        //        int idPage = cPage.InsertPage(DataMock.FakeData.GetString(8, false, true, false, false));
        //        Assert.AreNotEqual(0, idPage);

        //        int id = cRole.InsertRolePageFunction(idRole, idPage);
        //        Assert.AreNotEqual(0, id);

        //        //questa istruzione deve generare l'errore
        //        id = cRole.InsertRolePageFunction(idRole, idPage);
        //        //qui non dovrebe mai arrivare
        //        Assert.AreEqual(0, id);
        //    }
        //    catch (SusyLeagueDBException ex)
        //    {
        //        if (ManageError)
        //            Assert.AreEqual(52006, ex.Code);
        //        else if (!TestError) { }
        //        else
        //            throw;
        //    }
        //}

        //[TestMethod]
        //[Description("controlla che in caso di inserimento doppio di ruolo_pagina_funzione, venga restituito l'errore 52006 dal DB")]
        //public void InsertRolePageFunction_ERROR_Duplicato()
        //{
        //    try
        //    {
        //        RoleDBController cRole = new RoleDBController(DBIntegrationTests.ConnectionString);
        //        int idRole = cRole.InsertRole(DataMock.FakeData.GetString(8, false, true, false, false));

        //        PageDBController cPage = new PageDBController(DBIntegrationTests.ConnectionString);
        //        int idPage = cPage.InsertPage(DataMock.FakeData.GetString(8, false, true, false, false));
        //        Assert.AreNotEqual(0, idPage);

        //        int idFuncion = cPage.InsertPageFunction(DataMock.FakeData.GetString(8, false, true, false, false), idPage);
        //        Assert.AreNotEqual(0, idFuncion);

        //        int id = cRole.InsertRolePageFunction(idRole, idPage, idFuncion);
        //        Assert.AreNotEqual(0, id);

        //        //questa istruzione deve generare l'errore
        //        id = cRole.InsertRolePageFunction(idRole, idPage, idFuncion);
        //        //qui non dovrebe mai arrivare
        //        Assert.AreEqual(0, id);
        //    }
        //    catch (SusyLeagueDBException ex)
        //    {
        //        if (ManageError)
        //            Assert.AreEqual(52006, ex.Code);
        //        else if (!TestError) { }
        //        else
        //            throw;
        //    }
        //}

        //[TestMethod]
        //[Description("controlla che in caso di inserimento ruolo_pagina_funzion con ruolo NOT FOUND , venga restituito l'errore 547 dal DB")]
        //public void InsertRolePageFunction_Error_RoleNOTFOUND()
        //{
        //    try
        //    {

        //        RoleDBController cRole = new RoleDBController(DBIntegrationTests.ConnectionString);
        //        int idRole = cRole.InsertRole(DataMock.FakeData.GetString(8, false, true, false, false));

        //        PageDBController cPage = new PageDBController(DBIntegrationTests.ConnectionString);
        //        int idPage = cPage.InsertPage(DataMock.FakeData.GetString(8, false, true, false, false));
        //        Assert.AreNotEqual(0, idPage);

        //        int idFuncion = cPage.InsertPageFunction(DataMock.FakeData.GetString(8, false, true, false, false), idPage);
        //        Assert.AreNotEqual(0, idFuncion);

        //        //questa istruzione deve generare l'errore
        //        int id = cRole.InsertRolePageFunction(0, idPage, idFuncion);
        //        //qui non dovrebe mai arrivare
        //        Assert.AreEqual(0, idFuncion);
        //    }
        //    catch (SusyLeagueDBException ex)
        //    {
        //        if (ManageError)
        //            Assert.AreEqual(547, ex.Code);
        //        else if (!TestError) { }
        //        else
        //            throw;
        //    }
        //}

        //[TestMethod]
        //[Description("controlla che in caso di inserimento ruolo_pagina_funzion con page NOT FOUND , venga restituito l'errore 547 dal DB")]
        //public void InsertRolePageFunction_Error_PageNOTFOUND()
        //{
        //    try
        //    {

        //        RoleDBController cRole = new RoleDBController(DBIntegrationTests.ConnectionString);
        //        int idRole = cRole.InsertRole(DataMock.FakeData.GetString(8, false, true, false, false));

        //        PageDBController cPage = new PageDBController(DBIntegrationTests.ConnectionString);
        //        int idPage = cPage.InsertPage(DataMock.FakeData.GetString(8, false, true, false, false));
        //        Assert.AreNotEqual(0, idPage);

        //        int idFuncion = cPage.InsertPageFunction(DataMock.FakeData.GetString(8, false, true, false, false), idPage);
        //        Assert.AreNotEqual(0, idFuncion);

        //        //questa istruzione deve generare l'errore
        //        int id = cRole.InsertRolePageFunction(idRole, 0, idFuncion);
        //        //qui non dovrebe mai arrivare
        //        Assert.AreEqual(0, idFuncion);
        //    }
        //    catch (SusyLeagueDBException ex)
        //    {
        //        if (ManageError)
        //            Assert.AreEqual(547, ex.Code);
        //        else if (!TestError) { }
        //        else
        //            throw;
        //    }
        //}
        #endregion

        #region DBControllers.SusyLague
        [TestMethod]
        public void InsertSusyLeagueStagione()
        {
            try
            {

                DBControllers.SusyLeague.StagioneDBController c = new DBControllers.SusyLeague.StagioneDBController(DBIntegrationTests.ConnectionString);
                int id = c.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));

                Assert.AreNotEqual(0, id);

            }
            catch (Exception)
            {

                throw;
            }
        }

        [TestMethod]
        [Description("controlla che in caso di inserimento doppio della stagione  susy league, venga restituito l'errore 53001 dal DB")]
        public void InsertSusyLeagueStagione_Error_Duplicata()
        {
            try
            {
                DBControllers.SusyLeague.StagioneDBController c = new DBControllers.SusyLeague.StagioneDBController(DBIntegrationTests.ConnectionString);
                int id = 0;
                string nomeStagione = DataMock.FakeData.GetString(8, false, true, false, false);
                id = c.InsertStagione(nomeStagione);
                //questa istruzione deve generare l'errore
                id = c.InsertStagione(nomeStagione);

                Assert.AreNotEqual(0, id);
            }
            catch (SusyLeagueDBException ex)
            {
                if (ManageError)
                    Assert.AreEqual(53001, ex.Code);
                else if (!TestError) { }
                else
                    throw;
            }

        }

        [TestMethod]
        public void InsertSusyCompetizione()
        {
            try
            {

                DBControllers.SusyLeague.StagioneDBController cStagione = new DBControllers.SusyLeague.StagioneDBController(DBIntegrationTests.ConnectionString);
                int idStagione = cStagione.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));

                Assert.AreNotEqual(0, idStagione);

                DBControllers.SusyLeague.CompetizioneDBController cCompetizione = new DBControllers.SusyLeague.CompetizioneDBController(DBIntegrationTests.ConnectionString);
                int idCompetizione = cCompetizione.InsertCompetizione(DataMock.FakeData.GetString(8, false, true, false, false), idStagione);

                Assert.AreNotEqual(0, idCompetizione);

            }
            catch (Exception)
            {

                throw;
            }
        }

        [TestMethod]
        [Description("controlla che in caso di inserimento doppio della competizione  susy league, venga restituito l'errore 53002 dal DB")]
        public void InsertSusyCompetizione_Error_Duplicata()
        {
            try
            {
                DBControllers.SusyLeague.StagioneDBController cStagione = new DBControllers.SusyLeague.StagioneDBController(DBIntegrationTests.ConnectionString);
                int idStagione = cStagione.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));

                Assert.AreNotEqual(0, idStagione);

                string descrizione = DataMock.FakeData.GetString(8, false, true, false, false);
                DBControllers.SusyLeague.CompetizioneDBController cCompetizione = new DBControllers.SusyLeague.CompetizioneDBController(DBIntegrationTests.ConnectionString);
                int idCompetizione = cCompetizione.InsertCompetizione(descrizione, idStagione);

                Assert.AreNotEqual(0, idCompetizione);

                //questa istruzione genera l'eccezione 
                idCompetizione = cCompetizione.InsertCompetizione(descrizione, idStagione);
                //qui non dovrebbe mai passare
                Assert.AreEqual(0, idCompetizione);
            }
            catch (SusyLeagueDBException ex)
            {
                if (ManageError)
                    Assert.AreEqual(53002, ex.Code);
                else if (!TestError) { }
                else
                    throw;
            }

        }

        [TestMethod]
        [Description("controlla che in caso di inserimento competizione con stagione inesistente, venga restituito l'errore 547 dal DB")]
        public void InsertSusyCompetizione_Error_StagioneNOTFOUND()
        {
            try
            {
                DBControllers.SusyLeague.StagioneDBController cStagione = new DBControllers.SusyLeague.StagioneDBController(DBIntegrationTests.ConnectionString);
                int idStagione = cStagione.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));

                Assert.AreNotEqual(0, idStagione);

                string descrizione = DataMock.FakeData.GetString(8, false, true, false, false);
                //questa istruzione genera l'eccezione 
                DBControllers.SusyLeague.CompetizioneDBController cCompetizione = new DBControllers.SusyLeague.CompetizioneDBController(DBIntegrationTests.ConnectionString);
                int idCompetizione = cCompetizione.InsertCompetizione(descrizione, 0);
                //qui non dovrebbe mai passare
                Assert.AreNotEqual(0, idCompetizione);

            }
            catch (SusyLeagueDBException ex)
            {
                if (ManageError)
                    Assert.AreEqual(547, ex.Code);
                else if (!TestError) { }
                else
                    throw;
            }

        }

        [TestMethod]
        public void InsertSusyLeagueGiornata()
        {

            try
            {

                DBControllers.SerieA.StagioneDBController c = new DBControllers.SerieA.StagioneDBController(DBIntegrationTests.ConnectionString);
                int id = c.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, id);


                DateTime giornataStartDate = DataMock.FakeData.GetDate(new DateTime(2019, 8, 21, 15, 00, 00), 0, 0);
                DateTime giornataEndDate = DataMock.FakeData.GetDate(giornataStartDate, 0, 72);
                int idGiornataSerieA = c.InsertGiornataInStagione(DataMock.FakeData.GetString(8, false, true, false, false),
                    giornataStartDate,
                    giornataEndDate,
                    id);

                Assert.AreNotEqual(0, idGiornataSerieA);


                DBControllers.SusyLeague.StagioneDBController cStagione = new DBControllers.SusyLeague.StagioneDBController(DBIntegrationTests.ConnectionString);
                int idStagione = cStagione.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));

                Assert.AreNotEqual(0, idStagione);

                DBControllers.SusyLeague.CompetizioneDBController cCompetizione = new DBControllers.SusyLeague.CompetizioneDBController(DBIntegrationTests.ConnectionString);
                int idCompetizione = cCompetizione.InsertCompetizione(DataMock.FakeData.GetString(8, false, true, false, false), idStagione);

                Assert.AreNotEqual(0, idCompetizione);

                int idGiornata = cCompetizione.InsertGiornata(DataMock.FakeData.GetString(8, false, true, false, false), idGiornataSerieA, idCompetizione);
                Assert.AreNotEqual(0, idGiornata);
            }
            catch (Exception)
            {

                throw;
            }
        }

        [TestMethod]
        [Description("controlla che in caso di inserimento doppio della giornata nella competizione susy league, venga restituito l'errore 53003 dal DB")]
        public void InsertSusyLeagueGiornata_Error_Duplicata()
        {
            try
            {
                DBControllers.SerieA.StagioneDBController c = new DBControllers.SerieA.StagioneDBController(DBIntegrationTests.ConnectionString);
                int id = c.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, id);


                DateTime giornataStartDate = DataMock.FakeData.GetDate(new DateTime(2019, 8, 21, 15, 00, 00), 0, 0);
                DateTime giornataEndDate = DataMock.FakeData.GetDate(giornataStartDate, 0, 72);
                int idGiornataSerieA = c.InsertGiornataInStagione(DataMock.FakeData.GetString(8, false, true, false, false),
                    giornataStartDate,
                    giornataEndDate,
                    id);

                Assert.AreNotEqual(0, idGiornataSerieA);


                DBControllers.SusyLeague.StagioneDBController cStagione = new DBControllers.SusyLeague.StagioneDBController(DBIntegrationTests.ConnectionString);
                int idStagione = cStagione.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));

                Assert.AreNotEqual(0, idStagione);

                DBControllers.SusyLeague.CompetizioneDBController cCompetizione = new DBControllers.SusyLeague.CompetizioneDBController(DBIntegrationTests.ConnectionString);
                int idCompetizione = cCompetizione.InsertCompetizione(DataMock.FakeData.GetString(8, false, true, false, false), idStagione);

                Assert.AreNotEqual(0, idCompetizione);

                int idGiornata = cCompetizione.InsertGiornata(DataMock.FakeData.GetString(8, false, true, false, false), idGiornataSerieA, idCompetizione);

                //questa istruzione genera l'eccezione 
                idGiornata = cCompetizione.InsertGiornata(DataMock.FakeData.GetString(8, false, true, false, false), idGiornataSerieA, idCompetizione);
                //qui non dovrebbe mai passare
                Assert.AreEqual(0, idGiornata);
            }
            catch (SusyLeagueDBException ex)
            {
                if (ManageError)
                    Assert.AreEqual(53003, ex.Code);
                else if (!TestError) { }
                else
                    throw;
            }
        }

        [TestMethod]
        [Description("controlla che in caso di inserimento della giornata nella competizione susy league con stagione inesistente, venga restituito l'errore 547 dal DB")]
        public void InsertSusyLeagueGiornata_Error_StagioneNOTFOUND()
        {
            try
            {
                DBControllers.SerieA.StagioneDBController c = new DBControllers.SerieA.StagioneDBController(DBIntegrationTests.ConnectionString);
                int id = c.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, id);


                DateTime giornataStartDate = DataMock.FakeData.GetDate(new DateTime(2019, 8, 21, 15, 00, 00), 0, 0);
                DateTime giornataEndDate = DataMock.FakeData.GetDate(giornataStartDate, 0, 72);
                int idGiornataSerieA = c.InsertGiornataInStagione(DataMock.FakeData.GetString(8, false, true, false, false),
                    giornataStartDate,
                    giornataEndDate,
                    id);

                Assert.AreNotEqual(0, idGiornataSerieA);


                DBControllers.SusyLeague.StagioneDBController cStagione = new DBControllers.SusyLeague.StagioneDBController(DBIntegrationTests.ConnectionString);
                int idStagione = cStagione.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));

                Assert.AreNotEqual(0, idStagione);

                DBControllers.SusyLeague.CompetizioneDBController cCompetizione = new DBControllers.SusyLeague.CompetizioneDBController(DBIntegrationTests.ConnectionString);
                int idCompetizione = cCompetizione.InsertCompetizione(DataMock.FakeData.GetString(8, false, true, false, false), idStagione);

                Assert.AreNotEqual(0, idCompetizione);

                //questa istruzione genera l'eccezione 
                int idGiornata = cCompetizione.InsertGiornata(DataMock.FakeData.GetString(8, false, true, false, false), 0, idCompetizione);
                //qui non dovrebbe mai passare
                Assert.AreEqual(0, idGiornata);
            }
            catch (SusyLeagueDBException ex)
            {
                if (ManageError)
                    Assert.AreEqual(547, ex.Code);
                else if (!TestError) { }
                else
                    throw;
            }
        }

        [TestMethod]
        [Description("controlla che in caso di inserimento della giornata nella competizione susy league con competizione inesistente, venga restituito l'errore 547 dal DB")]
        public void InsertSusyLeagueGiornata_Error_competizioneNOTFOUND()
        {
            try
            {
                DBControllers.SerieA.StagioneDBController c = new DBControllers.SerieA.StagioneDBController(DBIntegrationTests.ConnectionString);
                int id = c.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, id);


                DateTime giornataStartDate = DataMock.FakeData.GetDate(new DateTime(2019, 8, 21, 15, 00, 00), 0, 0);
                DateTime giornataEndDate = DataMock.FakeData.GetDate(giornataStartDate, 0, 72);
                int idGiornataSerieA = c.InsertGiornataInStagione(DataMock.FakeData.GetString(8, false, true, false, false),
                    giornataStartDate,
                    giornataEndDate,
                    id);

                Assert.AreNotEqual(0, idGiornataSerieA);


                DBControllers.SusyLeague.StagioneDBController cStagione = new DBControllers.SusyLeague.StagioneDBController(DBIntegrationTests.ConnectionString);
                int idStagione = cStagione.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));

                Assert.AreNotEqual(0, idStagione);

                DBControllers.SusyLeague.CompetizioneDBController cCompetizione = new DBControllers.SusyLeague.CompetizioneDBController(DBIntegrationTests.ConnectionString);
                int idCompetizione = cCompetizione.InsertCompetizione(DataMock.FakeData.GetString(8, false, true, false, false), idStagione);

                Assert.AreNotEqual(0, idCompetizione);

                //questa istruzione genera l'eccezione 
                int idGiornata = cCompetizione.InsertGiornata(DataMock.FakeData.GetString(8, false, true, false, false), idGiornataSerieA, 0);
                //qui non dovrebbe mai passare
                Assert.AreEqual(0, idGiornata);
            }
            catch (SusyLeagueDBException ex)
            {
                if (ManageError)
                    Assert.AreEqual(547, ex.Code);
                else if (!TestError) { }
                else
                    throw;
            }
        }

        [TestMethod]
        public void InsertSusyLeagueSquadra()
        {
            try
            {
                DBControllers.SusyLeague.StagioneDBController cStagione = new DBControllers.SusyLeague.StagioneDBController(DBIntegrationTests.ConnectionString);
                int idStagione = cStagione.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idStagione);

                UserDBController cUser = new UserDBController(DBIntegrationTests.ConnectionString);
                int idUser = cUser.InsertUser(DataMock.FakeData.GetUsername());
                Assert.AreNotEqual(0, idUser);

                DBControllers.SusyLeague.SquadraDBController cSquadra = new DBControllers.SusyLeague.SquadraDBController(DBIntegrationTests.ConnectionString);
                int idSquadra = cSquadra.InsertSquadra(DataMock.FakeData.GetString(8, false, true, false, false), idStagione, idUser);
                Assert.AreNotEqual(0, idSquadra);
            }
            catch (Exception)
            {

                throw;
            }

        }

        [TestMethod]
        [Description("controlla che in caso di inserimento di squadra duplicata nella stagione susy league, venga restituito l'errore 53007 dal DB")]
        public void InsertSusyLeagueSquadra_Error_SquadraDuplicata()
        {
            try
            {
                DBControllers.SusyLeague.StagioneDBController cStagione = new DBControllers.SusyLeague.StagioneDBController(DBIntegrationTests.ConnectionString);
                int idStagione = cStagione.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idStagione);

                UserDBController cUser = new UserDBController(DBIntegrationTests.ConnectionString);
                int idUser = cUser.InsertUser(DataMock.FakeData.GetUsername());
                Assert.AreNotEqual(0, idUser);

                DBControllers.SusyLeague.SquadraDBController cSquadra = new DBControllers.SusyLeague.SquadraDBController(DBIntegrationTests.ConnectionString);
                string descrizione = DataMock.FakeData.GetString(8, false, true, false, false);
                int idSquadra = cSquadra.InsertSquadra(descrizione, idStagione, idUser);
                Assert.AreNotEqual(0, idSquadra);

                //questa istruzione genera l'eccezione 
                idSquadra = cSquadra.InsertSquadra(descrizione, idStagione, idUser);
                //qui non dovrebbe mai passare
                Assert.AreEqual(0, idSquadra);
            }
            catch (SusyLeagueDBException ex)
            {
                if (ManageError)
                    Assert.AreEqual(53007, ex.Code);
                else if (!TestError) { }
                else
                    throw;
            }
        }

        [TestMethod]
        [Description("controlla che in caso di inserimento di squadra doppia per l'utente, venga restituito l'errore 53008 dal DB")]
        public void InsertSusyLeagueSquadra_Error_SquadraDoppia()
        {
            try
            {
                DBControllers.SusyLeague.StagioneDBController cStagione = new DBControllers.SusyLeague.StagioneDBController(DBIntegrationTests.ConnectionString);
                int idStagione = cStagione.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idStagione);

                UserDBController cUser = new UserDBController(DBIntegrationTests.ConnectionString);
                int idUser = cUser.InsertUser(DataMock.FakeData.GetUsername());
                Assert.AreNotEqual(0, idUser);

                DBControllers.SusyLeague.SquadraDBController cSquadra = new DBControllers.SusyLeague.SquadraDBController(DBIntegrationTests.ConnectionString);
                string descrizione = DataMock.FakeData.GetString(8, false, true, false, false);
                int idSquadra = cSquadra.InsertSquadra(DataMock.FakeData.GetString(8, false, true, false, false), idStagione, idUser);
                Assert.AreNotEqual(0, idSquadra);

                //questa istruzione genera l'eccezione 
                idSquadra = cSquadra.InsertSquadra(DataMock.FakeData.GetString(8, false, true, false, false), idStagione, idUser);
                //qui non dovrebbe mai passare
                Assert.AreEqual(0, idSquadra);
            }
            catch (SusyLeagueDBException ex)
            {
                if (ManageError)
                    Assert.AreEqual(53008, ex.Code);
                else if (!TestError) { }
                else
                    throw;
            }
        }

        [TestMethod]
        [Description("controlla che in caso di inserimento di squadra con stagione inesistente , venga restituito l'errore 547 dal DB")]
        public void InsertSusyLeagueSquadra_Error_StagioneNOTFOUND()
        {
            try
            {
                DBControllers.SusyLeague.StagioneDBController cStagione = new DBControllers.SusyLeague.StagioneDBController(DBIntegrationTests.ConnectionString);
                int idStagione = cStagione.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idStagione);

                UserDBController cUser = new UserDBController(DBIntegrationTests.ConnectionString);
                int idUser = cUser.InsertUser(DataMock.FakeData.GetUsername());
                Assert.AreNotEqual(0, idUser);

                DBControllers.SusyLeague.SquadraDBController cSquadra = new DBControllers.SusyLeague.SquadraDBController(DBIntegrationTests.ConnectionString);
                string descrizione = DataMock.FakeData.GetString(8, false, true, false, false);
                //questa istruzione genera l'eccezione 
                int idSquadra = cSquadra.InsertSquadra(DataMock.FakeData.GetString(8, false, true, false, false), 0, idUser);
                //qui non dovrebbe mai passare
                Assert.AreEqual(0, idSquadra);
            }
            catch (SusyLeagueDBException ex)
            {
                if (ManageError)
                    Assert.AreEqual(547, ex.Code);
                else if (!TestError) { }
                else
                    throw;
            }
        }

        [TestMethod]
        [Description("controlla che in caso di inserimento di squadra con User inesistente , venga restituito l'errore 547 dal DB")]
        public void InsertSusyLeagueSquadra_Error_UserNOTFOUND()
        {
            try
            {
                DBControllers.SusyLeague.StagioneDBController cStagione = new DBControllers.SusyLeague.StagioneDBController(DBIntegrationTests.ConnectionString);
                int idStagione = cStagione.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idStagione);

                UserDBController cUser = new UserDBController(DBIntegrationTests.ConnectionString);
                int idUser = cUser.InsertUser(DataMock.FakeData.GetUsername());
                Assert.AreNotEqual(0, idUser);

                DBControllers.SusyLeague.SquadraDBController cSquadra = new DBControllers.SusyLeague.SquadraDBController(DBIntegrationTests.ConnectionString);
                string descrizione = DataMock.FakeData.GetString(8, false, true, false, false);
                //questa istruzione genera l'eccezione 
                int idSquadra = cSquadra.InsertSquadra(DataMock.FakeData.GetString(8, false, true, false, false), idStagione, 0);
                //qui non dovrebbe mai passare
                Assert.AreEqual(0, idSquadra);
            }
            catch (SusyLeagueDBException ex)
            {
                if (ManageError)
                    Assert.AreEqual(547, ex.Code);
                else if (!TestError) { }
                else
                    throw;
            }
        }

        [TestMethod]
        public void InsertSusyLeagueIncontro()
        {
            try
            {

                DBControllers.SerieA.StagioneDBController c = new DBControllers.SerieA.StagioneDBController(DBIntegrationTests.ConnectionString);
                int id = c.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, id);


                DateTime giornataStartDate = DataMock.FakeData.GetDate(new DateTime(2019, 8, 21, 15, 00, 00), 0, 0);
                DateTime giornataEndDate = DataMock.FakeData.GetDate(giornataStartDate, 0, 72);
                int idGiornata = c.InsertGiornataInStagione(DataMock.FakeData.GetString(8, false, true, false, false),
                    giornataStartDate,
                    giornataEndDate,
                    id);

                Assert.AreNotEqual(0, idGiornata);

                DBControllers.SusyLeague.StagioneDBController cStagione = new DBControllers.SusyLeague.StagioneDBController(DBIntegrationTests.ConnectionString);
                int idStagione = cStagione.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idStagione);



                UserDBController cUser = new UserDBController(DBIntegrationTests.ConnectionString);
                int idUser1 = cUser.InsertUser(DataMock.FakeData.GetUsername());
                Assert.AreNotEqual(0, idUser1);

                DBControllers.SusyLeague.SquadraDBController cSquadra = new DBControllers.SusyLeague.SquadraDBController(DBIntegrationTests.ConnectionString);
                int idSquadra1 = cSquadra.InsertSquadra(DataMock.FakeData.GetString(8, false, true, false, false), idStagione, idUser1);
                Assert.AreNotEqual(0, idSquadra1);

                int idUser2 = cUser.InsertUser(DataMock.FakeData.GetUsername());
                Assert.AreNotEqual(0, idUser2);

                int idSquadra2 = cSquadra.InsertSquadra(DataMock.FakeData.GetString(8, false, true, false, false), idStagione, idUser2);
                Assert.AreNotEqual(0, idSquadra2);

                DBControllers.SusyLeague.CompetizioneDBController cCompetizione = new DBControllers.SusyLeague.CompetizioneDBController(DBIntegrationTests.ConnectionString);
                int idCompetizione = cCompetizione.InsertCompetizione(DataMock.FakeData.GetString(8, false, true, false, false), idStagione);
                Assert.AreNotEqual(0, idCompetizione);

                int idGiornataSL = cCompetizione.InsertGiornata(DataMock.FakeData.GetString(8, false, true, false, false), idGiornata, idCompetizione);
                Assert.AreNotEqual(0, idGiornataSL);

                DBControllers.SusyLeague.IncontroDBController cIncontro = new DBControllers.SusyLeague.IncontroDBController(DBIntegrationTests.ConnectionString);
                int idIncontro = cIncontro.InsertIncontro(idSquadra1, idSquadra2, idGiornataSL);
                Assert.AreNotEqual(0, idIncontro);

            }
            catch (Exception)
            {

                throw;
            }
        }

        [TestMethod]
        [Description("controlla che in caso di inserimento di incontro doppio , venga restituito l'errore 53004 dal DB")]
        public void InsertSusyLeagueIncontro_Error_duplicato()
        {
            try
            {
                DBControllers.SerieA.StagioneDBController c = new DBControllers.SerieA.StagioneDBController(DBIntegrationTests.ConnectionString);
                int id = c.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, id);


                DateTime giornataStartDate = DataMock.FakeData.GetDate(new DateTime(2019, 8, 21, 15, 00, 00), 0, 0);
                DateTime giornataEndDate = DataMock.FakeData.GetDate(giornataStartDate, 0, 72);
                int idGiornata = c.InsertGiornataInStagione(DataMock.FakeData.GetString(8, false, true, false, false),
                    giornataStartDate,
                    giornataEndDate,
                    id);

                Assert.AreNotEqual(0, idGiornata);

                DBControllers.SusyLeague.StagioneDBController cStagione = new DBControllers.SusyLeague.StagioneDBController(DBIntegrationTests.ConnectionString);
                int idStagione = cStagione.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idStagione);

                UserDBController cUser = new UserDBController(DBIntegrationTests.ConnectionString);
                int idUser1 = cUser.InsertUser(DataMock.FakeData.GetUsername());
                Assert.AreNotEqual(0, idUser1);

                DBControllers.SusyLeague.SquadraDBController cSquadra = new DBControllers.SusyLeague.SquadraDBController(DBIntegrationTests.ConnectionString);
                int idSquadra1 = cSquadra.InsertSquadra(DataMock.FakeData.GetString(8, false, true, false, false), idStagione, idUser1);
                Assert.AreNotEqual(0, idSquadra1);

                int idUser2 = cUser.InsertUser(DataMock.FakeData.GetUsername());
                Assert.AreNotEqual(0, idUser2);

                int idSquadra2 = cSquadra.InsertSquadra(DataMock.FakeData.GetString(8, false, true, false, false), idStagione, idUser2);
                Assert.AreNotEqual(0, idSquadra2);

                DBControllers.SusyLeague.CompetizioneDBController cCompetizione = new DBControllers.SusyLeague.CompetizioneDBController(DBIntegrationTests.ConnectionString);
                int idCompetizione = cCompetizione.InsertCompetizione(DataMock.FakeData.GetString(8, false, true, false, false), idStagione);

                Assert.AreNotEqual(0, idCompetizione);

                int idGiornataSL = cCompetizione.InsertGiornata(DataMock.FakeData.GetString(8, false, true, false, false), idGiornata, idCompetizione);
                Assert.AreNotEqual(0, idGiornataSL);

                DBControllers.SusyLeague.IncontroDBController cIncontro = new DBControllers.SusyLeague.IncontroDBController(DBIntegrationTests.ConnectionString);
                int idIncontro = cIncontro.InsertIncontro(idSquadra1, idSquadra2, idGiornataSL);
                Assert.AreNotEqual(0, idIncontro);

                //questa istruzione genera l'eccezione 
                idIncontro = cIncontro.InsertIncontro(idSquadra1, idSquadra2, idGiornataSL);
                //qui non dovrebbe mai passare
                Assert.AreEqual(0, idIncontro);
            }
            catch (SusyLeagueDBException ex)
            {
                if (ManageError)
                    Assert.AreEqual(53004, ex.Code);
                else if (!TestError) { }
                else
                    throw;
            }
        }


        [TestMethod]
        [Description("controlla che in caso di inserimento di incontro con squadra1 not found , venga restituito l'errore 547 dal DB")]
        public void InsertSusyLeagueIncontro_Error_Squadra1NOTFOUND()
        {
            try
            {
                DBControllers.SerieA.StagioneDBController c = new DBControllers.SerieA.StagioneDBController(DBIntegrationTests.ConnectionString);
                int id = c.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, id);


                DateTime giornataStartDate = DataMock.FakeData.GetDate(new DateTime(2019, 8, 21, 15, 00, 00), 0, 0);
                DateTime giornataEndDate = DataMock.FakeData.GetDate(giornataStartDate, 0, 72);
                int idGiornata = c.InsertGiornataInStagione(DataMock.FakeData.GetString(8, false, true, false, false),
                    giornataStartDate,
                    giornataEndDate,
                    id);

                Assert.AreNotEqual(0, idGiornata);

                DBControllers.SusyLeague.StagioneDBController cStagione = new DBControllers.SusyLeague.StagioneDBController(DBIntegrationTests.ConnectionString);
                int idStagione = cStagione.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idStagione);

                UserDBController cUser = new UserDBController(DBIntegrationTests.ConnectionString);
                int idUser1 = cUser.InsertUser(DataMock.FakeData.GetUsername());
                Assert.AreNotEqual(0, idUser1);

                DBControllers.SusyLeague.SquadraDBController cSquadra = new DBControllers.SusyLeague.SquadraDBController(DBIntegrationTests.ConnectionString);
                int idSquadra1 = cSquadra.InsertSquadra(DataMock.FakeData.GetString(8, false, true, false, false), idStagione, idUser1);
                Assert.AreNotEqual(0, idSquadra1);

                int idUser2 = cUser.InsertUser(DataMock.FakeData.GetUsername());
                Assert.AreNotEqual(0, idUser2);

                int idSquadra2 = cSquadra.InsertSquadra(DataMock.FakeData.GetString(8, false, true, false, false), idStagione, idUser2);
                Assert.AreNotEqual(0, idSquadra2);

                DBControllers.SusyLeague.CompetizioneDBController cCompetizione = new DBControllers.SusyLeague.CompetizioneDBController(DBIntegrationTests.ConnectionString);
                int idCompetizione = cCompetizione.InsertCompetizione(DataMock.FakeData.GetString(8, false, true, false, false), idStagione);

                Assert.AreNotEqual(0, idCompetizione);

                int idGiornataSL = cCompetizione.InsertGiornata(DataMock.FakeData.GetString(8, false, true, false, false), idGiornata, idCompetizione);
                Assert.AreNotEqual(0, idGiornataSL);

                DBControllers.SusyLeague.IncontroDBController cIncontro = new DBControllers.SusyLeague.IncontroDBController(DBIntegrationTests.ConnectionString);
                //questa istruzione genera l'eccezione 
                int idIncontro = cIncontro.InsertIncontro(0, idSquadra2, idGiornataSL);
                //qui non dovrebbe mai passare
                Assert.AreEqual(0, idIncontro);
            }
            catch (SusyLeagueDBException ex)
            {
                if (ManageError)
                    Assert.AreEqual(547, ex.Code);
                else if (!TestError) { }
                else
                    throw;
            }
        }

        [TestMethod]
        [Description("controlla che in caso di inserimento di incontro con squadra2 not found , venga restituito l'errore 547 dal DB")]
        public void InsertSusyLeagueIncontro_Error_Squadra2NOTFOUND()
        {
            try
            {
                DBControllers.SerieA.StagioneDBController c = new DBControllers.SerieA.StagioneDBController(DBIntegrationTests.ConnectionString);
                int id = c.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, id);


                DateTime giornataStartDate = DataMock.FakeData.GetDate(new DateTime(2019, 8, 21, 15, 00, 00), 0, 0);
                DateTime giornataEndDate = DataMock.FakeData.GetDate(giornataStartDate, 0, 72);
                int idGiornata = c.InsertGiornataInStagione(DataMock.FakeData.GetString(8, false, true, false, false),
                    giornataStartDate,
                    giornataEndDate,
                    id);

                Assert.AreNotEqual(0, idGiornata);

                DBControllers.SusyLeague.StagioneDBController cStagione = new DBControllers.SusyLeague.StagioneDBController(DBIntegrationTests.ConnectionString);
                int idStagione = cStagione.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idStagione);

                UserDBController cUser = new UserDBController(DBIntegrationTests.ConnectionString);
                int idUser1 = cUser.InsertUser(DataMock.FakeData.GetUsername());
                Assert.AreNotEqual(0, idUser1);

                DBControllers.SusyLeague.SquadraDBController cSquadra = new DBControllers.SusyLeague.SquadraDBController(DBIntegrationTests.ConnectionString);
                int idSquadra1 = cSquadra.InsertSquadra(DataMock.FakeData.GetString(8, false, true, false, false), idStagione, idUser1);
                Assert.AreNotEqual(0, idSquadra1);

                int idUser2 = cUser.InsertUser(DataMock.FakeData.GetUsername());
                Assert.AreNotEqual(0, idUser2);

                int idSquadra2 = cSquadra.InsertSquadra(DataMock.FakeData.GetString(8, false, true, false, false), idStagione, idUser2);
                Assert.AreNotEqual(0, idSquadra2);

                DBControllers.SusyLeague.CompetizioneDBController cCompetizione = new DBControllers.SusyLeague.CompetizioneDBController(DBIntegrationTests.ConnectionString);
                int idCompetizione = cCompetizione.InsertCompetizione(DataMock.FakeData.GetString(8, false, true, false, false), idStagione);

                Assert.AreNotEqual(0, idCompetizione);

                int idGiornataSL = cCompetizione.InsertGiornata(DataMock.FakeData.GetString(8, false, true, false, false), idGiornata, idCompetizione);
                Assert.AreNotEqual(0, idGiornataSL);

                DBControllers.SusyLeague.IncontroDBController cIncontro = new DBControllers.SusyLeague.IncontroDBController(DBIntegrationTests.ConnectionString);
                //questa istruzione genera l'eccezione 
                int idIncontro = cIncontro.InsertIncontro(idSquadra1, 0, idGiornataSL);
                //qui non dovrebbe mai passare
                Assert.AreEqual(0, idIncontro);
            }
            catch (SusyLeagueDBException ex)
            {
                if (ManageError)
                    Assert.AreEqual(547, ex.Code);
                else if (!TestError) { }
                else
                    throw;
            }
        }

        [TestMethod]
        [Description("controlla che in caso di inserimento di incontro con gionrata susy league not found , venga restituito l'errore 547 dal DB")]
        public void InsertSusyLeagueIncontro_Error_GiornataSLNOTFOUND()
        {
            try
            {
                DBControllers.SerieA.StagioneDBController c = new DBControllers.SerieA.StagioneDBController(DBIntegrationTests.ConnectionString);
                int id = c.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, id);


                DateTime giornataStartDate = DataMock.FakeData.GetDate(new DateTime(2019, 8, 21, 15, 00, 00), 0, 0);
                DateTime giornataEndDate = DataMock.FakeData.GetDate(giornataStartDate, 0, 72);
                int idGiornata = c.InsertGiornataInStagione(DataMock.FakeData.GetString(8, false, true, false, false),
                    giornataStartDate,
                    giornataEndDate,
                    id);

                Assert.AreNotEqual(0, idGiornata);

                DBControllers.SusyLeague.StagioneDBController cStagione = new DBControllers.SusyLeague.StagioneDBController(DBIntegrationTests.ConnectionString);
                int idStagione = cStagione.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idStagione);

                UserDBController cUser = new UserDBController(DBIntegrationTests.ConnectionString);
                int idUser1 = cUser.InsertUser(DataMock.FakeData.GetUsername());
                Assert.AreNotEqual(0, idUser1);

                DBControllers.SusyLeague.SquadraDBController cSquadra = new DBControllers.SusyLeague.SquadraDBController(DBIntegrationTests.ConnectionString);
                int idSquadra1 = cSquadra.InsertSquadra(DataMock.FakeData.GetString(8, false, true, false, false), idStagione, idUser1);
                Assert.AreNotEqual(0, idSquadra1);

                int idUser2 = cUser.InsertUser(DataMock.FakeData.GetUsername());
                Assert.AreNotEqual(0, idUser2);

                int idSquadra2 = cSquadra.InsertSquadra(DataMock.FakeData.GetString(8, false, true, false, false), idStagione, idUser2);
                Assert.AreNotEqual(0, idSquadra2);

                DBControllers.SusyLeague.CompetizioneDBController cCompetizione = new DBControllers.SusyLeague.CompetizioneDBController(DBIntegrationTests.ConnectionString);
                int idCompetizione = cCompetizione.InsertCompetizione(DataMock.FakeData.GetString(8, false, true, false, false), idStagione);

                Assert.AreNotEqual(0, idCompetizione);

                int idGiornataSL = cCompetizione.InsertGiornata(DataMock.FakeData.GetString(8, false, true, false, false), idGiornata, idCompetizione);
                Assert.AreNotEqual(0, idGiornataSL);

                DBControllers.SusyLeague.IncontroDBController cIncontro = new DBControllers.SusyLeague.IncontroDBController(DBIntegrationTests.ConnectionString);
                //questa istruzione genera l'eccezione 
                int idIncontro = cIncontro.InsertIncontro(idSquadra1, idSquadra2, 0);
                //qui non dovrebbe mai passare
                Assert.AreEqual(0, idIncontro);
            }
            catch (SusyLeagueDBException ex)
            {
                if (ManageError)
                    Assert.AreEqual(547, ex.Code);
                else if (!TestError) { }
                else
                    throw;
            }
        }

        [TestMethod]
        public void UpdateSusyLeagueIncontroRisultato()
        {
            try
            {

                DBControllers.SerieA.StagioneDBController c = new DBControllers.SerieA.StagioneDBController(DBIntegrationTests.ConnectionString);
                int id = c.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, id);


                DateTime giornataStartDate = DataMock.FakeData.GetDate(new DateTime(2019, 8, 21, 15, 00, 00), 0, 0);
                DateTime giornataEndDate = DataMock.FakeData.GetDate(giornataStartDate, 0, 72);
                int idGiornata = c.InsertGiornataInStagione(DataMock.FakeData.GetString(8, false, true, false, false),
                    giornataStartDate,
                    giornataEndDate,
                    id);

                Assert.AreNotEqual(0, idGiornata);

                DBControllers.SusyLeague.StagioneDBController cStagione = new DBControllers.SusyLeague.StagioneDBController(DBIntegrationTests.ConnectionString);
                int idStagione = cStagione.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idStagione);



                UserDBController cUser = new UserDBController(DBIntegrationTests.ConnectionString);
                int idUser1 = cUser.InsertUser(DataMock.FakeData.GetUsername());
                Assert.AreNotEqual(0, idUser1);

                DBControllers.SusyLeague.SquadraDBController cSquadra = new DBControllers.SusyLeague.SquadraDBController(DBIntegrationTests.ConnectionString);
                int idSquadra1 = cSquadra.InsertSquadra(DataMock.FakeData.GetString(8, false, true, false, false), idStagione, idUser1);
                Assert.AreNotEqual(0, idSquadra1);

                int idUser2 = cUser.InsertUser(DataMock.FakeData.GetUsername());
                Assert.AreNotEqual(0, idUser2);

                int idSquadra2 = cSquadra.InsertSquadra(DataMock.FakeData.GetString(8, false, true, false, false), idStagione, idUser2);
                Assert.AreNotEqual(0, idSquadra2);

                DBControllers.SusyLeague.CompetizioneDBController cCompetizione = new DBControllers.SusyLeague.CompetizioneDBController(DBIntegrationTests.ConnectionString);
                int idCompetizione = cCompetizione.InsertCompetizione(DataMock.FakeData.GetString(8, false, true, false, false), idStagione);
                Assert.AreNotEqual(0, idCompetizione);

                int idGiornataSL = cCompetizione.InsertGiornata(DataMock.FakeData.GetString(8, false, true, false, false), idGiornata, idCompetizione);
                Assert.AreNotEqual(0, idGiornataSL);

                DBControllers.SusyLeague.IncontroDBController cIncontro = new DBControllers.SusyLeague.IncontroDBController(DBIntegrationTests.ConnectionString);
                int idIncontro = cIncontro.InsertIncontro(idSquadra1, idSquadra2, idGiornataSL);
                Assert.AreNotEqual(0, idIncontro);

                cIncontro.UpdateRisultato(idIncontro, DataMock.FakeData.GetInteger(0, 10), DataMock.FakeData.GetInteger(0, 10), DataMock.FakeData.GetDouble(2, 1), DataMock.FakeData.GetDouble(2, 1));

            }
            catch (Exception)
            {

                throw;
            }
        }


        [TestMethod]
        [Description("controlla che in caso di inserimento di update dell'incontro per inseire il risultato, se l'incontro non viene trovato, venga restituito l'errore 53006 dal DB")]
        public void UpdateSusyLeagueIncontroRisultato_Error_NOTFOUND()
        {
            try
            {

                DBControllers.SerieA.StagioneDBController c = new DBControllers.SerieA.StagioneDBController(DBIntegrationTests.ConnectionString);
                int id = c.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, id);


                DateTime giornataStartDate = DataMock.FakeData.GetDate(new DateTime(2019, 8, 21, 15, 00, 00), 0, 0);
                DateTime giornataEndDate = DataMock.FakeData.GetDate(giornataStartDate, 0, 72);
                int idGiornata = c.InsertGiornataInStagione(DataMock.FakeData.GetString(8, false, true, false, false),
                    giornataStartDate,
                    giornataEndDate,
                    id);

                Assert.AreNotEqual(0, idGiornata);

                DBControllers.SusyLeague.StagioneDBController cStagione = new DBControllers.SusyLeague.StagioneDBController(DBIntegrationTests.ConnectionString);
                int idStagione = cStagione.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idStagione);



                UserDBController cUser = new UserDBController(DBIntegrationTests.ConnectionString);
                int idUser1 = cUser.InsertUser(DataMock.FakeData.GetUsername());
                Assert.AreNotEqual(0, idUser1);

                DBControllers.SusyLeague.SquadraDBController cSquadra = new DBControllers.SusyLeague.SquadraDBController(DBIntegrationTests.ConnectionString);
                int idSquadra1 = cSquadra.InsertSquadra(DataMock.FakeData.GetString(8, false, true, false, false), idStagione, idUser1);
                Assert.AreNotEqual(0, idSquadra1);

                int idUser2 = cUser.InsertUser(DataMock.FakeData.GetUsername());
                Assert.AreNotEqual(0, idUser2);

                int idSquadra2 = cSquadra.InsertSquadra(DataMock.FakeData.GetString(8, false, true, false, false), idStagione, idUser2);
                Assert.AreNotEqual(0, idSquadra2);

                DBControllers.SusyLeague.CompetizioneDBController cCompetizione = new DBControllers.SusyLeague.CompetizioneDBController(DBIntegrationTests.ConnectionString);
                int idCompetizione = cCompetizione.InsertCompetizione(DataMock.FakeData.GetString(8, false, true, false, false), idStagione);
                Assert.AreNotEqual(0, idCompetizione);

                int idGiornataSL = cCompetizione.InsertGiornata(DataMock.FakeData.GetString(8, false, true, false, false), idGiornata, idCompetizione);
                Assert.AreNotEqual(0, idGiornataSL);

                DBControllers.SusyLeague.IncontroDBController cIncontro = new DBControllers.SusyLeague.IncontroDBController(DBIntegrationTests.ConnectionString);
                int idIncontro = cIncontro.InsertIncontro(idSquadra1, idSquadra2, idGiornataSL);
                Assert.AreNotEqual(0, idIncontro);
                //questa istruzione genera l'eccezione 
                cIncontro.UpdateRisultato(0, DataMock.FakeData.GetInteger(0, 10), DataMock.FakeData.GetInteger(0, 10), DataMock.FakeData.GetDouble(2, 1), DataMock.FakeData.GetDouble(2, 1));

            }
            catch (SusyLeagueDBException ex)
            {
                if (ManageError)
                    Assert.AreEqual(53006, ex.Code);
                else if (!TestError) { }
                else
                    throw;
            }
        }

        [TestMethod]
        public void InsertSusyLeagueIncontroGiocatoreSquadra()
        {
            try
            {
                DBControllers.SusyLeague.StagioneDBController cStagione = new DBControllers.SusyLeague.StagioneDBController(DBIntegrationTests.ConnectionString);
                int idStagione = cStagione.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idStagione);

                UserDBController cUser = new UserDBController(DBIntegrationTests.ConnectionString);
                int idUser = cUser.InsertUser(DataMock.FakeData.GetUsername());
                Assert.AreNotEqual(0, idUser);

                DBControllers.SusyLeague.SquadraDBController cSquadra = new DBControllers.SusyLeague.SquadraDBController(DBIntegrationTests.ConnectionString);
                int idSquadra1 = cSquadra.InsertSquadra(DataMock.FakeData.GetString(8, false, true, false, false), idStagione, idUser);
                Assert.AreNotEqual(0, idSquadra1);

                GiocatoreDBController cGiocatore = new GiocatoreDBController(DBIntegrationTests.ConnectionString);
                int idGiocatore = cGiocatore.InsertGiocatore(DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(4, true, false, true, false));
                Assert.AreNotEqual(0, idGiocatore);

                DBControllers.SerieA.StagioneDBController c = new DBControllers.SerieA.StagioneDBController(DBIntegrationTests.ConnectionString);
                int id = c.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, id);


                DateTime giornataStartDate = DataMock.FakeData.GetDate(new DateTime(2019, 8, 21, 15, 00, 00), 0, 0);
                DateTime giornataEndDate = DataMock.FakeData.GetDate(giornataStartDate, 0, 72);
                int idGiornata = c.InsertGiornataInStagione(DataMock.FakeData.GetString(8, false, true, false, false),
                    giornataStartDate,
                    giornataEndDate,
                    id);
                Assert.AreNotEqual(0, idGiornata);

                int idUser2 = cUser.InsertUser(DataMock.FakeData.GetUsername());
                Assert.AreNotEqual(0, idUser2);

                int idSquadra2 = cSquadra.InsertSquadra(DataMock.FakeData.GetString(8, false, true, false, false), idStagione, idUser2);
                Assert.AreNotEqual(0, idSquadra2);

                DBControllers.SusyLeague.CompetizioneDBController cCompetizione = new DBControllers.SusyLeague.CompetizioneDBController(DBIntegrationTests.ConnectionString);
                int idCompetizione = cCompetizione.InsertCompetizione(DataMock.FakeData.GetString(8, false, true, false, false), idStagione);
                Assert.AreNotEqual(0, idCompetizione);

                int idGiornataSL = cCompetizione.InsertGiornata(DataMock.FakeData.GetString(8, false, true, false, false), idGiornata, idCompetizione);
                Assert.AreNotEqual(0, idGiornataSL);

                DBControllers.SusyLeague.IncontroDBController cIncontro = new DBControllers.SusyLeague.IncontroDBController(DBIntegrationTests.ConnectionString);
                int idIncontro = cIncontro.InsertIncontro(idSquadra1, idSquadra2, idGiornataSL);
                Assert.AreNotEqual(0, idIncontro);

                int idFormazione = cIncontro.InsertGiocatoreInIncontroPerSquadra(idIncontro, idSquadra1, idGiocatore, DataMock.FakeData.GetInteger(1, 11));
                Assert.AreNotEqual(0, idFormazione);
            }
            catch (Exception)
            {

                throw;
            }
        }

        [TestMethod]
        [Description("inserisco un giocatore per un incontro per una squadra (ovvero parte della formazione), in caso di inserimento duplicato viene restituito l'errore 53005")]
        public void InsertSusyLeagueIncontroGiocatoreSquadra_Error_Duplicato()
        {
            try
            {
                DBControllers.SusyLeague.StagioneDBController cStagione = new DBControllers.SusyLeague.StagioneDBController(DBIntegrationTests.ConnectionString);
                int idStagione = cStagione.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idStagione);

                UserDBController cUser = new UserDBController(DBIntegrationTests.ConnectionString);
                int idUser = cUser.InsertUser(DataMock.FakeData.GetUsername());
                Assert.AreNotEqual(0, idUser);

                DBControllers.SusyLeague.SquadraDBController cSquadra = new DBControllers.SusyLeague.SquadraDBController(DBIntegrationTests.ConnectionString);
                int idSquadra1 = cSquadra.InsertSquadra(DataMock.FakeData.GetString(8, false, true, false, false), idStagione, idUser);
                Assert.AreNotEqual(0, idSquadra1);

                GiocatoreDBController cGiocatore = new GiocatoreDBController(DBIntegrationTests.ConnectionString);
                int idGiocatore = cGiocatore.InsertGiocatore(DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(4, true, false, true, false));
                Assert.AreNotEqual(0, idGiocatore);

                DBControllers.SerieA.StagioneDBController c = new DBControllers.SerieA.StagioneDBController(DBIntegrationTests.ConnectionString);
                int id = c.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, id);


                DateTime giornataStartDate = DataMock.FakeData.GetDate(new DateTime(2019, 8, 21, 15, 00, 00), 0, 0);
                DateTime giornataEndDate = DataMock.FakeData.GetDate(giornataStartDate, 0, 72);
                int idGiornata = c.InsertGiornataInStagione(DataMock.FakeData.GetString(8, false, true, false, false),
                    giornataStartDate,
                    giornataEndDate,
                    id);
                Assert.AreNotEqual(0, idGiornata);

                int idUser2 = cUser.InsertUser(DataMock.FakeData.GetUsername());
                Assert.AreNotEqual(0, idUser2);

                int idSquadra2 = cSquadra.InsertSquadra(DataMock.FakeData.GetString(8, false, true, false, false), idStagione, idUser2);
                Assert.AreNotEqual(0, idSquadra2);

                DBControllers.SusyLeague.CompetizioneDBController cCompetizione = new DBControllers.SusyLeague.CompetizioneDBController(DBIntegrationTests.ConnectionString);
                int idCompetizione = cCompetizione.InsertCompetizione(DataMock.FakeData.GetString(8, false, true, false, false), idStagione);
                Assert.AreNotEqual(0, idCompetizione);

                int idGiornataSL = cCompetizione.InsertGiornata(DataMock.FakeData.GetString(8, false, true, false, false), idGiornata, idCompetizione);
                Assert.AreNotEqual(0, idGiornataSL);

                DBControllers.SusyLeague.IncontroDBController cIncontro = new DBControllers.SusyLeague.IncontroDBController(DBIntegrationTests.ConnectionString);
                int idIncontro = cIncontro.InsertIncontro(idSquadra1, idSquadra2, idGiornataSL);
                Assert.AreNotEqual(0, idIncontro);

                int idFormazione = cIncontro.InsertGiocatoreInIncontroPerSquadra(idIncontro, idSquadra1, idGiocatore, DataMock.FakeData.GetInteger(1, 11));
                Assert.AreNotEqual(0, idFormazione);

                //questa istruzione genera l'eccezione 
                idFormazione = cIncontro.InsertGiocatoreInIncontroPerSquadra(idIncontro, idSquadra1, idGiocatore, DataMock.FakeData.GetInteger(1, 11));
                //qui non dovrebbe mai passare
                Assert.AreEqual(0, idFormazione);
            }
            catch (SusyLeagueDBException ex)
            {
                if (ManageError)
                    Assert.AreEqual(53005, ex.Code);
                else if (!TestError) { }
                else
                    throw;
            }
        }

        [TestMethod]
        [Description("inserisco un giocatore per un incontro per una squadra (ovvero parte della formazione), verifico che in caso di Incontro NOT FOUND sia restituito l'errore 547")]
        public void InsertSusyLeagueIncontroGiocatoreSquadra_Error_IncontroNOTFOUND()
        {
            try
            {
                DBControllers.SusyLeague.StagioneDBController cStagione = new DBControllers.SusyLeague.StagioneDBController(DBIntegrationTests.ConnectionString);
                int idStagione = cStagione.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idStagione);

                UserDBController cUser = new UserDBController(DBIntegrationTests.ConnectionString);
                int idUser = cUser.InsertUser(DataMock.FakeData.GetUsername());
                Assert.AreNotEqual(0, idUser);

                DBControllers.SusyLeague.SquadraDBController cSquadra = new DBControllers.SusyLeague.SquadraDBController(DBIntegrationTests.ConnectionString);
                int idSquadra1 = cSquadra.InsertSquadra(DataMock.FakeData.GetString(8, false, true, false, false), idStagione, idUser);
                Assert.AreNotEqual(0, idSquadra1);

                GiocatoreDBController cGiocatore = new GiocatoreDBController(DBIntegrationTests.ConnectionString);
                int idGiocatore = cGiocatore.InsertGiocatore(DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(4, true, false, true, false));
                Assert.AreNotEqual(0, idGiocatore);

                DBControllers.SerieA.StagioneDBController c = new DBControllers.SerieA.StagioneDBController(DBIntegrationTests.ConnectionString);
                int id = c.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, id);


                DateTime giornataStartDate = DataMock.FakeData.GetDate(new DateTime(2019, 8, 21, 15, 00, 00), 0, 0);
                DateTime giornataEndDate = DataMock.FakeData.GetDate(giornataStartDate, 0, 72);
                int idGiornata = c.InsertGiornataInStagione(DataMock.FakeData.GetString(8, false, true, false, false),
                    giornataStartDate,
                    giornataEndDate,
                    id);
                Assert.AreNotEqual(0, idGiornata);

                int idUser2 = cUser.InsertUser(DataMock.FakeData.GetUsername());
                Assert.AreNotEqual(0, idUser2);

                int idSquadra2 = cSquadra.InsertSquadra(DataMock.FakeData.GetString(8, false, true, false, false), idStagione, idUser2);
                Assert.AreNotEqual(0, idSquadra2);

                DBControllers.SusyLeague.CompetizioneDBController cCompetizione = new DBControllers.SusyLeague.CompetizioneDBController(DBIntegrationTests.ConnectionString);
                int idCompetizione = cCompetizione.InsertCompetizione(DataMock.FakeData.GetString(8, false, true, false, false), idStagione);
                Assert.AreNotEqual(0, idCompetizione);

                int idGiornataSL = cCompetizione.InsertGiornata(DataMock.FakeData.GetString(8, false, true, false, false), idGiornata, idCompetizione);
                Assert.AreNotEqual(0, idGiornataSL);

                DBControllers.SusyLeague.IncontroDBController cIncontro = new DBControllers.SusyLeague.IncontroDBController(DBIntegrationTests.ConnectionString);
                int idIncontro = cIncontro.InsertIncontro(idSquadra1, idSquadra2, idGiornataSL);
                Assert.AreNotEqual(0, idIncontro);

                //questa istruzione genera l'eccezione 
                int idFormazione = cIncontro.InsertGiocatoreInIncontroPerSquadra(0, idSquadra1, idGiocatore, DataMock.FakeData.GetInteger(1, 11));
                //qui non dovrebbe mai passare
                Assert.AreEqual(0, idFormazione);
            }
            catch (SusyLeagueDBException ex)
            {
                if (ManageError)
                    Assert.AreEqual(547, ex.Code);
                else if (!TestError) { }
                else
                    throw;
            }
        }

        [TestMethod]
        [Description("inserisco un giocatore per un incontro per una squadra (ovvero parte della formazione), verifico che in caso di Giocatore NOT FOUND sia restituito l'errore 547")]
        public void InsertSusyLeagueIncontroGiocatoreSquadra_Error_GiocatoreNOTFOUND()
        {
            try
            {
                DBControllers.SusyLeague.StagioneDBController cStagione = new DBControllers.SusyLeague.StagioneDBController(DBIntegrationTests.ConnectionString);
                int idStagione = cStagione.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idStagione);

                UserDBController cUser = new UserDBController(DBIntegrationTests.ConnectionString);
                int idUser = cUser.InsertUser(DataMock.FakeData.GetUsername());
                Assert.AreNotEqual(0, idUser);

                DBControllers.SusyLeague.SquadraDBController cSquadra = new DBControllers.SusyLeague.SquadraDBController(DBIntegrationTests.ConnectionString);
                int idSquadra1 = cSquadra.InsertSquadra(DataMock.FakeData.GetString(8, false, true, false, false), idStagione, idUser);
                Assert.AreNotEqual(0, idSquadra1);

                GiocatoreDBController cGiocatore = new GiocatoreDBController(DBIntegrationTests.ConnectionString);
                int idGiocatore = cGiocatore.InsertGiocatore(DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(4, true, false, true, false));
                Assert.AreNotEqual(0, idGiocatore);

                DBControllers.SerieA.StagioneDBController c = new DBControllers.SerieA.StagioneDBController(DBIntegrationTests.ConnectionString);
                int id = c.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, id);


                DateTime giornataStartDate = DataMock.FakeData.GetDate(new DateTime(2019, 8, 21, 15, 00, 00), 0, 0);
                DateTime giornataEndDate = DataMock.FakeData.GetDate(giornataStartDate, 0, 72);
                int idGiornata = c.InsertGiornataInStagione(DataMock.FakeData.GetString(8, false, true, false, false),
                    giornataStartDate,
                    giornataEndDate,
                    id);
                Assert.AreNotEqual(0, idGiornata);

                int idUser2 = cUser.InsertUser(DataMock.FakeData.GetUsername());
                Assert.AreNotEqual(0, idUser2);

                int idSquadra2 = cSquadra.InsertSquadra(DataMock.FakeData.GetString(8, false, true, false, false), idStagione, idUser2);
                Assert.AreNotEqual(0, idSquadra2);

                DBControllers.SusyLeague.CompetizioneDBController cCompetizione = new DBControllers.SusyLeague.CompetizioneDBController(DBIntegrationTests.ConnectionString);
                int idCompetizione = cCompetizione.InsertCompetizione(DataMock.FakeData.GetString(8, false, true, false, false), idStagione);
                Assert.AreNotEqual(0, idCompetizione);

                int idGiornataSL = cCompetizione.InsertGiornata(DataMock.FakeData.GetString(8, false, true, false, false), idGiornata, idCompetizione);
                Assert.AreNotEqual(0, idGiornataSL);

                DBControllers.SusyLeague.IncontroDBController cIncontro = new DBControllers.SusyLeague.IncontroDBController(DBIntegrationTests.ConnectionString);
                int idIncontro = cIncontro.InsertIncontro(idSquadra1, idSquadra2, idGiornataSL);
                Assert.AreNotEqual(0, idIncontro);

                //questa istruzione genera l'eccezione 
                int idFormazione = cIncontro.InsertGiocatoreInIncontroPerSquadra(idIncontro, 0, idGiocatore, DataMock.FakeData.GetInteger(1, 11));
                //qui non dovrebbe mai passare
                Assert.AreEqual(0, idFormazione);
            }
            catch (SusyLeagueDBException ex)
            {
                if (ManageError)
                    Assert.AreEqual(547, ex.Code);
                else if (!TestError) { }
                else
                    throw;
            }
        }

        [TestMethod]
        [Description("inserisco un giocatore per un incontro per una squadra (ovvero parte della formazione), verifico che in caso di Squadra NOT FOUND sia restituito l'errore 547")]
        public void InsertSusyLeagueIncontroGiocatoreSquadra_Error_SquadraNOTFOUND()
        {
            try
            {
                DBControllers.SusyLeague.StagioneDBController cStagione = new DBControllers.SusyLeague.StagioneDBController(DBIntegrationTests.ConnectionString);
                int idStagione = cStagione.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idStagione);

                UserDBController cUser = new UserDBController(DBIntegrationTests.ConnectionString);
                int idUser = cUser.InsertUser(DataMock.FakeData.GetUsername());
                Assert.AreNotEqual(0, idUser);

                DBControllers.SusyLeague.SquadraDBController cSquadra = new DBControllers.SusyLeague.SquadraDBController(DBIntegrationTests.ConnectionString);
                int idSquadra1 = cSquadra.InsertSquadra(DataMock.FakeData.GetString(8, false, true, false, false), idStagione, idUser);
                Assert.AreNotEqual(0, idSquadra1);

                GiocatoreDBController cGiocatore = new GiocatoreDBController(DBIntegrationTests.ConnectionString);
                int idGiocatore = cGiocatore.InsertGiocatore(DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(4, true, false, true, false));
                Assert.AreNotEqual(0, idGiocatore);

                DBControllers.SerieA.StagioneDBController c = new DBControllers.SerieA.StagioneDBController(DBIntegrationTests.ConnectionString);
                int id = c.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, id);


                DateTime giornataStartDate = DataMock.FakeData.GetDate(new DateTime(2019, 8, 21, 15, 00, 00), 0, 0);
                DateTime giornataEndDate = DataMock.FakeData.GetDate(giornataStartDate, 0, 72);
                int idGiornata = c.InsertGiornataInStagione(DataMock.FakeData.GetString(8, false, true, false, false),
                    giornataStartDate,
                    giornataEndDate,
                    id);
                Assert.AreNotEqual(0, idGiornata);

                int idUser2 = cUser.InsertUser(DataMock.FakeData.GetUsername());
                Assert.AreNotEqual(0, idUser2);

                int idSquadra2 = cSquadra.InsertSquadra(DataMock.FakeData.GetString(8, false, true, false, false), idStagione, idUser2);
                Assert.AreNotEqual(0, idSquadra2);

                DBControllers.SusyLeague.CompetizioneDBController cCompetizione = new DBControllers.SusyLeague.CompetizioneDBController(DBIntegrationTests.ConnectionString);
                int idCompetizione = cCompetizione.InsertCompetizione(DataMock.FakeData.GetString(8, false, true, false, false), idStagione);
                Assert.AreNotEqual(0, idCompetizione);

                int idGiornataSL = cCompetizione.InsertGiornata(DataMock.FakeData.GetString(8, false, true, false, false), idGiornata, idCompetizione);
                Assert.AreNotEqual(0, idGiornataSL);

                DBControllers.SusyLeague.IncontroDBController cIncontro = new DBControllers.SusyLeague.IncontroDBController(DBIntegrationTests.ConnectionString);
                int idIncontro = cIncontro.InsertIncontro(idSquadra1, idSquadra2, idGiornataSL);
                Assert.AreNotEqual(0, idIncontro);

                //questa istruzione genera l'eccezione 
                int idFormazione = cIncontro.InsertGiocatoreInIncontroPerSquadra(idIncontro, idSquadra1, 0, DataMock.FakeData.GetInteger(1, 11));
                //qui non dovrebbe mai passare
                Assert.AreEqual(0, idFormazione);
            }
            catch (SusyLeagueDBException ex)
            {
                if (ManageError)
                    Assert.AreEqual(547, ex.Code);
                else if (!TestError) { }
                else
                    throw;
            }
        }
        #endregion

        #region DBControllers.Mercato
        [TestMethod]
        public void InsertMercatoFase()
        {
            try
            {

                DBControllers.SusyLeague.MercatoDBController cMercato = new DBControllers.SusyLeague.MercatoDBController(DBIntegrationTests.ConnectionString);
                int id = cMercato.InsertMercatoFase(DataMock.FakeData.GetString(10, false, true, false, false),
                    DataMock.FakeData.GetDate(DateTime.Now.AddDays(iCounter++)), DataMock.FakeData.GetDate(DateTime.Now.AddDays(iCounter++)));

                Assert.AreNotEqual(0, id);

            }
            catch (Exception)
            {

                throw;
            }
        }

        [TestMethod]
        [Description("controlla che non sia possibile inserire due fasi di mercato con giorni sovrapposti. In questo caso deve essere gestito l'errore 53009 del DB")]
        public void InsertMercatoFase_ERROR_FasiSovrapposte()
        {
            try
            {
                DateTime dataInizio = DataMock.FakeData.GetDate(DateTime.Now.AddDays(iCounter++));
                DateTime dataFine = DataMock.FakeData.GetDate(DateTime.Now.AddDays(iCounter++));

                DBControllers.SusyLeague.MercatoDBController cMercato = new DBControllers.SusyLeague.MercatoDBController(DBIntegrationTests.ConnectionString);
                int id = cMercato.InsertMercatoFase(DataMock.FakeData.GetString(10, false, true, false, false),
                    dataInizio, dataFine);
                //queste istruzione genera l'errore
                id = cMercato.InsertMercatoFase(DataMock.FakeData.GetString(10, false, true, false, false),
                    dataInizio, dataFine);
                //qui non dovrebbe mai arrivare
                Assert.AreNotEqual(0, id);

            }
            catch (SusyLeagueDBException ex)
            {
                if (ManageError)
                    Assert.AreEqual(53009, ex.Code);
                else if (!TestError) { }
                else
                    throw;
            }
        }

        [TestMethod]
        public void UpdateMercatoFase()
        {
            try
            {
                DBControllers.SusyLeague.MercatoDBController cMercato = new DBControllers.SusyLeague.MercatoDBController(DBIntegrationTests.ConnectionString);
                int id = cMercato.InsertMercatoFase(DataMock.FakeData.GetString(10, false, true, false, false),
                   DataMock.FakeData.GetDate(DateTime.Now.AddDays(iCounter++)), DataMock.FakeData.GetDate(DateTime.Now.AddDays(iCounter++)));

                Assert.AreNotEqual(0, id);

                cMercato.UpdateMercatoFase(id, DataMock.FakeData.GetString(10, false, true, false, false),
                    DataMock.FakeData.GetDate(DateTime.Now.AddDays(iCounter++)), DataMock.FakeData.GetDate(DateTime.Now.AddDays(iCounter++)));
            }
            catch (Exception)
            {

                throw;
            }
        }

        [TestMethod]
        [Description("controlla chein caso di aggiornamento di una fase non esistente, deve essere restituito  l'errore 53010 del DB")]
        public void UpdateMercatoFase_ERROR_NOTFOUND()
        {
            try
            {
                DBControllers.SusyLeague.MercatoDBController cMercato = new DBControllers.SusyLeague.MercatoDBController(DBIntegrationTests.ConnectionString);
                int id = cMercato.InsertMercatoFase(DataMock.FakeData.GetString(10, false, true, false, false),
                    DataMock.FakeData.GetDate(DateTime.Now.AddDays(iCounter++)), DataMock.FakeData.GetDate(DateTime.Now.AddDays(iCounter++)));

                Assert.AreNotEqual(0, id);

                cMercato.UpdateMercatoFase(0, DataMock.FakeData.GetString(10, false, true, false, false),
                    DataMock.FakeData.GetDate(DateTime.Now.AddDays(iCounter++)), DataMock.FakeData.GetDate(DateTime.Now.AddDays(iCounter++)));

            }
            catch (SusyLeagueDBException ex)
            {
                if (ManageError)
                    Assert.AreEqual(53010, ex.Code);
                else if (!TestError) { }
                else
                    throw;
            }
        }

        [TestMethod]
        [Description("controlla che non sia possibile avere due fasi di mercato con giorni sovrapposti. In questo caso deve essere gestito l'errore 53009 del DB")]
        public void UpdateMercatoFase_ERROR_FasiSovrapposte()
        {
            try
            {
                DateTime dataInizioFase1 = DataMock.FakeData.GetDate(DateTime.Now.AddDays(iCounter++));
                DateTime dataFineFase1 = DataMock.FakeData.GetDate(DateTime.Now.AddDays(iCounter++));
                DateTime dataInizioFase2 = DataMock.FakeData.GetDate(DateTime.Now.AddDays(iCounter++));
                DateTime dataFineFase2 = DataMock.FakeData.GetDate(DateTime.Now.AddDays(iCounter++));

                DBControllers.SusyLeague.MercatoDBController cMercato = new DBControllers.SusyLeague.MercatoDBController(DBIntegrationTests.ConnectionString);
                int id = cMercato.InsertMercatoFase(DataMock.FakeData.GetString(10, false, true, false, false),
                  dataInizioFase1, dataFineFase1);

                Assert.AreNotEqual(0, id);

                int id2 = cMercato.InsertMercatoFase(DataMock.FakeData.GetString(10, false, true, false, false),
                   dataInizioFase2, dataFineFase2);



                //queste istruzione genera l'errore
                cMercato.UpdateMercatoFase(id2, DataMock.FakeData.GetString(10, false, true, false, false),
                     dataInizioFase1, dataFineFase1);
                //qui non dovrebbe mai arrivare
                Assert.AreNotEqual(0, id);
            }
            catch (SusyLeagueDBException ex)
            {
                if (ManageError)
                    Assert.AreEqual(53009, ex.Code);
                else if (!TestError) { }
                else
                    throw;
            }
        }

        [TestMethod]
        public void InsertMercatoTransazione()
        {
            try
            {
                DBControllers.SusyLeague.MercatoDBController cMercato = new DBControllers.SusyLeague.MercatoDBController(DBIntegrationTests.ConnectionString);
                int idFase = cMercato.InsertMercatoFase(DataMock.FakeData.GetString(10, false, true, false, false),
                    DataMock.FakeData.GetDate(DateTime.Now.AddDays(iCounter++)), DataMock.FakeData.GetDate(DateTime.Now.AddDays(iCounter++)));

                Assert.AreNotEqual(0, idFase);

                int idTransazione = cMercato.InsertTransazione(idFase);
                Assert.AreNotEqual(0, idTransazione);

            }
            catch (Exception)
            {

                throw;
            }
        }

        [TestMethod]
        public void InsertGiocatoreInFantasquadra()
        {
            try
            {

                DBControllers.SusyLeague.StagioneDBController cStagione = new DBControllers.SusyLeague.StagioneDBController(DBIntegrationTests.ConnectionString);
                int idStagione = cStagione.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idStagione);

                UserDBController cUser = new UserDBController(DBIntegrationTests.ConnectionString);
                int idUser = cUser.InsertUser(DataMock.FakeData.GetUsername());
                Assert.AreNotEqual(0, idUser);

                DBControllers.SusyLeague.SquadraDBController cSLSquadra = new DBControllers.SusyLeague.SquadraDBController(DBIntegrationTests.ConnectionString);
                int idFantasquadra = cSLSquadra.InsertSquadra(DataMock.FakeData.GetString(8, false, true, false, false), idStagione, idUser);
                Assert.AreNotEqual(0, idFantasquadra);


                GiocatoreDBController cGiocatore = new GiocatoreDBController(DBIntegrationTests.ConnectionString);
                int idGiocatore = cGiocatore.InsertGiocatore(DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(4, true, false, true, false));

                Assert.AreNotEqual(0, idGiocatore);

                DBControllers.SusyLeague.StagioneDBController cSLStagione = new DBControllers.SusyLeague.StagioneDBController(DBIntegrationTests.ConnectionString);
                int idSLStagione = cSLStagione.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idSLStagione);


                DBControllers.SusyLeague.MercatoDBController cMercato = new DBControllers.SusyLeague.MercatoDBController(DBIntegrationTests.ConnectionString);
                int idFase = cMercato.InsertMercatoFase(DataMock.FakeData.GetString(10, false, true, false, false),
                    DataMock.FakeData.GetDate(DateTime.Now.AddDays(iCounter++)), DataMock.FakeData.GetDate(DateTime.Now.AddDays(iCounter++)));
                Assert.AreNotEqual(0, idFase);

                int idTransazione = cMercato.InsertTransazione(idFase);
                Assert.AreNotEqual(0, idTransazione);

                int idGiocatoreInfantasquadra = cSLSquadra.InsertGiocatoreInFantasquadra(idGiocatore, idFantasquadra, idSLStagione, idTransazione, DateTime.Now);
                Assert.AreNotEqual(0, idGiocatoreInfantasquadra);
            }
            catch (Exception)
            {

                throw;
            }
        }

        [TestMethod]
        [Description("controlla che non sia possibile inserire in una fantasquadra un giocatore che ha una associazione attiva con un'altra fantasquadra. In questo caso deve essere gestito l'errore 53014 del DB")]
        public void InsertGiocatoreInFantasquadra_ERROR_GiocatoreNonLibero()
        {
            try
            {
                DBControllers.SusyLeague.StagioneDBController cStagione = new DBControllers.SusyLeague.StagioneDBController(DBIntegrationTests.ConnectionString);
                int idStagione = cStagione.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idStagione);

                UserDBController cUser = new UserDBController(DBIntegrationTests.ConnectionString);
                int idUser1 = cUser.InsertUser(DataMock.FakeData.GetUsername());
                Assert.AreNotEqual(0, idUser1);

                DBControllers.SusyLeague.SquadraDBController cSLSquadra = new DBControllers.SusyLeague.SquadraDBController(DBIntegrationTests.ConnectionString);
                int idFantasquadra = cSLSquadra.InsertSquadra(DataMock.FakeData.GetString(8, false, true, false, false), idStagione, idUser1);
                Assert.AreNotEqual(0, idFantasquadra);


                GiocatoreDBController cGiocatore = new GiocatoreDBController(DBIntegrationTests.ConnectionString);
                int idGiocatore = cGiocatore.InsertGiocatore(DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(4, true, false, true, false));

                Assert.AreNotEqual(0, idGiocatore);

                DBControllers.SusyLeague.StagioneDBController cSLStagione = new DBControllers.SusyLeague.StagioneDBController(DBIntegrationTests.ConnectionString);
                int idSLStagione = cSLStagione.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idSLStagione);

                DBControllers.SusyLeague.MercatoDBController cMercato = new DBControllers.SusyLeague.MercatoDBController(DBIntegrationTests.ConnectionString);
                int idFase = cMercato.InsertMercatoFase(DataMock.FakeData.GetString(10, false, true, false, false),
                    DataMock.FakeData.GetDate(DateTime.Now.AddDays(iCounter++)), DataMock.FakeData.GetDate(DateTime.Now.AddDays(iCounter++)));
                Assert.AreNotEqual(0, idFase);

                int idTransazione = cMercato.InsertTransazione(idFase);
                Assert.AreNotEqual(0, idTransazione);

                int idGiocatoreInfantasquadra = cSLSquadra.InsertGiocatoreInFantasquadra(idGiocatore, idFantasquadra, idSLStagione, idTransazione, DateTime.Now);
                Assert.AreNotEqual(0, idGiocatoreInfantasquadra);



                int idUser2 = cUser.InsertUser(DataMock.FakeData.GetUsername());
                Assert.AreNotEqual(0, idUser2);
                int idFantasquadra2 = cSLSquadra.InsertSquadra(DataMock.FakeData.GetString(8, false, true, false, false), idStagione, idUser2);
                Assert.AreNotEqual(0, idFantasquadra2);

                //queste istruzione genera l'errore
                int idGiocatoreInfantasquadra2 = cSLSquadra.InsertGiocatoreInFantasquadra(idGiocatore, idFantasquadra2, idSLStagione, idTransazione, DateTime.Now);
                //qui non dovrebbe mai arrivare
                Assert.AreNotEqual(0, idGiocatoreInfantasquadra2);
            }
            catch (SusyLeagueDBException ex)
            {
                if (ManageError)
                    Assert.AreEqual(53014, ex.Code);
                else if (!TestError) { }
                else
                    throw;
            }
        }

        [TestMethod]
        public void DeleteGiocatoreDaFantasquadra()
        {
            try
            {
                DBControllers.SusyLeague.StagioneDBController cStagione = new DBControllers.SusyLeague.StagioneDBController(DBIntegrationTests.ConnectionString);
                int idStagione = cStagione.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idStagione);

                UserDBController cUser = new UserDBController(DBIntegrationTests.ConnectionString);
                int idUser = cUser.InsertUser(DataMock.FakeData.GetUsername());
                Assert.AreNotEqual(0, idUser);

                DBControllers.SusyLeague.SquadraDBController cSLSquadra = new DBControllers.SusyLeague.SquadraDBController(DBIntegrationTests.ConnectionString);
                int idFantasquadra = cSLSquadra.InsertSquadra(DataMock.FakeData.GetString(8, false, true, false, false), idStagione, idUser);
                Assert.AreNotEqual(0, idFantasquadra);


                GiocatoreDBController cGiocatore = new GiocatoreDBController(DBIntegrationTests.ConnectionString);
                int idGiocatore = cGiocatore.InsertGiocatore(DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(4, true, false, true, false));

                Assert.AreNotEqual(0, idGiocatore);

                DBControllers.SusyLeague.StagioneDBController cSLStagione = new DBControllers.SusyLeague.StagioneDBController(DBIntegrationTests.ConnectionString);
                int idSLStagione = cSLStagione.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));

                Assert.AreNotEqual(0, idSLStagione);

                DBControllers.SusyLeague.MercatoDBController cMercato = new DBControllers.SusyLeague.MercatoDBController(DBIntegrationTests.ConnectionString);
                int idFase = cMercato.InsertMercatoFase(DataMock.FakeData.GetString(10, false, true, false, false),
                    DataMock.FakeData.GetDate(DateTime.Now.AddDays(iCounter++)), DataMock.FakeData.GetDate(DateTime.Now.AddDays(iCounter++)));
                Assert.AreNotEqual(0, idFase);

                int idTransazione = cMercato.InsertTransazione(idFase);
                Assert.AreNotEqual(0, idTransazione);

                int idGiocatoreInfantasquadra = cSLSquadra.InsertGiocatoreInFantasquadra(idGiocatore, idFantasquadra, idSLStagione, idTransazione, DateTime.Now); ;
                Assert.AreNotEqual(0, idGiocatoreInfantasquadra);

                int idTransazione2 = cMercato.InsertTransazione(idFase);
                Assert.AreNotEqual(0, idTransazione);

                cSLSquadra.DeleteGiocatoreDaFantasquadra(idGiocatore, idFantasquadra, idSLStagione, idTransazione2, DateTime.Now.AddMinutes(10));
            }
            catch (Exception)
            {

                throw;
            }
        }

        [TestMethod]
        [Description("controlla che non sia possibile chiudere una associazine non attiva tra un giocatore ed una fantasquadra. In questo caso deve essere gestito l'errore 53015 del DB")]
        public void DeleteGiocatoreDaFantasquadra_ERROR_NOTFOUND()
        {
            try
            {
                DBControllers.SusyLeague.StagioneDBController cStagione = new DBControllers.SusyLeague.StagioneDBController(DBIntegrationTests.ConnectionString);
                int idStagione = cStagione.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idStagione);

                UserDBController cUser = new UserDBController(DBIntegrationTests.ConnectionString);
                int idUser = cUser.InsertUser(DataMock.FakeData.GetUsername());
                Assert.AreNotEqual(0, idUser);

                DBControllers.SusyLeague.SquadraDBController cSLSquadra = new DBControllers.SusyLeague.SquadraDBController(DBIntegrationTests.ConnectionString);
                int idFantasquadra = cSLSquadra.InsertSquadra(DataMock.FakeData.GetString(8, false, true, false, false), idStagione, idUser);
                Assert.AreNotEqual(0, idFantasquadra);


                GiocatoreDBController cGiocatore = new GiocatoreDBController(DBIntegrationTests.ConnectionString);
                int idGiocatore = cGiocatore.InsertGiocatore(DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(4, true, false, true, false));

                Assert.AreNotEqual(0, idGiocatore);

                DBControllers.SusyLeague.StagioneDBController cSLStagione = new DBControllers.SusyLeague.StagioneDBController(DBIntegrationTests.ConnectionString);
                int idSLStagione = cSLStagione.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));

                DBControllers.SusyLeague.MercatoDBController cMercato = new DBControllers.SusyLeague.MercatoDBController(DBIntegrationTests.ConnectionString);
                int idFase = cMercato.InsertMercatoFase(DataMock.FakeData.GetString(10, false, true, false, false),
                    DataMock.FakeData.GetDate(DateTime.Now.AddDays(iCounter++)), DataMock.FakeData.GetDate(DateTime.Now.AddDays(iCounter++)));
                Assert.AreNotEqual(0, idFase);

                int idTransazione = cMercato.InsertTransazione(idFase);
                Assert.AreNotEqual(0, idTransazione);

                Assert.AreNotEqual(0, idSLStagione);
                //queste istruzione genera l'errore
                cSLSquadra.DeleteGiocatoreDaFantasquadra(idGiocatore, idFantasquadra, idSLStagione, idTransazione, DateTime.Now.AddMinutes(10));
                //qui non dovrebbe mai arrivare
            }
            catch (SusyLeagueDBException ex)
            {
                if (ManageError)
                    Assert.AreEqual(53015, ex.Code);
                else if (!TestError) { }
                else
                    throw;
            }
        }

        [TestMethod]
        public void InsertMercatoTransazioneAcquistoGiocatoreLibero()
        {
            try
            {
                //creo la stagione
                DBControllers.SusyLeague.StagioneDBController cStagione = new DBControllers.SusyLeague.StagioneDBController(DBIntegrationTests.ConnectionString);
                int idStagione = cStagione.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idStagione);


                //creo lo user e la sua fantasquadra
                UserDBController cUser = new UserDBController(DBIntegrationTests.ConnectionString);
                int idUser = cUser.InsertUser(DataMock.FakeData.GetUsername());
                Assert.AreNotEqual(0, idUser);

                DBControllers.SusyLeague.SquadraDBController cSLSquadra = new DBControllers.SusyLeague.SquadraDBController(DBIntegrationTests.ConnectionString);
                int idFantasquadra = cSLSquadra.InsertSquadra(DataMock.FakeData.GetString(8, false, true, false, false), idStagione, idUser);
                Assert.AreNotEqual(0, idFantasquadra);

                //creo il giocatore
                GiocatoreDBController cGiocatore = new GiocatoreDBController(DBIntegrationTests.ConnectionString);
                int idGiocatore = cGiocatore.InsertGiocatore(DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(4, true, false, true, false));
                Assert.AreNotEqual(0, idGiocatore);

                //creo una stagione di fantacalcio
                DBControllers.SusyLeague.StagioneDBController cSLStagione = new DBControllers.SusyLeague.StagioneDBController(DBIntegrationTests.ConnectionString);
                int idSLStagione = cSLStagione.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idSLStagione);

                //inserisco una fase di mercato nella stagione di fantacalcio
                DBControllers.SusyLeague.MercatoDBController cMercato = new DBControllers.SusyLeague.MercatoDBController(DBIntegrationTests.ConnectionString);
                int idFase = cMercato.InsertMercatoFase(DataMock.FakeData.GetString(10, false, true, false, false),
                    DataMock.FakeData.GetDate(DateTime.Now.AddDays(iCounter++)), DataMock.FakeData.GetDate(DateTime.Now.AddDays(iCounter++)));
                Assert.AreNotEqual(0, idFase);

                //creo una transazione nella fase di mercato della stagione di fantacalcio
                int idTransazione = cMercato.InsertTransazione(idFase);
                Assert.AreNotEqual(0, idTransazione);

                //inserisco il movimento di acquisto per la transazione delle fase di mercato
                int idFantaacquisto = cMercato.InsertAcquisto(idGiocatore, 0, idFantasquadra, DataMock.FakeData.GetInteger(0, 100), idTransazione);

                //inserisco il giocatore dal mercato libero alla fantasquadra
                int idGiocatoreInfantasquadra = cSLSquadra.InsertGiocatoreInFantasquadra(idGiocatore, idFantasquadra, idSLStagione, idTransazione, DateTime.Now);
                Assert.AreNotEqual(0, idGiocatoreInfantasquadra);
            }
            catch (Exception)
            {

                throw;
            }
        }

        [TestMethod]
        public void InsertMercatoTransazioneAcquistoGiocatoreDaAltraFantasquadra()
        {
            try
            {
                //creo la stagione
                DBControllers.SusyLeague.StagioneDBController cStagione = new DBControllers.SusyLeague.StagioneDBController(DBIntegrationTests.ConnectionString);
                int idStagione = cStagione.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idStagione);


                //creo lo user e la sua fantasquadra
                UserDBController cUser = new UserDBController(DBIntegrationTests.ConnectionString);
                int idUser = cUser.InsertUser(DataMock.FakeData.GetUsername());
                Assert.AreNotEqual(0, idUser);

                DBControllers.SusyLeague.SquadraDBController cSLSquadra = new DBControllers.SusyLeague.SquadraDBController(DBIntegrationTests.ConnectionString);
                int idFantasquadra = cSLSquadra.InsertSquadra(DataMock.FakeData.GetString(8, false, true, false, false), idStagione, idUser);
                Assert.AreNotEqual(0, idFantasquadra);

                //creo il giocatore
                GiocatoreDBController cGiocatore = new GiocatoreDBController(DBIntegrationTests.ConnectionString);
                int idGiocatore = cGiocatore.InsertGiocatore(DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(4, true, false, true, false));
                Assert.AreNotEqual(0, idGiocatore);

                //creo una stagione di fantacalcio
                DBControllers.SusyLeague.StagioneDBController cSLStagione = new DBControllers.SusyLeague.StagioneDBController(DBIntegrationTests.ConnectionString);
                int idSLStagione = cSLStagione.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idSLStagione);

                //inserisco una fase di mercato nella stagione di fantacalcio
                DBControllers.SusyLeague.MercatoDBController cMercato = new DBControllers.SusyLeague.MercatoDBController(DBIntegrationTests.ConnectionString);
                int idFase = cMercato.InsertMercatoFase(DataMock.FakeData.GetString(10, false, true, false, false),
                    DataMock.FakeData.GetDate(DateTime.Now.AddDays(iCounter++)), DataMock.FakeData.GetDate(DateTime.Now.AddDays(iCounter++)));
                Assert.AreNotEqual(0, idFase);

                //creo una transazione nella fase di mercato della stagione di fantacalcio
                int idTransazione = cMercato.InsertTransazione(idFase);
                Assert.AreNotEqual(0, idTransazione);

                //inserisco il movimento di acquisto per la transazione delle fase di mercato
                int idFantaacquisto = cMercato.InsertAcquisto(idGiocatore, 0, idFantasquadra, DataMock.FakeData.GetInteger(0, 100), idTransazione);
                Assert.AreNotEqual(0, idFantaacquisto);

                //trasferisco il giocatore dal mercato libero alla fantasquadra
                int idGiocatoreInfantasquadra = cSLSquadra.InsertGiocatoreInFantasquadra(idGiocatore, idFantasquadra, idSLStagione, idTransazione, DateTime.Now);
                Assert.AreNotEqual(0, idGiocatoreInfantasquadra);

                //creo un secondo user e una seconda fantasquadra
                int idUser2 = cUser.InsertUser(DataMock.FakeData.GetUsername());
                Assert.AreNotEqual(0, idUser2);
                int idFantasquadra2 = cSLSquadra.InsertSquadra(DataMock.FakeData.GetString(8, false, true, false, false), idStagione, idUser2);
                Assert.AreNotEqual(0, idFantasquadra2);

                //nella stessa fase di mercato trasferisco il giocatore dalla squadra 1 alla squadra 2
                //creo una transazione nella fase di mercato della stagione di fantacalcio
                int idTransazione2 = cMercato.InsertTransazione(idFase);
                Assert.AreNotEqual(0, idTransazione2);

                //trasferisco il giocatore dalla fantasquadra1 alla fantasquadra2

                //inserisco il movimento di acquisto per la transazione delle fase di mercato
                //in questa fase il gicatore deve ancora appartenere alla squadra iniziale
                int idFantaacquisto2 = cMercato.InsertAcquisto(idGiocatore, idFantasquadra, idFantasquadra2, DataMock.FakeData.GetInteger(0, 100), idTransazione2);
                Assert.AreNotEqual(0, idFantaacquisto2);

                //dopo aver inserito il movimento di acquisto:
                //1.chiudo la precedente associazione tra giocatore e fantasquadra1
                cSLSquadra.DeleteGiocatoreDaFantasquadra(idGiocatore, idFantasquadra, idSLStagione, idTransazione2, DateTime.Now);

                //2.inserisco il giocatore libero nella fantasquadra2
                int idGiocatoreInfantasquadra2 = cSLSquadra.InsertGiocatoreInFantasquadra(idGiocatore, idFantasquadra2, idSLStagione, idTransazione2, DateTime.Now);
                Assert.AreNotEqual(0, idGiocatoreInfantasquadra2);

            }
            catch (Exception)
            {

                throw;
            }
        }

        [TestMethod]
        public void InsertMercatoTransazioneAcquistoSoloCrediti()
        {
            try
            {
                //creo la stagione
                DBControllers.SusyLeague.StagioneDBController cStagione = new DBControllers.SusyLeague.StagioneDBController(DBIntegrationTests.ConnectionString);
                int idStagione = cStagione.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idStagione);


                //creo lo user e la sua fantasquadra
                UserDBController cUser = new UserDBController(DBIntegrationTests.ConnectionString);
                int idUser = cUser.InsertUser(DataMock.FakeData.GetUsername());
                Assert.AreNotEqual(0, idUser);

                DBControllers.SusyLeague.SquadraDBController cSLSquadra = new DBControllers.SusyLeague.SquadraDBController(DBIntegrationTests.ConnectionString);
                int idFantasquadra = cSLSquadra.InsertSquadra(DataMock.FakeData.GetString(8, false, true, false, false), idStagione, idUser);
                Assert.AreNotEqual(0, idFantasquadra);


                //creo una stagione di fantacalcio
                DBControllers.SusyLeague.StagioneDBController cSLStagione = new DBControllers.SusyLeague.StagioneDBController(DBIntegrationTests.ConnectionString);
                int idSLStagione = cSLStagione.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idSLStagione);

                //inserisco una fase di mercato nella stagione di fantacalcio
                DBControllers.SusyLeague.MercatoDBController cMercato = new DBControllers.SusyLeague.MercatoDBController(DBIntegrationTests.ConnectionString);
                int idFase = cMercato.InsertMercatoFase(DataMock.FakeData.GetString(10, false, true, false, false),
                    DataMock.FakeData.GetDate(DateTime.Now.AddDays(iCounter++)), DataMock.FakeData.GetDate(DateTime.Now.AddDays(iCounter++)));
                Assert.AreNotEqual(0, idFase);

                //creo una transazione nella fase di mercato della stagione di fantacalcio
                int idTransazione = cMercato.InsertTransazione(idFase);
                Assert.AreNotEqual(0, idTransazione);

                //creo un secondo user e una seconda fantasquadra
                int idUser2 = cUser.InsertUser(DataMock.FakeData.GetUsername());
                Assert.AreNotEqual(0, idUser2);
                int idFantasquadra2 = cSLSquadra.InsertSquadra(DataMock.FakeData.GetString(8, false, true, false, false), idStagione, idUser2);
                Assert.AreNotEqual(0, idFantasquadra2);

                //inserisco il movimento di Trasferimento fondi tra duq squadre
                int idFantaacquisto = cMercato.InsertAcquisto(0, idFantasquadra, idFantasquadra2, DataMock.FakeData.GetInteger(0, 100), idTransazione);
                Assert.AreNotEqual(0, idFantaacquisto);

            }
            catch (Exception)
            {

                throw;
            }
        }

        [TestMethod]
        [Description("controlla che non sia possibile inserire transazione per l\'acquisto di un giocatore liber, quando il giocatore non è libero. In questo caso deve essere gestito l'errore 53011 del DB")]
        public void InsertMercatoTransazioneAcquistoGiocatoreLibero_ERROR_GiocatoreNonLibero()
        {
            try
            {
                //creo la stagione
                DBControllers.SusyLeague.StagioneDBController cStagione = new DBControllers.SusyLeague.StagioneDBController(DBIntegrationTests.ConnectionString);
                int idStagione = cStagione.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idStagione);


                //creo lo user e la sua fantasquadra
                UserDBController cUser = new UserDBController(DBIntegrationTests.ConnectionString);
                int idUser = cUser.InsertUser(DataMock.FakeData.GetUsername());
                Assert.AreNotEqual(0, idUser);

                DBControllers.SusyLeague.SquadraDBController cSLSquadra = new DBControllers.SusyLeague.SquadraDBController(DBIntegrationTests.ConnectionString);
                int idFantasquadra = cSLSquadra.InsertSquadra(DataMock.FakeData.GetString(8, false, true, false, false), idStagione, idUser);
                Assert.AreNotEqual(0, idFantasquadra);

                //creo il giocatore
                GiocatoreDBController cGiocatore = new GiocatoreDBController(DBIntegrationTests.ConnectionString);
                int idGiocatore = cGiocatore.InsertGiocatore(DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(4, true, false, true, false));
                Assert.AreNotEqual(0, idGiocatore);

                //creo una stagione di fantacalcio
                DBControllers.SusyLeague.StagioneDBController cSLStagione = new DBControllers.SusyLeague.StagioneDBController(DBIntegrationTests.ConnectionString);
                int idSLStagione = cSLStagione.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idSLStagione);

                //inserisco una fase di mercato nella stagione di fantacalcio
                DBControllers.SusyLeague.MercatoDBController cMercato = new DBControllers.SusyLeague.MercatoDBController(DBIntegrationTests.ConnectionString);
                int idFase = cMercato.InsertMercatoFase(DataMock.FakeData.GetString(10, false, true, false, false),
                    DataMock.FakeData.GetDate(DateTime.Now.AddDays(iCounter++)), DataMock.FakeData.GetDate(DateTime.Now.AddDays(iCounter++)));
                Assert.AreNotEqual(0, idFase);

                //creo una transazione nella fase di mercato della stagione di fantacalcio
                int idTransazione = cMercato.InsertTransazione(idFase);
                Assert.AreNotEqual(0, idTransazione);

                //inserisco il movimento di acquisto per la transazione delle fase di mercato
                int idFantaacquisto = cMercato.InsertAcquisto(idGiocatore, 0, idFantasquadra, DataMock.FakeData.GetInteger(0, 100), idTransazione);
                Assert.AreNotEqual(0, idFantaacquisto);

                //trasferisco il giocatore dal mercato libero alla fantasquadra
                int idGiocatoreInfantasquadra = cSLSquadra.InsertGiocatoreInFantasquadra(idGiocatore, idFantasquadra, idSLStagione, idTransazione, DateTime.Now);
                Assert.AreNotEqual(0, idGiocatoreInfantasquadra);

                //creo una transazione nella fase di mercato della stagione di fantacalcio
                int idTransazione2 = cMercato.InsertTransazione(idFase);
                Assert.AreNotEqual(0, idTransazione2);

                //creo lo user e la sua fantasquadra
                int idUser2 = cUser.InsertUser(DataMock.FakeData.GetUsername());
                Assert.AreNotEqual(0, idUser2);
                int idFantasquadra2 = cSLSquadra.InsertSquadra(DataMock.FakeData.GetString(8, false, true, false, false), idStagione, idUser2);
                Assert.AreNotEqual(0, idFantasquadra2);

                //queste istruzione genera l'errore
                int idGiocatoreInfantasquadra2 = cSLSquadra.InsertGiocatoreInFantasquadra(idGiocatore, idFantasquadra2, idSLStagione, idTransazione2, DateTime.Now);
                //qui non dovrebbe mai arrivare
                Assert.AreNotEqual(0, idGiocatoreInfantasquadra2);

            }
            catch (SusyLeagueDBException ex)
            {
                if (ManageError)
                    Assert.AreEqual(53011, ex.Code);
                else if (!TestError) { }
                else
                    throw;
            }
        }

        [TestMethod]
        [Description("controlla che non sia possibile trasferire un giocatore tra due squadre se questo nn è tra i giocatori della squadra di origine. In questo caso deve essere gestito l'errore 53012 del DB")]
        public void InsertMercatoTransazioneAcquistoGiocatoreDaAltraFantasquadra_ERROR_GiocatoreNOTFOUND()
        {
            try
            {
                //creo la stagione
                DBControllers.SusyLeague.StagioneDBController cStagione = new DBControllers.SusyLeague.StagioneDBController(DBIntegrationTests.ConnectionString);
                int idStagione = cStagione.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idStagione);


                //creo lo user e la sua fantasquadra
                UserDBController cUser = new UserDBController(DBIntegrationTests.ConnectionString);
                int idUser = cUser.InsertUser(DataMock.FakeData.GetUsername());
                Assert.AreNotEqual(0, idUser);

                DBControllers.SusyLeague.SquadraDBController cSLSquadra = new DBControllers.SusyLeague.SquadraDBController(DBIntegrationTests.ConnectionString);
                int idFantasquadra = cSLSquadra.InsertSquadra(DataMock.FakeData.GetString(8, false, true, false, false), idStagione, idUser);
                Assert.AreNotEqual(0, idFantasquadra);

                //creo il giocatore
                GiocatoreDBController cGiocatore = new GiocatoreDBController(DBIntegrationTests.ConnectionString);
                int idGiocatore = cGiocatore.InsertGiocatore(DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(4, true, false, true, false));
                Assert.AreNotEqual(0, idGiocatore);

                //creo una stagione di fantacalcio
                DBControllers.SusyLeague.StagioneDBController cSLStagione = new DBControllers.SusyLeague.StagioneDBController(DBIntegrationTests.ConnectionString);
                int idSLStagione = cSLStagione.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idSLStagione);

                //inserisco una fase di mercato nella stagione di fantacalcio
                DBControllers.SusyLeague.MercatoDBController cMercato = new DBControllers.SusyLeague.MercatoDBController(DBIntegrationTests.ConnectionString);
                int idFase = cMercato.InsertMercatoFase(DataMock.FakeData.GetString(10, false, true, false, false),
                    DataMock.FakeData.GetDate(DateTime.Now.AddDays(iCounter++)), DataMock.FakeData.GetDate(DateTime.Now.AddDays(iCounter++)));
                Assert.AreNotEqual(0, idFase);

                //creo una transazione nella fase di mercato della stagione di fantacalcio
                int idTransazione = cMercato.InsertTransazione(idFase);
                Assert.AreNotEqual(0, idTransazione);

                ////inserisco il movimento di acquisto per la transazione delle fase di mercato
                //int idFantaacquisto = cMercato.InsertAcquisto(idGiocatore, 0, idFantasquadra, DataMock.FakeData.GetInteger(0, 100), idTransazione);
                //Assert.AreNotEqual(0, idFantaacquisto);

                ////trasferisco il giocatore dal mercato libero alla fantasquadra
                //int idGiocatoreInfantasquadra = cSLSquadra.InsertGiocatoreInFantasquadra(idGiocatore, idFantasquadra, idSLStagione, idTransazione, DateTime.Now);
                //Assert.AreNotEqual(0, idGiocatoreInfantasquadra);

                //creo un secondo user e una seconda fantasquadra
                int idUser2 = cUser.InsertUser(DataMock.FakeData.GetUsername());
                Assert.AreNotEqual(0, idUser2);
                int idFantasquadra2 = cSLSquadra.InsertSquadra(DataMock.FakeData.GetString(8, false, true, false, false), idStagione, idUser2);
                Assert.AreNotEqual(0, idFantasquadra2);

                //nella stessa fase di mercato trasferisco il giocatore dalla squadra 1 alla squadra 2
                //creo una transazione nella fase di mercato della stagione di fantacalcio
                int idTransazione2 = cMercato.InsertTransazione(idFase);
                Assert.AreNotEqual(0, idTransazione2);

                ////trasferisco il giocatore dalla fantasquadra1 alla fantasquadra2
                ////chiudo la precedente associazione tra giocatore e fantasquadra1
                //cSLSquadra.DeleteGiocatoreDaFantasquadra(idGiocatore, idFantasquadra, idSLStagione, idTransazione2, DateTime.Now);

                //inserisco il movimento di acquisto per la transazione delle fase di mercato
                //queste istruzione genera l'errore
                int idFantaacquisto2 = cMercato.InsertAcquisto(idGiocatore, idFantasquadra, idFantasquadra2, DataMock.FakeData.GetInteger(0, 100), idTransazione2);
                //qui non dovrebbe mai arrivare
                Assert.AreNotEqual(0, idFantaacquisto2);

                //Trasferisco il giocatore libero nella fantasquadra2
                int idGiocatoreInfantasquadra2 = cSLSquadra.InsertGiocatoreInFantasquadra(idGiocatore, idFantasquadra2, idSLStagione, idTransazione2, DateTime.Now);
                Assert.AreNotEqual(0, idGiocatoreInfantasquadra2);
            }
            catch (SusyLeagueDBException ex)
            {
                if (ManageError)
                    Assert.AreEqual(53012, ex.Code);
                else if (!TestError) { }
                else
                    throw;
            }
        }


        #endregion
    }
}
