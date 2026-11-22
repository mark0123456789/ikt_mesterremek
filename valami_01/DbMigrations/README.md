Migration 001: add created_by_user_id to product
=============================================

This migration adds a nullable created_by_user_id column to the product table and a foreign key to appuser(id).

Run:

  mysql -u root -p warehouse < 001_add_created_by_user_to_product.sql

Backup first:

  mysqldump -u root -p warehouse > warehouse_backup.sql

Rollback:

  mysql -u root -p warehouse < 001_add_created_by_user_to_product_rollback.sql

Notes:
- The migration attempts to add a foreign key constraint. If your tables are not InnoDB or you prefer not to add FK, edit the SQL and remove the ADD CONSTRAINT line.
- Tested on a typical MySQL / MariaDB setup.
