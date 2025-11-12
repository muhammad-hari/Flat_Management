using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBackupExecuteStoredProcedure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // REMOVED: Don't add CreatedAt column (already exists)
            // migrationBuilder.AddColumn<DateTime>(...);

            // Create stored procedure only
            var sql = @"
DROP PROCEDURE IF EXISTS sp_execute_backup;

CREATE PROCEDURE sp_execute_backup(
    IN p_schedule_id INT,
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
    
    SET v_start_time = NOW();
    SET v_file_name = CONCAT('backup_', DATE_FORMAT(NOW(), '%Y%m%d_%H%i%s'), '.sql');
    SET v_full_path = CONCAT(p_backup_path, '/', v_file_name);
    
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
        'Scheduled',
        v_file_name,
        v_full_path,
        0,
        'Pending',
        v_start_time,
        p_user_id,
        NOW()
    );
    
    SET v_backup_id = LAST_INSERT_ID();
END;
";

            migrationBuilder.Sql(sql);
            
            // Note: Event Scheduler will be enabled programmatically in BackupScheduleService
            // This requires SUPER or SYSTEM_VARIABLES_ADMIN privilege
            // If privilege is not available, the system will work without Event Scheduler
            // using the BackgroundService fallback
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // REMOVED: Don't drop CreatedAt column (was not added by this migration)
            // migrationBuilder.DropColumn(...);

            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS sp_execute_backup;");
        }
    }
}
