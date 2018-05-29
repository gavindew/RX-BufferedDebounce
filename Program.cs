using System;
using System.Reactive.Subjects;

namespace BufferedDebounce
{
    /// <summary>
    /// This is a sample program which demonstrates the usage of the buffered debounce
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            var subject = new Subject<int>();

            subject.BufferedDebounce(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5))
                .Subscribe(x => { Console.WriteLine(string.Join(",", x)); });

            Console.WriteLine("Enter values to add to buffer");
            while (true)
            {
                if (Int32.TryParse(Console.ReadLine(), out var x))
                {
                    subject.OnNext(x);
                }
            }
        }
    }
}