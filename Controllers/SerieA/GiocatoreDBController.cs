using CustomExceptions;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace DBControllers.SerieA
{
    public class GiocatoreDBController : BaseDBController
    {
        public GiocatoreDBController(string cs) : base(cs)
        {
        }

        public int InserisciGiocatore(string nome, string cognome, string gazzettaId)
        {
            int ret = 0;
            using (var connection = new SqlConnection(this.connection))
            {
                var sql = "[seriea].[giocatore_insert]";
                var p = new DynamicParameters();
                p.Add("@Nome", nome);
                p.Add("@Cognome", cognome);
                p.Add("@GazzettaId", gazzettaId);


                p.Add("@Id", dbType: DbType.Int32, direction: ParameterDirection.Output);

                p.Add("@ErrorMessage", dbType: DbType.String, direction: ParameterDirection.Output, size: 4000);
                p.Add("@return_value", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);

                int retVal = connection.Execute(sql, p, commandType: CommandType.StoredProcedure);
                if (retVal != -1)
                    throw new Exception("SP EXECUTION ERROR: " + sql);

                int retDB = p.Get<int>("@return_value");
                string Message = p.Get<string>("@ErrorMessage");
                if (retDB == 0)
                    ret = p.Get<int>("@Id");
                else
                    throw new SusyLeagueDBException(retDB, Message, sql);
            }
            return ret;
        }

        public void UpdateGiocatore(int id, string gazzettaId)
        {

            using (var connection = new SqlConnection(this.connection))
            {
                var sql = "[seriea].[giocatore_update]";
                var p = new DynamicParameters();
                p.Add("@Id", id);
                p.Add("@GazzettaId", gazzettaId);

                p.Add("@ErrorMessage", dbType: DbType.String, direction: ParameterDirection.Output, size: 4000);
                p.Add("@return_value", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);


                int retVal = connection.Execute(sql, p, commandType: CommandType.StoredProcedure);
                if (retVal != -1)
                    throw new Exception("SP EXECUTION ERROR: " + sql);

                int retDB = p.Get<int>("@return_value");
                string Message = p.Get<string>("@ErrorMessage");
                if (retDB != 0)
                    throw new SusyLeagueDBException(retDB, Message, sql);
            }

        }

        public int InserisciVotoDelGiocatore(int idGiocatore, int idGiornata,
            double voto, double fantavoto, int golFatti, int golSubiti, int autogol, int assist,
            bool ammonizione, bool espulsione,
            int rigoriSbagliati, int rigoriTrasformati, int rigoriParati, int rigoriSubiti)
        {
            int ret = 0;
            using (var connection = new SqlConnection(this.connection))
            {
                var sql = "[seriea].[giocatore_insert_voto]";
                var p = new DynamicParameters();
                p.Add("@GiocatoreId", idGiocatore);
                p.Add("@GiornataId", idGiornata);
                p.Add("@Voto", voto);
                p.Add("@Fantavoto", fantavoto);
                p.Add("@GolFatti", golFatti);
                p.Add("@GolSubiti", golSubiti);
                p.Add("@Autogol", autogol);
                p.Add("@Assist", assist);
                p.Add("@Ammonizione", ammonizione);
                p.Add("@Espulsione", espulsione);
                p.Add("@RigoriSbagliati", rigoriSbagliati);
                p.Add("@RigoriTrasformati", rigoriTrasformati);
                p.Add("@RigoriParati", rigoriParati);
                p.Add("@RigoriSubiti", rigoriSubiti);


                p.Add("@Id", dbType: DbType.Int32, direction: ParameterDirection.Output);

                p.Add("@ErrorMessage", dbType: DbType.String, direction: ParameterDirection.Output, size: 4000);
                p.Add("@return_value", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);

                int retVal = connection.Execute(sql, p, commandType: CommandType.StoredProcedure);
                if (retVal != -1)
                    throw new Exception("SP EXECUTION ERROR: " + sql);

                int retDB = p.Get<int>("@return_value");
                string Message = p.Get<string>("@ErrorMessage");
                if (retDB == 0)
                    ret = p.Get<int>("@Id");
                else
                    throw new SusyLeagueDBException(retDB, Message, sql);
            }
            return ret;
        }

        public void UpdateVotoDelGiocatore(int id, double voto, double fantavoto,
            int golFatti, int golSubiti, int autogol, int assist, bool ammonizione, bool espulsione,
            int rigoriSbagliati, int rigoriTrasformati, int rigoriParati, int rigoriSubiti)
        {
            using (var connection = new SqlConnection(this.connection))
            {
                var sql = "[seriea].[giocatore_update_voto]";
                var p = new DynamicParameters();
                p.Add("@Id", id);

                p.Add("@Voto", voto);
                p.Add("@Fantavoto", fantavoto);
                p.Add("@GolFatti", golFatti);
                p.Add("@GolSubiti", golSubiti);
                p.Add("@Autogol", autogol);
                p.Add("@Assist", assist);
                p.Add("@Ammonizione", ammonizione);
                p.Add("@Espulsione", espulsione);
                p.Add("@RigoriSbagliati", rigoriSbagliati);
                p.Add("@RigoriTrasformati", rigoriTrasformati);
                p.Add("@RigoriParati", rigoriParati);
                p.Add("@RigoriSubiti", rigoriSubiti);

                p.Add("@ErrorMessage", dbType: DbType.String, direction: ParameterDirection.Output, size: 4000);
                p.Add("@return_value", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);

                int retVal = connection.Execute(sql, p, commandType: CommandType.StoredProcedure);
                if (retVal != -1)
                    throw new Exception("SP EXECUTION ERROR: " + sql);

                int retDB = p.Get<int>("@return_value");
                string Message = p.Get<string>("@ErrorMessage");
                if (retDB != 0)
                    throw new SusyLeagueDBException(retDB, Message, sql);
            }
        }

        public int InserisciStatisticaDelGiocatorePerStagione(int idGiocatore, int idStagione,
            int presenze, int giocabili, double mediaVoto, double mediaFantavoto,
            int golFatti, int golSubiti, int autogol, int assist,
            int ammonizione, int espulsione,
            int rigoriSbagliati, int rigoriTrasformati, int rigoriParati, int rigoriSubiti)
        {
            int ret = 0;
            using (var connection = new SqlConnection(this.connection))
            {
                var sql = "[seriea].[giocatore_insert_statistica]";
                var p = new DynamicParameters();
                p.Add("@GiocatoreId", idGiocatore);
                p.Add("@StagioneId", idStagione);
                p.Add("@Presenze", presenze);
                p.Add("@Giocabili", giocabili);
                p.Add("@MediaVoto", mediaVoto);
                p.Add("@Fantamedia", mediaFantavoto);
                p.Add("@GolFatti", golFatti);
                p.Add("@GolSubiti", golSubiti);
                p.Add("@Autogol", autogol);
                p.Add("@Assist", assist);
                p.Add("@Ammonizioni", ammonizione);
                p.Add("@Espulsioni", espulsione);
                p.Add("@RigoriSbagliati", rigoriSbagliati);
                p.Add("@RigoriTrasformati", rigoriTrasformati);
                p.Add("@RigoriParati", rigoriParati);
                p.Add("@RigoriSubiti", rigoriSubiti);


                p.Add("@Id", dbType: DbType.Int32, direction: ParameterDirection.Output);

                p.Add("@ErrorMessage", dbType: DbType.String, direction: ParameterDirection.Output, size: 4000);
                p.Add("@return_value", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);

                int retVal = connection.Execute(sql, p, commandType: CommandType.StoredProcedure);
                if (retVal != -1)
                    throw new Exception("SP EXECUTION ERROR: " + sql);

                int retDB = p.Get<int>("@return_value");
                string Message = p.Get<string>("@ErrorMessage");
                if (retDB == 0)
                    ret = p.Get<int>("@Id");
                else
                    throw new SusyLeagueDBException(retDB, Message, sql);
            }
            return ret;
        }

        public void UpdateStatisticaDelGiocatorePerStagione(int id,
            int presenze, int giocabili, double mediaVoto, double mediaFantavoto,
            int golFatti, int golSubiti, int autogol, int assist,
            int ammonizione, int espulsione,
            int rigoriSbagliati, int rigoriTrasformati, int rigoriParati, int rigoriSubiti)
        {
            using (var connection = new SqlConnection(this.connection))
            {
                var sql = "[seriea].[giocatore_update_statistica]";
                var p = new DynamicParameters();
                p.Add("@Id", id);

                p.Add("@Presenze", presenze);
                p.Add("@Giocabili", giocabili);
                p.Add("@MediaVoto", mediaVoto);
                p.Add("@Fantamedia", mediaFantavoto);
                p.Add("@GolFatti", golFatti);
                p.Add("@GolSubiti", golSubiti);
                p.Add("@Autogol", autogol);
                p.Add("@Assist", assist);
                p.Add("@Ammonizioni", ammonizione);
                p.Add("@Espulsioni", espulsione);
                p.Add("@RigoriSbagliati", rigoriSbagliati);
                p.Add("@RigoriTrasformati", rigoriTrasformati);
                p.Add("@RigoriParati", rigoriParati);
                p.Add("@RigoriSubiti", rigoriSubiti);

                p.Add("@ErrorMessage", dbType: DbType.String, direction: ParameterDirection.Output, size: 4000);
                p.Add("@return_value", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);

                int retVal = connection.Execute(sql, p, commandType: CommandType.StoredProcedure);
                if (retVal != -1)
                    throw new Exception("SP EXECUTION ERROR: " + sql);

                int retDB = p.Get<int>("@return_value");
                string Message = p.Get<string>("@ErrorMessage");
                if (retDB != 0)
                    throw new SusyLeagueDBException(retDB, Message, sql);
            }
        }
    }
}
