--Transaction B

-- 1. Try two concurrent updates to same row → see lock in action.
begin;
update film set rental_rate = 6.99 where film_id = 1;
commit;

select * from film where film_id = 1;

-- 2. Write a query using SELECT...FOR UPDATE and check how it locks row.
begin;
update film set rental_rate = 6.99 where film_id = 1;
commit;

select 
    pg_locks.pid, 
    pg_locks.locktype, 
    pg_class.relname as relation, 
    pg_locks.mode, 
    pg_locks.granted
from pg_locks
join pg_class on pg_locks.relation = pg_class.oid
where pg_class.relname = 'film';

-- 3. Intentionally create a deadlock and observe PostgreSQL cancel one transaction.
begin;
update film set rental_rate = 6.99 where film_id = 2;

update film set rental_rate = 6.99 where film_id = 1;



