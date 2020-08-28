using System;
using System.Threading;
using System.Threading.Tasks;
using LightestNight.System.Data.MySql;
using Microsoft.Extensions.Logging;
using MySqlConnector;

namespace LightestNight.System.EventSourcing.Checkpoints.MySql
{
    public class MySqlCheckpointManager : ICheckpointManager
    {
        private readonly Func<MySqlConnector.MySqlConnection> _createConnection;
        private readonly Scripts.Scripts _scripts;
        private readonly ILogger<MySqlCheckpointManager> _logger;

        public MySqlCheckpointManager(IMySqlConnection connection, ILogger<MySqlCheckpointManager> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _scripts = new Scripts.Scripts();

            _createConnection = connection.Build;

            CreateSchemaIfNotExists();
        }

        public async Task SetCheckpoint(string checkpointName, long checkpoint,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await using var connection = _createConnection();
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

            await using var command = new MySqlCommand(_scripts.SetCheckpoint, connection);
            command.Parameters.AddWithValue("@CheckpointName", checkpointName);
            command.Parameters.AddWithValue("@Checkpoint", checkpoint);
            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        private void CreateSchemaIfNotExists()
        {
            using var connection = _createConnection();
            connection.Open();

            using var command = new MySqlCommand(_scripts.CreateSchema, connection);
            command.ExecuteNonQuery();
        }
    }
}