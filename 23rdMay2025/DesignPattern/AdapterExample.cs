using System;

// Traget
interface IShape
{
    void Draw(int x1, int y1, int x2, int y2);
}

// Adaptee
class LegacyCircle
{
    public void Render(int centerX, int centerY, int radius) 
    {
        Console.WriteLine($"Rendering LegacyCircle at ({centerX},{centerY}) with radius {radius}"); 
    }
}

// Adapter
class CircleAdapter : IShape
{
    private LegacyCircle _legacyCircle; 

    public CircleAdapter(LegacyCircle legacyCircle) 
    {
        _legacyCircle = legacyCircle;
    }

    public void Draw(int x1, int y1, int x2, int y2) 
    {
        
        int centerX = (x1 + x2) / 2;
        int centerY = (y1 + y2) / 2;
        int radius = Math.Abs(x2 - x1) / 2; 

        _legacyCircle.Render(centerX, centerY, radius);
    }
}

// Client
public class AdapterExample 
{
    public static void Run() 
    {
        LegacyCircle oldCircle = new LegacyCircle();
        IShape adaptedCircle = new CircleAdapter(oldCircle);

        adaptedCircle.Draw(10, 10, 50, 50);
    }
}