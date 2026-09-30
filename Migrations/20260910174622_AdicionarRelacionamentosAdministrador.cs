using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VassCommerce.Api.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarRelacionamentosAdministrador : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Administradores_CategoriaId",
                table: "Administradores",
                column: "CategoriaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Administradores_UsuarioId",
                table: "Administradores",
                column: "UsuarioId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Administradores_Categorias_CategoriaId",
                table: "Administradores",
                column: "CategoriaId",
                principalTable: "Categorias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Administradores_Usuarios_UsuarioId",
                table: "Administradores",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Administradores_Categorias_CategoriaId",
                table: "Administradores");

            migrationBuilder.DropForeignKey(
                name: "FK_Administradores_Usuarios_UsuarioId",
                table: "Administradores");

            migrationBuilder.DropIndex(
                name: "IX_Administradores_CategoriaId",
                table: "Administradores");

            migrationBuilder.DropIndex(
                name: "IX_Administradores_UsuarioId",
                table: "Administradores");
        }
    }
}
