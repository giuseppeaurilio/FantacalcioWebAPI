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

        public static bool ManageError = true;

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
        [Description("controlla che in caso di inserimento doppio della giornata nella stagione, venga restituito l'errore 50004 dal DB")]
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
        public void Error_UpdateGiornata()
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
                int id = c.InserisciSquadra(DataMock.FakeData.GetString(8, false, true, false, false));

                Assert.AreNotEqual(0, id);
            }
            catch (Exception)
            {

                throw;
            }
        }

        [TestMethod]
        [Description("controlla che in caso di inserimento doppio di squadra, venga restituito l'errore 50006 dal DB")]
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
                int id = c.InserisciStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, id);


                SquadraDBController cs = new SquadraDBController(DBIntegrationTests.ConnectionString);
                int ids = cs.InserisciSquadra(DataMock.FakeData.GetString(8, false, true, false, false));

                Assert.AreNotEqual(0, ids);

                int ret = c.InserisciSquadraInStagione(id, ids);
                Assert.AreNotEqual(0, ret);

            }
            catch (Exception)
            {

                throw;
            }
        }

        [TestMethod]
        [Description("controlla che in caso di inserimento doppio di squadra nella stagione, venga restituito l'errore 50016 dal DB")]
        public void Error_InsertSquadraInStagioneDuplicata()
        {
            try
            {

                StagioneDBController c = new StagioneDBController(DBIntegrationTests.ConnectionString);
                int id = c.InserisciStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, id);


                SquadraDBController cs = new SquadraDBController(DBIntegrationTests.ConnectionString);
                int ids = cs.InserisciSquadra(DataMock.FakeData.GetString(8, false, true, false, false));

                Assert.AreNotEqual(0, ids);

                int ret = c.InserisciSquadraInStagione(id, ids);
                Assert.AreNotEqual(0, ret);
                ret = c.InserisciSquadraInStagione(id, ids);
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
        [Description("controlla che in caso di inserimento doppio di giocatore, venga restituito l'errore 50001 dal DB")]
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
                int idSquadra = cs.InserisciSquadra(DataMock.FakeData.GetString(8, false, true, false, false));

                GiocatoreDBController cg = new GiocatoreDBController(DBIntegrationTests.ConnectionString);
                int idGiocatore = cg.InserisciGiocatore(DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(4, true, false, true, false));

                SquadraDBController csq = new SquadraDBController(DBIntegrationTests.ConnectionString);
                int ret = csq.InserisciGiocatoreInSquadra(idGiocatore, idSquadra,
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
                int idSquadra = cs.InserisciSquadra(DataMock.FakeData.GetString(8, false, true, false, false));

                GiocatoreDBController cg = new GiocatoreDBController(DBIntegrationTests.ConnectionString);
                int idGiocatore = cg.InserisciGiocatore(DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(4, true, false, true, false));

                SquadraDBController csq = new SquadraDBController(DBIntegrationTests.ConnectionString);
                int ret = csq.InserisciGiocatoreInSquadra(idGiocatore, idSquadra,
                     DataMock.FakeData.GetDate(DateTime.Now, 0, 0));
                Assert.AreNotEqual(0, ret);

                csq.CancellaGiocatoreDaSquadra(idGiocatore, idSquadra,
                     DataMock.FakeData.GetDate(DateTime.Now, 0, 0));

            }
            catch (Exception)
            {
                throw;
            }
        }

        [TestMethod]
        [Description("controlla che in caso di inserimento di un giocatore non libero in un'altra squadra, venga restituito l'errore 50014 dal DB")]
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
                int ret = csq.InserisciGiocatoreInSquadra(idGiocatore, idSquadra1,
                     DataMock.FakeData.GetDate(DateTime.Now, 0, 0));
                Assert.AreNotEqual(0, ret);

                int idSquadra2 = cs.InserisciSquadra(DataMock.FakeData.GetString(8, false, true, false, false));
                ret = csq.InserisciGiocatoreInSquadra(idGiocatore, idSquadra2,
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
                int idSquadra = cs.InserisciSquadra(DataMock.FakeData.GetString(8, false, true, false, false));

                GiocatoreDBController cg = new GiocatoreDBController(DBIntegrationTests.ConnectionString);
                int idGiocatore = cg.InserisciGiocatore(DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(4, true, false, true, false));

                SquadraDBController csq = new SquadraDBController(DBIntegrationTests.ConnectionString);
                int ret = csq.InserisciGiocatoreInSquadra(idGiocatore, idSquadra,
                     DataMock.FakeData.GetDate(DateTime.Now, 0, 0));
                Assert.AreNotEqual(0, ret);
                csq.CancellaGiocatoreDaSquadra(idGiocatore, idSquadra,
                     DataMock.FakeData.GetDate(DateTime.Now, 0, 0));

                int idSquadra2 = cs.InserisciSquadra(DataMock.FakeData.GetString(8, false, true, false, false));

                int ret2 = csq.InserisciGiocatoreInSquadra(idGiocatore, idSquadra2,
                     DataMock.FakeData.GetDate(DateTime.Now, 24, 24));
                Assert.AreNotEqual(0, ret2);
            }
            catch (Exception)
            {

                throw;
            }
        }

        [TestMethod]
        [Description("controlla che in caso di errore durante la cancellazione di un giocatore dalla squadra, venga restituito l'errore 50002 dal DB")]
        public void Error_DeleteGiocatoreDaSquadra()
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
                int ret = csq.InserisciGiocatoreInSquadra(idGiocatore, idSquadra,
                     DataMock.FakeData.GetDate(DateTime.Now, 0, 0));
                Assert.AreNotEqual(0, ret);

                csq.CancellaGiocatoreDaSquadra(idGiocatore, 0,
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
                int idStagione = cStagione.InserisciStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idStagione);

                DateTime giornataStartDate = DataMock.FakeData.GetDate(new DateTime(2019, 8, 21, 15, 00, 00), 0, 0);
                DateTime giornataEndDate = DataMock.FakeData.GetDate(giornataStartDate, 0, 72);
                int idGiornata = cStagione.InserisciGiornataInStagione(DataMock.FakeData.GetString(8, false, true, false, false),
                    giornataStartDate,
                    giornataEndDate,
                    idStagione);
                Assert.AreNotEqual(0, idGiornata);

                SquadraDBController cSquadra = new SquadraDBController(DBIntegrationTests.ConnectionString);
                int idSquadra = cSquadra.InserisciSquadra(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idSquadra);

                GiocatoreDBController cGiocatore = new GiocatoreDBController(DBIntegrationTests.ConnectionString);
                int idGiocatore = cGiocatore.InserisciGiocatore(DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(4, true, false, true, false));
                Assert.AreNotEqual(0, idGiocatore);

                int idSquadraGiocatore = cSquadra.InserisciGiocatoreInSquadra(idGiocatore, idSquadra,
                      DataMock.FakeData.GetDate(DateTime.Now, 0, 0));
                Assert.AreNotEqual(0, idSquadraGiocatore);

                int idVoto = cGiocatore.InserisciVotoDelGiocatore(idGiocatore, idGiornata
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
                int idStagione = cStagione.InserisciStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idStagione);

                DateTime giornataStartDate = DataMock.FakeData.GetDate(new DateTime(2019, 8, 21, 15, 00, 00), 0, 0);
                DateTime giornataEndDate = DataMock.FakeData.GetDate(giornataStartDate, 0, 72);
                int idGiornata = cStagione.InserisciGiornataInStagione(DataMock.FakeData.GetString(8, false, true, false, false),
                    giornataStartDate,
                    giornataEndDate,
                    idStagione);
                Assert.AreNotEqual(0, idGiornata);

                SquadraDBController cSquadra = new SquadraDBController(DBIntegrationTests.ConnectionString);
                int idSquadra = cSquadra.InserisciSquadra(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idSquadra);

                GiocatoreDBController cGiocatore = new GiocatoreDBController(DBIntegrationTests.ConnectionString);
                int idGiocatore = cGiocatore.InserisciGiocatore(DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(4, true, false, true, false));
                Assert.AreNotEqual(0, idGiocatore);

                int idSquadraGiocatore = cSquadra.InserisciGiocatoreInSquadra(idGiocatore, idSquadra,
                      DataMock.FakeData.GetDate(DateTime.Now, 0, 0));
                Assert.AreNotEqual(0, idSquadraGiocatore);

                int idVoto = cGiocatore.InserisciVotoDelGiocatore(idGiocatore, idGiornata
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
        public void Error_InsertVotoDelGiocatoreDuplicato()
        {
            try
            {
                StagioneDBController cStagione = new StagioneDBController(DBIntegrationTests.ConnectionString);
                int idStagione = cStagione.InserisciStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idStagione);

                DateTime giornataStartDate = DataMock.FakeData.GetDate(new DateTime(2019, 8, 21, 15, 00, 00), 0, 0);
                DateTime giornataEndDate = DataMock.FakeData.GetDate(giornataStartDate, 0, 72);
                int idGiornata = cStagione.InserisciGiornataInStagione(DataMock.FakeData.GetString(8, false, true, false, false),
                    giornataStartDate,
                    giornataEndDate,
                    idStagione);
                Assert.AreNotEqual(0, idGiornata);

                SquadraDBController cSquadra = new SquadraDBController(DBIntegrationTests.ConnectionString);
                int idSquadra = cSquadra.InserisciSquadra(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idSquadra);

                GiocatoreDBController cGiocatore = new GiocatoreDBController(DBIntegrationTests.ConnectionString);
                int idGiocatore = cGiocatore.InserisciGiocatore(DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(4, true, false, true, false));
                Assert.AreNotEqual(0, idGiocatore);

                int idSquadraGiocatore = cSquadra.InserisciGiocatoreInSquadra(idGiocatore, idSquadra,
                      DataMock.FakeData.GetDate(DateTime.Now, 0, 0));
                Assert.AreNotEqual(0, idSquadraGiocatore);

                int idVoto = cGiocatore.InserisciVotoDelGiocatore(idGiocatore, idGiornata
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
                int idVoto2 = cGiocatore.InserisciVotoDelGiocatore(idGiocatore, idGiornata
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
        public void Error_InsertVotoDelGiocatoreNonEsistente()
        {
            try
            {
                StagioneDBController cStagione = new StagioneDBController(DBIntegrationTests.ConnectionString);
                int idStagione = cStagione.InserisciStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idStagione);

                DateTime giornataStartDate = DataMock.FakeData.GetDate(new DateTime(2019, 8, 21, 15, 00, 00), 0, 0);
                DateTime giornataEndDate = DataMock.FakeData.GetDate(giornataStartDate, 0, 72);
                int idGiornata = cStagione.InserisciGiornataInStagione(DataMock.FakeData.GetString(8, false, true, false, false),
                    giornataStartDate,
                    giornataEndDate,
                    idStagione);
                Assert.AreNotEqual(0, idGiornata);

                SquadraDBController cSquadra = new SquadraDBController(DBIntegrationTests.ConnectionString);
                int idSquadra = cSquadra.InserisciSquadra(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idSquadra);

                GiocatoreDBController cGiocatore = new GiocatoreDBController(DBIntegrationTests.ConnectionString);
                //int idGiocatore = cGiocatore.InserisciGiocatore(DataMock.FakeData.GetString(8, false, true, false, false),
                //    DataMock.FakeData.GetString(8, false, true, false, false),
                //    DataMock.FakeData.GetString(4, true, false, true, false));
                //Assert.AreNotEqual(0, idGiocatore);

                //int idSquadraGiocatore = cSquadra.InserisciGiocatoreInSquadra(idGiocatore, idSquadra,
                //      DataMock.FakeData.GetDate(DateTime.Now, 0, 0));
                //Assert.AreNotEqual(0, idSquadraGiocatore);

                int idVoto = cGiocatore.InserisciVotoDelGiocatore(0, idGiornata
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
        public void Error_InsertVotoDelGiocatorePerGiornataNonEsistente()
        {
            try
            {
                StagioneDBController cStagione = new StagioneDBController(DBIntegrationTests.ConnectionString);
                int idStagione = cStagione.InserisciStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idStagione);

                DateTime giornataStartDate = DataMock.FakeData.GetDate(new DateTime(2019, 8, 21, 15, 00, 00), 0, 0);
                DateTime giornataEndDate = DataMock.FakeData.GetDate(giornataStartDate, 0, 72);
                int idGiornata = cStagione.InserisciGiornataInStagione(DataMock.FakeData.GetString(8, false, true, false, false),
                    giornataStartDate,
                    giornataEndDate,
                    idStagione);
                Assert.AreNotEqual(0, idGiornata);

                SquadraDBController cSquadra = new SquadraDBController(DBIntegrationTests.ConnectionString);
                int idSquadra = cSquadra.InserisciSquadra(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idSquadra);

                GiocatoreDBController cGiocatore = new GiocatoreDBController(DBIntegrationTests.ConnectionString);
                int idGiocatore = cGiocatore.InserisciGiocatore(DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(4, true, false, true, false));
                Assert.AreNotEqual(0, idGiocatore);

                //int idSquadraGiocatore = cSquadra.InserisciGiocatoreInSquadra(idGiocatore, idSquadra,
                //      DataMock.FakeData.GetDate(DateTime.Now, 0, 0));
                //Assert.AreNotEqual(0, idSquadraGiocatore);

                int idVoto = cGiocatore.InserisciVotoDelGiocatore(idGiocatore, 0
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
        public void Error_UpdateVotoDelGiocatore()
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
                int idStagione = cStagione.InserisciStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idStagione);

                GiocatoreDBController cGiocatore = new GiocatoreDBController(DBIntegrationTests.ConnectionString);
                int idGiocatore = cGiocatore.InserisciGiocatore(DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(4, true, false, true, false));
                Assert.AreNotEqual(0, idGiocatore);

                int idStatistica = cGiocatore.InserisciStatisticaDelGiocatorePerStagione(idGiocatore, idStagione
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
                int idStagione = cStagione.InserisciStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idStagione);

                GiocatoreDBController cGiocatore = new GiocatoreDBController(DBIntegrationTests.ConnectionString);
                int idGiocatore = cGiocatore.InserisciGiocatore(DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(4, true, false, true, false));
                Assert.AreNotEqual(0, idGiocatore);

                int idStatistica = cGiocatore.InserisciStatisticaDelGiocatorePerStagione(idGiocatore, idStagione
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
        public void Error_InsertStatisticaDelGiocatoreDuplicato()
        {
            try
            {
                StagioneDBController cStagione = new StagioneDBController(DBIntegrationTests.ConnectionString);
                int idStagione = cStagione.InserisciStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idStagione);

                GiocatoreDBController cGiocatore = new GiocatoreDBController(DBIntegrationTests.ConnectionString);
                int idGiocatore = cGiocatore.InserisciGiocatore(DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(4, true, false, true, false));
                Assert.AreNotEqual(0, idGiocatore);

                int idStatistica1 = cGiocatore.InserisciStatisticaDelGiocatorePerStagione(idGiocatore, idStagione
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
                int idStatistica2 = cGiocatore.InserisciStatisticaDelGiocatorePerStagione(idGiocatore, idStagione
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
        public void Error_InsertStatisticaDelGiocatoreNonEsistente()
        {
            try
            {
                StagioneDBController cStagione = new StagioneDBController(DBIntegrationTests.ConnectionString);
                int idStagione = cStagione.InserisciStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                Assert.AreNotEqual(0, idStagione);

                GiocatoreDBController cGiocatore = new GiocatoreDBController(DBIntegrationTests.ConnectionString);
                //int idGiocatore = cGiocatore.InserisciGiocatore(DataMock.FakeData.GetString(8, false, true, false, false),
                //    DataMock.FakeData.GetString(8, false, true, false, false),
                //    DataMock.FakeData.GetString(4, true, false, true, false));
                //Assert.AreNotEqual(0, idGiocatore);

                //questa istruzione deve generare l'errore
                int idStatistica1 = cGiocatore.InserisciStatisticaDelGiocatorePerStagione(0, idStagione
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
        public void Error_InsertStatisticaDelGiocatorePerStagioneNonEsistente()
        {
            try
            {
                StagioneDBController cStagione = new StagioneDBController(DBIntegrationTests.ConnectionString);
                //int idStagione = cStagione.InserisciStagione(DataMock.FakeData.GetString(8, false, true, false, false));
                //Assert.AreNotEqual(0, idStagione);

                GiocatoreDBController cGiocatore = new GiocatoreDBController(DBIntegrationTests.ConnectionString);
                int idGiocatore = cGiocatore.InserisciGiocatore(DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(8, false, true, false, false),
                    DataMock.FakeData.GetString(4, true, false, true, false));
                Assert.AreNotEqual(0, idGiocatore);

                //questa istruzione deve generare l'errore
                int idStatistica1 = cGiocatore.InserisciStatisticaDelGiocatorePerStagione(idGiocatore, 0
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
        public void Error_UpdateStatisticaDelGiocatore()
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

    }
}
