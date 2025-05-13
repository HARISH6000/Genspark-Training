-- Transaction A

-- 1. Try two concurrent updates to same row → see lock in action.
begin;
update film set rental_rate = 5.99 where film_id = 1;

commit;

-- 2. Write a query using SELECT...FOR UPDATE and check how it locks row.
show transaction_isolation;

begin;
select * from film where film_id = 1 for update;
update film set rental_rate = 5.99 where film_id = 1;
commit;
-- 3. Intentionally create a deadlock and observe PostgreSQL cancel one transaction.
begin;
update film set rental_rate = 5.99 where film_id = 1;

update film set rental_rate = 5.99 where film_id = 2;

--ERROR:  deadlock detected
--Process 13796 waits for ShareLock on transaction 1297; blocked by process 1296.
--Process 1296 waits for ShareLock on transaction 1296; blocked by process 13796. 

--SQL state: 40P01
--Detail: Process 13796 waits for ShareLock on transaction 1297; blocked by process 1296.
--Process 1296 waits for ShareLock on transaction 1296; blocked by process 13796.
--Hint: See server log for query details.
--Context: while locking tuple (53,15) in relation "film"

-- 4. Use pg_locks query to monitor active locks.

select pid, locktype, relation::regclass, mode, granted
from pg_locks
where locktype = 'relation' or locktype = 'tuple';

select * from pg_locks;