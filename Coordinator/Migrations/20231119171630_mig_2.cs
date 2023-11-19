using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Coordinator.Migrations
{
    /// <inheritdoc />
    public partial class mig_2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Nodes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("857250ce-94ab-4204-9826-66d8f0524f94"), "Stock.API" },
                    { new Guid("af10d17c-4b41-4895-97d2-c47f0991ad8b"), "Order.API" },
                    { new Guid("f58c277b-dbff-4418-9129-dd0d2d5b92d4"), "Payment.API" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Nodes",
                keyColumn: "Id",
                keyValue: new Guid("857250ce-94ab-4204-9826-66d8f0524f94"));

            migrationBuilder.DeleteData(
                table: "Nodes",
                keyColumn: "Id",
                keyValue: new Guid("af10d17c-4b41-4895-97d2-c47f0991ad8b"));

            migrationBuilder.DeleteData(
                table: "Nodes",
                keyColumn: "Id",
                keyValue: new Guid("f58c277b-dbff-4418-9129-dd0d2d5b92d4"));
        }
    }
}
