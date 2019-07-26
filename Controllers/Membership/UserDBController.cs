using CustomExceptions;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace DBControllers.Membership
{
    public class UserDBController : BaseDBController
    {
        public UserDBController(string cs) : base(cs)
        {
        }

        public int InsertUser(string username)
        {
            int ret = 0;
            using (var connection = new SqlConnection(this.connection))
            {
                var sql = "[membership].[user_insert]";
                var p = new DynamicParameters();
                p.Add("@Username", username);
                //p.Add("@Password", password);

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

        //public void SetPasswordTemporaneaUser(int id, string password)
        //{

        //    using (var connection = new SqlConnection(this.connection))
        //    {
        //        var sql = "[membership].[user_password_settemporary]";
        //        var p = new DynamicParameters();
        //        p.Add("@Id", id);

        //        p.Add("@Password", password);

        //        p.Add("@ErrorMessage", dbType: DbType.String, direction: ParameterDirection.Output, size: 4000);
        //        p.Add("@return_value", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);


        //        int retVal = connection.Execute(sql, p, commandType: CommandType.StoredProcedure);
        //        if (retVal != -1)
        //            throw new Exception("SP EXECUTION ERROR: " + sql);

        //        int retDB = p.Get<int>("@return_value");
        //        string Message = p.Get<string>("@ErrorMessage");
        //        if (retDB != 0)
        //            throw new SusyLeagueDBException(retDB, Message, sql);
        //    }

        //}

        //public void UpdatePasswordUser(int id, string password)
        //{

        //    using (var connection = new SqlConnection(this.connection))
        //    {
        //        var sql = "[membership].[user_password_update]";
        //        var p = new DynamicParameters();
        //        p.Add("@Id", id);

        //        p.Add("@Password", password);

        //        p.Add("@ErrorMessage", dbType: DbType.String, direction: ParameterDirection.Output, size: 4000);
        //        p.Add("@return_value", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);


        //        int retVal = connection.Execute(sql, p, commandType: CommandType.StoredProcedure);
        //        if (retVal != -1)
        //            throw new Exception("SP EXECUTION ERROR: " + sql);

        //        int retDB = p.Get<int>("@return_value");
        //        string Message = p.Get<string>("@ErrorMessage");
        //        if (retDB != 0)
        //            throw new SusyLeagueDBException(retDB, Message, sql);
        //    }

        //}
    }
}
