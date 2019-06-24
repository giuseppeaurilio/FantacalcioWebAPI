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
using DBControllers.Membership;
using DBControllers.Web;

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

        public static bool ManageError = true;

        [ClassInitialize]
        public static void SetUp(TestContext testContext)
        {

            //DBIntegrationTests.ConnectionString = string.Format("Integrated Security=SSPI;Persist Security Info=False;Initial Catalog={0};Data Source=DESKTOP-393K7QE\\SQLEXPRESS", "SusyLeague");

            /*INIT DB*/
            DBIntegrationTests.TempDBName = string.Format("{0}_{1}{2}{3}_{4}{5}",
                "SusyLagueUT",
                DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,
                DateTime.Now.Hour, DateTime.Now.Minute);
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
            int id1 = c.InsertSquadra("SERIE MINORE");
            int id2 = c.InsertSquadra("SVINCOLATO");
            int id3 = c.InsertSquadra("SERIE ESTERA");
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
        #region DBControllers.SERIEA
        [TestMethod]
        public void InsertStagione()
        {
            StagioneDBController c = new StagioneDBController(DBIntegrationTests.ConnectionString);
            int id = c.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));

            Assert.AreNotEqual(0, id);
        }

        [TestMethod]
        [Description("controlla che in caso di inserimento doppio della stagione, venga restituito l'errore 50007 dal DB")]
        public void InsertStagione_Error_Duplicata()
        {
            try
            {
                StagioneDBController c = new StagioneDBController(DBIntegrationTests.ConnectionString);
                int id = 0;
                string nomeStagione = DataMock.FakeData.GetString(8, false, true, false, false);
                id = c.InsertStagione(nomeStagione);
                id = c.InsertStagione(nomeStagione);

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
                StagioneDBController c = new StagioneDBController(DBIntegrationTests.ConnectionString);
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
                StagioneDBController c = new StagioneDBController(DBIntegrationTests.ConnectionString);
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
                StagioneDBController c = new StagioneDBController(DBIntegrationTests.ConnectionString);
                int id = c.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, id);


                DateTime giornataStartDate = DataMock.FakeData.GetDate(new DateTime(2019, 8, 21, 15, 00, 00), 0, 0);
                DateTime giornataEndDate = DataMock.FakeData.GetDate(giornataStartDate, 0, 72);
                string descrizioneGiornata = DataMock.FakeData.GetString(8, false, true, false, false);
                int idGiornata = c.InsertGiornataInStagione(descrizioneGiornata,
                    giornataStartDate,
                    giornataEndDate,
                    id);

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
                StagioneDBController c = new StagioneDBController(DBIntegrationTests.ConnectionString);
                int id = 0;
                DateTime giornataStartDate = DataMock.FakeData.GetDate(new DateTime(2019, 8, 21, 15, 00, 00), 0, 0);
                DateTime giornataEndDate = DataMock.FakeData.GetDate(giornataStartDate, 0, 72);
                c.UpdateGiornata(id, DataMock.FakeData.GetString(8, false, true, false, false), giornataStartDate,
                    giornataEndDate);

            }
            catch (SusyLeagueDBException ex)
            {
                if (ManageError)
                    Assert.AreEqual(50005, ex.Code);
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
                id = c.InsertSquadra(nome);

                Assert.AreNotEqual(0, id);
            }
            catch (SusyLeagueDBException ex)
            {
                if (ManageError)
                    Assert.AreEqual(50006, ex.Code);
                else
                    throw;
            }
        }

        [TestMethod]
        public void InsertSquadraInStagione()
        {
            try
            {

                StagioneDBController c = new StagioneDBController(DBIntegrationTests.ConnectionString);
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

                StagioneDBController c = new StagioneDBController(DBIntegrationTests.ConnectionString);
                int id = c.InsertStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, id);


                SquadraDBController cs = new SquadraDBController(DBIntegrationTests.ConnectionString);
                int ids = cs.InsertSquadra(DataMock.FakeData.GetString(8, false, true, false, false));

                Assert.AreNotEqual(0, ids);

                int ret = c.InsertSquadraInStagione(id, ids);
                Assert.AreNotEqual(0, ret);
                ret = c.InsertSquadraInStagione(id, ids);
                Assert.AreEqual(0, ret);

            }
            catch (SusyLeagueDBException ex)
            {
                if (ManageError)
                    Assert.AreEqual(50016, ex.Code);
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
                id = c.InsertGiocatore(nome, cognome, gazzaId);

                Assert.AreEqual(0, id);
            }
            catch (SusyLeagueDBException ex)
            {
                if (ManageError)
                    Assert.AreEqual(50001, ex.Code);
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
                ret = csq.InsertGiocatoreInSquadra(idGiocatore, idSquadra2,
                     DataMock.FakeData.GetDate(DateTime.Now, 0, 0));

                //non dovrebbbe mai arrivare qui
                Assert.AreEqual(0, ret);

            }
            catch (SusyLeagueDBException ex)
            {
                if (ManageError)
                    Assert.AreEqual(50014, ex.Code);
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

                csq.DeleteGiocatoreDaSquadra(idGiocatore, 0,
                     DataMock.FakeData.GetDate(DateTime.Now, 0, 0));

            }
            catch (SusyLeagueDBException ex)
            {
                if (ManageError)
                    Assert.AreEqual(50002, ex.Code);
                else
                    throw;
            }
        }

        [TestMethod]
        public void InsertVotoDelGiocatore()
        {
            try
            {
                StagioneDBController cStagione = new StagioneDBController(DBIntegrationTests.ConnectionString);
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
                StagioneDBController cStagione = new StagioneDBController(DBIntegrationTests.ConnectionString);
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
                StagioneDBController cStagione = new StagioneDBController(DBIntegrationTests.ConnectionString);
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
                StagioneDBController cStagione = new StagioneDBController(DBIntegrationTests.ConnectionString);
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
                StagioneDBController cStagione = new StagioneDBController(DBIntegrationTests.ConnectionString);
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
                else
                    throw;
            }
        }

        [TestMethod]
        public void InsertStatisticaDelGiocatore()
        {
            try
            {
                StagioneDBController cStagione = new StagioneDBController(DBIntegrationTests.ConnectionString);
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
                StagioneDBController cStagione = new StagioneDBController(DBIntegrationTests.ConnectionString);
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
                StagioneDBController cStagione = new StagioneDBController(DBIntegrationTests.ConnectionString);
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
                StagioneDBController cStagione = new StagioneDBController(DBIntegrationTests.ConnectionString);
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
                StagioneDBController cStagione = new StagioneDBController(DBIntegrationTests.ConnectionString);
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
                int id = c.InsertUser(DataMock.FakeData.GetUsername(), DataMock.FakeData.GetPassword());

                Assert.AreNotEqual(0, id);
            }
            catch (Exception)
            {

                throw;
            }
        }
        [TestMethod]
        public void SetTemporaryUserPassword()
        {
            try
            {
                UserDBController c = new UserDBController(DBIntegrationTests.ConnectionString);
                int id = c.InsertUser(DataMock.FakeData.GetUsername(), DataMock.FakeData.GetPassword());

                Assert.AreNotEqual(0, id);

                c.SetPasswordTemporaneaUser(id, DataMock.FakeData.GetPassword());
            }
            catch (Exception)
            {

                throw;
            }
        }
        [TestMethod]
        public void UpdateUserPassword()
        {
            try
            {
                UserDBController c = new UserDBController(DBIntegrationTests.ConnectionString);
                int id = c.InsertUser(DataMock.FakeData.GetUsername(), DataMock.FakeData.GetPassword());

                Assert.AreNotEqual(0, id);

                c.SetPasswordTemporaneaUser(id, DataMock.FakeData.GetPassword());

                c.UpdatePasswordUser(id, DataMock.FakeData.GetPassword());
            }
            catch (Exception)
            {

                throw;
            }
        }
        [TestMethod]
        [Description("controlla che in caso di inserimento doppio dellu username, venga restituito l'errore 51002 dal DB")]
        public void InsertUser_Error_Duplicato()
        {
            try
            {
                UserDBController c = new UserDBController(DBIntegrationTests.ConnectionString);
                string username = DataMock.FakeData.GetUsername();
                int id = c.InsertUser(username, DataMock.FakeData.GetPassword());

                Assert.AreNotEqual(0, id);
                //questa istruzione deve generare l'errore
                id = c.InsertUser(username, DataMock.FakeData.GetPassword());
                //qui non dovrebe mai arrivare
                Assert.AreEqual(0, id);
            }
            catch (SusyLeagueDBException ex)
            {
                if (ManageError)
                    Assert.AreEqual(51002, ex.Code);
                else
                    throw;
            }
        }

        [TestMethod]
        [Description("controlla che in caso di set passwrord temporanea per utente non esistente, venga restituito l'errore 51001 dal DB")]
        public void SetTemporaryUserPassword_Error_NOTFOUND()
        {
            try
            {
                UserDBController c = new UserDBController(DBIntegrationTests.ConnectionString);
                //questa istruzione deve generare l'errore
                c.SetPasswordTemporaneaUser(0, DataMock.FakeData.GetPassword());
                //qui non dovrebe mai arrivare

            }
            catch (SusyLeagueDBException ex)
            {
                if (ManageError)
                    Assert.AreEqual(51001, ex.Code);
                else
                    throw;
            }
        }

        [TestMethod]
        [Description("controlla che in caso di set passwrord temporanea per utente non esistente, venga restituito l'errore 51001 dal DB")]
        public void UpdateUserPassword_Error_NOTFOUND()
        {
            try
            {
                UserDBController c = new UserDBController(DBIntegrationTests.ConnectionString);
                //questa istruzione deve generare l'errore
                c.UpdatePasswordUser(0, DataMock.FakeData.GetPassword());
                //qui non dovrebe mai arrivare

            }
            catch (SusyLeagueDBException ex)
            {
                if (ManageError)
                    Assert.AreEqual(51001, ex.Code);
                else
                    throw;
            }
        }

        #endregion

        #region DBControllers.Web
        [TestMethod]
        public void InsertRole()
        {
            try
            {
                RoleDBController c = new RoleDBController(DBIntegrationTests.ConnectionString);
                int id = c.InsertRole(DataMock.FakeData.GetString(8, false, true, false, false));

                Assert.AreNotEqual(0, id);
            }
            catch (Exception)
            {

                throw;
            }
        }
        [TestMethod]
        public void UpdateRole()
        {
            try
            {
                RoleDBController c = new RoleDBController(DBIntegrationTests.ConnectionString);
                int id = c.InsertRole(DataMock.FakeData.GetString(8, false, true, false, false));

                Assert.AreNotEqual(0, id);

                c.UpdateRole(id, DataMock.FakeData.GetString(8, false, true, false, false));
            }
            catch (Exception)
            {

                throw;
            }
        }

        [TestMethod]
        [Description("controlla che in caso di inserimento doppio ruolo, venga restituito l'errore 52004 dal DB")]
        public void InsertRole_ERROR_Duplicato()
        {
            try
            {
                RoleDBController c = new RoleDBController(DBIntegrationTests.ConnectionString);
                string description = DataMock.FakeData.GetString(8, false, true, false, false);
                int id = c.InsertRole(description);

                Assert.AreNotEqual(0, id);

                //questa istruzione deve generare l'errore
                id = c.InsertRole(description);
                //qui non dovrebe mai arrivare
                Assert.AreEqual(0, id);
            }
            catch (SusyLeagueDBException ex)
            {
                if (ManageError)
                    Assert.AreEqual(52004, ex.Code);
                else
                    throw;
            }
        }
        [TestMethod]
        [Description("controlla che in caso di ruolo non esistente, venga restituito l'errore 52005 dal DB")]
        public void UpdateRole_ERROR_NOTFOUND()
        {
            try
            {
                RoleDBController c = new RoleDBController(DBIntegrationTests.ConnectionString);
                //questa istruzione deve generare l'errore
                c.UpdateRole(0, DataMock.FakeData.GetString(8, false, true, false, false));
                //qui non dovrebe mai arrivare
            }
            catch (SusyLeagueDBException ex)
            {
                if (ManageError)
                    Assert.AreEqual(52005, ex.Code);
                else
                    throw;
            }
        }

        [TestMethod]
        public void InsertUserRole()
        {
            try
            {

                UserDBController cUser = new UserDBController(DBIntegrationTests.ConnectionString);
                int idUser = cUser.InsertUser(DataMock.FakeData.GetUsername(), DataMock.FakeData.GetPassword());

                Assert.AreNotEqual(0, idUser);

                RoleDBController cRole = new RoleDBController(DBIntegrationTests.ConnectionString);
                int idRole = cRole.InsertRole(DataMock.FakeData.GetString(8, false, true, false, false));

                Assert.AreNotEqual(0, idRole);

                int id = cRole.InsertUserRole(idUser, idRole);

                Assert.AreNotEqual(0, id);
            }
            catch (Exception)
            {

                throw;
            }
        }

        [TestMethod]
        public void DeleteUserRole()
        {
            try
            {
                UserDBController cUser = new UserDBController(DBIntegrationTests.ConnectionString);
                int idUser = cUser.InsertUser(DataMock.FakeData.GetUsername(), DataMock.FakeData.GetPassword());
                Assert.AreNotEqual(0, idUser);

                RoleDBController cRole = new RoleDBController(DBIntegrationTests.ConnectionString);
                int idRole = cRole.InsertRole(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idRole);

                int id = cRole.InsertUserRole(idUser, idRole);
                Assert.AreNotEqual(0, id);

                cRole.DeleteUserRole(id);
            }
            catch (Exception)
            {

                throw;
            }
        }

        [TestMethod]
        [Description("controlla che in caso di inserimento user_role duplicato , venga restituito l'errore 52007 dal DB")]
        public void InsertUserRole_Error_Duplicato()
        {
            try
            {

                UserDBController cUser = new UserDBController(DBIntegrationTests.ConnectionString);
                int idUser = cUser.InsertUser(DataMock.FakeData.GetUsername(), DataMock.FakeData.GetPassword());
                Assert.AreNotEqual(0, idUser);

                RoleDBController cRole = new RoleDBController(DBIntegrationTests.ConnectionString);
                int idRole = cRole.InsertRole(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idRole);

                int id = cRole.InsertUserRole(idUser, idRole);
                Assert.AreNotEqual(0, id);

                //questa istruzione deve generare l'errore
                id = cRole.InsertUserRole(idUser, idRole);
                //qui non dovrebe mai arrivare
                Assert.AreEqual(0, id);
            }
            catch (SusyLeagueDBException ex)
            {
                if (ManageError)
                    Assert.AreEqual(52007, ex.Code);
                else
                    throw;
            }
        }

        [TestMethod]
        [Description("controlla che in caso di inserimento user_role con USER NOT FOUND , venga restituito l'errore 547 dal DB")]
        public void InsertUserRole_Error_UserNOTFOUND()
        {
            try
            {

                //UserDBController cUser = new UserDBController(DBIntegrationTests.ConnectionString);
                //int idUser = cUser.InsertUser(DataMock.FakeData.GetUsername(), DataMock.FakeData.GetPassword());

                //Assert.AreNotEqual(0, idUser);

                RoleDBController cRole = new RoleDBController(DBIntegrationTests.ConnectionString);
                int idRole = cRole.InsertRole(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idRole);

                //questa istruzione deve generare l'errore
                int id = cRole.InsertUserRole(0, idRole);
                //qui non dovrebe mai arrivare
                Assert.AreEqual(0, id);
            }
            catch (SusyLeagueDBException ex)
            {
                if (ManageError)
                    Assert.AreEqual(547, ex.Code);
                else
                    throw;
            }
        }

        [TestMethod]
        [Description("controlla che in caso di inserimento user_role con USER NOT FOUND , venga restituito l'errore 547 dal DB")]
        public void InsertUserRole_Error_RoleNOTFOUND()
        {
            try
            {
                UserDBController cUser = new UserDBController(DBIntegrationTests.ConnectionString);
                int idUser = cUser.InsertUser(DataMock.FakeData.GetUsername(), DataMock.FakeData.GetPassword());
                Assert.AreNotEqual(0, idUser);

                RoleDBController cRole = new RoleDBController(DBIntegrationTests.ConnectionString);
                //questa istruzione deve generare l'errore
                int id = cRole.InsertUserRole(idUser, 0);
                //qui non dovrebe mai arrivare
                Assert.AreNotEqual(0, id);
            }
            catch (SusyLeagueDBException ex)
            {
                if (ManageError)
                    Assert.AreEqual(547, ex.Code);
                else
                    throw;
            }
        }

        [TestMethod]
        public void InsertPage()
        {
            try
            {
                PageDBController c = new PageDBController(DBIntegrationTests.ConnectionString);
                int id = c.InsertPage(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, id);
            }
            catch (Exception)
            {

                throw;
            }
        }
        [TestMethod]
        public void UpdatePage()
        {
            try
            {
                PageDBController c = new PageDBController(DBIntegrationTests.ConnectionString);
                int id = c.InsertPage(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, id);

                c.UpdatePage(id, DataMock.FakeData.GetString(8, false, true, false, false));
            }
            catch (Exception)
            {

                throw;
            }
        }

        [TestMethod]
        public void InsertPageFunction()
        {
            try
            {
                PageDBController cPage = new PageDBController(DBIntegrationTests.ConnectionString);
                int idPage = cPage.InsertPage(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idPage);

                int idFuncion = cPage.InsertPageFunction(DataMock.FakeData.GetString(8, false, true, false, false), idPage);
                Assert.AreNotEqual(0, idFuncion);
            }
            catch (Exception)
            {

                throw;
            }
        }

        [TestMethod]
        [Description("controlla che in caso di inserimento doppio Page, venga restituito l'errore 52002 dal DB")]
        public void InsertPage_ERROR_Duplicato()
        {
            try
            {
                PageDBController c = new PageDBController(DBIntegrationTests.ConnectionString);

                string descrizione = DataMock.FakeData.GetString(8, false, true, false, false);
                int id = c.InsertPage(descrizione);
                Assert.AreNotEqual(0, id);

                //questa istruzione deve generare l'errore
                id = c.InsertPage(descrizione);
                //qui non dovrebe mai arrivare
                Assert.AreEqual(0, id);
            }
            catch (SusyLeagueDBException ex)
            {
                if (ManageError)
                    Assert.AreEqual(52002, ex.Code);
                else
                    throw;
            }
        }

        [TestMethod]
        [Description("controlla che in caso di pagina non esistente, venga restituito l'errore 52003 dal DB")]
        public void UpdatePage_ERROR_NOTFOUND()
        {
            try
            {
                PageDBController c = new PageDBController(DBIntegrationTests.ConnectionString);
                //questa istruzione deve generare l'errore
                c.UpdatePage(0, DataMock.FakeData.GetString(8, false, true, false, false));
                //qui non dovrebe mai arrivare
            }
            catch (SusyLeagueDBException ex)
            {
                if (ManageError)
                    Assert.AreEqual(52003, ex.Code);
                else
                    throw;
            }
        }

        [TestMethod]
        [Description("controlla che in caso di inserimento doppio funzione  di pagina, venga restituito l'errore 52001 dal DB")]
        public void InsertPageFunction_ERROR_Duplicato()
        {
            try
            {
                PageDBController cPage = new PageDBController(DBIntegrationTests.ConnectionString);
                int idPage = cPage.InsertPage(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idPage);

                string descrizione = DataMock.FakeData.GetString(8, false, true, false, false);
                int idFuncion = cPage.InsertPageFunction(descrizione, idPage);
                Assert.AreNotEqual(0, idFuncion);

                //questa istruzione deve generare l'errore
                idFuncion = cPage.InsertPageFunction(descrizione, idPage);
                //qui non dovrebe mai arrivare
                Assert.AreEqual(0, idFuncion);
            }
            catch (SusyLeagueDBException ex)
            {
                if (ManageError)
                    Assert.AreEqual(52001, ex.Code);
                else
                    throw;
            }
        }

        [TestMethod]
        [Description("controlla che in caso di inserimento funzione di pagina con pagina NOT FOUND , venga restituito l'errore 547 dal DB")]
        public void InsertPageFunction_Error_PageNOTFOUND()
        {
            try
            {
                PageDBController cPage = new PageDBController(DBIntegrationTests.ConnectionString);
                string descrizione = DataMock.FakeData.GetString(8, false, true, false, false);

                //questa istruzione deve generare l'errore
                int idFuncion = cPage.InsertPageFunction(descrizione, 0);
                //qui non dovrebe mai arrivare
                Assert.AreEqual(0, idFuncion);
            }
            catch (SusyLeagueDBException ex)
            {
                if (ManageError)
                    Assert.AreEqual(547, ex.Code);
                else
                    throw;
            }
        }

        [TestMethod]
        public void InsertRolePage()
        {
            try
            {

                RoleDBController cRole = new RoleDBController(DBIntegrationTests.ConnectionString);
                int idRole = cRole.InsertRole(DataMock.FakeData.GetString(8, false, true, false, false));

                PageDBController cPage = new PageDBController(DBIntegrationTests.ConnectionString);
                int idPage = cPage.InsertPage(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idPage);

                //int idFuncion = cPage.InsertPageFunction(DataMock.FakeData.GetString(8, false, true, false, false), idPage);
                //Assert.AreNotEqual(0, idFuncion);

                int id = cRole.InsertRolePageFunction(idRole, idPage);
                Assert.AreNotEqual(0, id);
            }
            catch (Exception)
            {

                throw;
            }
        }

        [TestMethod]
        public void InsertRolePageFunction()
        {
            try
            {
                RoleDBController cRole = new RoleDBController(DBIntegrationTests.ConnectionString);
                int idRole = cRole.InsertRole(DataMock.FakeData.GetString(8, false, true, false, false));

                PageDBController cPage = new PageDBController(DBIntegrationTests.ConnectionString);
                int idPage = cPage.InsertPage(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idPage);

                int idFuncion = cPage.InsertPageFunction(DataMock.FakeData.GetString(8, false, true, false, false), idPage);
                Assert.AreNotEqual(0, idFuncion);

                int id = cRole.InsertRolePageFunction(idRole, idPage, idFuncion);
                Assert.AreNotEqual(0, id);
            }
            catch (Exception)
            {

                throw;
            }
        }

        [TestMethod]
        public void DeleteRolePageFunction()
        {
            try
            {
                RoleDBController cRole = new RoleDBController(DBIntegrationTests.ConnectionString);
                int idRole = cRole.InsertRole(DataMock.FakeData.GetString(8, false, true, false, false));

                PageDBController cPage = new PageDBController(DBIntegrationTests.ConnectionString);
                int idPage = cPage.InsertPage(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idPage);

                int idFuncion = cPage.InsertPageFunction(DataMock.FakeData.GetString(8, false, true, false, false), idPage);
                Assert.AreNotEqual(0, idFuncion);

                int id = cRole.InsertRolePageFunction(idRole, idPage, idFuncion);
                Assert.AreNotEqual(0, id);

                cRole.DeleteRolePageFunction(id);
            }
            catch (Exception)
            {

                throw;
            }
        }

        [TestMethod]
        [Description("controlla che in caso di inserimento doppio di ruolo/pagina, venga restituito l'errore 52006 dal DB")]
        public void InsertRolePage_ERROR_Duplicato()
        {
            try
            {
                RoleDBController cRole = new RoleDBController(DBIntegrationTests.ConnectionString);
                int idRole = cRole.InsertRole(DataMock.FakeData.GetString(8, false, true, false, false));

                PageDBController cPage = new PageDBController(DBIntegrationTests.ConnectionString);
                int idPage = cPage.InsertPage(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idPage);

                int id = cRole.InsertRolePageFunction(idRole, idPage);
                Assert.AreNotEqual(0, id);

                //questa istruzione deve generare l'errore
                id = cRole.InsertRolePageFunction(idRole, idPage);
                //qui non dovrebe mai arrivare
                Assert.AreEqual(0, id);
            }
            catch (SusyLeagueDBException ex)
            {
                if (ManageError)
                    Assert.AreEqual(52006, ex.Code);
                else
                    throw;
            }
        }

        [TestMethod]
        [Description("controlla che in caso di inserimento doppio di ruolo_pagina_funzione, venga restituito l'errore 52006 dal DB")]
        public void InsertRolePageFunction_ERROR_Duplicato()
        {
            try
            {
                RoleDBController cRole = new RoleDBController(DBIntegrationTests.ConnectionString);
                int idRole = cRole.InsertRole(DataMock.FakeData.GetString(8, false, true, false, false));

                PageDBController cPage = new PageDBController(DBIntegrationTests.ConnectionString);
                int idPage = cPage.InsertPage(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idPage);

                int idFuncion = cPage.InsertPageFunction(DataMock.FakeData.GetString(8, false, true, false, false), idPage);
                Assert.AreNotEqual(0, idFuncion);

                int id = cRole.InsertRolePageFunction(idRole, idPage, idFuncion);
                Assert.AreNotEqual(0, id);

                //questa istruzione deve generare l'errore
                id = cRole.InsertRolePageFunction(idRole, idPage, idFuncion);
                //qui non dovrebe mai arrivare
                Assert.AreEqual(0, id);
            }
            catch (SusyLeagueDBException ex)
            {
                if (ManageError)
                    Assert.AreEqual(52006, ex.Code);
                else
                    throw;
            }
        }

        [TestMethod]
        [Description("controlla che in caso di inserimento ruolo_pagina_funzion con ruolo NOT FOUND , venga restituito l'errore 547 dal DB")]
        public void InsertRolePageFunction_Error_RoleNOTFOUND()
        {
            try
            {

                RoleDBController cRole = new RoleDBController(DBIntegrationTests.ConnectionString);
                int idRole = cRole.InsertRole(DataMock.FakeData.GetString(8, false, true, false, false));

                PageDBController cPage = new PageDBController(DBIntegrationTests.ConnectionString);
                int idPage = cPage.InsertPage(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idPage);

                int idFuncion = cPage.InsertPageFunction(DataMock.FakeData.GetString(8, false, true, false, false), idPage);
                Assert.AreNotEqual(0, idFuncion);

                //questa istruzione deve generare l'errore
                int id = cRole.InsertRolePageFunction(0, idPage, idFuncion);
                //qui non dovrebe mai arrivare
                Assert.AreEqual(0, idFuncion);
            }
            catch (SusyLeagueDBException ex)
            {
                if (ManageError)
                    Assert.AreEqual(547, ex.Code);
                else
                    throw;
            }
        }

        [TestMethod]
        [Description("controlla che in caso di inserimento ruolo_pagina_funzion con page NOT FOUND , venga restituito l'errore 547 dal DB")]
        public void InsertRolePageFunction_Error_PageNOTFOUND()
        {
            try
            {

                RoleDBController cRole = new RoleDBController(DBIntegrationTests.ConnectionString);
                int idRole = cRole.InsertRole(DataMock.FakeData.GetString(8, false, true, false, false));

                PageDBController cPage = new PageDBController(DBIntegrationTests.ConnectionString);
                int idPage = cPage.InsertPage(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idPage);

                int idFuncion = cPage.InsertPageFunction(DataMock.FakeData.GetString(8, false, true, false, false), idPage);
                Assert.AreNotEqual(0, idFuncion);

                //questa istruzione deve generare l'errore
                int id = cRole.InsertRolePageFunction(idRole, 0, idFuncion);
                //qui non dovrebe mai arrivare
                Assert.AreEqual(0, idFuncion);
            }
            catch (SusyLeagueDBException ex)
            {
                if (ManageError)
                    Assert.AreEqual(547, ex.Code);
                else
                    throw;
            }
        }
        #endregion
    }
}
