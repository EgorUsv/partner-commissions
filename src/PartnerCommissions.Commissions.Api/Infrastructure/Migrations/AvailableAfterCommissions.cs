using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PartnerCommissions.Commissions.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AvailableAfterCommissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_commissions_ClaimedAt_Id",
                table: "commissions");

            migrationBuilder.DropColumn(
                name: "ClaimedAt",
                table: "commissions");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "AvailableAfter",
                table: "commissions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_commissions_AvailableAfter_Id",
                table: "commissions",
                columns: new[] { "AvailableAfter", "Id" },
                filter: "NOT \"Paid\"");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_commissions_AvailableAfter_Id",
                table: "commissions");

            migrationBuilder.DropColumn(
                name: "AvailableAfter",
                table: "commissions");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ClaimedAt",
                table: "commissions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_commissions_ClaimedAt_Id",
                table: "commissions",
                columns: new[] { "ClaimedAt", "Id" },
                filter: "NOT \"Paid\"");
        }
    }
}
