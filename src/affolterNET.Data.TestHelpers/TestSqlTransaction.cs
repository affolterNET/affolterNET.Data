using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace affolterNET.Data.TestHelpers
{
    public class TestSqlTransaction : DbTransaction
    {
        public TestSqlTransaction(SqlTransaction transaction) { }

        protected override DbConnection DbConnection { get; }

        public override IsolationLevel IsolationLevel { get; }

        public override void Commit()
        {
            throw new NotImplementedException();
        }

        public override void Rollback()
        {
            throw new NotImplementedException();
        }
    }
}