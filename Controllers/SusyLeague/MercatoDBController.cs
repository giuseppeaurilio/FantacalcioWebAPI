using CustomExceptions;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace DBControllers.SusyLeague
{
    public class MercatoDBController : BaseDBController
    {
        public MercatoDBController(string cs) : base(cs)
        {
        }

        public int InsertMercatoFase(string descrizione, DateTime dataInizio, DateTime dataFine)
        {
            int ret = 0;
            using (var connection = new SqlConnection(this.connection))
            {
                var sql = "[susyleague].[mercato_fase_insert]";
                var p = new DynamicParameters();
                p.Add("@Descrizione", descrizione);
                p.Add("@DataInizio", dataInizio);
                p.Add("@DataFine", dataFine);

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

        public void UpdateMercatoFase(int id, string descrizione, DateTime dataInizio, DateTime dataFine)
        {

            using (var connection = new SqlConnection(this.connection))
            {
                var sql = "[susyleague].[mercato_fase_update]";
                var p = new DynamicParameters();
                p.Add("@Id", id);
                p.Add("@Descrizione", descrizione);
                p.Add("@DataInizio", dataInizio);
                p.Add("@DataFine", dataFine);

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

        public int InsertTransazione(int idFase)
        {
            int ret = 0;
            using (var connection = new SqlConnection(this.connection))
            {
                var sql = "[susyleague].[mercato_transazione_insert]";
                var p = new DynamicParameters();
                p.Add("@MercatoFaseId", idFase);

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

        public int InsertAcquisto(int idGiocatore, int idFantasquadraOrigine, int idFantasquadraDestinazione, int costo, int idTransazione)
        {
            int ret = 0;
            using (var connection = new SqlConnection(this.connection))
            {
                var sql = "[susyleague].[mercato_acquisto_insert]";
                var p = new DynamicParameters();
                if (idGiocatore != 0)
                    p.Add("@GiocatoreId", idGiocatore);
                if (idFantasquadraOrigine != 0)
                    p.Add("@SquadraOrigineId", idFantasquadraOrigine);

                p.Add("@SquadraDestinazioneId", idFantasquadraDestinazione);
                p.Add("@Costo", costo);
                p.Add("@MercatoTransazioneId", idTransazione);

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
        public int updateAcquisto(int idAcquisto, int idGiocatore, int idFantasquadraOrigine, int idFantasquadraDestinazione, int costo, int idTransazione)
        {
            int ret = 0;
            using (var connection = new SqlConnection(this.connection))
            {
                var sql = "[susyleague].[mercato_acquisto_update]";
                var p = new DynamicParameters();
                p.Add("@Id", idAcquisto);

                if (idGiocatore != 0)
                    p.Add("@GiocatoreId", idGiocatore);
                if (idFantasquadraOrigine != 0)
                    p.Add("@SquadraOrigineId", idFantasquadraOrigine);

                p.Add("@SquadraDestinazioneId", idFantasquadraDestinazione);
                p.Add("@Costo", costo);
                p.Add("@MercatoTransazioneId", idTransazione);

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
    }
}
