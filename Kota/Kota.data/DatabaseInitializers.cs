using System.Data;
using System.IO;
using System.Data.OleDb;

namespace Kota.Data
{
    public interface IDatabaseInitializer
    {
        Task InitializeDatabaseAsync(string filePath);
        bool DatabaseExists(string filePath);
    }

    public class AccessDatabaseInitializer : IDatabaseInitializer
    {
        private readonly IConnectionFactory _connectionFactory;

        public AccessDatabaseInitializer(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public bool DatabaseExists(string filePath)
        {
            return File.Exists(filePath);
        }

        public async Task InitializeDatabaseAsync(string filePath)
        {
            // Ensure directory exists
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Create empty Access database if it doesn't exist
            if (!File.Exists(filePath))
            {
                // Create empty database using ADOX
                var connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={filePath};";

                dynamic catalog = Activator.CreateInstance(Type.GetTypeFromProgID("ADOX.Catalog")!)!;
                try
                {
                    catalog.Create(connectionString);
                }
                finally
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(catalog);
                }
            }

            // Set connection factory path
            _connectionFactory.SetDatabasePath(filePath);

            // Always try to create tables (will skip if they exist)
            CreateTables();

            // Create default user only after tables exist
            CreateDefaultUser();
        }

        private void CreateTables()
        {
            var tables = new[]
            {
                @"CREATE TABLE sites (
                    id AUTONUMBER PRIMARY KEY,
                    code TEXT(32) NOT NULL,
                    name TEXT(255) NOT NULL
                )",

                @"CREATE TABLE buildings (
                    id AUTONUMBER PRIMARY KEY,
                    site_id LONG NOT NULL,
                    code TEXT(32) NOT NULL,
                    name TEXT(255) NOT NULL
                )",

                @"CREATE TABLE rooms (
                    id AUTONUMBER PRIMARY KEY,
                    building_id LONG NOT NULL,
                    code TEXT(32) NOT NULL,
                    name TEXT(255) NOT NULL
                )",

                @"CREATE TABLE areas (
                    id AUTONUMBER PRIMARY KEY,
                    room_id LONG NOT NULL,
                    code TEXT(32) NOT NULL,
                    name TEXT(255) NOT NULL
                )",

                @"CREATE TABLE storage_units (
                    id AUTONUMBER PRIMARY KEY,
                    room_id LONG NOT NULL,
                    area_id LONG,
                    code TEXT(32) NOT NULL,
                    kind TEXT(16) NOT NULL,
                    type TEXT(32) NOT NULL
                )",

                @"CREATE TABLE bins (
                    id AUTONUMBER PRIMARY KEY,
                    storage_unit_id LONG NOT NULL,
                    code TEXT(64) NOT NULL,
                    kind TEXT(16) NOT NULL
                )",

                @"CREATE TABLE items (
                    id AUTONUMBER PRIMARY KEY,
                    description TEXT(255) NOT NULL,
                    manufacturer_sku TEXT(64),
                    qty_on_hand DOUBLE DEFAULT 0,
                    min_qty DOUBLE DEFAULT 0,
                    bin_id LONG NOT NULL,
                    notes MEMO,
                    created_at DATETIME,
                    updated_at DATETIME,
                    row_version LONG DEFAULT 0
                )",

                @"CREATE TABLE stock_transactions (
                    id AUTONUMBER PRIMARY KEY,
                    ts DATETIME NOT NULL,
                    user_id LONG NOT NULL,
                    item_id LONG NOT NULL,
                    qty_delta DOUBLE NOT NULL,
                    reason_code TEXT(32) NOT NULL,
                    note MEMO
                )",

                @"CREATE TABLE audit_log (
                    id AUTONUMBER PRIMARY KEY,
                    ts DATETIME NOT NULL,
                    user_id LONG NOT NULL,
                    entity_type TEXT(32) NOT NULL,
                    entity_id LONG NOT NULL,
                    action TEXT(32) NOT NULL,
                    before_json MEMO,
                    after_json MEMO
                )",

                @"CREATE TABLE users (
                    id AUTONUMBER PRIMARY KEY,
                    username TEXT(64) NOT NULL UNIQUE,
                    password_hash TEXT(255) NOT NULL,
                    role TEXT(16) NOT NULL DEFAULT 'user',
                    is_active YESNO DEFAULT TRUE,
                    created_at DATETIME
                )",

                @"CREATE TABLE settings (
                    key TEXT(64) PRIMARY KEY,
                    value MEMO
                )"
            };

            using var connection = (OleDbConnection)_connectionFactory.CreateConnection();
            connection.Open();

            foreach (var tableScript in tables)
            {
                using var command = new OleDbCommand(tableScript, connection);

                try
                {
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    // Table might already exist, continue
                    Console.WriteLine($"Warning creating table: {ex.Message}");
                }
            }

            // Create indexes and constraints
            var constraints = new[]
            {
                "CREATE UNIQUE INDEX idx_sites_code ON sites (code)",
                "CREATE UNIQUE INDEX idx_buildings_site_code ON buildings (site_id, code)",
                "CREATE UNIQUE INDEX idx_rooms_building_code ON rooms (building_id, code)",
                "CREATE UNIQUE INDEX idx_areas_room_code ON areas (room_id, code)",
                "CREATE UNIQUE INDEX idx_storage_units_room_code ON storage_units (room_id, code)",
                "CREATE UNIQUE INDEX idx_bins_storage_unit_code ON bins (storage_unit_id, code)"
            };

            foreach (var constraint in constraints)
            {
                using var command = new OleDbCommand(constraint, connection);

                try
                {
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    // Index might already exist, continue
                    Console.WriteLine($"Warning creating index: {ex.Message}");
                }
            }
        }

        private void CreateDefaultUser()
        {
            try
            {
                using var connection = (OleDbConnection)_connectionFactory.CreateConnection();
                connection.Open();

                // Check if admin user exists
                using var checkCommand = new OleDbCommand("SELECT COUNT(*) FROM users WHERE username = 'admin'", connection);
                var count = 0;

                try
                {
                    var result = checkCommand.ExecuteScalar();
                    count = result != null ? Convert.ToInt32(result) : 0;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error checking for admin user: {ex.Message}");
                    return;
                }

                if (count == 0)
                {
                    // Create default admin user
                    var passwordHash = BCrypt.Net.BCrypt.HashPassword("ChangeMe!123");

                    using var command = new OleDbCommand(
                        @"INSERT INTO users (username, password_hash, role, is_active, created_at) 
                          VALUES (?, ?, 'admin', TRUE, ?)", connection);

                    command.Parameters.AddWithValue("@username", "admin");
                    command.Parameters.AddWithValue("@password", passwordHash);
                    command.Parameters.AddWithValue("@created", DateTime.Now);

                    command.ExecuteNonQuery();
                    Console.WriteLine("Default admin user created successfully");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating default user: {ex.Message}");
                // Don't throw - this isn't critical for app startup
            }
        }
    }
}