SELECT * FROM tbl_bank_accounts;

-- Trans B
BEGIN;
SELECT * FROM tbl_bank_accounts 
WHERE account_id = 1;
commit

-- Trans B REPEATABLE READ
BEGIN;
UPDATE tbl_bank_accounts
SET balance = 4000
WHERE account_id = 1;
COMMIT;

-- Trans B REPEATABLE COMMITTED
BEGIN;
UPDATE tbl_bank_accounts
SET balance = 5000
WHERE account_id = 1;
COMMIT;

-- Trans B READ UNCOMMITTED
BEGIN;
UPDATE tbl_bank_accounts
SET balance = 3000
WHERE account_id = 1;

COMMIT;

-- Trans B READ SERIALIZABLE
BEGIN;
UPDATE tbl_bank_accounts
SET balance = 6000
WHERE account_id = 1;

COMMIT;

