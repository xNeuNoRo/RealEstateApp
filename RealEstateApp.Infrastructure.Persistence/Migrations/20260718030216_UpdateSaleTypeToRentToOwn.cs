using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RealEstateApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSaleTypeToRentToOwn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "SaleTypes",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Code", "Description", "Name" },
                values: new object[] { "RentToOwn", "Alquiler con derecho a compra futura.", "Alquiler con opción a compra" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "SaleTypes",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Code", "Description", "Name" },
                values: new object[] { "Transfer", "Cesion de contrato a terceros.", "Traspaso" });
        }
    }
}
