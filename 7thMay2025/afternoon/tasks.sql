--1
select * from cd.facilities;

--2
select name, membercost from cd.facilities;

--3
select * from cd.facilities where membercost!=0;

--4
select facid, name, membercost, monthlymaintenance 
from cd.facilities 
where membercost!=0 and membercost<(monthlymaintenance/50);

--5
select * from cd.facilities where name like '%Tennis%';

--6
select * from cd.facilities where facid in (1,5);

--7
select name,
	case when (monthlymaintenance >100) then
		'expensive'
	else
		'cheap'
	end as cost

from cd.facilities;

--8
select memid, surname, firstname, joindate 
from cd.members 
where joindate>= '2012-09-01 00:00:00';

--9
select distinct(surname) from cd.members order by surname limit 10;

--10
select surname from cd.members
union
select name from cd.facilities

--11
select max(joindate) as latest from cd.members;

--12
select firstname, surname, joindate as latest 
from cd.members 
where joindate in (select max(joindate) from cd.members);
