using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditTrailFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_outbox_messages",
                schema: "dbo",
                table: "outbox_messages");

            migrationBuilder.RenameTable(
                name: "outbox_messages",
                schema: "dbo",
                newName: "OutboxMessages",
                newSchema: "dbo");

            migrationBuilder.RenameIndex(
                name: "IX_outbox_messages_ProcessedOnUtc_OccurredOnUtc",
                schema: "dbo",
                table: "OutboxMessages",
                newName: "IX_OutboxMessages_ProcessedOnUtc_OccurredOnUtc");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                schema: "dbo",
                table: "Users",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                schema: "dbo",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                schema: "dbo",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOn",
                schema: "dbo",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                schema: "dbo",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RemoteIpAddress",
                schema: "dbo",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                schema: "dbo",
                table: "Users",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                schema: "dbo",
                table: "TodoItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                schema: "dbo",
                table: "TodoItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOn",
                schema: "dbo",
                table: "TodoItems",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                schema: "dbo",
                table: "TodoItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RemoteIpAddress",
                schema: "dbo",
                table: "TodoItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                schema: "dbo",
                table: "TodoItems",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_OutboxMessages",
                schema: "dbo",
                table: "OutboxMessages",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_OutboxMessages",
                schema: "dbo",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "dbo",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "dbo",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "dbo",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DeletedOn",
                schema: "dbo",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                schema: "dbo",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RemoteIpAddress",
                schema: "dbo",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                schema: "dbo",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "dbo",
                table: "TodoItems");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "dbo",
                table: "TodoItems");

            migrationBuilder.DropColumn(
                name: "DeletedOn",
                schema: "dbo",
                table: "TodoItems");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                schema: "dbo",
                table: "TodoItems");

            migrationBuilder.DropColumn(
                name: "RemoteIpAddress",
                schema: "dbo",
                table: "TodoItems");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                schema: "dbo",
                table: "TodoItems");

            migrationBuilder.RenameTable(
                name: "OutboxMessages",
                schema: "dbo",
                newName: "outbox_messages",
                newSchema: "dbo");

            migrationBuilder.RenameIndex(
                name: "IX_OutboxMessages_ProcessedOnUtc_OccurredOnUtc",
                schema: "dbo",
                table: "outbox_messages",
                newName: "IX_outbox_messages_ProcessedOnUtc_OccurredOnUtc");

            migrationBuilder.AddPrimaryKey(
                name: "PK_outbox_messages",
                schema: "dbo",
                table: "outbox_messages",
                column: "Id");
        }
    }
}
