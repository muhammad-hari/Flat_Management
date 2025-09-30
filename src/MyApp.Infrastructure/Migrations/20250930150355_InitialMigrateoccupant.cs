using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigrateoccupant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Visitors_Occupants_OccupantId",
                table: "Visitors");

            migrationBuilder.DropForeignKey(
                name: "FK_Visitors_Rooms_RoomId",
                table: "Visitors");

            migrationBuilder.AlterColumn<int>(
                name: "RoomId",
                table: "Visitors",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "OccupantId",
                table: "Visitors",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateTable(
                name: "OccupantHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    OccupantId = table.Column<int>(type: "int", nullable: false),
                    RoomId = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Notes = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PhotoData = table.Column<byte[]>(type: "longblob", nullable: true),
                    DocumentName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DocumentContentType = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DocumentData = table.Column<byte[]>(type: "longblob", nullable: true),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OccupantHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OccupantHistories_Occupants_OccupantId",
                        column: x => x.OccupantId,
                        principalTable: "Occupants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OccupantHistories_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_OccupantHistories_OccupantId",
                table: "OccupantHistories",
                column: "OccupantId");

            migrationBuilder.CreateIndex(
                name: "IX_OccupantHistories_RoomId",
                table: "OccupantHistories",
                column: "RoomId");

            migrationBuilder.AddForeignKey(
                name: "FK_Visitors_Occupants_OccupantId",
                table: "Visitors",
                column: "OccupantId",
                principalTable: "Occupants",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Visitors_Rooms_RoomId",
                table: "Visitors",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Visitors_Occupants_OccupantId",
                table: "Visitors");

            migrationBuilder.DropForeignKey(
                name: "FK_Visitors_Rooms_RoomId",
                table: "Visitors");

            migrationBuilder.DropTable(
                name: "OccupantHistories");

            migrationBuilder.AlterColumn<int>(
                name: "RoomId",
                table: "Visitors",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "OccupantId",
                table: "Visitors",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Visitors_Occupants_OccupantId",
                table: "Visitors",
                column: "OccupantId",
                principalTable: "Occupants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Visitors_Rooms_RoomId",
                table: "Visitors",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
