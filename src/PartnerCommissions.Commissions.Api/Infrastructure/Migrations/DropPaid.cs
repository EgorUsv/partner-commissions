using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PartnerCommissions.Commissions.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DropPaid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_commissions_Paid",
                table: "commissions");

            migrationBuilder.DropIndex(
                name: "IX_commissions_AvailableAfter_Id",
                table: "commissions");

            migrationBuilder.DropColumn(
                name: "Paid",
                table: "commissions");

            migrationBuilder.CreateIndex(
                name: "IX_commissions_AvailableAfter_Id",
                table: "commissions",
                columns: new[] { "AvailableAfter", "Id" },
                filter: "\"PaidAt\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_commissions_AvailableAfter_Id",
                table: "commissions");

            migrationBuilder.AddColumn<bool>(
                name: "Paid",
                table: "commissions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("""
                UPDATE commissions
                SET "Paid" = TRUE
                WHERE "PaidAt" IS NOT NULL;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_commissions_Paid",
                table: "commissions",
                column: "Paid");

            migrationBuilder.CreateIndex(
                name: "IX_commissions_AvailableAfter_Id",
                table: "commissions",
                columns: new[] { "AvailableAfter", "Id" },
                filter: "NOT \"Paid\"");
        }
    }
}
