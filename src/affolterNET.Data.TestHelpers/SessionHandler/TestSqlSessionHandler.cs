using System.Data;
using affolterNET.Data.Interfaces;
using affolterNET.Data.Interfaces.SessionHandler;
using affolterNET.Data.SessionHandler;

namespace affolterNET.Data.TestHelpers.SessionHandler
{
    public class TestSqlSessionHandler : SqlSessionHandlerBase
    {
        private IDbConnection _cnn;
        private IDbTransaction? _transaction;

        public TestSqlSessionHandler(IDbConnection cnn, IDbTransaction? transaction)
        {
            _cnn = cnn;
            _transaction = transaction;
        }

        protected override ISqlSession CreateSession()
        {
            // always return the same test-session (which does no commit or rollback) for testing
            if (Session == null)
            {
                Session = new TestSqlSession(_cnn, _transaction);
            }
        
            return Session;
        }

        protected override void SaveHistory<TResult>(IQuery<TResult> query)
        {
            // do not save
        }
    }
}