using DynamicProxy;
using Natasha.Operator;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Natasha
{
    public class Proxier
    {
        public const string NoNeedWriting = "NW1000-NoNeedToWrite";
        private readonly NClass _builder;
        private readonly ConcurrentDictionary<string, MethodInfo> _methodMapping;
        private readonly ConcurrentDictionary<Delegate, string> _staticDelegateOrderScriptMapping;
        private readonly List<(string memberName, Delegate @delegate, string typeScript)> _staticNameDelegateMapping;
        private bool _needReComplie;
        private bool _useSingleton;
        public StringBuilder ProxyBody;
        public readonly ConcurrentDictionary<string, string> NeedReWriteMethods;

       
        public Proxier()
        {

            _builder = NClass.Random().Public.Namespace("Natasha.Proxy");
            NeedReWriteMethods = new ConcurrentDictionary<string, string>();
            _methodMapping = new ConcurrentDictionary<string, MethodInfo>();
            _staticDelegateOrderScriptMapping = new ConcurrentDictionary<Delegate, string>();
            _staticNameDelegateMapping = new List<(string memberName, Delegate @delegate, string typeScript)>();
            _needReComplie = true;
            ProxyBody = new StringBuilder();

        }




        public string CurrentProxyName
        {
            get { return _builder.OopNameScript; }
        }




        public Proxier UseSingleton(bool singleton = true)
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

            _builder.Using(@namespace);
            return this;

        }
        /// <summary>
        /// 额外添加 dll 引用
        /// </summary>
        /// <param name="path">dll文件路径</param>
        /// <returns></returns>
        public Proxier AddDll(string path)
        {
            _builder.Complier.Domain.LoadStream(path);
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

                if (value.StringParameter!=default)
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

            _needReComplie = true;
            _builder.Inheritance(type);
            var methods = type.GetMethods();
            foreach (var item in methods)
            {

                if (item.IsHideBySig)
                {

                    _methodMapping[item.Name] = item;
                    if (type.IsInterface)
                    {

                        SetMethod(item.Name, item.ReturnType == typeof(void) ? default : "return default;");

                    }
                    else if (item.IsAbstract)
                    {

                        if (!item.Equals(item.GetBaseDefinition()))
                        {

                            SetMethod(item.Name, NoNeedWriting);

                        }
                        else
                        {

                            SetMethod(item.Name, item.ReturnType == typeof(void) ? default : "return default;");

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




        public void SetMethod(string name,string script)
        {

            if (_methodMapping.ContainsKey(name))
            {

                _needReComplie = true;
                var method = _methodMapping[name];
                var type = method.DeclaringType;
                var template = FakeMethodOperator.Random();
                if (!type.IsInterface)
                {

                    _ = (method.IsAbstract || method.IsVirtual) ? template.OverrideMember : template.NewMember;

                }
                var result = template
                    .UseMethod(method)
                    .MethodContent(script).Builder().MethodScript;


                NeedReWriteMethods[name] = result;

            }

        }
        public void SetMethod(string name,Delegate @delegate)
        {

            if (_methodMapping.ContainsKey(name))
            {

                _needReComplie = true;
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
                    if (parameters.Length>0)
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




        private Proxier Complie()
        {

            if (_needReComplie)
            {

                _builder.UseRandomOopName();
                _builder.OopContentScript.Clear();
                StringBuilder _fieldBuilder = new StringBuilder();
                StringBuilder _methodBuilder = new StringBuilder();
                HashSet<string> _fieldCache = new HashSet<string>();

                for (int i = 0; i < _staticNameDelegateMapping.Count; i+=1)
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


                    if (method.ReturnType!=typeof(void))
                    {

                        builder.Insert(0, $"return {script}(");

                    }
                    else
                    {

                        builder.Insert(0, $"{script}(");

                    }

                    builder.Append(");");
                    SetMethod(temp.memberName, builder.ToString());

                }


                _fieldBuilder.AppendLine($@"public static void SetDelegate(List<(string memberName, Delegate @delegate, string typeScript)> delegatesInfo){{");
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
                    _fieldBuilder.Append($@"static {CurrentProxyName}(){{ Instance = new {CurrentProxyName}(); }}");
                }
                _fieldBuilder.Append(ProxyBody);
                _builder.OopBody(_fieldBuilder);
                var type = _builder.GetType();


                var action = NDomain.Create(_builder.Complier.Domain).Action<List<(string memberName, Delegate @delegate, string typeScript)>>($@"
                    {_builder.OopNameScript}.SetDelegate(obj);
                ", "Natasha.Proxy");
                action(_staticNameDelegateMapping);


                _needReComplie = false;

            }

            return this;
        }


        public Func<TInterface> GetCreator<TInterface>()
        {

            Complie();
            if (_useSingleton)
            {

                return NDomain.Create(_builder.Complier.Domain).Func<TInterface>($@"

                     return {_builder.OopNameScript}.Instance;

                ", "Natasha.Proxy");

            }
            else
            {

                return NDomain.Create(_builder.Complier.Domain).Func<TInterface>($@"

                     return new {_builder.OopNameScript}();

                ", "Natasha.Proxy");

            }
           

        }
    }


    public class Proxier<TInterface1> :Proxier
    {
        public Proxier()
        {
            Implement<TInterface1>();
        }
    }
    public class Proxier<TInterface1, TInterface2> : Proxier
    {
        public Proxier()
        {
            Implement<TInterface1>();
            Implement<TInterface2>();
        }
    }
    public class Proxier<TInterface1, TInterface2, TInterface3> : Proxier
    {
        public Proxier()
        {
            Implement<TInterface1>();
            Implement<TInterface2>();
            Implement<TInterface3>();
        }
    }
    public class Proxier<TInterface1, TInterface2, TInterface3, TInterface4> : Proxier
    {
        public Proxier()
        {
            Implement<TInterface1>();
            Implement<TInterface2>();
            Implement<TInterface3>();
            Implement<TInterface4>();
        }
    }
    public class Proxier<TInterface1, TInterface2, TInterface3, TInterface4, TInterface5> : Proxier
    {
        public Proxier()
        {
            Implement<TInterface1>();
            Implement<TInterface2>();
            Implement<TInterface3>();
            Implement<TInterface4>();
            Implement<TInterface5>();
        }
    }
    public class Proxier<TInterface1, TInterface2, TInterface3, TInterface4, TInterface5, TInterface6> : Proxier
    {
        public Proxier()
        {
            Implement<TInterface1>();
            Implement<TInterface2>();
            Implement<TInterface3>();
            Implement<TInterface4>();
            Implement<TInterface5>();
            Implement<TInterface6>();
        }
    }
    public class Proxier<TInterface1, TInterface2, TInterface3, TInterface4, TInterface5, TInterface6, TInterface7> : Proxier
    {
        public Proxier()
        {
            Implement<TInterface1>();
            Implement<TInterface2>();
            Implement<TInterface3>();
            Implement<TInterface4>();
            Implement<TInterface5>();
            Implement<TInterface6>();
            Implement<TInterface7>();
        }
    }
    public class Proxier<TInterface1, TInterface2, TInterface3, TInterface4, TInterface5, TInterface6, TInterface7, TInterface8> : Proxier
    {
        public Proxier()
        {
            Implement<TInterface1>();
            Implement<TInterface2>();
            Implement<TInterface3>();
            Implement<TInterface4>();
            Implement<TInterface5>();
            Implement<TInterface6>();
            Implement<TInterface7>();
            Implement<TInterface8>();
        }
    }
    public class Proxier<TInterface1, TInterface2, TInterface3, TInterface4, TInterface5, TInterface6, TInterface7, TInterface8, TInterface9> : Proxier
    {
        public Proxier()
        {
            Implement<TInterface1>();
            Implement<TInterface2>();
            Implement<TInterface3>();
            Implement<TInterface4>();
            Implement<TInterface5>();
            Implement<TInterface6>();
            Implement<TInterface7>();
            Implement<TInterface8>();
            Implement<TInterface9>();
        }
    }
    public class Proxier<TInterface1, TInterface2, TInterface3, TInterface4, TInterface5, TInterface6, TInterface7, TInterface8, TInterface9, TInterface10> : Proxier
    {
        public Proxier()
        {
            Implement<TInterface1>();
            Implement<TInterface2>();
            Implement<TInterface3>();
            Implement<TInterface4>();
            Implement<TInterface5>();
            Implement<TInterface6>();
            Implement<TInterface7>();
            Implement<TInterface8>();
            Implement<TInterface9>();
            Implement<TInterface10>();
        }
    }
}
