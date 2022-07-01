# KScript

## 基本介绍
此项目为本人从大二下学期开始写的一门基于C#的解释型脚本语言
经过近半年不懈的努力,不仅实现了一门编程语言的基本功能,还效仿其它语言增加了一些便利的特性。

KScript大致实现了如下内容:
* 基本的变量读写功能
* 分支(if/match)/循环结构(while/for/foreach)
* 函数定义与调用(支持重载、递归、变长参数表)
* 类的定义(包括静态成员、继承，还支持类似python的魔术方法)和实例化
* 调用C#原生代码(通过Native function/class)的接口
* 外部代码文件/模块引用
* 很垃圾的反射(基本没什么用...)
* 匿名方法/lambda表达式
* 各种对C#基本数据类型包装的内建类型(添加了很多丰富便捷的功能,也可以很容易添加自己的包装类)
* Debugger
* 其它各种细碎的小特性，如模仿python的切片、解包、
* 开发了一个配套小工具-KsIDE:
可以进行代码编辑(使用自制的具有语法高亮、简单的智能提示、断点设置等功能的代码编辑器组件)、代码调试。
目前可以通过插件接口使用我为KScript开发的小框架--KForm(可视化编程框架,通过封装WinForm实现),
可以像VB、C# WinForm一样通过拖拉拽的方式进行GUI编程

各部分实现都很初级，而且相当不规范。打算后续深入学习编译原理之后实现一门更完整的编译型语言。

## 效果展示
![image](https://user-images.githubusercontent.com/21328219/176927391-01e6f707-97e0-4039-bd72-aab44bf3f56b.png)

![image](https://user-images.githubusercontent.com/21328219/176927570-51ac50f2-7b39-471a-94e3-813fbf9a1e03.png)

![image](https://user-images.githubusercontent.com/21328219/176927798-15bbca81-87f0-4535-bdcf-e809ec0f86ee.png)

![image](https://user-images.githubusercontent.com/21328219/176928015-0e01ae65-99f5-4c02-bd21-919764308812.png)
