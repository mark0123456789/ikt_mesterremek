-- Migration: add created_by_user_id to product
-- Backup your database before running.
-- Adds nullable created_by_user_id and a foreign key to appuser(id).

ALTER TABLE `product`
  ADD COLUMN `created_by_user_id` INT NULL;

-- Add a foreign key constraint if appuser.id exists and product table uses InnoDB.
-- If your product or appuser tables are not InnoDB, either convert engine or skip FK.
ALTER TABLE `product`
  ADD CONSTRAINT `fk_product_created_by_user` FOREIGN KEY (`created_by_user_id`) REFERENCES `appuser`(`id`) ON DELETE SET NULL ON UPDATE CASCADE;

-- Optional: add index for faster lookup
CREATE INDEX `ix_product_created_by_user_id` ON `product` (`created_by_user_id`);
