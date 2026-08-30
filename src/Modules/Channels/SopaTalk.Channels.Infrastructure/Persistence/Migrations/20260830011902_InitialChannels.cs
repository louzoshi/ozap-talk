using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SopaTalk.Channels.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialChannels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "channels");

            migrationBuilder.CreateTable(
                name: "processed_inbound_messages",
                schema: "channels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderMessageId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ProcessedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_processed_inbound_messages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "whatsapp_channels",
                schema: "channels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PhoneNumberId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    DisplayPhoneNumber = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    WabaId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    AccessToken = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_whatsapp_channels", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_processed_inbound_messages_TenantId_ProviderMessageId",
                schema: "channels",
                table: "processed_inbound_messages",
                columns: new[] { "TenantId", "ProviderMessageId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_whatsapp_channels_PhoneNumberId",
                schema: "channels",
                table: "whatsapp_channels",
                column: "PhoneNumberId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_whatsapp_channels_TenantId",
                schema: "channels",
                table: "whatsapp_channels",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "processed_inbound_messages",
                schema: "channels");

            migrationBuilder.DropTable(
                name: "whatsapp_channels",
                schema: "channels");
        }
    }
}
