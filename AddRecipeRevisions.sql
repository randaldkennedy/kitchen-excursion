IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [Recipes] (
    [RecipeId] int NOT NULL IDENTITY,
    [Slug] nvarchar(max) NOT NULL,
    [Title] nvarchar(max) NOT NULL,
    [Summary] nvarchar(max) NULL,
    [Badge] nvarchar(max) NULL,
    [Image] nvarchar(max) NULL,
    [ImageAlt] nvarchar(max) NULL,
    [PrepTime] nvarchar(max) NULL,
    [CookTime] nvarchar(max) NULL,
    [Serves] nvarchar(max) NULL,
    CONSTRAINT [PK_Recipes] PRIMARY KEY ([RecipeId])
);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260805012826_InitialCreate', N'10.0.10');

COMMIT;
GO

BEGIN TRANSACTION;
IF SCHEMA_ID(N'kitchen') IS NULL EXEC(N'CREATE SCHEMA [kitchen];');

ALTER SCHEMA [kitchen] TRANSFER [dbo].[Recipes];

DECLARE @var nvarchar(max);
SELECT @var = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[kitchen].[Recipes]') AND [c].[name] = N'Title');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [kitchen].[Recipes] DROP CONSTRAINT ' + @var + ';');
ALTER TABLE [kitchen].[Recipes] ALTER COLUMN [Title] nvarchar(240) NOT NULL;

DECLARE @var1 nvarchar(max);
SELECT @var1 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[kitchen].[Recipes]') AND [c].[name] = N'Slug');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [kitchen].[Recipes] DROP CONSTRAINT ' + @var1 + ';');
ALTER TABLE [kitchen].[Recipes] ALTER COLUMN [Slug] nvarchar(160) NOT NULL;

DECLARE @var2 nvarchar(max);
SELECT @var2 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[kitchen].[Recipes]') AND [c].[name] = N'Serves');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [kitchen].[Recipes] DROP CONSTRAINT ' + @var2 + ';');
ALTER TABLE [kitchen].[Recipes] ALTER COLUMN [Serves] nvarchar(80) NULL;

DECLARE @var3 nvarchar(max);
SELECT @var3 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[kitchen].[Recipes]') AND [c].[name] = N'PrepTime');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [kitchen].[Recipes] DROP CONSTRAINT ' + @var3 + ';');
ALTER TABLE [kitchen].[Recipes] ALTER COLUMN [PrepTime] nvarchar(80) NULL;

DECLARE @var4 nvarchar(max);
SELECT @var4 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[kitchen].[Recipes]') AND [c].[name] = N'ImageAlt');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [kitchen].[Recipes] DROP CONSTRAINT ' + @var4 + ';');
ALTER TABLE [kitchen].[Recipes] ALTER COLUMN [ImageAlt] nvarchar(500) NULL;

DECLARE @var5 nvarchar(max);
SELECT @var5 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[kitchen].[Recipes]') AND [c].[name] = N'Image');
IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [kitchen].[Recipes] DROP CONSTRAINT ' + @var5 + ';');
ALTER TABLE [kitchen].[Recipes] ALTER COLUMN [Image] nvarchar(500) NULL;

DECLARE @var6 nvarchar(max);
SELECT @var6 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[kitchen].[Recipes]') AND [c].[name] = N'CookTime');
IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [kitchen].[Recipes] DROP CONSTRAINT ' + @var6 + ';');
ALTER TABLE [kitchen].[Recipes] ALTER COLUMN [CookTime] nvarchar(80) NULL;

DECLARE @var7 nvarchar(max);
SELECT @var7 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[kitchen].[Recipes]') AND [c].[name] = N'Badge');
IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [kitchen].[Recipes] DROP CONSTRAINT ' + @var7 + ';');
ALTER TABLE [kitchen].[Recipes] ALTER COLUMN [Badge] nvarchar(80) NULL;

ALTER TABLE [kitchen].[Recipes] ADD [GeneralNotes] nvarchar(max) NULL;

ALTER TABLE [kitchen].[Recipes] ADD [Meal] nvarchar(80) NULL;

ALTER TABLE [kitchen].[Recipes] ADD [Method] nvarchar(80) NULL;

ALTER TABLE [kitchen].[Recipes] ADD [Protein] nvarchar(80) NULL;

CREATE TABLE [kitchen].[RecipeCategories] (
    [Id] int NOT NULL IDENTITY,
    [RecipeId] int NOT NULL,
    [Name] nvarchar(120) NOT NULL,
    [SortOrder] int NOT NULL,
    CONSTRAINT [PK_RecipeCategories] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RecipeCategories_Recipes_RecipeId] FOREIGN KEY ([RecipeId]) REFERENCES [kitchen].[Recipes] ([RecipeId]) ON DELETE CASCADE
);

CREATE TABLE [kitchen].[RecipeCookLogs] (
    [Id] bigint NOT NULL IDENTITY,
    [RecipeId] int NOT NULL,
    [CookedAt] datetimeoffset NOT NULL,
    [Author] nvarchar(120) NOT NULL,
    [Note] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_RecipeCookLogs] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RecipeCookLogs_Recipes_RecipeId] FOREIGN KEY ([RecipeId]) REFERENCES [kitchen].[Recipes] ([RecipeId]) ON DELETE CASCADE
);

CREATE TABLE [kitchen].[RecipeIngredients] (
    [Id] int NOT NULL IDENTITY,
    [RecipeId] int NOT NULL,
    [Text] nvarchar(max) NOT NULL,
    [SortOrder] int NOT NULL,
    CONSTRAINT [PK_RecipeIngredients] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RecipeIngredients_Recipes_RecipeId] FOREIGN KEY ([RecipeId]) REFERENCES [kitchen].[Recipes] ([RecipeId]) ON DELETE CASCADE
);

CREATE TABLE [kitchen].[RecipeShoppingItems] (
    [Id] int NOT NULL IDENTITY,
    [RecipeId] int NOT NULL,
    [Text] nvarchar(max) NOT NULL,
    [SortOrder] int NOT NULL,
    CONSTRAINT [PK_RecipeShoppingItems] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RecipeShoppingItems_Recipes_RecipeId] FOREIGN KEY ([RecipeId]) REFERENCES [kitchen].[Recipes] ([RecipeId]) ON DELETE CASCADE
);

CREATE TABLE [kitchen].[RecipeStatuses] (
    [Id] int NOT NULL IDENTITY,
    [RecipeId] int NOT NULL,
    [Value] nvarchar(80) NOT NULL,
    [SortOrder] int NOT NULL,
    CONSTRAINT [PK_RecipeStatuses] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RecipeStatuses_Recipes_RecipeId] FOREIGN KEY ([RecipeId]) REFERENCES [kitchen].[Recipes] ([RecipeId]) ON DELETE CASCADE
);

CREATE TABLE [kitchen].[RecipeSteps] (
    [Id] int NOT NULL IDENTITY,
    [RecipeId] int NOT NULL,
    [Text] nvarchar(max) NOT NULL,
    [SortOrder] int NOT NULL,
    CONSTRAINT [PK_RecipeSteps] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RecipeSteps_Recipes_RecipeId] FOREIGN KEY ([RecipeId]) REFERENCES [kitchen].[Recipes] ([RecipeId]) ON DELETE CASCADE
);

CREATE TABLE [kitchen].[RecipeCookRatings] (
    [Id] bigint NOT NULL IDENTITY,
    [RecipeCookLogId] bigint NOT NULL,
    [Rater] nvarchar(120) NOT NULL,
    [Stars] int NOT NULL,
    CONSTRAINT [PK_RecipeCookRatings] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_RecipeCookRatings_Stars] CHECK ([Stars] >= 1 AND [Stars] <= 5),
    CONSTRAINT [FK_RecipeCookRatings_RecipeCookLogs_RecipeCookLogId] FOREIGN KEY ([RecipeCookLogId]) REFERENCES [kitchen].[RecipeCookLogs] ([Id]) ON DELETE CASCADE
);

CREATE UNIQUE INDEX [IX_Recipes_Slug] ON [kitchen].[Recipes] ([Slug]);

CREATE UNIQUE INDEX [IX_RecipeCategories_RecipeId_SortOrder] ON [kitchen].[RecipeCategories] ([RecipeId], [SortOrder]);

CREATE INDEX [IX_RecipeCookLogs_RecipeId_CookedAt] ON [kitchen].[RecipeCookLogs] ([RecipeId], [CookedAt]);

CREATE UNIQUE INDEX [IX_RecipeCookRatings_RecipeCookLogId_Rater] ON [kitchen].[RecipeCookRatings] ([RecipeCookLogId], [Rater]);

CREATE UNIQUE INDEX [IX_RecipeIngredients_RecipeId_SortOrder] ON [kitchen].[RecipeIngredients] ([RecipeId], [SortOrder]);

CREATE UNIQUE INDEX [IX_RecipeShoppingItems_RecipeId_SortOrder] ON [kitchen].[RecipeShoppingItems] ([RecipeId], [SortOrder]);

CREATE UNIQUE INDEX [IX_RecipeStatuses_RecipeId_SortOrder] ON [kitchen].[RecipeStatuses] ([RecipeId], [SortOrder]);

CREATE UNIQUE INDEX [IX_RecipeSteps_RecipeId_SortOrder] ON [kitchen].[RecipeSteps] ([RecipeId], [SortOrder]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260913152017_NormalizeRecipeData', N'10.0.10');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [kitchen].[RecipeCategories] DROP CONSTRAINT [FK_RecipeCategories_Recipes_RecipeId];

ALTER TABLE [kitchen].[RecipeIngredients] DROP CONSTRAINT [FK_RecipeIngredients_Recipes_RecipeId];

ALTER TABLE [kitchen].[RecipeShoppingItems] DROP CONSTRAINT [FK_RecipeShoppingItems_Recipes_RecipeId];

ALTER TABLE [kitchen].[RecipeStatuses] DROP CONSTRAINT [FK_RecipeStatuses_Recipes_RecipeId];

ALTER TABLE [kitchen].[RecipeSteps] DROP CONSTRAINT [FK_RecipeSteps_Recipes_RecipeId];

ALTER TABLE [kitchen].[Recipes] ADD [CreatedByUserId] int NULL;

ALTER TABLE [kitchen].[Recipes] ADD [CreatedUtc] datetimeoffset NULL;

ALTER TABLE [kitchen].[Recipes] ADD [CurrentRevisionId] int NULL;

ALTER TABLE [kitchen].[Recipes] ADD [HouseholdId] int NULL;

ALTER TABLE [kitchen].[RecipeCookLogs] ADD [CreatedByUserId] int NULL;

CREATE TABLE [kitchen].[RecipeRevisions] (
    [RecipeRevisionId] int NOT NULL IDENTITY,
    [RecipeId] int NOT NULL,
    [RevisionNumber] int NOT NULL,
    [CreatedByUserId] int NOT NULL,
    [CreatedUtc] datetimeoffset NOT NULL,
    [ChangeNote] nvarchar(500) NULL,
    [Title] nvarchar(240) NOT NULL,
    [Summary] nvarchar(max) NULL,
    [Badge] nvarchar(80) NULL,
    [ImageAlt] nvarchar(500) NULL,
    [PrepTime] nvarchar(80) NULL,
    [CookTime] nvarchar(80) NULL,
    [Serves] nvarchar(80) NULL,
    [Meal] nvarchar(80) NULL,
    [Protein] nvarchar(80) NULL,
    [Method] nvarchar(80) NULL,
    [GeneralNotes] nvarchar(max) NULL,
    CONSTRAINT [PK_RecipeRevisions] PRIMARY KEY ([RecipeRevisionId]),
    CONSTRAINT [FK_RecipeRevisions_Recipes_RecipeId] FOREIGN KEY ([RecipeId]) REFERENCES [kitchen].[Recipes] ([RecipeId]) ON DELETE CASCADE
);

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

EXEC sp_rename N'[kitchen].[RecipeSteps].[RecipeId]', N'RecipeRevisionId', 'COLUMN';

EXEC sp_rename N'[kitchen].[RecipeSteps].[IX_RecipeSteps_RecipeId_SortOrder]', N'IX_RecipeSteps_RecipeRevisionId_SortOrder', 'INDEX';

EXEC sp_rename N'[kitchen].[RecipeStatuses].[RecipeId]', N'RecipeRevisionId', 'COLUMN';

EXEC sp_rename N'[kitchen].[RecipeStatuses].[IX_RecipeStatuses_RecipeId_SortOrder]', N'IX_RecipeStatuses_RecipeRevisionId_SortOrder', 'INDEX';

EXEC sp_rename N'[kitchen].[RecipeShoppingItems].[RecipeId]', N'RecipeRevisionId', 'COLUMN';

EXEC sp_rename N'[kitchen].[RecipeShoppingItems].[IX_RecipeShoppingItems_RecipeId_SortOrder]', N'IX_RecipeShoppingItems_RecipeRevisionId_SortOrder', 'INDEX';

EXEC sp_rename N'[kitchen].[RecipeIngredients].[RecipeId]', N'RecipeRevisionId', 'COLUMN';

EXEC sp_rename N'[kitchen].[RecipeIngredients].[IX_RecipeIngredients_RecipeId_SortOrder]', N'IX_RecipeIngredients_RecipeRevisionId_SortOrder', 'INDEX';

EXEC sp_rename N'[kitchen].[RecipeCategories].[RecipeId]', N'RecipeRevisionId', 'COLUMN';

EXEC sp_rename N'[kitchen].[RecipeCategories].[IX_RecipeCategories_RecipeId_SortOrder]', N'IX_RecipeCategories_RecipeRevisionId_SortOrder', 'INDEX';

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

DECLARE @var8 nvarchar(max);
SELECT @var8 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[kitchen].[Recipes]') AND [c].[name] = N'Badge');
IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [kitchen].[Recipes] DROP CONSTRAINT ' + @var8 + ';');
ALTER TABLE [kitchen].[Recipes] DROP COLUMN [Badge];

DECLARE @var9 nvarchar(max);
SELECT @var9 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[kitchen].[Recipes]') AND [c].[name] = N'CookTime');
IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [kitchen].[Recipes] DROP CONSTRAINT ' + @var9 + ';');
ALTER TABLE [kitchen].[Recipes] DROP COLUMN [CookTime];

DECLARE @var10 nvarchar(max);
SELECT @var10 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[kitchen].[Recipes]') AND [c].[name] = N'GeneralNotes');
IF @var10 IS NOT NULL EXEC(N'ALTER TABLE [kitchen].[Recipes] DROP CONSTRAINT ' + @var10 + ';');
ALTER TABLE [kitchen].[Recipes] DROP COLUMN [GeneralNotes];

DECLARE @var11 nvarchar(max);
SELECT @var11 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[kitchen].[Recipes]') AND [c].[name] = N'ImageAlt');
IF @var11 IS NOT NULL EXEC(N'ALTER TABLE [kitchen].[Recipes] DROP CONSTRAINT ' + @var11 + ';');
ALTER TABLE [kitchen].[Recipes] DROP COLUMN [ImageAlt];

DECLARE @var12 nvarchar(max);
SELECT @var12 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[kitchen].[Recipes]') AND [c].[name] = N'Meal');
IF @var12 IS NOT NULL EXEC(N'ALTER TABLE [kitchen].[Recipes] DROP CONSTRAINT ' + @var12 + ';');
ALTER TABLE [kitchen].[Recipes] DROP COLUMN [Meal];

DECLARE @var13 nvarchar(max);
SELECT @var13 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[kitchen].[Recipes]') AND [c].[name] = N'Method');
IF @var13 IS NOT NULL EXEC(N'ALTER TABLE [kitchen].[Recipes] DROP CONSTRAINT ' + @var13 + ';');
ALTER TABLE [kitchen].[Recipes] DROP COLUMN [Method];

DECLARE @var14 nvarchar(max);
SELECT @var14 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[kitchen].[Recipes]') AND [c].[name] = N'PrepTime');
IF @var14 IS NOT NULL EXEC(N'ALTER TABLE [kitchen].[Recipes] DROP CONSTRAINT ' + @var14 + ';');
ALTER TABLE [kitchen].[Recipes] DROP COLUMN [PrepTime];

DECLARE @var15 nvarchar(max);
SELECT @var15 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[kitchen].[Recipes]') AND [c].[name] = N'Protein');
IF @var15 IS NOT NULL EXEC(N'ALTER TABLE [kitchen].[Recipes] DROP CONSTRAINT ' + @var15 + ';');
ALTER TABLE [kitchen].[Recipes] DROP COLUMN [Protein];

DECLARE @var16 nvarchar(max);
SELECT @var16 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[kitchen].[Recipes]') AND [c].[name] = N'Serves');
IF @var16 IS NOT NULL EXEC(N'ALTER TABLE [kitchen].[Recipes] DROP CONSTRAINT ' + @var16 + ';');
ALTER TABLE [kitchen].[Recipes] DROP COLUMN [Serves];

DECLARE @var17 nvarchar(max);
SELECT @var17 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[kitchen].[Recipes]') AND [c].[name] = N'Summary');
IF @var17 IS NOT NULL EXEC(N'ALTER TABLE [kitchen].[Recipes] DROP CONSTRAINT ' + @var17 + ';');
ALTER TABLE [kitchen].[Recipes] DROP COLUMN [Summary];

DECLARE @var18 nvarchar(max);
SELECT @var18 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[kitchen].[Recipes]') AND [c].[name] = N'Title');
IF @var18 IS NOT NULL EXEC(N'ALTER TABLE [kitchen].[Recipes] DROP CONSTRAINT ' + @var18 + ';');
ALTER TABLE [kitchen].[Recipes] DROP COLUMN [Title];

DECLARE @var19 nvarchar(max);
SELECT @var19 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[kitchen].[Recipes]') AND [c].[name] = N'CreatedByUserId');
IF @var19 IS NOT NULL EXEC(N'ALTER TABLE [kitchen].[Recipes] DROP CONSTRAINT ' + @var19 + ';');
ALTER TABLE [kitchen].[Recipes] ALTER COLUMN [CreatedByUserId] int NOT NULL;

DECLARE @var20 nvarchar(max);
SELECT @var20 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[kitchen].[Recipes]') AND [c].[name] = N'CreatedUtc');
IF @var20 IS NOT NULL EXEC(N'ALTER TABLE [kitchen].[Recipes] DROP CONSTRAINT ' + @var20 + ';');
ALTER TABLE [kitchen].[Recipes] ALTER COLUMN [CreatedUtc] datetimeoffset NOT NULL;

DECLARE @var21 nvarchar(max);
SELECT @var21 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[kitchen].[Recipes]') AND [c].[name] = N'HouseholdId');
IF @var21 IS NOT NULL EXEC(N'ALTER TABLE [kitchen].[Recipes] DROP CONSTRAINT ' + @var21 + ';');
ALTER TABLE [kitchen].[Recipes] ALTER COLUMN [HouseholdId] int NOT NULL;

CREATE INDEX [IX_Recipes_CurrentRevisionId] ON [kitchen].[Recipes] ([CurrentRevisionId]);

CREATE INDEX [IX_Recipes_HouseholdId] ON [kitchen].[Recipes] ([HouseholdId]);

CREATE UNIQUE INDEX [IX_RecipeRevisions_RecipeId_RevisionNumber] ON [kitchen].[RecipeRevisions] ([RecipeId], [RevisionNumber]);

ALTER TABLE [kitchen].[RecipeCategories] ADD CONSTRAINT [FK_RecipeCategories_RecipeRevisions_RecipeRevisionId] FOREIGN KEY ([RecipeRevisionId]) REFERENCES [kitchen].[RecipeRevisions] ([RecipeRevisionId]) ON DELETE CASCADE;

ALTER TABLE [kitchen].[RecipeIngredients] ADD CONSTRAINT [FK_RecipeIngredients_RecipeRevisions_RecipeRevisionId] FOREIGN KEY ([RecipeRevisionId]) REFERENCES [kitchen].[RecipeRevisions] ([RecipeRevisionId]) ON DELETE CASCADE;

ALTER TABLE [kitchen].[Recipes] ADD CONSTRAINT [FK_Recipes_RecipeRevisions_CurrentRevisionId] FOREIGN KEY ([CurrentRevisionId]) REFERENCES [kitchen].[RecipeRevisions] ([RecipeRevisionId]);

ALTER TABLE [kitchen].[RecipeShoppingItems] ADD CONSTRAINT [FK_RecipeShoppingItems_RecipeRevisions_RecipeRevisionId] FOREIGN KEY ([RecipeRevisionId]) REFERENCES [kitchen].[RecipeRevisions] ([RecipeRevisionId]) ON DELETE CASCADE;

ALTER TABLE [kitchen].[RecipeStatuses] ADD CONSTRAINT [FK_RecipeStatuses_RecipeRevisions_RecipeRevisionId] FOREIGN KEY ([RecipeRevisionId]) REFERENCES [kitchen].[RecipeRevisions] ([RecipeRevisionId]) ON DELETE CASCADE;

ALTER TABLE [kitchen].[RecipeSteps] ADD CONSTRAINT [FK_RecipeSteps_RecipeRevisions_RecipeRevisionId] FOREIGN KEY ([RecipeRevisionId]) REFERENCES [kitchen].[RecipeRevisions] ([RecipeRevisionId]) ON DELETE CASCADE;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260920213943_AddRecipeRevisions', N'10.0.10');

COMMIT;
GO

