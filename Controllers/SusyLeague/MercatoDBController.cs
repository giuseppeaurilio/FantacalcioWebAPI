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

        /// <summary>
        /// Inserisce una finestra di mercato per la stagione.
        /// </summary>
        /// <param name="descrizione">Descrizione della finestra di mercato</param>
        /// <param name="dataInizio">Data inizio della finestra di mercato</param>
        /// <param name="dataFine">Data fine della finestra di mercato</param>
        /// <returns>id dell'entità appena inserita</returns>
        /// <exception cref="SusyLeagueDBException">non possono esistere due finestre di mercato attive sovrapposte.</exception>
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
        /// <summary>
        /// Aggiorna i dati della finestra di mercato. Posso variare la descrizione o le dati di inizio e fine.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="descrizione"></param>
        /// <param name="dataInizio"></param>
        /// <param name="dataFine"></param>
        /// <exception cref="SusyLeagueDBException">non possono esistere due finestre di mercato attive sovrapposte.</exception>
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

        /// <summary>
        /// Inserisco una transazione per la finestra di mercato. Una transazione raggruppa una seire di movimenti di mercato, intesi come acquisto di giocatori o trasferimento di credito
        /// </summary>
        /// <param name="idFase">Idfase di mercata</param>
        /// <returns></returns>
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

        /// <summary>
        /// Inserisco un movimento nella transazione: il movimento può essere di acquisto di un giocatore libero, di trasferimento del giocatore tra due squadre, di trasferimento di fondi tra due squadre
        /// </summary>
        /// <param name="idGiocatore">Id del giocatore da trasferire: se 0 viene trasferito solo il credito tra le due squadre</param>
        /// <param name="idFantasquadraOrigine">Id della fantasquadra di origine. Se 0 sto trasferendo un giocatore dal mercato libero alla fantasquadra di destinazione</param>
        /// <param name="idFantasquadraDestinazione">Parametro obbligatorio. Un acquisto ha senso solo se questo valore è diverso da 0</param>
        /// <param name="costo">Importo del trasferimento</param>
        /// <param name="idTransazione">Transazione di riferimento del movimento di mercato. Una transazione raggruppa più movimenti di mercato.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Per ora non la implemento perche voglio capire come è meglio fare in caso di errore nella transazione
        /// </summary>
        /// <param name="idAcquisto"></param>
        /// <param name="idGiocatore"></param>
        /// <param name="idFantasquadraOrigine"></param>
        /// <param name="idFantasquadraDestinazione"></param>
        /// <param name="costo"></param>
        /// <param name="idTransazione"></param>
        /// <returns></returns>
        public int updateAcquisto(int idAcquisto, int idGiocatore, int idFantasquadraOrigine, int idFantasquadraDestinazione, int costo, int idTransazione)
        {
            //int ret = 0;
            //using (var connection = new SqlConnection(this.connection))
            //{
            //    var sql = "[susyleague].[mercato_acquisto_update]";
            //    var p = new DynamicParameters();
            //    p.Add("@Id", idAcquisto);

            //    if (idGiocatore != 0)
            //        p.Add("@GiocatoreId", idGiocatore);
            //    if (idFantasquadraOrigine != 0)
            //        p.Add("@SquadraOrigineId", idFantasquadraOrigine);

            //    p.Add("@SquadraDestinazioneId", idFantasquadraDestinazione);
            //    p.Add("@Costo", costo);
            //    p.Add("@MercatoTransazioneId", idTransazione);

            //    p.Add("@ErrorMessage", dbType: DbType.String, direction: ParameterDirection.Output, size: 4000);
            //    p.Add("@return_value", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);

            //    int retVal = connection.Execute(sql, p, commandType: CommandType.StoredProcedure);
            //    if (retVal != -1)
            //        throw new Exception("SP EXECUTION ERROR: " + sql);

            //    int retDB = p.Get<int>("@return_value");
            //    string Message = p.Get<string>("@ErrorMessage");
            //    if (retDB == 0)
            //        ret = p.Get<int>("@Id");
            //    else
            //        throw new SusyLeagueDBException(retDB, Message, sql);
            //}
            //return ret;
            throw new NotImplementedException();
        }
    }
}
