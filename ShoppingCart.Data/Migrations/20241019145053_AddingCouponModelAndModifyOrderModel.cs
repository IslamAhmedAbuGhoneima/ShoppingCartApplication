using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShoppingCart.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddingCouponModelAndModifyOrderModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CouponCode",
                table: "Orders",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Discount",
                table: "Orders",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Coupon",
                columns: table => new
                {
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Discount = table.Column<int>(type: "int", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Coupon", x => x.Code);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CouponCode",
                table: "Orders",
                column: "CouponCode");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Coupon_CouponCode",
                table: "Orders",
                column: "CouponCode",
                principalTable: "Coupon",
                principalColumn: "Code");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Coupon_CouponCode",
                table: "Orders");

            migrationBuilder.DropTable(
                name: "Coupon");

            migrationBuilder.DropIndex(
                name: "IX_Orders_CouponCode",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CouponCode",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Discount",
                table: "Orders");
        }
    }
}
