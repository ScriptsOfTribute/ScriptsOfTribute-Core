
class A
{
    
}

class B : A
{
    
}

class C : B
{
    
}

namespace TalesOfTribute
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var c = new C();
            Console.WriteLine(c is A);
        }
    }
}
