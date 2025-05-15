/*
	This is the initial version of the tasks which i have done by using functions instead of stored procedures.
*/


-- 1. Create a stored procedure to encrypt a given text
-- Task: Write a stored procedure sp_encrypt_text that takes a plain text input (e.g., email or mobile number) and returns an encrypted version using PostgreSQL's pgcrypto extension.
--Use pgp_sym_encrypt(text, key) from pgcrypto.

CREATE EXTENSION IF NOT EXISTS pgcrypto;

CREATE OR REPLACE FUNCTION sp_encrypt_text (plain_text TEXT)
RETURNS TEXT
AS $$
BEGIN
    RETURN pgp_sym_encrypt(plain_text, 'f9wH4xuDG43rRpIMVTohKXsG3VH6M55qSxjNfoJQB5Q='::TEXT);
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

SELECT sp_encrypt_text('example@email.com');

--------------------------------------------------------------------------------------------------
 
-- 2. Create a stored procedure to compare two encrypted texts
-- Task: Write a procedure sp_compare_encrypted that takes two encrypted values and checks if they decrypt to the same plain text.

CREATE OR REPLACE FUNCTION sp_compare_encrypted (encrypted_text1 TEXT, encrypted_text2 TEXT)
RETURNS BOOLEAN
AS $$
DECLARE
    decrypted_text1 TEXT;
    decrypted_text2 TEXT;
BEGIN
    BEGIN
        decrypted_text1 := pgp_sym_decrypt(encrypted_text1::BYTEA, 'f9wH4xuDG43rRpIMVTohKXsG3VH6M55qSxjNfoJQB5Q='::TEXT);
    EXCEPTION
        WHEN others THEN
            decrypted_text1 := NULL;
    END;

    BEGIN
        decrypted_text2 := pgp_sym_decrypt(encrypted_text2::BYTEA, 'f9wH4xuDG43rRpIMVTohKXsG3VH6M55qSxjNfoJQB5Q='::TEXT);
    EXCEPTION
        WHEN others THEN
            decrypted_text2 := NULL;
    END;

    RETURN decrypted_text1 = decrypted_text2;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

SELECT sp_compare_encrypted('\xc30d04070302dcfc068d98c74eb97ed24201e7cb3ad3dc9f351bbc99fa8e32f5bb8fc3bd65c8e904070498694bda08b0418a917f6484f4b6806c196bda3081d15089bb116668661c501d290233b96b8241afdf', 
'\xc30d04070302dcfc068d98c74eb97ed24201e7cb3ad3dc9f351bbc99fa8e32f5bb8fc3bd65c8e904070498694bda08b0418a917f6484f4b6806c196bda3081d15089bb116668661c501d290233b96b8241afdf');

-- '\xc30d040703022fe1b252c4ad0d5a64d24201b75f35a208d6dad0ee166d3f8b45e0f5b27d0f87326879995e58ac5c361635c5e78989222d53c230c38a61d7fec15bcd0c4687bf06871849da0440860dff0f6707'
----------------------------------------------------------------------------------------------------------
-- 3. Create a stored procedure to partially mask a given text
-- Task: Write a procedure sp_mask_text that:
 
-- Shows only the first 2 and last 2 characters of the input string
 
-- Masks the rest with *
 
-- E.g., input: 'john.doe@example.com' → output: 'jo***************om'

CREATE OR REPLACE FUNCTION sp_mask_text (input_text TEXT)
RETURNS TEXT
AS $$
DECLARE
    text_length INTEGER;
    masked_part TEXT := '';
    i INTEGER;
BEGIN
    text_length := LENGTH(input_text);

    IF text_length <= 4 THEN
        RETURN input_text;
    END IF;

    FOR i IN 3..(text_length - 2) LOOP
        masked_part := masked_part || '*';
    END LOOP;

    RETURN SUBSTR(input_text, 1, 2) || masked_part || SUBSTR(input_text, text_length - 1, 2);
END;
$$ LANGUAGE plpgsql IMMUTABLE;

SELECT sp_mask_text('john.doe@example.com');

------------------------------------------------------------------------------------------------

-- 4. Create a procedure to insert into customer with encrypted email and masked name
-- Task:
 
-- Call sp_encrypt_text for email
 
-- Call sp_mask_text for first_name
 
-- Insert masked and encrypted values into the customer table
 
-- Use any valid address_id and store_id to satisfy FK constraints.

CREATE TABLE UserDetails (
    id SERIAL PRIMARY KEY,
    first_name VARCHAR(255) NOT NULL,
    last_name VARCHAR(255) NOT NULL,
    email TEXT UNIQUE NOT NULL
);

CREATE OR REPLACE FUNCTION mask_and_encrypt_data()
RETURNS TRIGGER AS $$
DECLARE
    masked_first_name TEXT;
    encrypted_email TEXT;
BEGIN
    
    SELECT sp_encrypt_text(NEW.email) into encrypted_email;
    NEW.email := encrypted_email;

	SELECT sp_mask_text(NEW.first_name) into masked_first_name;
	NEW.first_name := masked_first_name;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

CREATE or REPLACE TRIGGER trigger_mask_and_encrypt_data
BEFORE INSERT ON UserDetails
FOR EACH ROW
EXECUTE FUNCTION mask_and_encrypt_data();

INSERT INTO UserDetails (first_name, last_name, email)
VALUES ('Christian', 'Bale', 'christian@gmail.com');

select * from UserDetails

-----------------------------------------------------------------------------------------------------
-- 5. Create a procedure to fetch and display masked first_name and decrypted email for all customers
-- Task:
-- Write sp_read_customer_masked() that:
 
-- Loops through all rows
 
-- Decrypts email
 
-- Displays customer_id, masked first name, and decrypted email

CREATE OR REPLACE PROCEDURE sp_read_customer_masked()
LANGUAGE plpgsql
AS $$
DECLARE
    decrypted_email TEXT;
    customer_record RECORD; 
    email_encryption_key TEXT := 'f9wH4xuDG43rRpIMVTohKXsG3VH6M55qSxjNfoJQB5Q='; 
BEGIN
    
    FOR customer_record IN SELECT id, first_name, email FROM UserDetails LOOP  
        
        BEGIN
            decrypted_email := pgp_sym_decrypt(customer_record.email::bytea, email_encryption_key);
        EXCEPTION
            WHEN OTHERS THEN
                decrypted_email := 'Decryption Error';
        END;

        RAISE NOTICE 'Customer ID: %, Masked First Name: %, Decrypted Email: %', customer_record.id, customer_record.first_name, decrypted_email;
    END LOOP;
END;
$$;

CALL sp_read_customer_masked();

------------------------------------------------------------------------------------------------------