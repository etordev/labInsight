using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using LabInsight.Api.Data;

#nullable disable

namespace LabInsight.Api.Migrations
{
    [DbContext(typeof(LabInsightDbContext))]
    [Migration("20260830165000_AddIsDeletedAndCreatedAt")]
    public partial class AddIsDeletedAndCreatedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            AddAuditColumns(migrationBuilder, "analysis_categories");
            AddAuditColumns(migrationBuilder, "graph_data_types");
            AddAuditColumns(migrationBuilder, "graph_items");
            AddAuditColumns(migrationBuilder, "graph_types");
            AddAuditColumns(migrationBuilder, "lab_analyses");
            AddAuditColumns(migrationBuilder, "laboratories");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            DropAuditColumns(migrationBuilder, "analysis_categories");
            DropAuditColumns(migrationBuilder, "graph_data_types");
            DropAuditColumns(migrationBuilder, "graph_items");
            DropAuditColumns(migrationBuilder, "graph_types");
            DropAuditColumns(migrationBuilder, "lab_analyses");
            DropAuditColumns(migrationBuilder, "laboratories");
        }

        private static void AddAuditColumns(MigrationBuilder migrationBuilder, string table)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: table,
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: table,
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: $"IX_{table}_IsDeleted",
                table: table,
                column: "IsDeleted");
        }

        private static void DropAuditColumns(MigrationBuilder migrationBuilder, string table)
        {
            migrationBuilder.DropIndex(name: $"IX_{table}_IsDeleted", table: table);
            migrationBuilder.DropColumn(name: "CreatedAt", table: table);
            migrationBuilder.DropColumn(name: "IsDeleted", table: table);
        }
    }
}
