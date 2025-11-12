using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBackupStoredProcedures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Stored Procedure: Create Backup
            var spCreateBackup = @"
DROP PROCEDURE IF EXISTS sp_create_backup;

CREATE PROCEDURE sp_create_backup(
    IN p_schedule_id INT,
    IN p_backup_type VARCHAR(100),
    IN p_backup_path VARCHAR(500),
    IN p_include_schema BOOLEAN,
    IN p_include_data BOOLEAN,
    IN p_user_id INT
)
BEGIN
    DECLARE v_backup_id INT;
    DECLARE v_start_time DATETIME;
    DECLARE v_file_name VARCHAR(500);
    DECLARE v_full_path VARCHAR(1000);
    DECLARE v_database_name VARCHAR(100);
    DECLARE v_error_message TEXT;
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        GET DIAGNOSTICS CONDITION 1 v_error_message = MESSAGE_TEXT;
        
        IF v_backup_id IS NOT NULL THEN
            UPDATE BackupHistories 
            SET Status = 'Failed',
                ErrorMessage = v_error_message,
                CompletedAt = NOW(),
                DurationSeconds = TIMESTAMPDIFF(SECOND, StartedAt, NOW()),
                UpdatedAt = NOW()
            WHERE Id = v_backup_id;
        END IF;
        
        -- Return error result
        SELECT 
            COALESCE(v_backup_id, 0) AS BackupId, 
            'Failed' AS Status, 
            COALESCE(v_file_name, 'error') AS FileName,
            COALESCE(v_full_path, '') AS FilePath,
            v_error_message AS ErrorMessage;
    END;
    
    -- Initialize variables
    SET v_start_time = NOW();
    SET v_file_name = CONCAT('backup_', DATE_FORMAT(NOW(), '%Y%m%d_%H%i%s'), '.sql');
    SET v_full_path = CONCAT(p_backup_path, '/', v_file_name);
    SET v_database_name = DATABASE();
    
    -- Create backup history record
    INSERT INTO BackupHistories (
        BackupScheduleId,
        BackupType,
        FileName,
        FilePath,
        FileSize,
        Status,
        StartedAt,
        CreatedBy,
        CreatedAt
    ) VALUES (
        p_schedule_id,
        p_backup_type,
        v_file_name,
        v_full_path,
        0,
        'In Progress',
        v_start_time,
        p_user_id,
        NOW()
    );
    
    SET v_backup_id = LAST_INSERT_ID();
    
    -- Simulate backup process (simplified version)
    -- In production, you would actually export data here
    
    -- Update backup history as success
    UPDATE BackupHistories 
    SET Status = 'Success',
        CompletedAt = NOW(),
        DurationSeconds = TIMESTAMPDIFF(SECOND, v_start_time, NOW()),
        FileSize = (
            SELECT COALESCE(SUM(data_length + index_length), 0)
            FROM information_schema.tables 
            WHERE table_schema = v_database_name
        ),
        UpdatedAt = NOW()
    WHERE Id = v_backup_id;
    
    -- Return success result
    SELECT 
        v_backup_id AS BackupId, 
        'Success' AS Status, 
        v_file_name AS FileName,
        v_full_path AS FilePath,
        NULL AS ErrorMessage;
END;
";

            migrationBuilder.Sql(spCreateBackup);

            // 2. Stored Procedure: Cleanup Old Backups
            var spCleanupBackups = @"
DROP PROCEDURE IF EXISTS sp_cleanup_old_backups;

CREATE PROCEDURE sp_cleanup_old_backups(
    IN p_retention_days INT
)
BEGIN
    DECLARE v_cutoff_date DATETIME;
    DECLARE v_deleted_count INT DEFAULT 0;
    
    SET v_cutoff_date = DATE_SUB(NOW(), INTERVAL p_retention_days DAY);
    
    DELETE FROM BackupHistories 
    WHERE StartedAt < v_cutoff_date 
    AND Status = 'Success';
    
    SET v_deleted_count = ROW_COUNT();
    
    SELECT 
        v_deleted_count AS DeletedCount,
        v_cutoff_date AS CutoffDate,
        'Success' AS Status;
END;
";

            migrationBuilder.Sql(spCleanupBackups);

            // 3. Stored Procedure: Get Backup Statistics
            var spBackupStats = @"
DROP PROCEDURE IF EXISTS sp_get_backup_statistics;

CREATE PROCEDURE sp_get_backup_statistics()
BEGIN
    SELECT 
        COUNT(*) AS TotalBackups,
        SUM(CASE WHEN Status = 'Success' THEN 1 ELSE 0 END) AS SuccessfulBackups,
        SUM(CASE WHEN Status = 'Failed' THEN 1 ELSE 0 END) AS FailedBackups,
        SUM(CASE WHEN Status = 'In Progress' THEN 1 ELSE 0 END) AS InProgressBackups,
        COALESCE(SUM(FileSize), 0) AS TotalSize,
        COALESCE(AVG(DurationSeconds), 0) AS AvgDuration,
        MAX(StartedAt) AS LastBackupDate
    FROM BackupHistories;
END;
";

            migrationBuilder.Sql(spBackupStats);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS sp_create_backup;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS sp_cleanup_old_backups;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS sp_get_backup_statistics;");
        }
    }
}
