using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using LabInsight.Api.Data;

#nullable disable

namespace LabInsight.Api.Migrations
{
    [DbContext(typeof(LabInsightDbContext))]
    [Migration("20260831120000_AddGraphItemOrdering")]
    public partial class AddGraphItemOrdering : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Ordering",
                table: "graph_items",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(
                """
                UPDATE graph_items AS g
                SET "Ordering" = s.rn
                FROM (
                    SELECT "Id", ROW_NUMBER() OVER (ORDER BY "Id") AS rn
                    FROM graph_items
                ) AS s
                WHERE g."Id" = s."Id";
                """);

            migrationBuilder.CreateIndex(
                name: "IX_graph_items_Ordering",
                table: "graph_items",
                column: "Ordering");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_graph_items_Ordering",
                table: "graph_items");

            migrationBuilder.DropColumn(
                name: "Ordering",
                table: "graph_items");
        }
    }
}
