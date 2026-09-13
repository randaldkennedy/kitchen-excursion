using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KitchenExcursion.Api.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeRecipeData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "kitchen");

            migrationBuilder.Sql(
                "ALTER SCHEMA [kitchen] TRANSFER [dbo].[Recipes];");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                schema: "kitchen",
                table: "Recipes",
                type: "nvarchar(240)",
                maxLength: 240,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                schema: "kitchen",
                table: "Recipes",
                type: "nvarchar(160)",
                maxLength: 160,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Serves",
                schema: "kitchen",
                table: "Recipes",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PrepTime",
                schema: "kitchen",
                table: "Recipes",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ImageAlt",
                schema: "kitchen",
                table: "Recipes",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Image",
                schema: "kitchen",
                table: "Recipes",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CookTime",
                schema: "kitchen",
                table: "Recipes",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Badge",
                schema: "kitchen",
                table: "Recipes",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GeneralNotes",
                schema: "kitchen",
                table: "Recipes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Meal",
                schema: "kitchen",
                table: "Recipes",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Method",
                schema: "kitchen",
                table: "Recipes",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Protein",
                schema: "kitchen",
                table: "Recipes",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RecipeCategories",
                schema: "kitchen",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecipeId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecipeCategories_Recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalSchema: "kitchen",
                        principalTable: "Recipes",
                        principalColumn: "RecipeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecipeCookLogs",
                schema: "kitchen",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecipeId = table.Column<int>(type: "int", nullable: false),
                    CookedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Author = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeCookLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecipeCookLogs_Recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalSchema: "kitchen",
                        principalTable: "Recipes",
                        principalColumn: "RecipeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecipeIngredients",
                schema: "kitchen",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecipeId = table.Column<int>(type: "int", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeIngredients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecipeIngredients_Recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalSchema: "kitchen",
                        principalTable: "Recipes",
                        principalColumn: "RecipeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecipeShoppingItems",
                schema: "kitchen",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecipeId = table.Column<int>(type: "int", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeShoppingItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecipeShoppingItems_Recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalSchema: "kitchen",
                        principalTable: "Recipes",
                        principalColumn: "RecipeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecipeStatuses",
                schema: "kitchen",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecipeId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeStatuses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecipeStatuses_Recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalSchema: "kitchen",
                        principalTable: "Recipes",
                        principalColumn: "RecipeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecipeSteps",
                schema: "kitchen",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecipeId = table.Column<int>(type: "int", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecipeSteps_Recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalSchema: "kitchen",
                        principalTable: "Recipes",
                        principalColumn: "RecipeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecipeCookRatings",
                schema: "kitchen",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecipeCookLogId = table.Column<long>(type: "bigint", nullable: false),
                    Rater = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Stars = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeCookRatings", x => x.Id);
                    table.CheckConstraint("CK_RecipeCookRatings_Stars", "[Stars] >= 1 AND [Stars] <= 5");
                    table.ForeignKey(
                        name: "FK_RecipeCookRatings_RecipeCookLogs_RecipeCookLogId",
                        column: x => x.RecipeCookLogId,
                        principalSchema: "kitchen",
                        principalTable: "RecipeCookLogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Recipes_Slug",
                schema: "kitchen",
                table: "Recipes",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecipeCategories_RecipeId_SortOrder",
                schema: "kitchen",
                table: "RecipeCategories",
                columns: new[] { "RecipeId", "SortOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecipeCookLogs_RecipeId_CookedAt",
                schema: "kitchen",
                table: "RecipeCookLogs",
                columns: new[] { "RecipeId", "CookedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_RecipeCookRatings_RecipeCookLogId_Rater",
                schema: "kitchen",
                table: "RecipeCookRatings",
                columns: new[] { "RecipeCookLogId", "Rater" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecipeIngredients_RecipeId_SortOrder",
                schema: "kitchen",
                table: "RecipeIngredients",
                columns: new[] { "RecipeId", "SortOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecipeShoppingItems_RecipeId_SortOrder",
                schema: "kitchen",
                table: "RecipeShoppingItems",
                columns: new[] { "RecipeId", "SortOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecipeStatuses_RecipeId_SortOrder",
                schema: "kitchen",
                table: "RecipeStatuses",
                columns: new[] { "RecipeId", "SortOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecipeSteps_RecipeId_SortOrder",
                schema: "kitchen",
                table: "RecipeSteps",
                columns: new[] { "RecipeId", "SortOrder" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecipeCategories",
                schema: "kitchen");

            migrationBuilder.DropTable(
                name: "RecipeCookRatings",
                schema: "kitchen");

            migrationBuilder.DropTable(
                name: "RecipeIngredients",
                schema: "kitchen");

            migrationBuilder.DropTable(
                name: "RecipeShoppingItems",
                schema: "kitchen");

            migrationBuilder.DropTable(
                name: "RecipeStatuses",
                schema: "kitchen");

            migrationBuilder.DropTable(
                name: "RecipeSteps",
                schema: "kitchen");

            migrationBuilder.DropTable(
                name: "RecipeCookLogs",
                schema: "kitchen");

            migrationBuilder.DropIndex(
                name: "IX_Recipes_Slug",
                schema: "kitchen",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "GeneralNotes",
                schema: "kitchen",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "Meal",
                schema: "kitchen",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "Method",
                schema: "kitchen",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "Protein",
                schema: "kitchen",
                table: "Recipes");

            migrationBuilder.Sql(
                "ALTER SCHEMA [dbo] TRANSFER [kitchen].[Recipes];");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Recipes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(240)",
                oldMaxLength: 240);

            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "Recipes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(160)",
                oldMaxLength: 160);

            migrationBuilder.AlterColumn<string>(
                name: "Serves",
                table: "Recipes",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(80)",
                oldMaxLength: 80,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PrepTime",
                table: "Recipes",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(80)",
                oldMaxLength: 80,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ImageAlt",
                table: "Recipes",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Image",
                table: "Recipes",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CookTime",
                table: "Recipes",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(80)",
                oldMaxLength: 80,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Badge",
                table: "Recipes",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(80)",
                oldMaxLength: 80,
                oldNullable: true);
        }
    }
}
