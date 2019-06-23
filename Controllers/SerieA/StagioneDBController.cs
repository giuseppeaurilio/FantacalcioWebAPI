using Dapper;
using System;
using System.Data;
using System.Data.SqlClient;
using CustomExceptions;

namespace DBControllers.SerieA
{
    public class StagioneDBController : BaseDBController
    {
        public StagioneDBController(string cs) : base(cs)
        {
        }

        public int InserisciStagione(string descrizione)
        {
            int ret = 0;
            using (var connection = new SqlConnection(this.connection))
            {
                var sql = "seriea.stagione_insert_giornata";
                var p = new DynamicParameters();
                p.Add("@Descrizione", descrizione);
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
        public int InserisciGiornataInStagione(string descrizione, DateTime dataInizio, DateTime dataFine, int stagioneId)
        {
            int ret = 0;
            using (
                var connection = new SqlConnection(this.connection))
            {
                var sql = "[seriea].[giornata_insert]";
                var p = new DynamicParameters();
                p.Add("@Descrizione", descrizione);
                p.Add("@DataInizio", dataInizio, DbType.DateTime);
                p.Add("@DataFine", dataFine, DbType.DateTime);
                p.Add("@StagioneId", stagioneId);


                p.Add("@Id", dbType: DbType.Int32, direction: ParameterDirection.Output);
                p.Add("@ErrorMessage", dbType: DbType.String, direction: ParameterDirection.Output, size: 4000);
                p.Add("@return_value", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);

                //p.Add("@return_value", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);

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

        public void UpdateGiornata(int id, string descrizione, DateTime dataInizio, DateTime dataFine)
        {

            using (var connection = new SqlConnection(this.connection))
            {
                var sql = "[seriea].[stagione_update_giornata]";
                var p = new DynamicParameters();
                p.Add("@Id", id);

                p.Add("@Descrizione", descrizione);
                p.Add("@DataInizio", dataInizio, DbType.DateTime);
                p.Add("@DataFine", dataFine, DbType.DateTime);

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

        public void InserisciSquadraInStagione(int idStagione, int idSquadra)
        {
            using (var connection = new SqlConnection(this.connection))
            {
                var sql = "[seriea].[stagione_insert_squadra]";
                var p = new DynamicParameters();
                p.Add("@IdStagione", idStagione);
                p.Add("@IdSquadra", idSquadra);

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
