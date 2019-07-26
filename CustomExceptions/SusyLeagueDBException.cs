using System;

namespace CustomExceptions
{
    public class SusyLeagueDBException : Exception
    {
        public int Code { get; private set; }
        public string CustomMessage { get; private set; }

        public string StoredProcedure { get; private set; }

        public object[] Parameters { get; private set; }

        public SusyLeagueDBException(int code, string message, string storedProcedure, object[] param = null)
        {
            this.Code = code;
            this.CustomMessage = message;
            this.StoredProcedure = storedProcedure;
            this.Parameters = param;

        }
    }
}
