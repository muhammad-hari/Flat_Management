using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyApp.Infrastructure.Migrations
{
    public partial class FixBackupStoredProcedure : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
    DECLARE v_backup_id INT DEFAULT 0;
    DECLARE v_start_time DATETIME;
    DECLARE v_file_name VARCHAR(500);
    DECLARE v_full_path VARCHAR(1000);
    DECLARE v_database_name VARCHAR(100);
    DECLARE v_error_message TEXT DEFAULT NULL;
    DECLARE v_status VARCHAR(50) DEFAULT 'Success';
    
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        GET DIAGNOSTICS CONDITION 1 v_error_message = MESSAGE_TEXT;
        SET v_status = 'Failed';
        
        IF v_backup_id > 0 THEN
            UPDATE BackupHistories 
            SET Status = 'Failed',
                ErrorMessage = v_error_message,
                CompletedAt = NOW(),
                DurationSeconds = TIMESTAMPDIFF(SECOND, StartedAt, NOW()),
                UpdatedAt = NOW()
            WHERE Id = v_backup_id;
        END IF;
        
        SELECT 
            v_backup_id AS BackupId, 
            'Failed' AS Status, 
            COALESCE(v_file_name, 'error.sql') AS FileName,
            COALESCE(v_full_path, '') AS FilePath,
            v_error_message AS ErrorMessage;
    END;
    
    SET v_start_time = NOW();
    SET v_file_name = CONCAT('backup_', DATE_FORMAT(NOW(), '%Y%m%d_%H%i%s'), '.sql');
    SET v_full_path = CONCAT(p_backup_path, '/', v_file_name);
    SET v_database_name = DATABASE();
    
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
    
    SELECT 
        v_backup_id AS BackupId, 
        v_status AS Status, 
        v_file_name AS FileName,
        v_full_path AS FilePath,
        v_error_message AS ErrorMessage;
END;
";

            migrationBuilder.Sql(spCreateBackup);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS sp_create_backup;");
        }
    }
}
