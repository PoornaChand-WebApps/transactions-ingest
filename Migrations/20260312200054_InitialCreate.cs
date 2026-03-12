using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodingExerciseTransactions.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TransactionAuditTrail",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TransactionId = table.Column<int>(type: "INTEGER", nullable: false),
                    ChangeType = table.Column<string>(type: "TEXT", nullable: false),
                    FieldName = table.Column<string>(type: "TEXT", nullable: false),
                    OldValue = table.Column<string>(type: "TEXT", nullable: false),
                    NewValue = table.Column<string>(type: "TEXT", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionAuditTrail", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TransactionId = table.Column<int>(type: "INTEGER", nullable: false),
                    CardNumberHash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    CardLast4 = table.Column<string>(type: "TEXT", maxLength: 4, nullable: false),
                    LocationCode = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ProductName = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TransactionTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsRevoked = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsFinalized = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_TransactionId",
                table: "Transactions",
                column: "TransactionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_TransactionTime",
                table: "Transactions",
                column: "TransactionTime");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TransactionAuditTrail");

            migrationBuilder.DropTable(
                name: "Transactions");
        }
    }
}
