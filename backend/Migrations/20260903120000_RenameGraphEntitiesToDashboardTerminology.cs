using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using LabInsight.Api.Data;

#nullable disable

namespace LabInsight.Api.Migrations
{
    [DbContext(typeof(LabInsightDbContext))]
    [Migration("20260903120000_RenameGraphEntitiesToDashboardTerminology")]
    public partial class RenameGraphEntitiesToDashboardTerminology : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_graph_items_graph_data_types_GraphDataTypeId",
                table: "graph_items");

            migrationBuilder.DropForeignKey(
                name: "FK_graph_items_graph_types_GraphTypeId",
                table: "graph_items");

            migrationBuilder.RenameTable(
                name: "graph_types",
                newName: "visualization_types");

            migrationBuilder.RenameTable(
                name: "graph_data_types",
                newName: "metric_definitions");

            migrationBuilder.RenameTable(
                name: "graph_items",
                newName: "dashboard_widgets");

            migrationBuilder.RenameColumn(
                name: "GraphTypeId",
                table: "dashboard_widgets",
                newName: "VisualizationTypeId");

            migrationBuilder.RenameColumn(
                name: "GraphDataTypeId",
                table: "dashboard_widgets",
                newName: "MetricDefinitionId");

            migrationBuilder.RenameIndex(
                name: "IX_graph_types_TechnicalName",
                table: "visualization_types",
                newName: "IX_visualization_types_TechnicalName");

            migrationBuilder.RenameIndex(
                name: "IX_graph_types_IsDeleted",
                table: "visualization_types",
                newName: "IX_visualization_types_IsDeleted");

            migrationBuilder.RenameIndex(
                name: "IX_graph_data_types_TechnicalName",
                table: "metric_definitions",
                newName: "IX_metric_definitions_TechnicalName");

            migrationBuilder.RenameIndex(
                name: "IX_graph_data_types_IsDeleted",
                table: "metric_definitions",
                newName: "IX_metric_definitions_IsDeleted");

            migrationBuilder.RenameIndex(
                name: "IX_graph_items_GraphTypeId",
                table: "dashboard_widgets",
                newName: "IX_dashboard_widgets_VisualizationTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_graph_items_GraphDataTypeId",
                table: "dashboard_widgets",
                newName: "IX_dashboard_widgets_MetricDefinitionId");

            migrationBuilder.RenameIndex(
                name: "IX_graph_items_Ordering",
                table: "dashboard_widgets",
                newName: "IX_dashboard_widgets_Ordering");

            migrationBuilder.RenameIndex(
                name: "IX_graph_items_IsDeleted",
                table: "dashboard_widgets",
                newName: "IX_dashboard_widgets_IsDeleted");

            migrationBuilder.Sql(
                """ALTER TABLE visualization_types RENAME CONSTRAINT "PK_graph_types" TO "PK_visualization_types";""");
            migrationBuilder.Sql(
                """ALTER TABLE metric_definitions RENAME CONSTRAINT "PK_graph_data_types" TO "PK_metric_definitions";""");
            migrationBuilder.Sql(
                """ALTER TABLE dashboard_widgets RENAME CONSTRAINT "PK_graph_items" TO "PK_dashboard_widgets";""");

            migrationBuilder.AddForeignKey(
                name: "FK_dashboard_widgets_metric_definitions_MetricDefinitionId",
                table: "dashboard_widgets",
                column: "MetricDefinitionId",
                principalTable: "metric_definitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_dashboard_widgets_visualization_types_VisualizationTypeId",
                table: "dashboard_widgets",
                column: "VisualizationTypeId",
                principalTable: "visualization_types",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_dashboard_widgets_metric_definitions_MetricDefinitionId",
                table: "dashboard_widgets");

            migrationBuilder.DropForeignKey(
                name: "FK_dashboard_widgets_visualization_types_VisualizationTypeId",
                table: "dashboard_widgets");

            migrationBuilder.Sql(
                """ALTER TABLE dashboard_widgets RENAME CONSTRAINT "PK_dashboard_widgets" TO "PK_graph_items";""");
            migrationBuilder.Sql(
                """ALTER TABLE metric_definitions RENAME CONSTRAINT "PK_metric_definitions" TO "PK_graph_data_types";""");
            migrationBuilder.Sql(
                """ALTER TABLE visualization_types RENAME CONSTRAINT "PK_visualization_types" TO "PK_graph_types";""");

            migrationBuilder.RenameIndex(
                name: "IX_dashboard_widgets_IsDeleted",
                table: "dashboard_widgets",
                newName: "IX_graph_items_IsDeleted");

            migrationBuilder.RenameIndex(
                name: "IX_dashboard_widgets_Ordering",
                table: "dashboard_widgets",
                newName: "IX_graph_items_Ordering");

            migrationBuilder.RenameIndex(
                name: "IX_dashboard_widgets_MetricDefinitionId",
                table: "dashboard_widgets",
                newName: "IX_graph_items_GraphDataTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_dashboard_widgets_VisualizationTypeId",
                table: "dashboard_widgets",
                newName: "IX_graph_items_GraphTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_metric_definitions_IsDeleted",
                table: "metric_definitions",
                newName: "IX_graph_data_types_IsDeleted");

            migrationBuilder.RenameIndex(
                name: "IX_metric_definitions_TechnicalName",
                table: "metric_definitions",
                newName: "IX_graph_data_types_TechnicalName");

            migrationBuilder.RenameIndex(
                name: "IX_visualization_types_IsDeleted",
                table: "visualization_types",
                newName: "IX_graph_types_IsDeleted");

            migrationBuilder.RenameIndex(
                name: "IX_visualization_types_TechnicalName",
                table: "visualization_types",
                newName: "IX_graph_types_TechnicalName");

            migrationBuilder.RenameColumn(
                name: "MetricDefinitionId",
                table: "dashboard_widgets",
                newName: "GraphDataTypeId");

            migrationBuilder.RenameColumn(
                name: "VisualizationTypeId",
                table: "dashboard_widgets",
                newName: "GraphTypeId");

            migrationBuilder.RenameTable(
                name: "dashboard_widgets",
                newName: "graph_items");

            migrationBuilder.RenameTable(
                name: "metric_definitions",
                newName: "graph_data_types");

            migrationBuilder.RenameTable(
                name: "visualization_types",
                newName: "graph_types");

            migrationBuilder.AddForeignKey(
                name: "FK_graph_items_graph_data_types_GraphDataTypeId",
                table: "graph_items",
                column: "GraphDataTypeId",
                principalTable: "graph_data_types",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_graph_items_graph_types_GraphTypeId",
                table: "graph_items",
                column: "GraphTypeId",
                principalTable: "graph_types",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
