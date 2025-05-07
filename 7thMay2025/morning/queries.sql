--print the publisher deatils of the publisher who has never published
Select pub_name from publishers where pub_id not in (Select DISTINCT(pub_id) from titles);

--Select the author_id for all the books. Print the author_id and the book name
select au_id as 'Author Id', title as 'Book name' from titleauthor 
join titles on titleauthor.title_id = titles.title_id;

--Print the publisher's name, book name and the order date of the  books
select p.pub_name, t.title, s.ord_date from titles t 
join publishers p on t.pub_id=p.pub_id 
join sales s on t.title_id=s.title_id;

--Print the publisher name and the first book sale date for all the publishers
select p.pub_name as Publisher_Name, min(s.ord_date) as First_Order_Date
from titles t right outer join publishers p on t.pub_id=p.pub_id left outer
join sales s on t.title_id=s.title_id 
group by p.pub_name
order by 2 desc;

--print the bookname and the store address of the sale
select t.title as 'Book Name', st.stor_address as 'Store Address' from sales s 
join titles t on s.title_id=t.title_id
join stores st on s.stor_id= st.stor_id;

--Procedures
create procedure proc_FirstProcedure
as
begin
	print 'Hello world'
end

exec proc_FirstProcedure


create table Products
(id int identity(1,1) constraint pk_productId primary key,
name nvarchar(100) not null,
details nvarchar(max))
Go
create proc proc_InsertProduct(@pname nvarchar(100),@pdetails nvarchar(max))
as
begin
    insert into Products(name,details) values(@pname,@pdetails)
end
go
proc_InsertProduct 'Laptop','{"brand":"Dell","spec":{"ram":"16GB","cpu":"i5"}}'
go
select * from Products


select JSON_QUERY(details, '$.spec') Product_Specification  from products;

create proc proc_UpdateProductSpec(@pid int,@newvalue varchar(20))
as
begin
   update products set details = JSON_MODIFY(details, '$.spec.ram',@newvalue) where id = @pid
end

proc_UpdateProductSpec 1, '32GB'

select JSON_QUERY(details, '$.spec') Product_Specification from products


select id, name, JSON_VALUE(details, '$.brand') Brand_Name
from Products

create table Posts(
  id int primary key,
  title nvarchar(100),
  user_id int,
  body nvarchar(max)
)

select * from Posts

create procedure proc_BulkInsertPosts(@jsondata nvarchar(max))
as
begin
	insert into Posts(user_id,id,title,body)
	select userId,id,title,body from openjson(@jsondata)
	with (userId int,id int, title nvarchar(100), body nvarchar(max))
end



proc_BulkInsertPosts '
[
  {
    "userId": 1,
    "id": 1,
    "title": "sunt aut facere repellat provident occaecati excepturi optio reprehenderit",
    "body": "quia et suscipit\nsuscipit recusandae consequuntur expedita et cum\nreprehenderit molestiae ut ut quas totam\nnostrum rerum est autem sunt rem eveniet architecto"
  },
  {
    "userId": 1,
    "id": 2,
    "title": "qui est esse",
    "body": "est rerum tempore vitae\nsequi sint nihil reprehenderit dolor beatae ea dolores neque\nfugiat blanditiis voluptate porro vel nihil molestiae ut reiciendis\nqui aperiam non debitis possimus qui neque nisi nulla"
  }]'

select * from Posts

select * from products where try_cast(json_value(details,'$.spec.cpu') as nvarchar(20)) ='i5';

--create a procedure that brings post by taking the user_id as parameter

select * from Posts

create procedure proc_FetchPostByUserId(@uid int)
as
begin
	select * from posts where user_id=@uid;
end

proc_FetchPostByUserId 1
