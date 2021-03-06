﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FL_OtherTest
{
    class Program
    {
        static void Main(string[] args)
        {
            TypeChangeTest.GO();

            Console.ReadKey();
        }
    }

    class TypeChangeTest
    {

        /*
         * as：
         * 当转换失败时  as不会抛出异常 而是 转换后的对象为null
         * 比强制类型转换要安全，在运行时层面也有比较好的效率
         * as 和 is操作符 都不执行任何 用户自定义的转换（强制转换好像可以自定义方法来实现强制转换）
         * 只有当运行时类型与目标转换类型匹配时，它们才会转换成功
         * 总的来看 使用as时  如果实际的对象是本身 或者父类 都可以转换成功 而不关心指向对象的指针类型
         * as  必须和引用类型 或者 可为null的类型一起使用
         * 
         * 强制转换：
         * 当转换失败的时候 会直接抛出异常
         * 当 对象本身是要转成的对象的类型或者其父类试  可以成功
         * 可以自定义类型转换方法：隐式 / 显示
         *      
         * 编译时和运行时：
         * 用户自定义的转化操作符  只作用于  [对象的编译时类型]
         * 
         * 总结：
         * as和强制转换之间最大的区别就在于如何处理用户自定义的转换。操作符 as和 is 都只检查被转换对象的运行时类型，并不执行其他的操作。如果被转换对象的运行时类型既不是所转换的目标类型，也不是其派生类型，那么转型将告失败。但是强制转型则会使用转换操作符来执行转型操作，这包括任何内建的数值转换（如：long转int）
         */

        static TypeBase tBase = new TypeBase();
        static TypeBase tBase2 = new TypeA();
        static TypeOther to = new TypeOther();
        static TypeA ta = new TypeA(to);
        static object taobj = new TypeA(to);
        //static TypeA ta2 = new TypeBase();      //无法  直接  将一个子类指针指向父类对象

        public static void GO()
        {
            //AS_TypeChange();
            Forced_TypeChange();
            //IS_TypeTest();
        }

        private static void AS_TypeChange()
        {
            //使用as 将 父类对象  转成 子类   ==>  失败
            TypeA temp = tBase as TypeA;
            if (temp != null)
                Console.WriteLine("1 as change success.");
            else
                Console.WriteLine("1 as change fail.");

            //使用as 将 父类指针指向的子类对象  转成 子类   ==>  成功
            temp = tBase2 as TypeA;
            if (temp != null)
                Console.WriteLine("2 as change success.");
            else
                Console.WriteLine("2 as change fail.");

            //使用as 将 子类指针指向的子类对象  转成 父类   ==>  成功
            TypeBase tempBase = ta as TypeBase;
            if (tempBase != null)
                Console.WriteLine("3 as change success.");
            else
                Console.WriteLine("3 as change fail.");

            //使用as 将 父类 指针指向的子类对象  转成 父类   ==>  无法  直接  将一个子类指针指向父类对象

            //使用 as 转换 用户自定义的类型转换方法 ==> 报错
            //TypeOther tempOther = ta as TypeOther;  

            //as  必须和引用类型 或者 可为null的类型一起使用
            int num = 10;
            //float f = num as float;    错误
            float? f = num as float?;    //正确 但也没什么意义
        }

        private static void Forced_TypeChange()
        {
            TypeA temp;
            ////使用 强制转换 将 父类对象  转成 子类   ==>  失败
            //TypeA temp = (TypeA)tBase;      //直接抛出异常
            //if (temp != null)
            //    Console.WriteLine("1 Forced change success.");
            //else
            //    Console.WriteLine("1 Forced change fail.");

            //使用 强制转换 将 父类指针指向的子类对象  转成 子类   ==>  成功
            temp = (TypeA)tBase2;
            if (temp != null)
                Console.WriteLine("2 Forced change success.");
            else
                Console.WriteLine("2 Forced change fail.");

            //使用 强制转换 将 子类指针指向的子类对象  转成 父类   ==>  成功
            TypeBase tempBase = (TypeBase)ta;
            if (tempBase != null)
                Console.WriteLine("3 Forced change success.");
            else
                Console.WriteLine("3 Forced change fail.");

            TypeOther tempOther;
            //使用 强制转换 转换 用户自定义的类型转换方法 ==> 成功 也可自定义为隐式转换
            tempOther = (TypeOther)ta;
            if (tempOther != null)
                Console.WriteLine("4 Forced change success.");
            else
                Console.WriteLine("4 Forced change fail.");

            //使用 强制转换 转换 用户自定义的类型转换方法 object指向的TypeA类型的对象 ==> 失败 
            //原因 用户自定义的转化操作符  只作用于  [对象的编译时类型]
            //即 此时的taobj的编译时类型是 object object类型 不存在TypeOther 强制转换操作  所以会抛出异常 （可以先as TypeA再转TypeOther...）
            //（为什么编译时不会报错呢？）
            tempOther = (TypeOther)taobj;       //运行时抛出异常
            if (tempOther != null)
                Console.WriteLine("5 Forced change success.");
            else
                Console.WriteLine("5 Forced change fail.");

        }

        private static void IS_TypeTest()
        {
            bool isType = false;

            //使用 is 判断父类对象  是否是 子类   ==>  不是
            isType = tBase is TypeA;      //直接抛出异常
            if (isType)
                Console.WriteLine("1 is type.");
            else
                Console.WriteLine("1 is not type.");

            //使用 is 判断 父类指针指向的子类对象  是否是 子类   ==>  是
            isType = tBase2 is TypeA;
            if (isType)
                Console.WriteLine("2 is type.");
            else
                Console.WriteLine("2 is not type.");

            //使用 is 判断 子类指针指向的子类对象  是否是 父类   ==>  是
            isType = ta is TypeBase;
            if (isType)
                Console.WriteLine("3 is type.");
            else
                Console.WriteLine("3 is not type.");

            //不是
            isType = ta is TypeOther;
            if (isType)
                Console.WriteLine("4 is type.");
            else
                Console.WriteLine("4 is not type.");
        }

        class TypeBase
        {
            public string Name { set; get; }

        }

        class TypeA : TypeBase
        {
            private TypeOther to;
            public string Age { set; get; }

            public TypeA()
            {
            }

            public TypeA(TypeOther to)
            {
                this.to = to;
            }

            //自定义强制转换
            //implicit是隐式 explicit是显示
            public static implicit operator TypeOther(TypeA ta)
            {
                return ta.to;
            }

            //运算符重载
            public static string operator +(TypeA a, TypeA b)
            {
                return a.Age + b.Age;
            }
        }

        class TypeOther
        {

        }
    }

    class RAM_Stack {

    }


}
