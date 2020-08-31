# Lightest Night
## Event Sourcing > Checkpoints > MySql

The elements required to manage a Stream checkpoint inside a MySql data store

### Build Status
![](https://github.com/lightest-night/system.eventsourcing.checkpoints.mysql/workflows/CI/badge.svg)
![](https://github.com/lightest-night/system.eventsourcing.checkpoints.mysql/workflows/Release/badge.svg)

#### How To Use
##### Registration
* Asp.Net Standard/Core Dependency Injection
  * Use the provided `services.AddMySqlCheckpointManagement(Action<MySqlOptions>? options = null)` method

##### Usage
* `Task SetCheckpoint(string checkpointName, long checkpoint, CancellationToken cancellationToken = default)`
  * An asynchronous function to call when setting the checkpoint
* `Task<long?> GetCheckpoint(string checkpointName, CancellationToken cancellationToken = default)`
  * An asynchronous function to call when getting the checkpoint
* `Task ClearCheckpoint(string checkpointName, CancellationToken cancellationToken = default)`
  * An asynchronous function to call when clearing the checkpoint