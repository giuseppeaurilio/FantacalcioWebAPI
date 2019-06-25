using CustomExceptions;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace DBControllers.SerieA
{
    public class RuoloDBController : BaseDBController
    {
        public RuoloDBController(string cs) : base(cs)
        {
        }

        public int InsertRuolo(string descrizione1, string sigla1, string descrizione2, string sigla2)
        {
            int ret = 0;
            using (var connection = new SqlConnection(this.connection))
            {
                var sql = "[seriea].[ruolo_insert]";
                var p = new DynamicParameters();
                p.Add("@Descrizione1", descrizione1);
                p.Add("@Sigla1", sigla1);
                p.Add("@Descrizione2", descrizione1);
                p.Add("@Sigla2", sigla2);

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
    }
}
