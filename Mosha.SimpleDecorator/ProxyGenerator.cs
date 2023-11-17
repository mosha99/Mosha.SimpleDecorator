using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;

public class ProxyGenerator
{
    public T Generate<T,Y>(object target) 
        where T : class
        where Y : IInseptor,new()
    {
        var type = GenerateProxyType<T, Y>();

        var i = Activator.CreateInstance(typeof(Y));

        var t = Activator.CreateInstance(type, target, i);

        return t as T;

    }
    public T Generate<T, Y>(object target, IServiceProvider serviceProvider)
        where T : class 
        where Y : IInseptor
    {
        var type = GenerateProxyType<T, Y>();

        var i = ActivatorUtilities.CreateInstance(serviceProvider, typeof(Y));

        var t = ActivatorUtilities.CreateInstance(serviceProvider, type, target, i);

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
                    typeof(Y),
                });

        var ilGenerator = tb.GetILGenerator();


        ilGenerator.Emit(OpCodes.Ldarg_0);
        ilGenerator.Emit(OpCodes.Ldarg_2);
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