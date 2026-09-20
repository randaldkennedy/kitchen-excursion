using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KitchenExcursion.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddRecipeRevisions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The existing child rows still point directly at Recipes. Remove those
            // foreign keys first, but keep all data in place until Revision 1 rows
            // have been created and the children have been remapped.
            migrationBuilder.DropForeignKey(
                name: "FK_RecipeCategories_Recipes_RecipeId",
                schema: "kitchen",
                table: "RecipeCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_RecipeIngredients_Recipes_RecipeId",
                schema: "kitchen",
                table: "RecipeIngredients");

            migrationBuilder.DropForeignKey(
                name: "FK_RecipeShoppingItems_Recipes_RecipeId",
                schema: "kitchen",
                table: "RecipeShoppingItems");

            migrationBuilder.DropForeignKey(
                name: "FK_RecipeStatuses_Recipes_RecipeId",
                schema: "kitchen",
                table: "RecipeStatuses");

            migrationBuilder.DropForeignKey(
                name: "FK_RecipeSteps_Recipes_RecipeId",
                schema: "kitchen",
                table: "RecipeSteps");

            // Add the new Recipe identity columns as nullable first so existing rows
            // can be backfilled safely before enforcing the final NOT NULL shape.
            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                schema: "kitchen",
                table: "Recipes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedUtc",
                schema: "kitchen",
                table: "Recipes",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CurrentRevisionId",
                schema: "kitchen",
                table: "Recipes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HouseholdId",
                schema: "kitchen",
                table: "Recipes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                schema: "kitchen",
                table: "RecipeCookLogs",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RecipeRevisions",
                schema: "kitchen",
                columns: table => new
                {
                    RecipeRevisionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecipeId = table.Column<int>(type: "int", nullable: false),
                    RevisionNumber = table.Column<int>(type: "int", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ChangeNote = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Title = table.Column<string>(type: "nvarchar(240)", maxLength: 240, nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Badge = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    ImageAlt = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PrepTime = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    CookTime = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    Serves = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    Meal = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    Protein = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    Method = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    GeneralNotes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeRevisions", x => x.RecipeRevisionId);
                    table.ForeignKey(
                        name: "FK_RecipeRevisions_Recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalSchema: "kitchen",
                        principalTable: "Recipes",
                        principalColumn: "RecipeId",
                        onDelete: ReferentialAction.Cascade);
                });

            // Existing Kitchen data predates household ownership and revision history.
            // Assign it to the first valid platform user/default-household membership,
            // then copy each current recipe into Revision 1 BEFORE dropping any columns.
            migrationBuilder.Sql(
                """
                DECLARE @OwnerUserId int;
                DECLARE @HouseholdId int;

                SELECT TOP (1)
                    @OwnerUserId = u.Id,
                    @HouseholdId = u.DefaultHouseholdId
                FROM platform.Users u
                WHERE u.DefaultHouseholdId IS NOT NULL
                  AND EXISTS
                  (
                      SELECT 1
                      FROM platform.HouseholdMembers hm
                      WHERE hm.UserId = u.Id
                        AND hm.HouseholdId = u.DefaultHouseholdId
                  )
                ORDER BY u.Id;

                IF @OwnerUserId IS NULL OR @HouseholdId IS NULL
                    THROW 51000, 'AddRecipeRevisions requires at least one platform user with a valid default household membership.', 1;

                UPDATE kitchen.Recipes
                SET CreatedByUserId = @OwnerUserId,
                    HouseholdId = @HouseholdId,
                    CreatedUtc = SYSUTCDATETIME()
                WHERE CreatedByUserId IS NULL
                   OR HouseholdId IS NULL
                   OR CreatedUtc IS NULL;

                INSERT INTO kitchen.RecipeRevisions
                (
                    RecipeId,
                    RevisionNumber,
                    CreatedByUserId,
                    CreatedUtc,
                    ChangeNote,
                    Title,
                    Summary,
                    Badge,
                    ImageAlt,
                    PrepTime,
                    CookTime,
                    Serves,
                    Meal,
                    Protein,
                    Method,
                    GeneralNotes
                )
                SELECT
                    r.RecipeId,
                    1,
                    r.CreatedByUserId,
                    r.CreatedUtc,
                    N'Initial revision migrated from the original Kitchen recipe.',
                    r.Title,
                    r.Summary,
                    r.Badge,
                    r.ImageAlt,
                    r.PrepTime,
                    r.CookTime,
                    r.Serves,
                    r.Meal,
                    r.Protein,
                    r.Method,
                    r.GeneralNotes
                FROM kitchen.Recipes r;

                UPDATE r
                SET CurrentRevisionId = rr.RecipeRevisionId
                FROM kitchen.Recipes r
                INNER JOIN kitchen.RecipeRevisions rr
                    ON rr.RecipeId = r.RecipeId
                   AND rr.RevisionNumber = 1;

                UPDATE kitchen.RecipeCookLogs
                SET CreatedByUserId = @OwnerUserId
                WHERE CreatedByUserId IS NULL;
                """);

            // Rename the child FK columns, then translate their old RecipeId values to
            // the new RecipeRevisionId for each recipe's Revision 1.
            migrationBuilder.RenameColumn(
                name: "RecipeId",
                schema: "kitchen",
                table: "RecipeSteps",
                newName: "RecipeRevisionId");

            migrationBuilder.RenameIndex(
                name: "IX_RecipeSteps_RecipeId_SortOrder",
                schema: "kitchen",
                table: "RecipeSteps",
                newName: "IX_RecipeSteps_RecipeRevisionId_SortOrder");

            migrationBuilder.RenameColumn(
                name: "RecipeId",
                schema: "kitchen",
                table: "RecipeStatuses",
                newName: "RecipeRevisionId");

            migrationBuilder.RenameIndex(
                name: "IX_RecipeStatuses_RecipeId_SortOrder",
                schema: "kitchen",
                table: "RecipeStatuses",
                newName: "IX_RecipeStatuses_RecipeRevisionId_SortOrder");

            migrationBuilder.RenameColumn(
                name: "RecipeId",
                schema: "kitchen",
                table: "RecipeShoppingItems",
                newName: "RecipeRevisionId");

            migrationBuilder.RenameIndex(
                name: "IX_RecipeShoppingItems_RecipeId_SortOrder",
                schema: "kitchen",
                table: "RecipeShoppingItems",
                newName: "IX_RecipeShoppingItems_RecipeRevisionId_SortOrder");

            migrationBuilder.RenameColumn(
                name: "RecipeId",
                schema: "kitchen",
                table: "RecipeIngredients",
                newName: "RecipeRevisionId");

            migrationBuilder.RenameIndex(
                name: "IX_RecipeIngredients_RecipeId_SortOrder",
                schema: "kitchen",
                table: "RecipeIngredients",
                newName: "IX_RecipeIngredients_RecipeRevisionId_SortOrder");

            migrationBuilder.RenameColumn(
                name: "RecipeId",
                schema: "kitchen",
                table: "RecipeCategories",
                newName: "RecipeRevisionId");

            migrationBuilder.RenameIndex(
                name: "IX_RecipeCategories_RecipeId_SortOrder",
                schema: "kitchen",
                table: "RecipeCategories",
                newName: "IX_RecipeCategories_RecipeRevisionId_SortOrder");

            migrationBuilder.Sql(
                """
                UPDATE child
                SET RecipeRevisionId = rr.RecipeRevisionId
                FROM kitchen.RecipeCategories child
                INNER JOIN kitchen.RecipeRevisions rr
                    ON rr.RecipeId = child.RecipeRevisionId
                   AND rr.RevisionNumber = 1;

                UPDATE child
                SET RecipeRevisionId = rr.RecipeRevisionId
                FROM kitchen.RecipeIngredients child
                INNER JOIN kitchen.RecipeRevisions rr
                    ON rr.RecipeId = child.RecipeRevisionId
                   AND rr.RevisionNumber = 1;

                UPDATE child
                SET RecipeRevisionId = rr.RecipeRevisionId
                FROM kitchen.RecipeShoppingItems child
                INNER JOIN kitchen.RecipeRevisions rr
                    ON rr.RecipeId = child.RecipeRevisionId
                   AND rr.RevisionNumber = 1;

                UPDATE child
                SET RecipeRevisionId = rr.RecipeRevisionId
                FROM kitchen.RecipeStatuses child
                INNER JOIN kitchen.RecipeRevisions rr
                    ON rr.RecipeId = child.RecipeRevisionId
                   AND rr.RevisionNumber = 1;

                UPDATE child
                SET RecipeRevisionId = rr.RecipeRevisionId
                FROM kitchen.RecipeSteps child
                INNER JOIN kitchen.RecipeRevisions rr
                    ON rr.RecipeId = child.RecipeRevisionId
                   AND rr.RevisionNumber = 1;
                """);

            // Recipe scalar data now lives in RecipeRevisions. It is safe to remove
            // these columns only after Revision 1 has been populated.
            migrationBuilder.DropColumn(
                name: "Badge",
                schema: "kitchen",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "CookTime",
                schema: "kitchen",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "GeneralNotes",
                schema: "kitchen",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "ImageAlt",
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
                name: "PrepTime",
                schema: "kitchen",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "Protein",
                schema: "kitchen",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "Serves",
                schema: "kitchen",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "Summary",
                schema: "kitchen",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "Title",
                schema: "kitchen",
                table: "Recipes");

            migrationBuilder.AlterColumn<int>(
                name: "CreatedByUserId",
                schema: "kitchen",
                table: "Recipes",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedUtc",
                schema: "kitchen",
                table: "Recipes",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "HouseholdId",
                schema: "kitchen",
                table: "Recipes",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Recipes_CurrentRevisionId",
                schema: "kitchen",
                table: "Recipes",
                column: "CurrentRevisionId");

            migrationBuilder.CreateIndex(
                name: "IX_Recipes_HouseholdId",
                schema: "kitchen",
                table: "Recipes",
                column: "HouseholdId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeRevisions_RecipeId_RevisionNumber",
                schema: "kitchen",
                table: "RecipeRevisions",
                columns: new[] { "RecipeId", "RevisionNumber" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RecipeCategories_RecipeRevisions_RecipeRevisionId",
                schema: "kitchen",
                table: "RecipeCategories",
                column: "RecipeRevisionId",
                principalSchema: "kitchen",
                principalTable: "RecipeRevisions",
                principalColumn: "RecipeRevisionId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RecipeIngredients_RecipeRevisions_RecipeRevisionId",
                schema: "kitchen",
                table: "RecipeIngredients",
                column: "RecipeRevisionId",
                principalSchema: "kitchen",
                principalTable: "RecipeRevisions",
                principalColumn: "RecipeRevisionId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Recipes_RecipeRevisions_CurrentRevisionId",
                schema: "kitchen",
                table: "Recipes",
                column: "CurrentRevisionId",
                principalSchema: "kitchen",
                principalTable: "RecipeRevisions",
                principalColumn: "RecipeRevisionId");

            migrationBuilder.AddForeignKey(
                name: "FK_RecipeShoppingItems_RecipeRevisions_RecipeRevisionId",
                schema: "kitchen",
                table: "RecipeShoppingItems",
                column: "RecipeRevisionId",
                principalSchema: "kitchen",
                principalTable: "RecipeRevisions",
                principalColumn: "RecipeRevisionId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RecipeStatuses_RecipeRevisions_RecipeRevisionId",
                schema: "kitchen",
                table: "RecipeStatuses",
                column: "RecipeRevisionId",
                principalSchema: "kitchen",
                principalTable: "RecipeRevisions",
                principalColumn: "RecipeRevisionId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RecipeSteps_RecipeRevisions_RecipeRevisionId",
                schema: "kitchen",
                table: "RecipeSteps",
                column: "RecipeRevisionId",
                principalSchema: "kitchen",
                principalTable: "RecipeRevisions",
                principalColumn: "RecipeRevisionId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rollback preserves the CURRENT revision by copying it back onto Recipes.
            // Historical revisions are discarded because the old schema has nowhere
            // to store them.

            migrationBuilder.DropForeignKey(
                name: "FK_RecipeCategories_RecipeRevisions_RecipeRevisionId",
                schema: "kitchen",
                table: "RecipeCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_RecipeIngredients_RecipeRevisions_RecipeRevisionId",
                schema: "kitchen",
                table: "RecipeIngredients");

            migrationBuilder.DropForeignKey(
                name: "FK_Recipes_RecipeRevisions_CurrentRevisionId",
                schema: "kitchen",
                table: "Recipes");

            migrationBuilder.DropForeignKey(
                name: "FK_RecipeShoppingItems_RecipeRevisions_RecipeRevisionId",
                schema: "kitchen",
                table: "RecipeShoppingItems");

            migrationBuilder.DropForeignKey(
                name: "FK_RecipeStatuses_RecipeRevisions_RecipeRevisionId",
                schema: "kitchen",
                table: "RecipeStatuses");

            migrationBuilder.DropForeignKey(
                name: "FK_RecipeSteps_RecipeRevisions_RecipeRevisionId",
                schema: "kitchen",
                table: "RecipeSteps");

            migrationBuilder.AddColumn<string>(
                name: "Badge",
                schema: "kitchen",
                table: "Recipes",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CookTime",
                schema: "kitchen",
                table: "Recipes",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GeneralNotes",
                schema: "kitchen",
                table: "Recipes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageAlt",
                schema: "kitchen",
                table: "Recipes",
                type: "nvarchar(500)",
                maxLength: 500,
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
                name: "PrepTime",
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

            migrationBuilder.AddColumn<string>(
                name: "Serves",
                schema: "kitchen",
                table: "Recipes",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Summary",
                schema: "kitchen",
                table: "Recipes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                schema: "kitchen",
                table: "Recipes",
                type: "nvarchar(240)",
                maxLength: 240,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(
                """
                UPDATE r
                SET Title = rr.Title,
                    Summary = rr.Summary,
                    Badge = rr.Badge,
                    ImageAlt = rr.ImageAlt,
                    PrepTime = rr.PrepTime,
                    CookTime = rr.CookTime,
                    Serves = rr.Serves,
                    Meal = rr.Meal,
                    Protein = rr.Protein,
                    Method = rr.Method,
                    GeneralNotes = rr.GeneralNotes
                FROM kitchen.Recipes r
                INNER JOIN kitchen.RecipeRevisions rr
                    ON rr.RecipeRevisionId = r.CurrentRevisionId;
                """);

            // Translate each child row from revision ownership back to recipe ownership.
            migrationBuilder.Sql(
                """
                UPDATE child
                SET RecipeRevisionId = rr.RecipeId
                FROM kitchen.RecipeCategories child
                INNER JOIN kitchen.RecipeRevisions rr
                    ON rr.RecipeRevisionId = child.RecipeRevisionId;

                UPDATE child
                SET RecipeRevisionId = rr.RecipeId
                FROM kitchen.RecipeIngredients child
                INNER JOIN kitchen.RecipeRevisions rr
                    ON rr.RecipeRevisionId = child.RecipeRevisionId;

                UPDATE child
                SET RecipeRevisionId = rr.RecipeId
                FROM kitchen.RecipeShoppingItems child
                INNER JOIN kitchen.RecipeRevisions rr
                    ON rr.RecipeRevisionId = child.RecipeRevisionId;

                UPDATE child
                SET RecipeRevisionId = rr.RecipeId
                FROM kitchen.RecipeStatuses child
                INNER JOIN kitchen.RecipeRevisions rr
                    ON rr.RecipeRevisionId = child.RecipeRevisionId;

                UPDATE child
                SET RecipeRevisionId = rr.RecipeId
                FROM kitchen.RecipeSteps child
                INNER JOIN kitchen.RecipeRevisions rr
                    ON rr.RecipeRevisionId = child.RecipeRevisionId;
                """);

            migrationBuilder.DropIndex(
                name: "IX_Recipes_CurrentRevisionId",
                schema: "kitchen",
                table: "Recipes");

            migrationBuilder.DropIndex(
                name: "IX_Recipes_HouseholdId",
                schema: "kitchen",
                table: "Recipes");

            migrationBuilder.RenameColumn(
                name: "RecipeRevisionId",
                schema: "kitchen",
                table: "RecipeSteps",
                newName: "RecipeId");

            migrationBuilder.RenameIndex(
                name: "IX_RecipeSteps_RecipeRevisionId_SortOrder",
                schema: "kitchen",
                table: "RecipeSteps",
                newName: "IX_RecipeSteps_RecipeId_SortOrder");

            migrationBuilder.RenameColumn(
                name: "RecipeRevisionId",
                schema: "kitchen",
                table: "RecipeStatuses",
                newName: "RecipeId");

            migrationBuilder.RenameIndex(
                name: "IX_RecipeStatuses_RecipeRevisionId_SortOrder",
                schema: "kitchen",
                table: "RecipeStatuses",
                newName: "IX_RecipeStatuses_RecipeId_SortOrder");

            migrationBuilder.RenameColumn(
                name: "RecipeRevisionId",
                schema: "kitchen",
                table: "RecipeShoppingItems",
                newName: "RecipeId");

            migrationBuilder.RenameIndex(
                name: "IX_RecipeShoppingItems_RecipeRevisionId_SortOrder",
                schema: "kitchen",
                table: "RecipeShoppingItems",
                newName: "IX_RecipeShoppingItems_RecipeId_SortOrder");

            migrationBuilder.RenameColumn(
                name: "RecipeRevisionId",
                schema: "kitchen",
                table: "RecipeIngredients",
                newName: "RecipeId");

            migrationBuilder.RenameIndex(
                name: "IX_RecipeIngredients_RecipeRevisionId_SortOrder",
                schema: "kitchen",
                table: "RecipeIngredients",
                newName: "IX_RecipeIngredients_RecipeId_SortOrder");

            migrationBuilder.RenameColumn(
                name: "RecipeRevisionId",
                schema: "kitchen",
                table: "RecipeCategories",
                newName: "RecipeId");

            migrationBuilder.RenameIndex(
                name: "IX_RecipeCategories_RecipeRevisionId_SortOrder",
                schema: "kitchen",
                table: "RecipeCategories",
                newName: "IX_RecipeCategories_RecipeId_SortOrder");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                schema: "kitchen",
                table: "RecipeCookLogs");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                schema: "kitchen",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "CreatedUtc",
                schema: "kitchen",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "CurrentRevisionId",
                schema: "kitchen",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "HouseholdId",
                schema: "kitchen",
                table: "Recipes");

            migrationBuilder.DropTable(
                name: "RecipeRevisions",
                schema: "kitchen");

            migrationBuilder.AddForeignKey(
                name: "FK_RecipeCategories_Recipes_RecipeId",
                schema: "kitchen",
                table: "RecipeCategories",
                column: "RecipeId",
                principalSchema: "kitchen",
                principalTable: "Recipes",
                principalColumn: "RecipeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RecipeIngredients_Recipes_RecipeId",
                schema: "kitchen",
                table: "RecipeIngredients",
                column: "RecipeId",
                principalSchema: "kitchen",
                principalTable: "Recipes",
                principalColumn: "RecipeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RecipeShoppingItems_Recipes_RecipeId",
                schema: "kitchen",
                table: "RecipeShoppingItems",
                column: "RecipeId",
                principalSchema: "kitchen",
                principalTable: "Recipes",
                principalColumn: "RecipeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RecipeStatuses_Recipes_RecipeId",
                schema: "kitchen",
                table: "RecipeStatuses",
                column: "RecipeId",
                principalSchema: "kitchen",
                principalTable: "Recipes",
                principalColumn: "RecipeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RecipeSteps_Recipes_RecipeId",
                schema: "kitchen",
                table: "RecipeSteps",
                column: "RecipeId",
                principalSchema: "kitchen",
                principalTable: "Recipes",
                principalColumn: "RecipeId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
