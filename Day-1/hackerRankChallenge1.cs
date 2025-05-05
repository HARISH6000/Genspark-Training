using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Text;
using System;

class Result
{


    public static void plusMinus(List<int> arr)
    {
        int p=0;
        int n=0;
        int z=0;
        for(int i=0;i<arr.Count;i++){
            if(arr[i]>0){
                p++;
            }
            else if(arr[i]<0){
                n++;
            }
            else{
                z++;
            }
        }
        Console.WriteLine((double)p/arr.Count);
        Console.WriteLine((double)n/arr.Count);
        Console.WriteLine((double)z/arr.Count);
    }

}

class Solution
{
    public static void Main(string[] args)
    {
        int n = Convert.ToInt32(Console.ReadLine().Trim());

        List<int> arr = Console.ReadLine().TrimEnd().Split(' ').ToList().Select(arrTemp => Convert.ToInt32(arrTemp)).ToList();

        Result.plusMinus(arr);
    }
}
