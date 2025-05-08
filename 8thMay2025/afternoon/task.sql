create database northwind;
go
use northwind;

-- ran northwind script


--1) List all orders with the customer name and the employee who handled the order.

--(Join Orders, Customers, and Employees)

select o.OrderID, c.CompanyName as Customer_Name, concat(e.FirstName,' ',e.LastName) as Employee_Name 
from orders o
join Customers c on o.CustomerID=c.CustomerID
join Employees e on o.EmployeeID=e.EmployeeID;

--2) Get a list of products along with their category and supplier name.

--(Join Products, Categories, and Suppliers)
select p.ProductID, p.ProductName, c.CategoryName, s.CompanyName as SupplierName
from Products p
join Categories c on p.CategoryID=c.CategoryID
join Suppliers s on p.SupplierID=s.SupplierID;

-- 3) Show all orders and the products included in each order with quantity and unit price.

-- (Join Orders, Order Details, Products)
select o.OrderID, p.ProductName, od.Quantity, od.UnitPrice
from Orders o
join [Order Details] od on o.OrderId = od.OrderId
join Products p on od.ProductID=p.ProductID;


-- 4) List employees who report to other employees (manager-subordinate relationship).

-- (Self join on Employees)
select concat(e1.FirstName,' ',e1.LastName) as Employee_Name, concat(e2.FirstName,' ',e2.LastName) as Reports_to
from Employees e1
join Employees e2 on e1.ReportsTo=e2.EmployeeID;


-- 5) Display each customer and their total order count.

-- (Join Customers and Orders, then GROUP BY)
select o.CustomerId, c.CompanyName, o.ordercount 
from (select CustomerID, count(*)as ordercount
from Orders 
group by CustomerID) o
join Customers c on o.CustomerID = c.CustomerID



-- 6) Find the average unit price of products per category.

-- Use AVG() with GROUP BY
select p.CategoryID, c.CategoryName,avg(UnitPrice) as Average_Unit_Price
from Products p
join Categories c on p.CategoryID=c.CategoryID
group by p.CategoryID , c.CategoryName



-- 7) List customers where the contact title starts with 'Owner'.

-- Use LIKE or LEFT(ContactTitle, 5)
select * from customers
where ContactTitle like 'Owner';


-- 8) Show the top 5 most expensive products.

-- Use ORDER BY UnitPrice DESC and TOP 5
select top 5 * from Products
order by UnitPrice desc;

-- 9) Return the total sales amount (quantity × unit price) per order.

-- Use SUM(OrderDetails.Quantity * OrderDetails.UnitPrice) and GROUP BY
select o.OrderID, sum(od.Quantity*od.UnitPrice)as Total from Orders o
join [Order Details] od on o.OrderID=od.OrderID
group by o.OrderID;


-- 10) Create a stored procedure that returns all orders for a given customer ID.

-- Input: @CustomerID
create or alter procedure proc_GetOrderByCustomerId (@Cusid nvarchar(20))
as
begin
	select * from Orders where CustomerID=@Cusid
end
go
proc_GetOrderByCustomerId 'VINET'




-- 11) Write a stored procedure that inserts a new product.

-- Inputs: ProductName, SupplierID, CategoryID, UnitPrice, etc.
create or alter procedure proc_AddProduct
(@name nvarchar(20), @sid int,@cid int, @qnt nvarchar(100), @up int, @uis int, @uoo int, @rol int, @discontinued int)
as
begin
	insert into Products(ProductName,SupplierID,CategoryID,QuantityPerUnit,UnitPrice,UnitsInStock,UnitsOnOrder,ReorderLevel,Discontinued)
	values (@name,@sid,@cid,@qnt,@up,@uis,@uoo,@rol,@discontinued)
end

proc_AddProduct 'Tea', 1,1,'10 boxes x 20 bags', 20.0, 40, 0, 10,0

select * from Products

-- 12) Create a stored procedure that returns total sales per employee.

-- Join Orders, Order Details, and Employees
create or alter proc proc_GetSalesForEmployee
as 
begin
	select o.EmployeeId, sum(od.UnitPrice*od.Quantity) as totalsales 
	from Orders o join [Order Details] od on o.OrderID=od.OrderID 
	group by o.EmployeeID;
end
 
proc_GetSalesForEmployee


-- 13) Use a CTE to rank products by unit price within each category.

-- Use ROW_NUMBER() or RANK() with PARTITION BY CategoryID
with RankedProducts as 
( select ProductID, ProductName, CategoryID, 
  ROW_NUMBER() over (partition by CategoryID order by UnitPrice desc) AS RankWithinCategory
  from Products     
)

Select * from RankedProducts
order by CategoryID, RankWithinCategory;


-- 14) Create a CTE to calculate total revenue per product and filter products with revenue > 10,000.
with RevenuePerProduct as 
( select p.ProductID, p.ProductName, sum(od.UnitPrice*od.Quantity) as Total_Revenue
  from Products p
  join [Order Details] od on p.ProductID=od.ProductID
  group by od.ProductID, p.ProductID, p.ProductName
)

Select * from RevenuePerProduct
where Total_Revenue >10000;


-- 15) Use a CTE with recursion to display employee hierarchy.

-- Start from top-level employee (ReportsTo IS NULL) and drill down

with EmployeeHierarchy as (
    select EmployeeID, FirstName, LastName, ReportsTo, 0 as hierarchy_level
    from Employees
    where ReportsTo is null
 
    union all
 
    select e.EmployeeID, e.FirstName, e.LastName, e.ReportsTo, eh.hierarchy_level + 1
    from Employees e
    join EmployeeHierarchy eh on e.ReportsTo = eh.EmployeeID
)
 
select * from EmployeeHierarchy
order by hierarchy_level;