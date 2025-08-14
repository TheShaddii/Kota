// Kota.Data/ConnectionFactory.cs
using System.Data;
using System.Data.OleDb;

namespace Kota.Data
{
    public interface IConnectionFactory
    {
        IDbConnection CreateConnection();
        string ConnectionString { get; }
        void SetDatabasePath(string path);
    }

    public class AccessConnectionFactory : IConnectionFactory
    {
        private string _connectionString = string.Empty;

        public string ConnectionString => _connectionString;

        public void SetDatabasePath(string path)
        {
            _connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={path};";
        }

        public IDbConnection CreateConnection()
        {
            if (string.IsNullOrEmpty(_connectionString))
                throw new InvalidOperationException("Database path not set. Call SetDatabasePath first.");

            return new OleDbConnection(_connectionString);
        }
    }
}