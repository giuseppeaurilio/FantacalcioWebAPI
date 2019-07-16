using CustomExceptions;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace DBControllers.SusyLeague
{
    public class SquadraDBController : BaseDBController
    {
        public SquadraDBController(string cs) : base(cs)
        {
        }
        public int InsertSquadra(string descrizione, int stagioneSusyLeagueId, int userId)
        {
            int ret = 0;
            using (var connection = new SqlConnection(this.connection))
            {
                var sql = "[susyleague].[squadra_insert]";
                var p = new DynamicParameters();
                p.Add("@Descrizione", descrizione);
                p.Add("@StagioneId", stagioneSusyLeagueId);
                p.Add("@UserId", userId);

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

        public int InsertGiocatoreInFantasquadra(int idGiocatore, int idFantasquadra, int idStagione, int idTransazione, DateTime dataInizio)
        {
            int ret = 0;
            using (var connection = new SqlConnection(this.connection))
            {
                var sql = "[susyleague].[squadre_lk_giocatori_insert]";
                var p = new DynamicParameters();
                p.Add("@GiocatoreId", idGiocatore);
                p.Add("@FantasquadraId", idFantasquadra);
                p.Add("@StagioneId", idStagione);
                p.Add("@TransazioneId", idTransazione);
                p.Add("@DataInizio", dataInizio, DbType.DateTime);


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

        public void DeleteGiocatoreDaFantasquadra(int idGiocatore, int idFantasquadra, int idStagione, int idTransazione, DateTime dataFine)
        {
            using (var connection = new SqlConnection(this.connection))
            {
                var sql = "[susyleague].[squadre_lk_giocatori_logicdelete]";
                var p = new DynamicParameters();
                p.Add("@GiocatoreId", idGiocatore);
                p.Add("@FantasquadraId", idFantasquadra);
                p.Add("@StagioneId", idStagione);
                p.Add("@TransazioneId", idTransazione);
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
    }
}
