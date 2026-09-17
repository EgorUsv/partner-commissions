using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PartnerCommissions.Commissions.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCommissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OperationId = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerExternalId = table.Column<Guid>(type: "uuid", nullable: false),
                    Profit = table.Column<decimal>(type: "numeric(18,8)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "schema_settings",
                columns: table => new
                {
                    SchemaType = table.Column<string>(type: "text", nullable: false),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_schema_settings", x => x.SchemaType);
                });

            migrationBuilder.CreateTable(
                name: "commissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartnerExternalId = table.Column<Guid>(type: "uuid", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,8)", nullable: false),
                    SchemaType = table.Column<string>(type: "text", nullable: false),
                    Paid = table.Column<bool>(type: "boolean", nullable: false),
                    PaidAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_commissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_commissions_events_EventId",
                        column: x => x.EventId,
                        principalTable: "events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "schema_settings",
                columns: new[] { "SchemaType", "Enabled" },
                values: new object[,]
                {
                    { "Linear", true },
                    { "Fibonacci", false }
                });

            migrationBuilder.CreateIndex(
                name: "IX_schema_settings_Enabled",
                table: "schema_settings",
                column: "Enabled",
                unique: true,
                filter: "\"Enabled\"");

            migrationBuilder.CreateIndex(
                name: "IX_events_OperationId",
                table: "events",
                column: "OperationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_events_OwnerExternalId",
                table: "events",
                column: "OwnerExternalId");

            migrationBuilder.CreateIndex(
                name: "IX_commissions_EventId_PartnerExternalId",
                table: "commissions",
                columns: new[] { "EventId", "PartnerExternalId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_commissions_Paid",
                table: "commissions",
                column: "Paid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "commissions");

            migrationBuilder.DropTable(
                name: "events");

            migrationBuilder.DropTable(
                name: "schema_settings");
        }
    }
}
