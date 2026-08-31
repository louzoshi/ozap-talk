using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SopaTalk.Accounts.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class TeamManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Papel "Agent" foi renomeado para "Operator" no domínio.
            migrationBuilder.Sql("""UPDATE accounts.users SET "Role" = 'Operator' WHERE "Role" = 'Agent';""");

            migrationBuilder.CreateTable(
                name: "invitations",
                schema: "accounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    InvitedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpiresAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AcceptedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invitations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_invitations_Email",
                schema: "accounts",
                table: "invitations",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_invitations_TenantId",
                schema: "accounts",
                table: "invitations",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_invitations_TokenHash",
                schema: "accounts",
                table: "invitations",
                column: "TokenHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "invitations",
                schema: "accounts");

            migrationBuilder.Sql("""UPDATE accounts.users SET "Role" = 'Agent' WHERE "Role" = 'Operator';""");
        }
    }
}
