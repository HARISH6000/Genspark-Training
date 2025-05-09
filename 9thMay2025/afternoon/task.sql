
-- Cursor-Based Questions (5)
-- Write a cursor that loops through all films and prints titles longer than 120 minutes.
do $$
declare
    cursor_film cursor for select title from film where length > 120;
    title text;
begin
    for title in cursor_film loop
        raise notice 'Title: %', title;
    end loop;
end $$;

-- Create a cursor that iterates through all customers and counts how many rentals each made.
do $$
declare
	cursor_rentals cursor for 
		select c.customer_id, c.first_name, c.last_name,count(*) as rental_count 
		from rental r
		join customer c on c.customer_id = r.customer_id
		group by c.customer_id, c.first_name, c.last_name
		order by c.customer_id;
    rec record;
begin
    for rec in cursor_rentals 
	loop
        raise notice 'cust_id: %, first_name: %, last name: %, count: %', 
		rec.customer_id, rec.first_name, rec.last_name, rec.rental_count;
    end loop;
end $$;


-- Using a cursor, update rental rates: Increase rental rate by $1 for films with less than 5 rentals.
do $$
declare
	cursor_rental_update cursor for
		select f.film_id, f.rental_rate from film f
		join
        (
			select film_id, count(rental_id) as rental_count
			from inventory i
			left join rental r on i.inventory_id = r.inventory_id
			group by film_id
		) rental_stats 
		on f.film_id = rental_stats.film_id
		where rental_stats.rental_count < 5;
	rec record;
begin
	for rec in cursor_rental_update loop
    	update film
    	set rental_rate = rec.rental_rate + 1
    	where film_id = rec.film_id;
		raise notice 'Updated film_id: %, new rental_rate: %', rec.film_id, rec.rental_rate + 1;
	end loop;
end $$;


-- Create a function using a cursor that collects titles of all films from a particular category.

create or replace function films_by_category(cname text)
returns void
as
$$
declare
	cursor_films_by_category cursor for
		select f.title, c.name from film_category fc
		join film f on fc.film_id=f.film_id
		join category c on fc.category_id=c.category_id
		where c.name= cname;
	rec record;
begin
	for rec in cursor_films_by_category loop
		raise notice 'Title: %',rec.title;
	end loop;
end;
$$
language plpgsql

select * from films_by_category('Comedy');


-- Loop through all stores and count how many distinct films are available in each store using a cursor.
do
$$
declare
	cursor_films_per_store cursor for
		select store_id, count(film_id) as film_count from inventory
		group by store_id;
	rec record;
begin
	for rec in cursor_films_per_store loop
		raise notice 'store id: %, count: %',rec.store_id, rec.film_count;
	end loop;
end;
$$


-- Trigger-Based Questions (5)
-- Write a trigger that logs whenever a new customer is inserted.
create or replace function insert_trigger()
returns trigger 
as $$
begin
	raise notice 'New customer with id: % is inserted',New.customer_id;
	return new;
end;
$$ 
language plpgsql
 
create trigger trigger_customerinsert 
after insert on customer
for each row
execute function insert_trigger();
 
insert into customer (store_id, first_name, last_name, email, address_id, activebool, create_date, last_update, active)
VALUES (1, 'Edward', 'Kenway', 'edwardk@gmail.com', 100, TRUE, '2025-05-09', '2025-05-09', 1);

select * from customer where email = 'edwardk@gmail.com';

-- Create a trigger that prevents inserting a payment of amount 0.
create or replace function payment_trigger_func()
returns trigger 
as $$
begin
	if new.amount=0.00 then 
		raise exception 'Amount cannot be 0';
	end if;
	return new;
end;
$$ 
language plpgsql;
 
create or replace trigger trigger_payment_insert 
after insert on payment
for each row
execute function payment_trigger_func();
 
insert into payment (payment_id, customer_id, staff_id, rental_id, amount, payment_date)
values (1, 1, 1, 2, 0.00, '2025-05-09');

-- Set up a trigger to automatically set last_update on the film table before update.
create or replace function last_update_function()
returns trigger 
as $$
begin
	new.last_update=now();
	raise notice 'Last updated set using trigger';
	return new;
end;
$$ 
language plpgsql;
 
create or replace trigger trigger_film_update
after update on film
for each row
execute function last_update_function();
 
select * from film where film_id=2;
update film set title='Ace Goldfinger' where film_id=2;

-- Create a trigger to log changes in the inventory table (insert/delete).
create or replace function inventory_log()
returns trigger 
as $$
begin
	if TG_OP='INSERT' then
		raise notice 'Data inserted to inventory with id %',new.inventory_id;
		return new;
	end if;
	if TG_OP='DELETE' then 
		raise notice 'Data with id % has been deleted',old.inventory_id;
		return new;
	end if;
	return null;
end;
$$ language plpgsql;
 
create or replace trigger trigger_inventory_insert
after insert on inventory
for each row 
execute function inventory_log();
 
create or replace trigger trigger_inventory_delete
after delete on inventory
for each row 
execute function inventory_log();
 
select * from inventory;
select * from rental where inventory_id=24;
delete from payment where rental_id in (2255,5362,7793,13043);
delete from rental where inventory_id=24;
delete from inventory where inventory_id=24;

-- Write a trigger that ensures a rental can’t be made for a customer who owes more than $50.

-- Transaction-Based Questions (5)
-- Write a transaction that inserts a customer and an initial rental in one atomic operation.
do $$
declare new_customer_id int;
begin
    insert into customer (store_id, address_id, first_name, last_name, email, create_date)
    values (1, 1, 'John', 'Doe', 'johndoe@example.com', now())
    returning customer_id into new_customer_id;
 
    insert into public.rental (rental_date, inventory_id, customer_id, staff_id, return_date)
    values (now(), 1, new_customer_id, 1, null);
end;
$$;

-- Simulate a failure in a multi-step transaction (update film + insert into inventory) and roll back.
do $$
begin
    update film 
    set rental_rate = 4.99
    WHERE film_id = 1;
 
    insert into inventory (film_id, store_id) 
    VALUES (10000, 1);
 
exception
    when others then
        ROLLBACK;
        raise notice 'Transaction rolled back due to error.';
end;
$$;

select * from film where film_id=1

-- Create a transaction that transfers an inventory item from one store to another.

select * from inventory where inventory_id = 1

do $$
declare 
	v_inventory_id INT := 1;
	v_from_store_id INT := 1;
	v_to_store_id INT := 2;
Begin
    update inventory
    set store_id = v_to_store_id
    where inventory_id = v_inventory_id
    and store_id = v_from_store_id;
 
    if not found then
        raise exception 'Inventory item % not found in store %', v_inventory_id, v_from_store_id;
    end if;
 
    raise notice 'Inventory item % moved from store % to store %', v_inventory_id, v_from_store_id, v_to_store_id;
end;
$$;

select * from inventory where inventory_id = 1

-- Demonstrate SAVEPOINT and ROLLBACK TO SAVEPOINT by updating payment amounts, then undoing one.
begin;

update payment
set amount = amount + 1
where payment_id = 17503;
 
savepoint payment_update;

select * from payment where payment_id=17503

update payment
set amount = amount + 10
where payment_id = 17503;

select * from payment where payment_id=17503

rollback to savepoint payment_update;

select * from payment where payment_id=17503

commit;

-- Write a transaction that deletes a customer and all associated rentals and payments, ensuring atomicity.
-- Procedure: get_overdue_rentals() that selects relevant columns.
do $$
declare v_customer_id INT := 155;
begin
    delete from payment
    where customer_id = v_customer_id;
 
    delete from rental
    where customer_id = v_customer_id;
 
    delete from customer
    where customer_id = v_customer_id;
end;
$$;

call get_overdue_rentals()
select * from customer where customer_id=155