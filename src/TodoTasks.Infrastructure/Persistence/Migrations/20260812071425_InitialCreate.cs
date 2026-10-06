using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TodoTasks.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "todo_tasks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    priority = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    scheduled_date = table.Column<DateOnly>(type: "date", nullable: false),
                    assignee = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    completed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_todo_tasks", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_todo_tasks_scheduled_date",
                table: "todo_tasks",
                column: "scheduled_date");

            migrationBuilder.CreateIndex(
                name: "ix_todo_tasks_status",
                table: "todo_tasks",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "todo_tasks");
        }
    }
}
