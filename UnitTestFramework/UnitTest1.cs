using System;
using Controllers.SerieA;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestFramework
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void CreaStagione()
        {
            StagioneController sc = new StagioneController();
            int idStagione = 0;
            idStagione = sc.StagioneInsert(DataMock.Data.GetString(8, false, true, false, false));

            Assert.AreNotEqual(0, idStagione);
        }
    }
}
