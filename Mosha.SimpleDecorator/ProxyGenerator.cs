using System.Reflection;
using System.Reflection.Emit;

public class ProxyGenerator<Y> where Y : IInseptor
{
    public T Generate<T>(object target) where T : class
    {
        var type = GenerateProxyType<T, Y>();

        var t = Activator.CreateInstance(type, target);

        return t as T;

    }

    Type GenerateProxyType<T, Y>()
    {
        Type proxedType = typeof(T);

        AssemblyName assemblyName = new AssemblyName("CalculatorProxy");
        AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
        ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("customProxyModule");
        TypeBuilder typeBuilder;

        if (proxedType.IsInterface)
        {
            typeBuilder = moduleBuilder.DefineType(proxedType.Name + "_Proxy", TypeAttributes.Public, null, new Type[] { proxedType });
        }
        else
        {
            typeBuilder = moduleBuilder.DefineType(proxedType.Name + "_Proxy", TypeAttributes.Public, proxedType);
        }


        var inseptor = typeBuilder.DefineField("__Inseptor", typeof(Y), FieldAttributes.Public);

        var tb = typeBuilder
            .DefineConstructor(MethodAttributes.Public,
                CallingConventions.Standard,
                new[]
                {
                    typeof(object),
                });

        var ilGenerator = tb.GetILGenerator();


        ilGenerator.Emit(OpCodes.Ldarg_0);
        ilGenerator.Emit(OpCodes.Ldstr, typeof(Y).FullName);
        ilGenerator.Emit(OpCodes.Call, typeof(Invoker).GetMethod("CreateObject", new Type[] { typeof(string) }));
        ilGenerator.Emit(OpCodes.Stfld, inseptor);


        ilGenerator.Emit(OpCodes.Ldarg_0);
        ilGenerator.Emit(OpCodes.Ldfld, inseptor);
        ilGenerator.Emit(OpCodes.Ldarg_1);
        ilGenerator.Emit(OpCodes.Call, typeof(IInseptor).GetMethod("SetModel"));


        ilGenerator.Emit(OpCodes.Ret);


        foreach (var method in typeof(T).GetMethods())
        {
            Type[] parameters = method.GetParameters().Select(x => x.ParameterType).ToArray();


            var SumMethod = typeBuilder
                .DefineMethod(method.Name, MethodAttributes.Public | MethodAttributes.Virtual, method.ReturnType,
                    parameters);


            var Smi = SumMethod.GetILGenerator();

            Smi.Emit(OpCodes.Ldarg_0);

            Smi.Emit(OpCodes.Ldfld, inseptor);

            Smi.Emit(OpCodes.Ldarg_0);

            Smi.Emit(OpCodes.Box, proxedType);

            Smi.Emit(OpCodes.Ldstr, method.MetadataToken.ToString());

            Smi.Emit(OpCodes.Call, typeof(Invoker).GetMethod("Create"));

            int i = 1;
            foreach (var pr in parameters)
            {
                SetArg(Smi, i++, pr);
            }

            Smi.Emit(OpCodes.Callvirt, typeof(Y).GetMethod("Insept"));
            Type voidType = typeof(void);

            if (method.ReturnType.Equals(voidType))
            {
                Smi.Emit(OpCodes.Call, typeof(Invoker).GetMethod("VoidReader"));
                Smi.Emit(OpCodes.Ret);
            }
            else
            {
                Smi.Emit(OpCodes.Unbox_Any, method.ReturnType);
                Smi.Emit(OpCodes.Ret);
            }


        }

        void SetArg(ILGenerator ilGenerator, int arg, Type type)
        {

            ilGenerator.Emit(OpCodes.Ldarg, arg);
            ilGenerator.Emit(OpCodes.Box, type);
            ilGenerator.Emit(OpCodes.Callvirt, typeof(Invoker).GetMethod("SetArg"));
        }

        return typeBuilder.CreateType();
    }

}