INSERT INTO __table-name__ (checkpoint_name, checkpoint)
VALUES (@CheckpointName, @Checkpoint)
ON DUPLICATE KEY UPDATE checkpoint = @Checkpoint;