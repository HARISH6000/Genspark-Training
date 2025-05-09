-- SELECT Queries
-- List all films with their length and rental rate, sorted by length descending.
-- Columns: title, length, rental_rate
select title, length, rental_rate from film
order by length desc

-- Find the top 5 customers who have rented the most films.
-- Hint: Use the rental and customer tables.
select r.customer_id, c.first_name, c.last_name,count(*) as rentalcount from rental r
join customer c on c.customer_id = r.customer_id
group by r.customer_id, c.first_name, c.last_name
order by rentalcount desc
limit 5

-- Display all films that have never been rented.
-- Hint: Use LEFT JOIN between film and inventory → rental.
select f.title from inventory i
right join film f on f.film_id = i.film_id
left join rental r on i.inventory_id = r.inventory_id
where r.rental_id is null

-- JOIN Queries
-- List all actors who appeared in the film ‘Academy Dinosaur’.
-- Tables: film, film_actor, actor
select concat(a.first_name, ' ', a.last_name) as Actors from film f
join film_actor fa on f.film_id = fa.film_id
join actor a on fa.actor_id = a.actor_id
where f.title like 'Academy Dinosaur'

-- List each customer along with the total number of rentals they made and the total amount paid.
-- Tables: customer, rental, payment
select c.customer_id, concat(c.first_name, ' ',c.last_name) as name,count(*) as Total_no_of_rents, sum(p.amount) as Total_Amount from rental r
join customer c on r.customer_id=c.customer_id
join payment p on r.rental_id=p.rental_id
group by c.customer_id, c.first_name, c.last_name
order by total_amount desc


-- CTE-Based Queries
-- Using a CTE, show the top 3 rented movies by number of rentals.
-- Columns: title, rental_count
with TopMovies as 
(
select f.film_id, f.title, count(*) as no_of_rentals from rental r
join inventory i on r.inventory_id=i.inventory_id
join film f on i.film_id=f.film_id
group by f.film_id , f.title
order by no_of_rentals desc
)

Select * from TopMovies limit 3


-- Find customers who have rented more than the average number of films.
-- Use a CTE to compute the average rentals per customer, then filter.
with NumberOfRentsPerCustomer as 
(
select r.customer_id, c.first_name, c.last_name,count(*) as rent_count from rental r
join customer c on c.customer_id = r.customer_id
group by r.customer_id, c.first_name, c.last_name
order by rent_count desc
)

Select * from  NumberOfRentsPerCustomer
where rent_count > (select sum(rent_count)/count(*) from NumberOfRentsPerCustomer)


--  Function Questions
-- Write a function that returns the total number of rentals for a given customer ID.
-- Function: get_total_rentals(customer_id INT)
 create or replace function get_total_rentals(cid int) 
 returns int as 
 $$ 
 declare total_rentals int; 
 begin
 select count(*) into total_rentals from rental 
 where customer_id = cid; 
 RETURN total_rentals; 
 END; 
 $$ 
 LANGUAGE plpgsql

select customer_id, get_total_rentals(customer_id) from customer

-- Stored Procedure Questions
-- Write a stored procedure that updates the rental rate of a film by film ID and new rate.
-- Procedure: update_rental_rate(film_id INT, new_rate NUMERIC)
 create or replace procedure update_rental_rate(fid int, new_rate numeric) 
 as 
 $$ 
 begin
 update film
 set rental_rate=new_rate
 where film_id=fid;
 END; 
 $$ 
 LANGUAGE plpgsql

call update_rental_rate(8,5.00)

select * from film where film_id=8


-- Write a procedure to list overdue rentals (return date is NULL and rental date older than 7 days).
-- Procedure: get_overdue_rentals() that selects relevant columns.

--- creating a table
 create or replace procedure get_overdue_rentals()
 as 
 $$
 begin
 create temp table if not exists overdue_table as
 	select * from rental
 	where return_date is null and rental_date < now() - interval '7 days';
 end; 
 $$ 
 LANGUAGE plpgsql

call get_overdue_rentals()

select * from overdue_table;

--- alternatively using raise

 create or replace procedure get_overdue_rentals()
 as 
 $$
 declare
  row record;
 begin
 for row in
 	select * from rental
 	where return_date is null and rental_date < now() - interval '7 days'
 loop
  raise notice 'rental_id: %, customer_id: %, rental_date: %, inventory_id: %',
            row.rental_id, row.customer_id, row.rental_date, row.inventory_id;
 end loop;
 end; 
 $$ 
 LANGUAGE plpgsql

call get_overdue_rentals()

