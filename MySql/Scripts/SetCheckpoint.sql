INSERT INTO projection_checkpoints (checkpoint_name, checkpoint)
VALUES (@CheckpointName, @Checkpoint)
ON DUPLICATE KEY UPDATE checkpoint = @Checkpoint;