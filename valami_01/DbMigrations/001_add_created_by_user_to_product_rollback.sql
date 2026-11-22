-- Rollback for 001_add_created_by_user_to_product.sql
-- Removes foreign key, index and column created_by_user_id from product

-- Drop foreign key if exists
ALTER TABLE `product` DROP FOREIGN KEY IF EXISTS `fk_product_created_by_user`;

-- Drop index if exists
DROP INDEX IF EXISTS `ix_product_created_by_user_id` ON `product`;

-- Drop column
ALTER TABLE `product` DROP COLUMN IF EXISTS `created_