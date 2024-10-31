using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoomReservationSystem.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserIsActiveMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "ADMIN");

            migrationBuilder.CreateTable(
                name: "ROOMS",
                schema: "ADMIN",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Name = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    Capacity = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ROOMS", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "USERS",
                schema: "ADMIN",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    FirstName = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "NVARCHAR2(64)", maxLength: 64, nullable: false),
                    Role = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false, defaultValue: "User"),
                    IS_ACTIVE = table.Column<string>(type: "NVARCHAR2(1)", maxLength: 1, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USERS", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_USERS_Email",
                schema: "ADMIN",
                table: "USERS",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ROOMS",
                schema: "ADMIN");

            migrationBuilder.DropTable(
                name: "USERS",
                schema: "ADMIN");
        }
    }
}
