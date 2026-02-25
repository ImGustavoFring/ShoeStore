CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
    "ProductVersion" TEXT NOT NULL
);

BEGIN TRANSACTION;
CREATE TABLE "Articles" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Articles" PRIMARY KEY AUTOINCREMENT,
    "Title" TEXT NOT NULL
);

CREATE TABLE "Categories" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Categories" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL
);

CREATE TABLE "Manufacturers" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Manufacturers" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL
);

CREATE TABLE "PickUpPoints" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_PickUpPoints" PRIMARY KEY AUTOINCREMENT,
    "PostCode" INTEGER NOT NULL,
    "City" TEXT NOT NULL,
    "Street" TEXT NOT NULL,
    "House" INTEGER NOT NULL
);

CREATE TABLE "Roles" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Roles" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL
);

CREATE TABLE "Statuses" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Statuses" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL
);

CREATE TABLE "Suppliers" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Suppliers" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL
);

CREATE TABLE "Units" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Units" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL
);

CREATE TABLE "Users" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Users" PRIMARY KEY AUTOINCREMENT,
    "RoleId" INTEGER NOT NULL,
    "FullName" TEXT NOT NULL,
    "Login" TEXT NOT NULL,
    "Password" TEXT NOT NULL,
    CONSTRAINT "FK_Users_Roles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "Roles" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "Products" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Products" PRIMARY KEY AUTOINCREMENT,
    "ArticleId" INTEGER NOT NULL,
    "Name" TEXT NOT NULL,
    "UnitId" INTEGER NOT NULL,
    "Price" TEXT NOT NULL,
    "SupplierId" INTEGER NOT NULL,
    "ManufacturerId" INTEGER NOT NULL,
    "CategoryId" INTEGER NOT NULL,
    "Discount" TEXT NOT NULL,
    "QuantityInStock" INTEGER NOT NULL,
    "Description" TEXT NOT NULL,
    "PhotoPath" TEXT NULL,
    CONSTRAINT "FK_Products_Articles_ArticleId" FOREIGN KEY ("ArticleId") REFERENCES "Articles" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Products_Categories_CategoryId" FOREIGN KEY ("CategoryId") REFERENCES "Categories" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Products_Manufacturers_ManufacturerId" FOREIGN KEY ("ManufacturerId") REFERENCES "Manufacturers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Products_Suppliers_SupplierId" FOREIGN KEY ("SupplierId") REFERENCES "Suppliers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Products_Units_UnitId" FOREIGN KEY ("UnitId") REFERENCES "Units" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "Orders" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Orders" PRIMARY KEY AUTOINCREMENT,
    "Number" INTEGER NOT NULL,
    "OrderDate" TEXT NOT NULL,
    "DeliveryDate" TEXT NOT NULL,
    "PickUpPointId" INTEGER NOT NULL,
    "UserId" INTEGER NOT NULL,
    "ReceiptCode" INTEGER NOT NULL,
    "StatusId" INTEGER NOT NULL,
    CONSTRAINT "FK_Orders_PickUpPoints_PickUpPointId" FOREIGN KEY ("PickUpPointId") REFERENCES "PickUpPoints" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Orders_Statuses_StatusId" FOREIGN KEY ("StatusId") REFERENCES "Statuses" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Orders_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "OrderItems" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_OrderItems" PRIMARY KEY AUTOINCREMENT,
    "ArticleId" INTEGER NOT NULL,
    "OrderId" INTEGER NOT NULL,
    "Quantity" INTEGER NOT NULL,
    "Price" TEXT NOT NULL,
    CONSTRAINT "FK_OrderItems_Articles_ArticleId" FOREIGN KEY ("ArticleId") REFERENCES "Articles" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_OrderItems_Orders_OrderId" FOREIGN KEY ("OrderId") REFERENCES "Orders" ("Id") ON DELETE CASCADE
);

CREATE UNIQUE INDEX "IX_Articles_Title" ON "Articles" ("Title");

CREATE UNIQUE INDEX "IX_Categories_Name" ON "Categories" ("Name");

CREATE UNIQUE INDEX "IX_Manufacturers_Name" ON "Manufacturers" ("Name");

CREATE INDEX "IX_OrderItems_ArticleId" ON "OrderItems" ("ArticleId");

CREATE INDEX "IX_OrderItems_OrderId" ON "OrderItems" ("OrderId");

CREATE UNIQUE INDEX "IX_Orders_Number" ON "Orders" ("Number");

CREATE INDEX "IX_Orders_PickUpPointId" ON "Orders" ("PickUpPointId");

CREATE INDEX "IX_Orders_StatusId" ON "Orders" ("StatusId");

CREATE INDEX "IX_Orders_UserId" ON "Orders" ("UserId");

CREATE UNIQUE INDEX "IX_PickUpPoints_PostCode_City_Street_House" ON "PickUpPoints" ("PostCode", "City", "Street", "House");

CREATE UNIQUE INDEX "IX_Products_ArticleId" ON "Products" ("ArticleId");

CREATE INDEX "IX_Products_CategoryId" ON "Products" ("CategoryId");

CREATE INDEX "IX_Products_ManufacturerId" ON "Products" ("ManufacturerId");

CREATE INDEX "IX_Products_SupplierId" ON "Products" ("SupplierId");

CREATE INDEX "IX_Products_UnitId" ON "Products" ("UnitId");

CREATE UNIQUE INDEX "IX_Roles_Name" ON "Roles" ("Name");

CREATE UNIQUE INDEX "IX_Statuses_Name" ON "Statuses" ("Name");

CREATE UNIQUE INDEX "IX_Suppliers_Name" ON "Suppliers" ("Name");

CREATE UNIQUE INDEX "IX_Units_Name" ON "Units" ("Name");

CREATE UNIQUE INDEX "IX_Users_Login" ON "Users" ("Login");

CREATE INDEX "IX_Users_RoleId" ON "Users" ("RoleId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260225150252_InitialCreate', '10.0.3');

COMMIT;

