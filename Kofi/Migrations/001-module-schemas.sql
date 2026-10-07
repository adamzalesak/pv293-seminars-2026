BEGIN;
CREATE SCHEMA IF NOT EXISTS catalog;
CREATE SCHEMA IF NOT EXISTS warehouse;
CREATE SCHEMA IF NOT EXISTS ordering;
CREATE SCHEMA IF NOT EXISTS subscriptions;
ALTER TABLE public."Products" SET SCHEMA catalog;
ALTER TABLE public."Orders" SET SCHEMA ordering;
ALTER TABLE public."OrderItems" SET SCHEMA ordering;
ALTER TABLE public."Customers" SET SCHEMA ordering;
ALTER TABLE public."Subscriptions" SET SCHEMA subscriptions;
ALTER TABLE public."SubscriptionDeliveries" SET SCHEMA subscriptions;
ALTER TABLE public."SubscriptionDeliveryExtras" SET SCHEMA subscriptions;
CREATE TABLE warehouse."Products" (
    "ProductId" uuid PRIMARY KEY, "StockQuantity" integer NOT NULL,
    "ShelfLocation" text NOT NULL, "Version" integer NOT NULL);
INSERT INTO warehouse."Products" ("ProductId","StockQuantity","ShelfLocation","Version")
SELECT "Id","StockQuantity",'A-01',"Version" FROM catalog."Products";
ALTER TABLE catalog."Products" DROP COLUMN "StockQuantity";
DO $migration$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.columns
        WHERE table_schema='catalog' AND table_name='Products' AND column_name='IsActive') THEN
        ALTER TABLE catalog."Products" ADD COLUMN "Status" text;
        UPDATE catalog."Products" SET "Status"=CASE WHEN "IsActive" THEN 'Active' ELSE 'Inactive' END;
        ALTER TABLE catalog."Products" ALTER COLUMN "Status" SET NOT NULL;
        ALTER TABLE catalog."Products" DROP COLUMN "IsActive";
    END IF;
END;
$migration$;
COMMIT;
