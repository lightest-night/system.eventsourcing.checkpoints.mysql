CREATE TABLE IF NOT EXISTS projection_checkpoints (
    id                  BIGINT          NOT NULL    AUTO_INCREMENT,
    checkpoint_name     VARCHAR(500)    NOT NULL,
    checkpoint          BIGINT          NULL
    CONSTRAINT pk_projection_checkpoints PRIMARY KEY (id),
    CONSTRAINT uq_projection_checkpoints_checkpoint_name UNIQUE (checkpoint_name),
) ENGINE = InnoDB
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;
  
CREATE INDEX IF NOT EXISTS ix_projection_checkpoints_checkpoint_name
    ON projection_checkpoints (checkpoint_name);