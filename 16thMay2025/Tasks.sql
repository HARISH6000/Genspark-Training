/*
Phase 2: DDL & DML

* Create all tables with appropriate constraints (PK, FK, UNIQUE, NOT NULL)
* Insert sample data using `INSERT` statements
* Create indexes on `student_id`, `email`, and `course_id`
*/

CREATE TABLE students (
    student_id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    email VARCHAR(255) UNIQUE NOT NULL,
    phone VARCHAR(20) NOT NULL
);

CREATE TABLE courses (
    course_id SERIAL PRIMARY KEY,
    course_name VARCHAR(255) NOT NULL,
    category VARCHAR(100),
    duration_days INTEGER
);

CREATE TABLE trainers (
    trainer_id SERIAL PRIMARY KEY,
    trainer_name VARCHAR(255) NOT NULL,
    expertise VARCHAR(255)
);

CREATE TABLE enrollments (
    enrollment_id SERIAL PRIMARY KEY,
    student_id INTEGER NOT NULL,
    course_id INTEGER NOT NULL,
    enroll_date DATE NOT NULL,
    FOREIGN KEY (student_id) REFERENCES students(student_id),
    FOREIGN KEY (course_id) REFERENCES courses(course_id)
);

CREATE TABLE certificates (
    certificate_id SERIAL PRIMARY KEY,
    enrollment_id INTEGER NOT NULL,
    issue_date DATE NOT NULL,
    serial_no VARCHAR(255) NOT NULL UNIQUE,
    FOREIGN KEY (enrollment_id) REFERENCES enrollments(enrollment_id)
);

CREATE TABLE course_trainers (
    course_id INTEGER NOT NULL,
    trainer_id INTEGER NOT NULL,
    PRIMARY KEY (course_id, trainer_id),
    FOREIGN KEY (course_id) REFERENCES courses(course_id),
    FOREIGN KEY (trainer_id) REFERENCES trainers(trainer_id)
);

CREATE TABLE student_trainers (
    enrollment_id INTEGER NOT NULL,
    trainer_id INTEGER NOT NULL,
    PRIMARY KEY (enrollment_id, trainer_id),
    FOREIGN KEY (enrollment_id) REFERENCES enrollments(enrollment_id),
    FOREIGN KEY (trainer_id) REFERENCES trainers(trainer_id)
);

CREATE INDEX idx_students_student_id ON students (student_id);

CREATE INDEX idx_students_email ON students (email);

CREATE INDEX idx_enrollments_course_id ON courses (course_id);


-- Insert into tables

INSERT INTO students (name, email, phone) VALUES
('Edward Kenway', 'edward@gmail.com', '1234567890'),
('Johnny Silverhand', 'silverhand@gmail.com', '1234567891'),
('Peter Parker', 'perterparker@gmail.com', '1234567892');

INSERT INTO courses (course_name, category, duration_days) VALUES
('Introduction to SQL', 'DBMS', 10),
('C#', 'Programming', 30),
('AWS Cloud Practitioner', 'Cloud', 20);

INSERT INTO trainers (trainer_name, expertise) VALUES
('Jane Doe', 'C#, Databases'),
('John Smith', 'Web Development'),
('Emily White', 'Cloud, Databases');

INSERT INTO enrollments (student_id, course_id, enroll_date) VALUES
(1, 1, '2025-05-05'),
(2, 2, '2025-05-05'),
(3, 1, '2025-05-05');

INSERT INTO certificates (enrollment_id, issue_date, serial_no) VALUES
(1, '2025-10-05', 'CERT-001'),
(2, '2025-10-05', 'CERT-002');

INSERT INTO course_trainers (course_id, trainer_id) VALUES
(1, 1),
(1, 3),
(2, 1),
(3, 3);

INSERT INTO student_trainers (enrollment_id, trainer_id) VALUES
(1, 1),
(2, 3),
(3, 1);

------------------------------------------------------------------------------------
/*
Phase 3: SQL Joins Practice

Write queries to:

1. List students and the courses they enrolled in
2. Show students who received certificates with trainer names
3. Count number of students per course
*/

SELECT s.name, c.course_name FROM students s
JOIN enrollments e ON s.student_id = e.student_id
JOIN courses c ON e.course_id = c.course_id;

SELECT s.name, co.course_name, t.trainer_name FROM certificates c
JOIN enrollments e ON c.enrollment_id = e.enrollment_id
JOIN students s ON s.student_id = e.student_id
JOIN courses co ON e.course_id = co.course_id
JOIN student_trainers st ON e.enrollment_id = st.enrollment_id
JOIN trainers t ON st.trainer_id = t.trainer_id;

SELECT c.course_name, t.count FROM courses c
JOIN (SELECT course_id, count(*) FROM enrollments
GROUP BY course_id) t ON c.course_id = t.course_id;

---------------------------------------------------------------
-- Phase 4: Functions & Stored Procedures

-- Function:

-- Create `get_certified_students(course_id INT)`
-- → Returns a list of students who completed the given course and received certificates.

CREATE OR REPLACE FUNCTION get_certified_students(cid INT)
RETURNS TABLE (student_name VARCHAR(255)) 
AS $$
BEGIN
    RETURN QUERY
    SELECT s.name FROM students s
    JOIN enrollments e ON s.student_id = e.student_id
    JOIN certificates c ON e.enrollment_id = c.enrollment_id
    WHERE e.course_id = cid;
END;
$$ LANGUAGE plpgsql;

SELECT * FROM get_certified_students(1);

-- Stored Procedure:

-- Create `sp_enroll_student(p_student_id, p_course_id)`
-- → Inserts into `enrollments` and conditionally adds a certificate if completed (simulate with status flag).

CREATE OR REPLACE PROCEDURE sp_enroll_student(p_student_id INT, p_course_id INT, p_enrollment_date DATE, status BOOLEAN) 
AS $$
DECLARE
    v_enrollment_id INT;
BEGIN
   
    INSERT INTO enrollments (student_id, course_id, enroll_date)
    VALUES (p_student_id, p_course_id, p_enrollment_date)
    RETURNING enrollment_id INTO v_enrollment_id;

    
    IF status THEN
        INSERT INTO certificates (enrollment_id, issue_date, serial_no)
        VALUES (v_enrollment_id, CURRENT_DATE, CONCAT('CERT-', p_course_id, '-', v_enrollment_id));
    END IF;

END;
$$ LANGUAGE plpgsql;


CALL sp_enroll_student(1, 3, '2025-04-16', true);

SELECT * FROM enrollments;

----------------------------------------------------------------------------------------

/* 
Phase 5: Cursor

Use a cursor to:

* Loop through all students in a course
* Print name and email of those who do not yet have certificates
*/

CREATE OR REPLACE PROCEDURE get_uncertified_students(p_course_id INT)
AS $$
DECLARE
    cur_students CURSOR FOR
        SELECT s.name, s.email
        FROM students s
        JOIN enrollments e ON s.student_id = e.student_id
		LEFT JOIN certificates c ON e.enrollment_id = c.enrollment_id 
        WHERE e.course_id = p_course_id and c.certificate_id is null;
	rec RECORD;
BEGIN

    OPEN cur_students;

    LOOP
        FETCH cur_students INTO rec;
        EXIT WHEN NOT FOUND;
		RAISE NOTICE 'Name: %, email: %',rec.name,rec.email;
    END LOOP;
	
    CLOSE cur_students;
    RETURN;
END;
$$ LANGUAGE plpgsql;

CALL get_uncertified_students(2);

---------------------------------------------------------
/*
Phase 6: Security & Roles

1. Create a `readonly_user` role:

   * Can run `SELECT` on `students`, `courses`, and `certificates`
   * Cannot `INSERT`, `UPDATE`, or `DELETE`

2. Create a `data_entry_user` role:

   * Can `INSERT` into `students`, `enrollments`
   * Cannot modify certificates directly
*/

CREATE USER readonly_test_user WITH PASSWORD 'password';
CREATE USER data_entry_test_user WITH PASSWORD 'password';

CREATE ROLE readonly_user;

GRANT SELECT ON students, courses, certificates TO readonly_user;


CREATE ROLE data_entry_user;

GRANT INSERT ON students, enrollments TO data_entry_user;

GRANT USAGE, SELECT ON SEQUENCE students_student_id_seq TO data_entry_user;
GRANT USAGE, SELECT ON SEQUENCE enrollments_enrollment_id_seq TO data_entry_user;

GRANT readonly_user TO readonly_test_user;
GRANT data_entry_user TO data_entry_test_user;

-- Try running the following insert with readonly_test_user 
-- can switch user with psql -U readonly_test_user -d StudentManagement -W
INSERT INTO students (name, email, phone) VALUES ('Han Lue', 'hanlue@gmail.com', '1234567893');

-- Now try with data_entry_user
INSERT INTO students (name, email, phone) VALUES ('Han Lue', 'hanlue@gmail.com', '1234567893');
SELECT * FROM students;
-------------------------------------------------------------------------------------------
/*
Phase 7: Transactions & Atomicity

Write a transaction block that:

* Enrolls a student
* Issues a certificate
* Fails if certificate generation fails (rollback)

```sql
BEGIN;
-- insert into enrollments
-- insert into certificates
-- COMMIT or ROLLBACK on error
```
*/

CREATE OR REPLACE PROCEDURE sp_enroll_student(p_student_id INT, p_course_id INT, p_enrollment_date DATE, status BOOLEAN) 
AS $$
DECLARE
    v_enrollment_id INT;
BEGIN
   
    INSERT INTO enrollments (student_id, course_id, enroll_date)
    VALUES (p_student_id, p_course_id, p_enrollment_date)
    RETURNING enrollment_id INTO v_enrollment_id;
	
    
    IF status THEN
        INSERT INTO certificates (enrollment_id, issue_date, serial_no)
        VALUES (v_enrollment_id, CURRENT_DATE, CONCAT('CERT-', p_course_id, '-', v_enrollment_id));
    END IF;

	EXCEPTION
		WHEN OTHERS THEN
			ROLLBACK;
	COMMIT;
END;
$$ LANGUAGE plpgsql;

CALL sp_enroll_student(4, 2, CURRENT_DATE, false);
