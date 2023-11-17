
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Diagnostics;
using System.Runtime.InteropServices;


Test t1 = new Test();

var d = new ProxyGenerator<Inseptor>().Generate<Itest>(t1);

var result = d.Sum(1,2);
Console.WriteLine("result :" +result);
public class Test
{
    public int Sum(int a ,int b)
    {
        Console.WriteLine($"sum {a} & {b}");
        return a+b;
    }
}
public interface Itest
{
    public int Sum(int i, int b );
}
public class Inseptor : IInseptor
{
    public object target { get; set; }
    public object Insept(Invoker invoker)
    {
        Console.WriteLine("befor");

        var a =(int)invoker.parameter[0];
        var b =(int)invoker.parameter[1];

        object result = invoker.Run(target, a+1,b+1 );

        Console.WriteLine("after");

        return (int)result +1;
    }
}


