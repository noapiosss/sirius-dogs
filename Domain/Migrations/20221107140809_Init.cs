using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Domain.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "tbl_dogs",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: true),
                    breed = table.Column<string>(type: "text", nullable: true),
                    size = table.Column<string>(type: "text", nullable: true),
                    birth_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    about = table.Column<string>(type: "text", nullable: true),
                    row = table.Column<int>(type: "integer", nullable: false),
                    enclosure = table.Column<int>(type: "integer", nullable: false),
                    title_photo = table.Column<string>(type: "text", nullable: true),
                    last_update = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_dogs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_images",
                schema: "public",
                columns: table => new
                {
                    dog_id = table.Column<int>(type: "integer", nullable: false),
                    photo_path = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_images", x => new { x.dog_id, x.photo_path });
                    table.ForeignKey(
                        name: "FK_tbl_images_tbl_dogs_dog_id",
                        column: x => x.dog_id,
                        principalSchema: "public",
                        principalTable: "tbl_dogs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tbl_images",
                schema: "public");

            migrationBuilder.DropTable(
                name: "tbl_dogs",
                schema: "public");
        }
    }
}
