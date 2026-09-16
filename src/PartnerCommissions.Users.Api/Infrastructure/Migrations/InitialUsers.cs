using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PartnerCommissions.Users.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    ExternalId = table.Column<Guid>(type: "uuid", nullable: false),
                    InviterId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.ExternalId);
                    table.ForeignKey(
                        name: "FK_users_users_InviterId",
                        column: x => x.InviterId,
                        principalTable: "users",
                        principalColumn: "ExternalId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_users_InviterId",
                table: "users",
                column: "InviterId");

            migrationBuilder.Sql("""
                CREATE FUNCTION get_inviter_tree(user_external_id uuid, max_depth integer, direction text)
                RETURNS TABLE (
                    external_id uuid,
                    inviter_id uuid,
                    level integer
                )
                LANGUAGE plpgsql
                STABLE
                PARALLEL SAFE
                SET search_path TO pg_catalog, public
                AS $function$
                BEGIN
                    IF direction = 'down' THEN
                        RETURN QUERY
                        WITH RECURSIVE tree AS (
                            SELECT
                                u."ExternalId",
                                u."InviterId",
                                0 AS level
                            FROM users u
                            WHERE u."ExternalId" = user_external_id
                            UNION ALL
                            SELECT
                                u."ExternalId",
                                u."InviterId",
                                t.level + 1
                            FROM users u
                            INNER JOIN tree t ON u."InviterId" = t."ExternalId"
                            WHERE t.level < max_depth
                        )
                        SELECT t."ExternalId", t."InviterId", t.level
                        FROM tree t;
                    ELSE
                        RETURN QUERY
                        WITH RECURSIVE tree AS (
                            SELECT
                                u."ExternalId",
                                u."InviterId",
                                0 AS level
                            FROM users u
                            WHERE u."ExternalId" = user_external_id
                            UNION ALL
                            SELECT
                                u."ExternalId",
                                u."InviterId",
                                t.level + 1
                            FROM users u
                            INNER JOIN tree t ON u."ExternalId" = t."InviterId"
                            WHERE t.level < max_depth
                        )
                        SELECT t."ExternalId", t."InviterId", t.level
                        FROM tree t;
                    END IF;
                END;
                $function$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS get_inviter_tree(uuid, integer, text);");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
