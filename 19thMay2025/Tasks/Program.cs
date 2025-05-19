using System;
using System.Dynamic;
using System.Globalization;
using System.Numerics;

public class Program
{

    static int getValidIntInput(string s = "Enter a number: ")
    {
        do
        {
            Console.Write(s);
            string? input = Console.ReadLine();
            if (string.IsNullOrEmpty(input))
            {
                Console.WriteLine("Invalid Input. Try again!");
                continue;
            }
            if (!int.TryParse(input, out int number))
            {
                Console.WriteLine("Invalid Input. Try again!");
                continue;
            }
            return number;
        } while (true);
    }

    static int[] getIntArr()
    {
        int arrSize = getValidIntInput("Enter array size: ");
        int[] arr = new int[arrSize];

        for (int i = 0; i < arrSize; i++)
        {
            arr[i] = getValidIntInput();
        }
        return arr;
    }

    static void printArr<T>(T[] arr)
    {
        foreach (T i in arr)
        {
            Console.Write($"{i} ");
        }
    }

    static void task1()
    {
        Console.Write("Enter your name: ");
        string? userName = Console.ReadLine();
        Console.WriteLine($"Hello, {userName}!");
    }

    static void task2()
    {
        int num1 = getValidIntInput();
        int num2 = getValidIntInput("Enter another number: ");
        Console.WriteLine(int.Max(num1, num2));
    }

    static void task3()
    {
        Console.Write("Enter a number: ");
        string? input1 = Console.ReadLine();

        Console.Write("Enter another number: ");
        string? input2 = Console.ReadLine();

        if (string.IsNullOrEmpty(input1) || string.IsNullOrEmpty(input2))
        {
            Console.WriteLine("Invalid input");
            return;
        }

        int num1 = int.Parse(input1);
        int num2 = int.Parse(input2);
        Console.WriteLine(int.Max(num1, num2));
        Console.Write("choose +,-,*,/ : ");
        string? op = Console.ReadLine();
        int num3 = 0;

        switch (op)
        {
            case "+":
                num3 = num1 + num2;
                Console.WriteLine(num3);
                break;
            case "-":
                num3 = num1 - num2;
                Console.WriteLine(num3);
                break;
            case "*":
                num3 = num1 * num2;
                Console.WriteLine(num3);
                break;
            case "/":
                if (num2 != 0)
                {
                    num3 = num1 / num2;
                    Console.WriteLine(num3);
                }
                else
                {
                    Console.WriteLine("Cannot divide by 0");
                }
                break;
            default:
                break;
        }
    }

    static void task4()
    {
        int count = 0;
        bool flag = true;
        do
        {
            Console.Write("user: ");
            string? user = Console.ReadLine();
            Console.Write("password: ");
            string? password = Console.ReadLine();
            if (user == "admin" && password == "pass")
            {
                Console.WriteLine("Successfully logged in");
                flag = false;
                break;
            }
            count++;
        } while (count < 3);
        if (flag)
        {
            Console.WriteLine("Invalid attempts for 3 times. Exiting....");
        }
    }

    static void task5()
    {
        int count = 0;
        for (int i = 0; i < 10; i++)
        {
            Console.Write("Enter a number: ");
            string? input = Console.ReadLine();
            if (string.IsNullOrEmpty(input))
            {
                continue;
            }
            int num = int.Parse(input);
            if (num % 7 == 0)
            {
                count++;
            }
        }
        Console.WriteLine($"Number of numbers divisible by 7: {count}");

    }

    static void task6()
    {
        int[] arr = getIntArr();

        Dictionary<int, int> frequencyMap = new Dictionary<int, int>();

        foreach (int i in arr)
        {
            if (frequencyMap.ContainsKey(i))
            {
                frequencyMap[i]++;
            }
            else
            {
                frequencyMap[i] = 1;
            }
        }
        foreach (KeyValuePair<int, int> pair in frequencyMap)
        {
            Console.WriteLine($"{pair.Key} occurs {pair.Value} times");
        }
    }

    static void task7()
    {
        int[] arr = getIntArr();

        Console.Write("Before: ");
        printArr(arr);

        int t = arr[0];
        for (int i = 0; i < arr.Length - 1; i++)
        {
            arr[i] = arr[i + 1];
        }
        arr[arr.Length - 1] = t;

        Console.Write("\nAfter: ");
        printArr(arr);

    }

    static void task8()
    {
        int[] arr1 = getIntArr();
        int[] arr2 = getIntArr();
        int[] mergedArr = new int[arr1.Length + arr2.Length];
        int arr1Size = arr1.Length, arr2Size = arr2.Length;
        for (int i = 0; i < arr1Size; i++)
        {
            mergedArr[i] = arr1[i];
        }
        for (int i = 0; i < arr2Size; i++)
        {
            mergedArr[arr1Size + i] = arr2[i];
        }

        printArr(mergedArr);
    }

    static void task9()
    {
        string secret = "GAME";
        int bull, cows, attempt = 0;

        do
        {
            bull = 0;
            cows = 0;
            Dictionary<char, int> frequencyMapSecret = new Dictionary<char, int>();
            Dictionary<char, int> frequencyMapGuess = new Dictionary<char, int>();

            Console.Write($"Guess the word (Length={secret.Length}): ");
            string? guess = Console.ReadLine();
            if (string.IsNullOrEmpty(guess) || guess.Length != secret.Length)
            {
                continue;
            }

            for (int i = 0; i < secret.Length; i++)
            {
                if (frequencyMapSecret.ContainsKey(secret[i]))
                {
                    frequencyMapSecret[secret[i]]++;
                }
                else
                {
                    frequencyMapSecret[secret[i]] = 1;
                }

                if (frequencyMapGuess.ContainsKey(guess[i]))
                {
                    frequencyMapGuess[guess[i]]++;
                }
                else
                {
                    frequencyMapGuess[guess[i]] = 1;
                }

                if (secret[i] == guess[i])
                {
                    bull++;
                }
            }

            foreach (KeyValuePair<char, int> pair in frequencyMapGuess)
            {
                if (frequencyMapSecret.ContainsKey(pair.Key))
                {
                    cows += int.Min(frequencyMapSecret[pair.Key], pair.Value);
                }
            }

            attempt++;
            Console.WriteLine($"\nAttempt no: {attempt}");
            Console.WriteLine($"Bulls: {bull}");
            Console.WriteLine($"Cows: {cows - bull}\n");

        } while (!(bull == secret.Length && cows - bull == 0));

        Console.WriteLine("Congratulations!!!");
    }

    static bool IsValidSet(int[] arr)
    {
        var seen = new HashSet<int>();
        foreach (int num in arr)
        {
            if (num < 1 || num > 9 || !seen.Add(num))
            {
                return false;
            }
        }
        return true;
    }
    static void task10()
    {
        int[] arr = { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        if (IsValidSet(arr))
        {
            Console.WriteLine("Valid!!!");
        }
        else
        {
            Console.WriteLine("Invalid Array");
        }
    }


    static bool IsRowValid(int[,] board, int row)
    {
        int[] rowValues = new int[9];
        for (int j = 0; j < 9; j++)
        {
            rowValues[j] = board[row, j];
        }
        return IsValidSet(rowValues);
    }

    static bool IsColumnValid(int[,] board, int col)
    {
        int[] colValues = new int[9];
        for (int i = 0; i < 9; i++)
        {
            colValues[i] = board[i, col];
        }
        return IsValidSet(colValues);
    }

    static bool IsSubgridValid(int[,] board, int startRow, int startCol)
    {
        int[] subgridValues = new int[9];
        int index = 0;
        for (int i = startRow; i < startRow + 3; i++)
        {
            for (int j = startCol; j < startCol + 3; j++)
            {
                subgridValues[index++] = board[i, j];
            }
        }
        return IsValidSet(subgridValues);
    }

    static void task11()
    {
        int[,] board ={
            {5, 3, 4, 6, 7, 8, 9, 1, 2},
            {6, 7, 2, 1, 9, 5, 3, 4, 8},
            {1, 9, 8, 3, 4, 2, 5, 6, 7},
            {8, 5, 9, 7, 6, 1, 4, 2, 3},
            {4, 2, 6, 8, 5, 3, 7, 9, 1},
            {7, 1, 3, 9, 2, 4, 8, 5, 6},
            {9, 6, 1, 5, 3, 7, 2, 8, 4},
            {2, 8, 7, 4, 1, 9, 6, 3, 5},
            {3, 4, 5, 2, 8, 6, 1, 7, 9}
        };

        bool isValid = true;

        for (int i = 0; i < 9; i++)
        {
            if (!IsRowValid(board, i))
            {
                Console.WriteLine($"Row {i + 1} is invalid.");
                isValid = false;
            }
        }

        for (int j = 0; j < 9; j++)
        {
            if (!IsColumnValid(board, j))
            {
                Console.WriteLine($"Column {j + 1} is invalid.");
                isValid = false;
            }
        }

        for (int i = 0; i < 9; i += 3)
        {
            for (int j = 0; j < 9; j += 3)
            {
                if (!IsSubgridValid(board, i, j))
                {
                    Console.WriteLine($"Subgrid starting at row {i + 1}, column {j + 1} is invalid.");
                    isValid = false;
                }
            }
        }

        if (isValid)
        {
            Console.WriteLine("The Sudoku board is valid.");
        }
        else
        {
            Console.WriteLine("The Sudoku board is invalid.");
        }
    }

    public static string Encrypt(string message, int key)
    {
        string encMsg ="";
        foreach (char c in message)
        {
            if (char.IsLower(c))
            {
                int index = c - 'a';
                int shiftedIndex = (index + key) % 26;
                char encryptedChar = (char)('a' + shiftedIndex);
                encMsg+=char.ToString(encryptedChar);
            }
        }
        return encMsg;
    }

    public static string Decrypt(string enc, int key)
    {
        string msg ="";
        foreach (char c in enc)
        {
            if (char.IsLower(c))
            {
                int index = c - 'a';
                int shiftedIndex = (index - key) % 26;
                char decryptedChar = (char)('a' + shiftedIndex);
                msg+=char.ToString(decryptedChar);
            }
        }
        return msg;
    }

    static void task12()
    {
        Console.Write("Enter Text:");
        string? message = Console.ReadLine();
        if (string.IsNullOrEmpty(message))
        {
            Console.WriteLine("Invalid input");
            return;
        }

        int key = getValidIntInput("Enter Key:");

        string enc = Encrypt(message,key);
        Console.WriteLine($"Encrypted message: {enc}");

        string msg = Decrypt(enc,key);
        Console.WriteLine($"Decrypted message: {msg}");

    }


    public static void Main(string[] args)
    {
        task12();
    }
}
