/*
Liskov Substitution Principle (LSP):
Subtypes must be replaceable for their base types without altering the correctness of the program. 
For example, if a Bird base class has a Fly method, all subclasses like Sparrow must be able to fly without exceptions or changes in behavior.
*/


// bad example

// public class Bird
// {
//     public virtual void Fly()
//     {
//         Console.WriteLine("I am flying!");
//     }
// }

// public class Penguin : Bird
// {
//     public override void Fly()
//     {
//         throw new InvalidOperationException("Penguins cannot fly!");
//     }
// }

// class LiskovSubstitutionPrinciple
// {
//     static void TestBird(Bird bird)
//     {
//         bird.Fly(); 
//     }

//     public static void Run()
//     {
//         Bird penguin = new Penguin();
//         TestBird(penguin);
//     }
// }

// Good example
public interface IFlyable
{
    void Fly();
}

public class Bird
{
    public void func()
    {
        Console.WriteLine("I am bird!");
    }
}

public class Sparrow : Bird, IFlyable
{
    public void Fly()
    {
        Console.WriteLine("flying!");
    }
}

public class Penguin : Bird
{
    public void Swim()
    {
        Console.WriteLine("swimming!");
    }
}

class LiskovSubstitutionPrinciple
{
    static void TestBird(Bird bird)
    {
        bird.func();
    }

    public static void Run()
    {
        Bird sparrow = new Sparrow();
        Bird penguin = new Penguin();

        TestBird(sparrow);
        TestBird(penguin);

        //penguin.Swim(); 
    }
}
