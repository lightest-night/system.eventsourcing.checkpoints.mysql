CREATE TABLE IF NOT EXISTS __table-name__ (
    id                  BIGINT          NOT NULL    PRIMARY KEY AUTO_INCREMENT,
    checkpoint_name     VARCHAR(500)    NOT NULL,
    checkpoint          BIGINT          NULL,
    CONSTRAINT uq_projection_checkpoints_checkpoint_name UNIQUE (checkpoint_name)
) ENGINE = InnoDB
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;
  
SET @x := (SELECT COUNT(*) FROM information_schema.statistics WHERE table_name = '__table-name__' and index_name = 'ix___table-name___checkpoint_name' and table_schema = database());
SET @sql := IF( @x > 0, 'SELECT ''Index exists.''', 'ALTER TABLE __table-name__ ADD INDEX ix___table-name___checkpoint_name (checkpoint_name);');
PREPARE stmt FROM @sql;
EXECUTE stmt;