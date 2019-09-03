using Microsoft.Data.SqlClient;
using System.Data.Common;

namespace MemeTV.BusinessLogic
{
    public class SqlConnectionProvider : ISqlConnectionProvider
    {
        private readonly IDbConnectionStringProvider connectionStringProvider;
        public SqlConnectionProvider(IDbConnectionStringProvider connectionStringProvider)
        {
            this.connectionStringProvider = connectionStringProvider;
        }

        public DbConnection Get()
        {
            return new SqlConnection(connectionStringProvider.Get());
        }
    }
}