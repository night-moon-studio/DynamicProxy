# DynamicProxy
基于 Natasha 的动态函数代理。 

<br/>
<br/>  

## 准备工作


#### 使用方法(User Api)：  

 <br/>  
 
 - 引入 动态构件库： NMS.DynamicProxy

 - 引入 编译环境库： DotNetCore.Compile.Environment

 - 向引擎中注入定制的域： DomainManagement.RegisterDefault< AssemblyDomain >()

 - 敲代码  
 
<br/>  


#### 添加引用

```C#

 // Nuget NMS.DynamicProxy
 // using Natahsa; 
```

<br/> 
<br/> 

## 使用方法

#### 实现接口

##### 实现一个接口

```C# 

var proxier = new Proxier<Interface>();

//或

var proxier = new Proxier();
proxier.Implement(Interface1);
proxier.Implement<Interface2>(); 

```

##### 继承抽象类及多个接口

```C# 

//抽象类务必要放在第一位
var proxier = new Proxier<Abstract,Interface1,Interface2...>(); 

```
<br/>

#### 重写函数


```C# 

//可以对需方法，接口方法，抽象方法，重载方法进行重写
var proxier = new Proxier<Abstract,Interface1,Interface2...>();

//直接写C#代码即可
proxier["abstract_method"] = "大胆写你的代码 e.g: return 0;";
proxier["virutal_method"] = "像后面的例子都行 e.g; if(arg == 0){ return xxx;} return default;";
proxier["overrider_method"] = "写就完了 e.g: return base.ToString();";
proxier["interface_method"] = "随便写什么代码";

//还可以使用委托直接赋值
Func<string,int> func = item=>item.Length;
proxier["interface_method"] = func;

//还可以直接传送实例的方法
var instance = new Student();
proxier["xxxMethod"] = (Func<School>)instance.GetSchool;  

```
<br/> 

#### 额外的引用

```c#

proxier.Using( string / assembly / type / string[] / assembly[] / type );

//例如：
proxier.Using("System.Collection");
proxier.Using("MySql.Data");
proxier.Using(typeof(School));

```
> 原则上 Natasha 会构建所有的 using, 但如果您的类型是插件 DLL 加载进来的，或者动态生成，您需要使用 Using 方法添加命名空间。

<br/>

#### 创建实例

```C#

//根据上面的构建，拿到实例创建的委托
var interfaceCreator = proxier.GetCreator<Interface>();
var abstractCreator = proxier.GetCreator<Abstract>();

Interface iTemp = interfaceCreator();
Abstract aTemp = abstractCreator();

```

<br/>  
  
  
#### 执行方法

```C#

iTemp.XXX();
aTemp.XXX();

```

<br/>  
  
#### 单例实例

```C#

proxier.UseSingleton();

//这样上面的代码:
//  Interface iTemp = interfaceCreator();
//  Abstract aTemp = abstractCreator();
// iTemp == aTemp  两个委托返回的都是那个单例

```

<br/>  
 
#### 加载额外引用

```C#

proxier.AddDll("xxx.dll");

```
