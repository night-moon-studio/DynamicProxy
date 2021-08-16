using DynamicProxy;
using Natasha.CSharp;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Natasha
{
    public class Proxier : IEnumerable<MethodInfo>
    {
        public string SingletonMethodName { get { return "SetProxySingleton" + CurrentProxyName; } }
        public const string NoNeedWriting = "NW1000-NoNeedToWrite";
        public readonly NClass ClassBuilder;
        private readonly ConcurrentDictionary<string, MethodInfo> _methodMapping;
        private readonly ConcurrentDictionary<Delegate, string> _staticDelegateOrderScriptMapping;
        private readonly List<(string memberName, Delegate @delegate, string typeScript)> _staticNameDelegateMapping;
        private bool _useSingleton;
        public StringBuilder ProxyBody;
        public readonly ConcurrentDictionary<string, string> NeedReWriteMethods;
        public bool UseSingleton { get { return _useSingleton; } }
        public Proxier()
        {

            ClassBuilder = NClass
                .RandomDomain(item => item.LogSyntaxError().LogCompilerError())
                .Public()
                .Namespace("Natasha.Proxy")
                .UseRandomName();

            NeedReWriteMethods = new ConcurrentDictionary<string, string>();
            _methodMapping = new ConcurrentDictionary<string, MethodInfo>();
            _staticDelegateOrderScriptMapping = new ConcurrentDictionary<Delegate, string>();
            _staticNameDelegateMapping = new List<(string memberName, Delegate @delegate, string typeScript)>();
            ProxyBody = new StringBuilder();

        }




        public string CurrentProxyName
        {
            get { return ClassBuilder.NameScript; }
        }




        public Proxier SetSingleton(bool singleton = true)
        {
            _useSingleton = singleton;
            return this;
        }




        /// <summary>
        /// 额外添加 Using 引用
        /// </summary>
        /// <param name="namespace"> 命名空间的来源 </param>
        /// <returns></returns>
        public Proxier Using(NamespaceConverter @namespace)
        {

            ClassBuilder.Using(@namespace);
            return this;

        }
        /// <summary>
        /// 额外添加 dll 引用
        /// </summary>
        /// <param name="path">dll文件路径</param>
        /// <returns></returns>
        public Proxier AddDll(string path)
        {
            ClassBuilder.AssemblyBuilder.Compiler.Domain.LoadPlugin(path);
            return this;
        }




        /// <summary>
        /// 操作当前函数
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public StringOrDelegate this[string key]
        {

            set
            {

                if (value.StringParameter != default)
                {

                    SetMethod(key, value.StringParameter);

                }
                else
                {

                    SetMethod(key, value.DelegateParameter);

                }

            }

        }




        public Proxier Implement(Type type)
        {

            ClassBuilder.InheritanceAppend(type);
            var methods = type.GetMethods();
            foreach (var item in methods)
            {
                //是虚方法
                if (item.IsHideBySig)
                {

                    _methodMapping[item.Name] = item;
                    Type returnType = item.ReturnType;
                    int returnScriptEnum = 0;
                    if (returnType != typeof(void))
                    {
                        if (returnType.IsGenericType)
                        {
                            if (returnType.GetGenericTypeDefinition() == typeof(Task<>) || returnType.GetGenericTypeDefinition() == typeof(ValueTask<>))
                            {
                                returnScriptEnum = 3;
                            }
                        }
                        else if (returnType == typeof(Task) || returnType.BaseType == typeof(ValueTask))
                        {
                            returnScriptEnum = 2;
                        }
                        else
                        {
                            returnScriptEnum = 1;
                        }
                    }



                    string script = "";
                    if (returnScriptEnum == 1 || returnScriptEnum == 3)
                    {
                        script = "return default;";
                    }
                    else if (returnScriptEnum == 2)
                    {
                        script = "";
                    }

                    if (type.IsInterface)
                    {

                        SetMethod(item.Name, script);

                    }
                    else if (item.IsAbstract)
                    {

                        if (!item.Equals(item.GetBaseDefinition()))
                        {

                            SetMethod(item.Name, NoNeedWriting);

                        }
                        else
                        {

                            SetMethod(item.Name, script);

                        }

                    }
                    else
                    {

                        SetMethod(item.Name, NoNeedWriting);

                    }


                }

            }
            return this;

        }
        public Proxier Implement<T>()
        {
            return Implement(typeof(T));
        }




        public void SetMethod(string name, string script)
        {

            if (_methodMapping.ContainsKey(name))
            {

                var method = _methodMapping[name];
                var type = method.DeclaringType;

                //异步代码检测
                var returnType = method.ReturnType;
                bool isAsync = false;
                if (returnType.IsGenericType)
                {
                    if (returnType.GetGenericTypeDefinition() == typeof(Task<>) || returnType.GetGenericTypeDefinition() == typeof(ValueTask<>))
                    {
                        isAsync = true;
                        returnType = returnType.GenericTypeArguments[0];
                    }
                }
                else if (returnType == typeof(Task) || returnType.BaseType == typeof(ValueTask))
                {
                    isAsync = true;
                }


                //构建脚本
                var template = FakeMethodOperator.RandomDomain();
                if (!type.IsInterface)
                {
                    _ = (method.IsAbstract || method.IsVirtual) ? template.Override() : template.New();
                }
                if (isAsync)
                {
                    template.Async();
                }
                var result = template
                    .UseMethod(method)
                    .MethodBody(script)
                    .Script;


                NeedReWriteMethods[name] = result;

            }

        }
        public void SetMethod(string name, Delegate @delegate)
        {

            if (_methodMapping.ContainsKey(name))
            {

                StringBuilder builder = new StringBuilder();
                var methodInfo = @delegate.Method;
                if (methodInfo.ReturnType != typeof(void))
                {

                    builder.Append("Func<");
                    var parameters = methodInfo.GetParameters();
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        builder.Append(parameters[i].ParameterType.GetDevelopName());
                        builder.Append(',');
                    }
                    builder.Append(methodInfo.ReturnType.GetDevelopName());
                    builder.Append('>');

                }
                else
                {

                    builder.Append("Action");
                    var parameters = methodInfo.GetParameters();
                    if (parameters.Length > 0)
                    {
                        builder.Append('<');
                        for (int i = 0; i < parameters.Length; i++)
                        {
                            builder.Append(parameters[i].ParameterType.GetDevelopName());
                            if (i != parameters.Length - 1)
                            {
                                builder.Append(',');
                            }
                        }
                        builder.Append('>');
                    }


                }


                _staticDelegateOrderScriptMapping[@delegate] = "_func" + _staticNameDelegateMapping.Count;
                _staticNameDelegateMapping.Add((name, @delegate, builder.ToString()));
            }

        }


        public Proxier Complie()
        {

            StringBuilder _fieldBuilder = new StringBuilder();
            StringBuilder _methodBuilder = new StringBuilder();
            HashSet<string> _fieldCache = new HashSet<string>();

            for (int i = 0; i < _staticNameDelegateMapping.Count; i += 1)
            {
                var temp = _staticNameDelegateMapping[i];
                var script = _staticDelegateOrderScriptMapping[temp.@delegate];
                if (!_fieldCache.Contains(script))
                {
                    _fieldCache.Add(script);
                    _fieldBuilder.AppendLine($"public static {temp.typeScript} {script};");
                    _methodBuilder.AppendLine($"{script} = ({temp.typeScript})(delegatesInfo[{i}].@delegate);");
                }



                //添加委托调用
                var method = _methodMapping[temp.memberName];
                StringBuilder builder = new StringBuilder();
                var infos = method.GetParameters().OrderBy(item => item.Position);
                foreach (var item in infos)
                {

                    builder.Append(item.Name + ",");

                }
                if (builder.Length > 0)
                {

                    builder.Length -= 1;

                }


                Type returnType = method.ReturnType;
                if (returnType != typeof(void))
                {
                    if (returnType.IsGenericType)
                    {
                        if (returnType.GetGenericTypeDefinition() == typeof(Task<>) || returnType.GetGenericTypeDefinition() == typeof(ValueTask<>))
                        {
                            builder.Insert(0, $"return await {script}(");
                        }
                    }
                    else if (returnType == typeof(Task) || returnType.BaseType == typeof(ValueTask))
                    {
                        builder.Insert(0, $"await {script}(");
                    }
                    else
                    {
                        builder.Insert(0, $"return {script}(");
                    }
                }
                else
                {
                    builder.Insert(0, $"{script}(");
                }

                builder.Append(");");
                SetMethod(temp.memberName, builder.ToString());

            }


            _fieldBuilder.AppendLine($@"public static void SetProxyDelegate(List<(string memberName, Delegate @delegate, string typeScript)> delegatesInfo){{");
            _fieldBuilder.Append(_methodBuilder);
            _fieldBuilder.Append('}');


            foreach (var item in NeedReWriteMethods)
            {

                if (!item.Value.Contains(NoNeedWriting))
                {
                    _fieldBuilder.AppendLine(item.Value);
                }

            }
            if (_useSingleton)
            {
                _fieldBuilder.Append($@"public static readonly {CurrentProxyName} Instance;");
                _fieldBuilder.Append($@"public static void {SingletonMethodName}({CurrentProxyName} value){{ Unsafe.AsRef(Instance) = value; }}");
            }
            _fieldBuilder.Append(ProxyBody);
            ClassBuilder.Body(_fieldBuilder.ToString());
            var type = ClassBuilder.GetType();


            var action = NDelegate.UseCompiler(ClassBuilder.AssemblyBuilder).Action<List<(string memberName, Delegate @delegate, string typeScript)>>($@"
                    {CurrentProxyName}.SetProxyDelegate(obj);
                ");
            action(_staticNameDelegateMapping);
            return this;
        }


        private string GetSetSingletonInstanceScript(params string[] parameters)
        {
            return $"{CurrentProxyName}.{SingletonMethodName}(new {CurrentProxyName}({string.Join(",", parameters)}));";
        }

        public Func<TInterface> GetDefaultCreator<TInterface>()
        {

            Complie();
            if (_useSingleton)
            {

                return NDelegate.UseCompiler(ClassBuilder.AssemblyBuilder).Func<TInterface>($@"

                     return {CurrentProxyName}.Instance;

                ");

            }
            else
            {

                return NDelegate.UseCompiler(ClassBuilder.AssemblyBuilder).Func<TInterface>($@"

                     return new {CurrentProxyName}();

                ");

            }


        }

        public IEnumerator<MethodInfo> GetEnumerator()
        {
            foreach (var item in _methodMapping)
            {
                yield return item.Value;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _methodMapping.Values.GetEnumerator();
        }


        #region 单例初始化函数
        public void InitSingletonCreator<P>(P value)
        {

            Complie();
            NDelegate.UseCompiler(ClassBuilder.AssemblyBuilder).Action<P>($@"

                     {GetSetSingletonInstanceScript("obj")};

                ")(value);
            FillInstance();

        }

        public void InitSingletonCreator<P1, P2>(P1 value1,P2 value2)
        {

            Complie();
            NDelegate.UseCompiler(ClassBuilder.AssemblyBuilder).Action<P1, P2>($@"

                     {GetSetSingletonInstanceScript("arg1", "arg2")};

                ")(value1, value2);
            FillInstance();

        }

        public void InitSingletonCreator<P1, P2, P3>(P1 value1, P2 value2, P3 value3)
        {

            Complie();
            NDelegate.UseCompiler(ClassBuilder.AssemblyBuilder).Action<P1, P2, P3>($@"

                     {GetSetSingletonInstanceScript("arg1", "arg2", "arg3")};

                ")(value1, value2, value3);
            FillInstance();

        }

        public void InitSingletonCreator<P1, P2, P3, P4>(P1 value1, P2 value2, P3 value3, P4 value4)
        {

            Complie();
            NDelegate.UseCompiler(ClassBuilder.AssemblyBuilder).Action<P1, P2, P3, P4>($@"

                     {GetSetSingletonInstanceScript("arg1", "arg2", "arg3", "arg4")};

                ")(value1, value2, value3, value4);
            FillInstance(); 

        }

        public void InitSingletonCreator<P1, P2, P3, P4, P5>(P1 value1, P2 value2, P3 value3, P4 value4, P5 value5)
        {

            Complie();
            NDelegate.UseCompiler(ClassBuilder.AssemblyBuilder).Action<P1, P2, P3, P4, P5>($@"

                     {GetSetSingletonInstanceScript("arg1", "arg2", "arg3", "arg4", "arg5")};

                ")(value1, value2, value3, value4, value5);
            FillInstance();

        }

        public void InitSingletonCreator<P1, P2, P3, P4, P5, P6>(P1 value1, P2 value2, P3 value3, P4 value4, P5 value5, P6 value6)
        {

            Complie();
            NDelegate.UseCompiler(ClassBuilder.AssemblyBuilder).Action<P1, P2, P3, P4, P5, P6>($@"

                     {GetSetSingletonInstanceScript("arg1", "arg2", "arg3", "arg4", "arg5", "arg6")};

                ")(value1, value2, value3, value4, value5, value6);
            FillInstance();

        }

        public void InitSingletonCreator<P1, P2, P3, P4, P5, P6, P7>(P1 value1, P2 value2, P3 value3, P4 value4, P5 value5, P6 value6, P7 value7)
        {

            Complie();
            NDelegate.UseCompiler(ClassBuilder.AssemblyBuilder).Action<P1, P2, P3, P4, P5, P6, P7>($@"

                     {GetSetSingletonInstanceScript("arg1", "arg2", "arg3", "arg4", "arg5", "arg6", "arg7")};

                ")(value1, value2, value3, value4, value5, value6, value7);
            FillInstance();

        }

        public void InitSingletonCreator<P1, P2, P3, P4, P5, P6, P7, P8>(P1 value1, P2 value2, P3 value3, P4 value4, P5 value5, P6 value6, P7 value7, P8 value8)
        {

            Complie();
            NDelegate.UseCompiler(ClassBuilder.AssemblyBuilder).Action<P1, P2, P3, P4, P5, P6, P7, P8>($@"

                     {GetSetSingletonInstanceScript("arg1", "arg2", "arg3", "arg4", "arg5", "arg6", "arg7","arg8")};

                ")(value1, value2, value3, value4, value5, value6, value7, value8);
            FillInstance();

        }

        public void InitSingletonCreator<P1, P2, P3, P4, P5, P6, P7, P8, P9>(P1 value1, P2 value2, P3 value3, P4 value4, P5 value5, P6 value6, P7 value7, P8 value8, P9 value9)
        {

            Complie();
            NDelegate.UseCompiler(ClassBuilder.AssemblyBuilder).Action<P1, P2, P3, P4, P5, P6, P7, P8, P9>($@"

                     {GetSetSingletonInstanceScript("arg1", "arg2", "arg3", "arg4", "arg5", "arg6", "arg7", "arg8", "arg9")};

                ")(value1, value2, value3, value4, value5, value6, value7, value8, value9);
            FillInstance();

        }
        #endregion

        protected virtual void FillInstance() { }


    }


    public class Proxier<TInterface1> : Proxier
    {
        public readonly TInterface1 Singleton;
        public Proxier()
        {
            Implement<TInterface1>();
        }
        protected override void FillInstance()
        {
            Unsafe.AsRef(Singleton) = NDelegate.UseCompiler(ClassBuilder.AssemblyBuilder).Func<TInterface1>($@"

                  return {CurrentProxyName}.Instance;

            ")();
        }


        #region 获取初始化委托
        public Func<TInterface1> GetCreator()
        {

            Complie();
            return NDelegate.UseCompiler(ClassBuilder.AssemblyBuilder).Func<TInterface1>($@"

                     return new {CurrentProxyName}();

                ");

        }

        public Func<T1, TInterface1> GetCreator<T1>()
        {

            Complie();
            return NDelegate.UseCompiler(ClassBuilder.AssemblyBuilder).Func<T1,TInterface1>($@"

                     return new {CurrentProxyName}(arg);

            ");

        }

        public Func<T1, T2, TInterface1> GetCreator<T1,T2>()
        {

            Complie();
            return NDelegate.UseCompiler(ClassBuilder.AssemblyBuilder).Func<T1, T2, TInterface1>($@"

                     return new {CurrentProxyName}(arg1,arg2);

            ");

        }

        public Func<T1, T2, T3, TInterface1> GetCreator<T1, T2, T3>()
        {

            Complie();
            return NDelegate.UseCompiler(ClassBuilder.AssemblyBuilder).Func<T1, T2, T3, TInterface1>($@"

                     return new {CurrentProxyName}(arg1,arg2,arg3);

            ");

        }

        public Func<T1, T2, T3, T4, TInterface1> GetCreator<T1, T2, T3, T4>()
        {

            Complie();
            return NDelegate.UseCompiler(ClassBuilder.AssemblyBuilder).Func<T1, T2, T3, T4, TInterface1>($@"

                     return new {CurrentProxyName}(arg1,arg2,arg3,arg4);

            ");

        }

        public Func<T1, T2, T3, T4, T5, TInterface1> GetCreator<T1, T2, T3, T4, T5>()
        {

            Complie();
            return NDelegate.UseCompiler(ClassBuilder.AssemblyBuilder).Func<T1, T2, T3, T4, T5, TInterface1>($@"

                     return new {CurrentProxyName}(arg1,arg2,arg3,arg4,arg5);

            ");

        }

        public Func<T1, T2, T3, T4, T5, T6, TInterface1> GetCreator<T1, T2, T3, T4, T5, T6>()
        {

            Complie();
            return NDelegate.UseCompiler(ClassBuilder.AssemblyBuilder).Func<T1, T2, T3, T4, T5, T6, TInterface1>($@"

                     return new {CurrentProxyName}(arg1,arg2,arg3,arg4,arg5,arg6);

            ");

        }

        public Func<T1, T2, T3, T4, T5, T6, T7, TInterface1> GetCreator<T1, T2, T3, T4, T5, T6, T7>()
        {

            Complie();
            return NDelegate.UseCompiler(ClassBuilder.AssemblyBuilder).Func<T1, T2, T3, T4, T5, T6, T7, TInterface1>($@"

                     return new {CurrentProxyName}(arg1,arg2,arg3,arg4,arg5,arg6,arg7);

            ");

        }

        public Func<T1, T2, T3, T4, T5, T6, T7, T8, TInterface1> GetCreator<T1, T2, T3, T4, T5, T6, T7, T8>()
        {

            Complie();
            return NDelegate.UseCompiler(ClassBuilder.AssemblyBuilder).Func<T1, T2, T3, T4, T5, T6, T7, T8, TInterface1>($@"

                     return new {CurrentProxyName}(arg1,arg2,arg3,arg4,arg5,arg6,arg7,arg8);

            ");

        }

        public Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TInterface1> GetCreator<T1, T2, T3, T4, T5, T6, T7, T8, T9>()
        {

            Complie();
            return NDelegate.UseCompiler(ClassBuilder.AssemblyBuilder).Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TInterface1>($@"

                     return new {CurrentProxyName}(arg1,arg2,arg3,arg4,arg5,arg6,arg7,arg8,arg9);

            ");

        }
        #endregion

    }
    public class Proxier<TInterface1, TInterface2> : Proxier<TInterface1>
    {
        public Proxier()
        {
            Implement<TInterface2>();
        }
    }
    public class Proxier<TInterface1, TInterface2, TInterface3> : Proxier<TInterface1, TInterface2>
    {
        public Proxier()
        {
            Implement<TInterface3>();
        }
    }
    public class Proxier<TInterface1, TInterface2, TInterface3, TInterface4> : Proxier<TInterface1, TInterface2, TInterface3>
    {
        public Proxier()
        {
            Implement<TInterface4>();
        }
    }
    public class Proxier<TInterface1, TInterface2, TInterface3, TInterface4, TInterface5> : Proxier<TInterface1, TInterface2, TInterface3, TInterface4>
    {
        public Proxier()
        {
            Implement<TInterface5>();
        }
    }
    public class Proxier<TInterface1, TInterface2, TInterface3, TInterface4, TInterface5, TInterface6> : Proxier<TInterface1, TInterface2, TInterface3, TInterface4, TInterface5>
    {
        public Proxier()
        {
            Implement<TInterface6>();
        }
    }
    public class Proxier<TInterface1, TInterface2, TInterface3, TInterface4, TInterface5, TInterface6, TInterface7> : Proxier<TInterface1, TInterface2, TInterface3, TInterface4, TInterface5, TInterface6>
    {
        public Proxier()
        {
            Implement<TInterface7>();
        }
    }
    public class Proxier<TInterface1, TInterface2, TInterface3, TInterface4, TInterface5, TInterface6, TInterface7, TInterface8> : Proxier<TInterface1, TInterface2, TInterface3, TInterface4, TInterface5, TInterface6, TInterface7>
    {
        public Proxier()
        {
            Implement<TInterface8>();
        }
    }
    public class Proxier<TInterface1, TInterface2, TInterface3, TInterface4, TInterface5, TInterface6, TInterface7, TInterface8, TInterface9> : Proxier<TInterface1, TInterface2, TInterface3, TInterface4, TInterface5, TInterface6, TInterface7, TInterface8>
    {
        public Proxier()
        {
            Implement<TInterface9>();
        }
    }
    public class Proxier<TInterface1, TInterface2, TInterface3, TInterface4, TInterface5, TInterface6, TInterface7, TInterface8, TInterface9, TInterface10> : Proxier<TInterface1, TInterface2, TInterface3, TInterface4, TInterface5, TInterface6, TInterface7, TInterface8, TInterface9>
    {
        public Proxier()
        {
            Implement<TInterface10>();
        }
    }
}
