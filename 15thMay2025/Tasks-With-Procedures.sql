-- 1. Create a stored procedure to encrypt a given text
-- Task: Write a stored procedure sp_encrypt_text that takes a plain text input (e.g., email or mobile number) and returns an encrypted version using PostgreSQL's pgcrypto extension.
--Use pgp_sym_encrypt(text, key) from pgcrypto.

CREATE OR REPLACE PROCEDURE encrypt_text (IN plain_text TEXT, OUT encrypted_text TEXT)
AS $$
BEGIN
    encrypted_text := pgp_sym_encrypt(plain_text, 'f9wH4xuDG43rRpIMVTohKXsG3VH6M55qSxjNfoJQB5Q='::TEXT);
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

DO $$
DECLARE
    my_plain_text TEXT := 'example@gmail.com';
    my_encrypted_text TEXT;
BEGIN
    CALL encrypt_text(my_plain_text, my_encrypted_text);
    RAISE NOTICE 'Encrypted text: %', my_encrypted_text;
END $$;

--------------------------------------------------------------------------------------------------
 
-- 2. Create a stored procedure to compare two encrypted texts
-- Task: Write a procedure sp_compare_encrypted that takes two encrypted values and checks if they decrypt to the same plain text.

CREATE OR REPLACE PROCEDURE compare_encrypted (
    IN encrypted_text1 TEXT,
    IN encrypted_text2 TEXT,
    OUT result BOOLEAN
)
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

    result := (decrypted_text1 = decrypted_text2);
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

DO $$
DECLARE
    comparison_result BOOLEAN;
BEGIN
    
    CALL compare_encrypted(
        '\xc30d04070302dcfc068d98c74eb97ed24201e7cb3ad3dc9f351bbc99fa8e32f5bb8fc3bd65c8e904070498694bda08b0418a917f6484f4b6806c196bda3081d15089bb116668661c501d290233b96b8241afdf',
        '\xc30d04070302dcfc068d98c74eb97ed24201e7cb3ad3dc9f351bbc99fa8e32f5bb8fc3bd65c8e904070498694bda08b0418a917f6484f4b6806c196bda3081d15089bb116668661c501d290233b96b8241afdf',
        comparison_result 
    );

    RAISE NOTICE 'Comparison Result: %', comparison_result;
END $$;

-- '\xc30d040703022fe1b252c4ad0d5a64d24201b75f35a208d6dad0ee166d3f8b45e0f5b27d0f87326879995e58ac5c361635c5e78989222d53c230c38a61d7fec15bcd0c4687bf06871849da0440860dff0f6707'

----------------------------------------------------------------------------------------------------------
-- 3. Create a stored procedure to partially mask a given text
-- Task: Write a procedure sp_mask_text that:
 
-- Shows only the first 2 and last 2 characters of the input string
 
-- Masks the rest with *
 
-- E.g., input: 'john.doe@example.com' → output: 'jo***************om'


CREATE OR REPLACE PROCEDURE mask_text (
    IN input_text TEXT,
    OUT masked_text TEXT
)
AS $$
DECLARE
    text_length INTEGER;
    masked_part TEXT := '';
    i INTEGER;
BEGIN
    text_length := LENGTH(input_text);

    IF text_length <= 4 THEN
        masked_text := input_text;
        RETURN;
    END IF;

    FOR i IN 3..(text_length - 2) LOOP
        masked_part := masked_part || '*';
    END LOOP;

    masked_text := SUBSTR(input_text, 1, 2) || masked_part || SUBSTR(input_text, text_length - 1, 2);
END;
$$ LANGUAGE plpgsql;

DO $$
DECLARE
    masked_email TEXT;
BEGIN
    CALL mask_text('john.doe@example.com', masked_email);
    RAISE NOTICE 'Masked email: %', masked_email;
END $$;

------------------------------------------------------------------------------------------------

-- 4. Create a procedure to insert into customer with encrypted email and masked name
-- Task:
 
-- Call sp_encrypt_text for email
 
-- Call sp_mask_text for first_name
 
-- Insert masked and encrypted values into the customer table
 
-- Use any valid address_id and store_id to satisfy FK constraints.

DROP TRIGGER trigger_mask_and_encrypt_data on UserDetails

CREATE OR REPLACE PROCEDURE insert_customer_data (
    IN first_name TEXT,
    IN last_name TEXT,
    IN email TEXT
)
AS $$
DECLARE
    encrypted_email TEXT;
    masked_first_name TEXT;
BEGIN
    
    CALL encrypt_text(email, encrypted_email);

    CALL mask_text(first_name, masked_first_name);
	
    INSERT INTO UserDetails (first_name, last_name, email)
    VALUES (masked_first_name, last_name, encrypted_email);

END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

CALL insert_customer_data('Christian', 'Bale', 'bale@gmail.com');

SELECT * FROM UserDetails

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



