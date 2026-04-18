using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MailboxConnections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EmailAddress = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Provider = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    DefaultQueueId = table.Column<Guid>(type: "uuid", nullable: true),
                    ImapHost = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ImapPort = table.Column<int>(type: "integer", nullable: true),
                    ImapUseSsl = table.Column<bool>(type: "boolean", nullable: false),
                    ImapUsername = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EncryptedImapPassword = table.Column<string>(type: "text", nullable: true),
                    SmtpHost = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SmtpPort = table.Column<int>(type: "integer", nullable: true),
                    SmtpUseSsl = table.Column<bool>(type: "boolean", nullable: false),
                    SmtpUsername = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EncryptedSmtpPassword = table.Column<string>(type: "text", nullable: true),
                    GraphTenantId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    GraphClientId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    EncryptedGraphClientSecret = table.Column<string>(type: "text", nullable: true),
                    GraphMailboxUserId = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    GraphDeltaLink = table.Column<string>(type: "text", nullable: true),
                    GraphSubscriptionId = table.Column<string>(type: "text", nullable: true),
                    GraphSubscriptionExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PollIntervalSeconds = table.Column<int>(type: "integer", nullable: false),
                    AutoCreateContacts = table.Column<bool>(type: "boolean", nullable: false),
                    LastPollAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastSyncUid = table.Column<string>(type: "text", nullable: true),
                    MessageCount = table.Column<int>(type: "integer", nullable: false),
                    LastError = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MailboxConnections", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmailMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MailboxConnectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TicketId = table.Column<Guid>(type: "uuid", nullable: true),
                    ContactId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: true),
                    Direction = table.Column<int>(type: "integer", nullable: false),
                    DeliveryStatus = table.Column<int>(type: "integer", nullable: false),
                    FromAddress = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FromName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ToAddress = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Subject = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    BodyHtml = table.Column<string>(type: "text", nullable: true),
                    BodyText = table.Column<string>(type: "text", nullable: true),
                    MessageId = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    InReplyTo = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    References = table.Column<string>(type: "text", nullable: true),
                    RawEmlPath = table.Column<string>(type: "text", nullable: true),
                    AttachmentCount = table.Column<int>(type: "integer", nullable: false),
                    ErrorDetails = table.Column<string>(type: "text", nullable: true),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailMessages_MailboxConnections_MailboxConnectionId",
                        column: x => x.MailboxConnectionId,
                        principalTable: "MailboxConnections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmailMessages_ClientId",
                table: "EmailMessages",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailMessages_ContactId",
                table: "EmailMessages",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailMessages_Direction",
                table: "EmailMessages",
                column: "Direction");

            migrationBuilder.CreateIndex(
                name: "IX_EmailMessages_MailboxConnectionId",
                table: "EmailMessages",
                column: "MailboxConnectionId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailMessages_MessageId",
                table: "EmailMessages",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailMessages_SentAt",
                table: "EmailMessages",
                column: "SentAt");

            migrationBuilder.CreateIndex(
                name: "IX_EmailMessages_TicketId",
                table: "EmailMessages",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_MailboxConnections_EmailAddress",
                table: "MailboxConnections",
                column: "EmailAddress",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MailboxConnections_Status",
                table: "MailboxConnections",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailMessages");

            migrationBuilder.DropTable(
                name: "MailboxConnections");
        }
    }
}
