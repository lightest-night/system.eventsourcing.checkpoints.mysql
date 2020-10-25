using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using LightestNight.System.Data.MySql;
using MySqlConnector;
using Shouldly;
using Xunit;
using MySqlConnection = LightestNight.System.Data.MySql.MySqlConnection;

namespace LightestNight.System.EventSourcing.Checkpoints.MySql.Tests
{
    public class MySqlCheckpointManagerTests
    {
        private readonly MySqlOptions _options;
        private readonly IMySqlConnection _connection;
        private readonly ICheckpointManager _sut;

        private const string CheckpointName = "Test Checkpoint";
        private const long Checkpoint = 100;

        public MySqlCheckpointManagerTests()
        {
            var fixture = new Fixture();
            _options = fixture.Build<MySqlOptions>()
                .Without(o => o.Server)
                .Without(o => o.Port)
                .Without(o => o.UserId)
                .Without(o => o.Password)
                .Without(o => o.Database)
                .Without(o => o.Pooling)
                .Without(o => o.MinimumPoolSize)
                .Without(o => o.MaximumPoolSize)
                .Without(o => o.AllowUserVariables)
                .Do(o =>
                {
                    o.Server = Environment.GetEnvironmentVariable("MYSQL_SERVER") ?? "localhost";
                    o.Port = Convert.ToUInt32(Environment.GetEnvironmentVariable("MYSQL_PORT") ?? "3306");
                    o.UserId = Environment.GetEnvironmentVariable("MYSQL_USERID") ?? "mysql";
                    o.Password = Environment.GetEnvironmentVariable("MYSQL_PASSWORD") ?? "mysql";
                    o.Database = Environment.GetEnvironmentVariable("MYSQL_DATABASE") ?? "mysql";
                    o.Pooling = false;
                    o.MinimumPoolSize = 1;
                    o.MaximumPoolSize = 1;
                    o.AllowUserVariables = true;
                })
                .Create();

            _connection = new MySqlConnection(() => _options);
            _sut = new MySqlCheckpointManager(_connection);
        }

        [Fact, Trait("Category", "Integration")]
        public async Task ShouldCreateSchema()
        {
            // Assert
            await using var connection = _connection.GetConnection();
            await connection.OpenAsync().ConfigureAwait(false);
            await using var command = new MySqlCommand("SHOW TABLES LIKE 'projection_checkpoints'", connection);

            var resultSet = await command.ExecuteScalarAsync();
            resultSet.ShouldNotBeNull();
        }
        
        [Fact, Trait("Category", "Unit")]
        public void ShouldThrowIfCancellationRequestedWhenSettingCheckpoint()
        {
            // Arrange
            using var cancellationSource = new CancellationTokenSource();
            var token = cancellationSource.Token;
            cancellationSource.Cancel();
            
            // Act/Assert
            Should.Throw<TaskCanceledException>(async () =>
                await _sut.SetCheckpoint(CheckpointName, Checkpoint, token).ConfigureAwait(false));
        }

        [Fact, Trait("Category", "Integration")]
        public async Task ShouldSetCheckpoint()
        {
            // Act
            await _sut.SetCheckpoint(CheckpointName, Checkpoint);
            
            // Assert
            await using var connection = _connection.GetConnection();
            await connection.OpenAsync().ConfigureAwait(false);
            await using var command =
                new MySqlCommand(
                    $"SELECT checkpoint FROM {Constants.TableName} WHERE checkpoint_name = '{CheckpointName}'",
                    connection);
            (await command.ExecuteScalarAsync().ConfigureAwait(false) as long? ?? 0).ShouldBe(Checkpoint);
        }
        
        [Fact, Trait("Category", "Unit")]
        public void ShouldThrowIfCancellationRequestedWhenGettingCheckpoint()
        {
            // Arrange
            using var cancellationSource = new CancellationTokenSource();
            var token = cancellationSource.Token;
            cancellationSource.Cancel();
            
            // Act/Assert
            Should.Throw<TaskCanceledException>(async () =>
                await _sut.GetCheckpoint(CheckpointName, token).ConfigureAwait(false));
        }

        [Fact, Trait("Category", "Integration")]
        public async Task ShouldGetCheckpoint()
        {
            // Arrange
            await using var connection = _connection.GetConnection();
            await connection.OpenAsync().ConfigureAwait(false);
            await using var command =
                new MySqlCommand(
                    $"INSERT INTO {Constants.TableName} (checkpoint_name, checkpoint) VALUES ('{CheckpointName}', {Checkpoint}) ON DUPLICATE KEY UPDATE checkpoint = {Checkpoint}",
                    connection);
            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            await connection.CloseAsync().ConfigureAwait(false);
            
            // Act
            var result = await _sut.GetCheckpoint(CheckpointName);
            
            // Assert
            result.ShouldBe(Checkpoint);
        }

        [Fact, Trait("Category", "Unit")]
        public void ShouldThrowIfCancellationRequestedWhenClearingCheckpoint()
        {
            // Arrange
            using var cancellationSource = new CancellationTokenSource();
            var token = cancellationSource.Token;
            cancellationSource.Cancel();
            
            // Act/Assert
            Should.Throw<TaskCanceledException>(async () =>
                await _sut.ClearCheckpoint(CheckpointName, token).ConfigureAwait(false));
        }
        
        [Fact, Trait("Category", "Integration")]
        public async Task ShouldClearCheckpoint()
        {
            // Arrange
            await using var connection = _connection.GetConnection();
            await connection.OpenAsync().ConfigureAwait(false);
            await using (var command =
                new MySqlCommand(
                    $"INSERT INTO {Constants.TableName} (checkpoint_name, checkpoint) VALUES ('{CheckpointName}', {Checkpoint}) ON DUPLICATE KEY UPDATE checkpoint = {Checkpoint}",
                    connection))
                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            await connection.CloseAsync().ConfigureAwait(false);
            
            // Act
            await _sut.ClearCheckpoint(CheckpointName).ConfigureAwait(false);
            
            // Assert
            await connection.OpenAsync().ConfigureAwait(false);
            await using (var command =
                new MySqlCommand(
                    $"SELECT EXISTS(SELECT checkpoint FROM {_options.Database}.{Constants.TableName} WHERE checkpoint_name = '{CheckpointName}')",
                    connection))
                ((long) (await command.ExecuteScalarAsync())).ShouldBe(0);
        }
    }
}