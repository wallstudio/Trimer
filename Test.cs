using System;
using Trimer;

public class Test
{
    public static void RingBuffer()
    {
        var rb = new RingBuffer<string>(15);
        for (int i = 0; i < 20; i++)
        {
            rb.Add(i.ToString());
        }
        var dump = string.Join(",", rb);
        Console.WriteLine(dump);
        Console.ReadKey();
        return;
    }
}