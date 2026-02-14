using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "exchange_system");

            migrationBuilder.CreateTable(
                name: "exchange",
                schema: "exchange_system",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValue: new DateTime(2026, 2, 14, 14, 38, 7, 784, DateTimeKind.Utc).AddTicks(552))
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exchange", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tick",
                schema: "exchange_system",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    exchange_id = table.Column<int>(type: "integer", nullable: false),
                    source = table.Column<string>(type: "text", nullable: false),
                    raw_json = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValue: new DateTime(2026, 2, 14, 14, 38, 7, 785, DateTimeKind.Utc).AddTicks(8769))
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tick", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tick_exchange_exchange_id",
                        column: x => x.exchange_id,
                        principalSchema: "exchange_system",
                        principalTable: "exchange",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "exchange_system",
                table: "exchange",
                columns: new[] { "Id", "Title" },
                values: new object[,]
                {
                    { 1, "Binance" },
                    { 2, "Bittrex" }
                });

            migrationBuilder.CreateIndex(
                name: "idx_raw_tick_client_id",
                schema: "exchange_system",
                table: "tick",
                column: "exchange_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tick",
                schema: "exchange_system");

            migrationBuilder.DropTable(
                name: "exchange",
                schema: "exchange_system");
        }
    }
}
