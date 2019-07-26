namespace DBControllers.SerieA
{
    public class StagioneDBControllerGet : BaseDBController
    {
        public StagioneDBControllerGet(string cs) : base(cs)
        {
        }

        //public List<Stagione> GetAll()
        //{
        //    List<Stagione> ret = new List<Stagione>();
        //    using (var connection = new SqlConnection(this.connection))
        //    {

        //        var sql = "[seriea].[stagione_all_get]";
        //        SqlCommand cmd = new SqlCommand(sql, connection);
        //        SqlDataAdapter da = new SqlDataAdapter(cmd);
        //        DataTable dt = new DataTable();
        //        da.Fill(dt);

        //        foreach (DataRow dr in dt.Rows)
        //        {
        //            ret.Add(new Stagione(dr));
        //        }
        //    }
        //    return ret;
        //}
    }
}
