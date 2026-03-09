using System.Data;
using Npgsql;

namespace affolterNET.Data.TestHelpers
{
    public abstract class PgDbFixture : DbFixture
    {
        protected PgDbFixture(string connStringKey = "CONNSTRING_PG", string? userSecretsId = null)
            : base(connStringKey, userSecretsId)
        {
        }

        protected override IDbConnection CreateConnection(string connString)
        {
            return new NpgsqlConnection(connString);
        }
    }
}
