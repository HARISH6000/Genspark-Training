CREATE TABLE tbl_bank_accounts
(
account_id SERIAL PRIMARY KEY,
account_name VARCHAR(100),
balance NUMERIC(10, 2)
);

INSERT INTO tbl_bank_accounts
(account_name, balance)
VALUES
('Alice', 5000.00),
('Bob', 3000.00);

SELECT * FROM tbl_bank_accounts;


BEGIN;

UPDATE tbl_bank_accounts
SET balance = balance - 500
WHERE account_name = 'Alice';

UPDATE tbl_bank_accounts
SET balance = balance + 500
WHERE account_name = 'Bob';

COMMIT;

SELECT * FROM tbl_bank_accounts;

-- Introducing Error (Rollback)
BEGIN;

UPDATE tbl_bank_accounts
SET balance = balance - 500
WHERE account_name = 'Alice';

UPDATE tbl_bank_account
SET balance = balance + 500
WHERE account_name = 'Bob';

ROLLBACK;

SELECT * FROM tbl_bank_accounts;

BEGIN;

UPDATE tbl_bank_accounts
SET balance = balance - 100
WHERE account_name = 'Alice';

SAVEPOINT after_alice;

UPDATE tbl_bank_account
SET balance = balance + 100
WHERE account_name = 'Bob';

ROLLBACK TO after_alice;

UPDATE tbl_bank_accounts
SET balance = balance + 100
WHERE account_name = 'Bob';

COMMIT;

SELECT * FROM tbl_bank_accounts;

BEGIN;
DO $$
DECLARE
  current_balance NUMERIC;
BEGIN
SELECT balance INTO current_balance
FROM tbl_bank_accounts
WHERE account_name = 'Alice';

IF current_balance >= 1500 THEN
   UPDATE tbl_bank_accounts SET balance = balance - 1500 WHERE account_name = 'Alice';
   UPDATE tbl_bank_accounts SET balance = balance + 1500 WHERE account_name = 'Bob';
ELSE
   RAISE NOTICE 'Insufficient Funds!';
   ROLLBACK;
END IF;
END;
$$;
commit
select * from tbl_bank_accounts

START TRANSACTION;
UPDATE tbl_bank_accounts
SET balance = balance + 500
WHERE account_name = 'Alice';

SELECT * FROM tbl_bank_accounts;
-- At this point, change is not committed yet.
COMMIT; -- Change is permanently saved.


-- Trans A

BEGIN;
UPDATE tbl_bank_accounts
SET balance = balance + 500
WHERE account_id = 1;
commit

--repeatable reads

-- Trans A REPEATABLE READ
BEGIN ISOLATION LEVEL REPEATABLE READ;
SELECT * FROM tbl_bank_accounts 
WHERE account_id = 1;

SELECT * FROM tbl_bank_accounts 
WHERE account_id = 1;
COMMIT

-- Trans A
BEGIN ISOLATION LEVEL READ COMMITTED;
SELECT * FROM tbl_bank_accounts 
WHERE account_id = 1;

SELECT * FROM tbl_bank_accounts 
WHERE account_id = 1;
COMMIT

-- Trans A READ UNCOMMITTED 
BEGIN ISOLATION LEVEL READ UNCOMMITTED;
SELECT * FROM tbl_bank_accounts 
WHERE account_id = 1;

SELECT * FROM tbl_bank_accounts 
WHERE account_id = 1;
COMMIT
-- behaves same as READ COMMITTED

-- Trans A SERIALIZABLE
BEGIN ISOLATION LEVEL SERIALIZABLE;
SELECT * FROM tbl_bank_accounts 
WHERE account_id = 1;

SELECT * FROM tbl_bank_accounts 
WHERE account_id = 1;
COMMIT

