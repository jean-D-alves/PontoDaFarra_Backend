using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PontoDaFarra.Migrations
{
    /// <inheritdoc />
    public partial class Productsvalueupdatefordecimal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "ALTER TABLE \"Products\" ALTER COLUMN \"Value\" TYPE numeric USING replace(\"Value\", ',', '.')::numeric;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "ALTER TABLE \"Products\" ALTER COLUMN \"Value\" TYPE text USING \"Value\"::text;");
        }
    }
}