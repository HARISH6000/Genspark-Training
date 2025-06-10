using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagementAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddedDeleteBehaviorsToRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryManagers_Inventories_InventoryID",
                table: "InventoryManagers");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryProducts_Inventories_InventoryID",
                table: "InventoryProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryProducts_Products_ProductID",
                table: "InventoryProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Roles_RoleId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_InventoryProducts_InventoryID",
                table: "InventoryProducts");

            migrationBuilder.DropIndex(
                name: "IX_InventoryManagers_InventoryID",
                table: "InventoryManagers");

            migrationBuilder.RenameColumn(
                name: "ProductID",
                table: "InventoryProducts",
                newName: "ProductId");

            migrationBuilder.RenameColumn(
                name: "InventoryID",
                table: "InventoryProducts",
                newName: "InventoryId");

            migrationBuilder.RenameIndex(
                name: "IX_InventoryProducts_ProductID",
                table: "InventoryProducts",
                newName: "IX_InventoryProducts_ProductId");

            migrationBuilder.RenameColumn(
                name: "InventoryID",
                table: "InventoryManagers",
                newName: "InventoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_RoleName",
                table: "Roles",
                column: "RoleName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryProducts_InventoryId_ProductId",
                table: "InventoryProducts",
                columns: new[] { "InventoryId", "ProductId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryManagers_InventoryId_ManagerId",
                table: "InventoryManagers",
                columns: new[] { "InventoryId", "ManagerId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryManagers_Inventories_InventoryId",
                table: "InventoryManagers",
                column: "InventoryId",
                principalTable: "Inventories",
                principalColumn: "InventoryId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryProducts_Inventories_InventoryId",
                table: "InventoryProducts",
                column: "InventoryId",
                principalTable: "Inventories",
                principalColumn: "InventoryId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryProducts_Products_ProductId",
                table: "InventoryProducts",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "ProductId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "CategoryId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Roles_RoleId",
                table: "Users",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "RoleId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryManagers_Inventories_InventoryId",
                table: "InventoryManagers");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryProducts_Inventories_InventoryId",
                table: "InventoryProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryProducts_Products_ProductId",
                table: "InventoryProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Roles_RoleId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Roles_RoleName",
                table: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_InventoryProducts_InventoryId_ProductId",
                table: "InventoryProducts");

            migrationBuilder.DropIndex(
                name: "IX_InventoryManagers_InventoryId_ManagerId",
                table: "InventoryManagers");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "InventoryProducts",
                newName: "ProductID");

            migrationBuilder.RenameColumn(
                name: "InventoryId",
                table: "InventoryProducts",
                newName: "InventoryID");

            migrationBuilder.RenameIndex(
                name: "IX_InventoryProducts_ProductId",
                table: "InventoryProducts",
                newName: "IX_InventoryProducts_ProductID");

            migrationBuilder.RenameColumn(
                name: "InventoryId",
                table: "InventoryManagers",
                newName: "InventoryID");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryProducts_InventoryID",
                table: "InventoryProducts",
                column: "InventoryID");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryManagers_InventoryID",
                table: "InventoryManagers",
                column: "InventoryID");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryManagers_Inventories_InventoryID",
                table: "InventoryManagers",
                column: "InventoryID",
                principalTable: "Inventories",
                principalColumn: "InventoryId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryProducts_Inventories_InventoryID",
                table: "InventoryProducts",
                column: "InventoryID",
                principalTable: "Inventories",
                principalColumn: "InventoryId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryProducts_Products_ProductID",
                table: "InventoryProducts",
                column: "ProductID",
                principalTable: "Products",
                principalColumn: "ProductId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "CategoryId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Roles_RoleId",
                table: "Users",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "RoleId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
