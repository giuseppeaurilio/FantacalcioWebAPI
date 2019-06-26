using CustomExceptions;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace DBControllers.SusyLeague
{
    public class IncontroDBController : BaseDBController
    {
        public IncontroDBController(string cs) : base(cs)
        {
        }
        public int InsertIncontro(int squadraId1, int squadraId2, int giornataSusyLeagueId)
        {
            int ret = 0;
            using (var connection = new SqlConnection(this.connection))
            {
                var sql = "[susyleague].[incontro_insert]";
                var p = new DynamicParameters();
                p.Add("@Squadra1Id", squadraId1);
                p.Add("@Squadra2Id", squadraId2);
                p.Add("@GiornataSusyLeagueId", giornataSusyLeagueId);

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
        public int InsertGiocatoreInIncontroPerSquadra(int incontroId, int squadraId, int giocatoreId, int ordine)
        {
            int ret = 0;
            using (var connection = new SqlConnection(this.connection))
            {
                var sql = "[susyleague].[incontro_lk_giocatore_lk_squadra_insert]";
                var p = new DynamicParameters();
                p.Add("@IncontroId", incontroId);
                p.Add("@SquadraId", squadraId);
                p.Add("@GiocatoreId", giocatoreId);
                p.Add("@Ordine", ordine);

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
        public void UpdateRisultato(int id, int golSquadra1, int golSquadra2, double punteggioSquadra1, double punteggioSquadra2)
        {
            using (var connection = new SqlConnection(this.connection))
            {
                var sql = "[susyleague].[incontro_update_risultato]";
                var p = new DynamicParameters();
                p.Add("@Id", id);
                p.Add("@GolSquadra1", golSquadra1);
                p.Add("@GolSquadra2", golSquadra2);
                p.Add("@PunteggioSquadra1", punteggioSquadra1);
                p.Add("@PunteggioSquadra2", punteggioSquadra2);

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
