-- Out parameters
  
  select * from products where 
  try_cast(json_value(details,'$.spec.cpu') as nvarchar(20)) ='i7'

-- The above query is made into a procedure
create proc proc_FilterProducts(@pcpu varchar(20), @pcount int out)
as
begin
  set @pcount = (select count(*) from products where 
  try_cast(json_value(details,'$.spec.cpu') as nvarchar(20)) =@pcpu)
end

begin
  declare @cnt int
  exec proc_FilterProducts 'i7', @cnt out
  print concat('The number of computers is ',@cnt)
end

-- sp_help Table name (a predefined stored procedure)

-- Bulk Insert
create table people
(id int primary key,
name nvarchar(20),
age int)

create or alter proc proc_BulkInsert(@filepath nvarchar(500))
as
begin
   declare @insertQuery nvarchar(max)

   set @insertQuery = 'BULK INSERT people from '''+ @filepath +'''
   with(
   FIRSTROW =2,
   FIELDTERMINATOR='','',
   ROWTERMINATOR = ''\n'')'
   exec sp_executesql @insertQuery
end

proc_BulkInsert 'C:\PresidioTraining\8thMay2025\morning\Data.csv'

select * from people

-- Try catch for error handling and logging

truncate table people

create table BulkInsertLog
(LogId int identity(1,1) primary key,
FilePath nvarchar(1000),
status nvarchar(50) constraint chk_status Check(status in('Success','Failed')),
Message nvarchar(1000),
InsertedOn DateTime default GetDate())


create or alter proc proc_BulkInsert(@filepath nvarchar(500))
as
begin
  Begin try
	   declare @insertQuery nvarchar(max)

	   set @insertQuery = 'BULK INSERT people from '''+ @filepath +'''
	   with(
	   FIRSTROW =2,
	   FIELDTERMINATOR='','',
	   ROWTERMINATOR = ''\n'')'

	   exec sp_executesql @insertQuery

	   insert into BulkInsertLog(filepath,status,message)
	   values(@filepath,'Success','Bulk insert completed')
  end try
  begin catch
		 insert into BulkInsertLog(filepath,status,message)
		 values(@filepath,'Failed',Error_Message())
  END Catch
end

proc_BulkInsert 'C:\PresidioTraining\8thMay2025\morning\Data.csv'

select * from BulkInsertLog

-- Common Table Expression CTE
with cteAuthors
as
( select au_id, concat(au_fname,' ',au_lname) author_name from authors )

select * from cteAuthors


declare @page int =1, @pageSize int=10;
with PaginatedBooks as
( select  title_id,title, price, ROW_Number() over (order by price desc) as RowNum
  from titles
)
select * from PaginatedBooks where rowNUm between((@page-1)*@pageSize) and (@page*@pageSize)

--create a sp that will take the page number and size as param and print the books

create or alter procedure proc_iterateBooks(@pageno int, @size int)
as
begin
	with PaginatedBooks as
	( select  title_id,title, price, ROW_Number() over (order by price desc) as RowNum
	  from titles
	)
	select * from PaginatedBooks where rowNUm between((@pageno-1)*@size+1) and (@pageno*@size);
end;

exec proc_iterateBooks 1, 10

--Offset and Fetch

select  title_id,title, price
from titles
order by price desc
offset 10 rows 
fetch next 10 rows only

--functions

  create or alter function  fn_CalculateTax(@baseprice float, @tax float)
  returns float
  as
  begin
     return (@baseprice +(@baseprice*@tax/100))
  end

  select dbo.fn_CalculateTax(1000,10)

  select title,price ,dbo.fn_CalculateTax(price,12) as Final_Price from titles

  create function fn_tableSample(@minprice float)
  returns table
  as
  return select title,price from titles where price>= @minprice

  select * from dbo.fn_tableSample(20)


---pgexercise joins and subqurries
--1
select starttime from cd.members m 
join cd.bookings b on m.memid = b.memid 
where firstname like 'David' and surname like 'Farrell';

--2
select starttime as start, name from cd.bookings b
join cd.facilities f on b.facid = f.facid 
where b.starttime::date = '2012-09-21' and f.name like 'Tennis Court%'
order by b.starttime

--3
select firstname, surname from cd.members 
where memid in (select distinct(recommendedby) from cd.members)
order by surname, firstname

--4
select m.firstname, m.surname, r.firstname, r.surname from cd.members m
left join cd.members r on m.recommendedby = r.memid
order by m.surname, m.firstname

--5
select distinct concat(firstname,' ',surname) as member, f.name from cd.members m
join cd.bookings b on m.memid = b.memid
join cd.facilities f on b.facid = f.facid
where f.name like 'Tennis%'
Order by member, name

--6
select concat(firstname, ' ',surname), name,
case when m.memid = 0 then
	b.slots*f.guestcost
else
	b.slots*f.membercost
end as cost
from cd.members m 
join cd.bookings b on m.memid = b.memid
join cd.facilities f on b.facid = f.facid
where b.starttime::date = '2012-09-14'
and ((m.memid!=0 and b.slots*f.membercost >30) or (m.memid=0 and b.slots*f.guestcost >30))
order by cost desc