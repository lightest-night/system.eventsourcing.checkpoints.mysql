INSERT INTO __table-name__ (checkpoint_name, checkpoint_name_hash, checkpoint)
VALUES (@CheckpointName, UNHEX(SHA1(@CheckpointName)), @Checkpoint)
ON DUPLICATE KEY UPDATE checkpoint = @Checkpoint;