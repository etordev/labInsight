using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LabInsight.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "analysis_categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ExpectedProcessingHours = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_analysis_categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "graph_data_types",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TechnicalName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_graph_data_types", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "graph_types",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TechnicalName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_graph_types", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "laboratories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_laboratories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "graph_items",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Content = table.Column<string>(type: "text", nullable: true),
                    GraphTypeId = table.Column<int>(type: "integer", nullable: false),
                    GraphDataTypeId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_graph_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_graph_items_graph_data_types_GraphDataTypeId",
                        column: x => x.GraphDataTypeId,
                        principalTable: "graph_data_types",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_graph_items_graph_types_GraphTypeId",
                        column: x => x.GraphTypeId,
                        principalTable: "graph_types",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "lab_analyses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AnalysisNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LaboratoryId = table.Column<int>(type: "integer", nullable: false),
                    AnalysisCategoryId = table.Column<int>(type: "integer", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Priority = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lab_analyses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_lab_analyses_analysis_categories_AnalysisCategoryId",
                        column: x => x.AnalysisCategoryId,
                        principalTable: "analysis_categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_lab_analyses_laboratories_LaboratoryId",
                        column: x => x.LaboratoryId,
                        principalTable: "laboratories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_graph_data_types_TechnicalName",
                table: "graph_data_types",
                column: "TechnicalName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_graph_items_GraphDataTypeId",
                table: "graph_items",
                column: "GraphDataTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_graph_items_GraphTypeId",
                table: "graph_items",
                column: "GraphTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_graph_types_TechnicalName",
                table: "graph_types",
                column: "TechnicalName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_lab_analyses_AnalysisCategoryId",
                table: "lab_analyses",
                column: "AnalysisCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_lab_analyses_AnalysisNumber",
                table: "lab_analyses",
                column: "AnalysisNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_lab_analyses_LaboratoryId",
                table: "lab_analyses",
                column: "LaboratoryId");

            migrationBuilder.CreateIndex(
                name: "IX_lab_analyses_ReceivedAt",
                table: "lab_analyses",
                column: "ReceivedAt");

            migrationBuilder.CreateIndex(
                name: "IX_lab_analyses_Status",
                table: "lab_analyses",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "graph_items");

            migrationBuilder.DropTable(
                name: "lab_analyses");

            migrationBuilder.DropTable(
                name: "graph_data_types");

            migrationBuilder.DropTable(
                name: "graph_types");

            migrationBuilder.DropTable(
                name: "analysis_categories");

            migrationBuilder.DropTable(
                name: "laboratories");
        }
    }
}
