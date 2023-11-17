using System.Reflection;

public class Invoker
{
    private MethodInfo method;
    private Invoker(MethodInfo method)
    {
        this.method = method;
    }

    private List<object> _Parameters;

    public object[] parameter => _Parameters?.ToArray();
    public Invoker SetArg(object arg)
    {
        var x = (int)arg;
        _Parameters = _Parameters ?? new List<object>();
        this._Parameters.Add(arg);
        return this;
    }

    public static Invoker Create(object t, string s)
    {

        MethodInfo method = t.GetType().GetInterfaces().SelectMany(x => x.GetMethods()).Single(x => x.MetadataToken.ToString().Equals(s));

        return new Invoker(method);
    }

    public static void VoidReader(object o)
    {

    }

    public static object CreateObject(string typeName)
    {
        return Activator.CreateInstance(Type.GetType(typeName));
    }


    public object Run(object input, params object[] parameter)
    {
        MethodInfo targeMethodInfo = input.GetType().GetMethod(method.Name, method.GetParameters().Select(x => x.ParameterType).ToArray());
        if (targeMethodInfo == null) throw new Exception("method not find");
        return targeMethodInfo.Invoke(input, parameter);
    }



}