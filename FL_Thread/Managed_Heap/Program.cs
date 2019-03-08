using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

//托管堆
//下面的所有都是在托管堆的前提下
namespace Managed_Heap
{

    /*
     * 在面向对象的环境中，每个类型都代表可供程序使用的一种资源
     * 使用资源的步骤：
     *  a.调用IL指令Newobj，为代表资源的类型分配内存。
     *  b.初始化内存，设置资源的初始化状态并使资源可用。
     *  c.访问类型的成员来使用资源。
     *  d.摧毁资源的状态以进行清理。（摧毁状态？）
     *  e.释放内存。垃圾回收器负责这一步。
     *  
     *  
     *  只有包装了本机资源（文件、套接字和数据库连接等）的类型才需要特殊清理。
     *  
     *  
     *  C#的new操作符导致CLR执行的步骤：
     *      a.计算类型的字段（以及从基类型继承的字段）所需的字节数。
     *      b.加上对象的开销所需的字节数。两个开销字段：类型对象指针和同步块索引。
     *      c.CLR检查区域中是否有分配对象所需的字节数。
     *      
     * 垃圾回收算法：
     *  
     *  对象的生存期管理：
     *      引用计数算法：例如COM 在COM中堆上的每个对象都维护着一个内存字段来统计程序中多少“部分”正在使用对象。
     *                                      最大问题：循环引用
     *                                      
     *      为避免循环引用问题，CLR采用引用跟踪算法
     *      引用跟踪算法：只关心引用类型（能引用堆上的对象）的变量
     * 
     * 引用类型变量：类的静态和实例字段，方法的参数和局部变量，将所有的引用类型变量称为【根】。
     * 
     * 引用跟踪算法的过程：
     *  标记：
     *  压缩：使幸存的对象占用连续的空间：好处 ->
     *                  从每个根减去所引用的对象在内存中偏移的字节数，保证每个根引用和之前一样的对象
     *  设置NextObjPtr的位置：
     *  
     * 注：静态字段引用的对象一直存在，直到用于加载类型的AppDomian卸载为止。静态字段容易导致内存泄漏。
     * 
     * 
     * System.Threading.Timer 的特殊性
     * 堆中存在的一个Timer对象会造成：一个线程池线程定期调用一个方法 其他任何类型都不具有该行为
     * 所有非Timer的对象都会根据应用程序的需要而自动存活
     */



    class Program
    {
        static void Main(string[] args)
        {
            #region timer 示例
            ////创建每2000毫秒就调用一次TimerCallback方法的Timer对象
            //Timer t = new Timer(TimerCallback, null, 0, 2000);
            ////在 Debug模式下  会循环输出
            ////此模式下 会为结果程序集设置DebuggingModes的DisableOptimizations标志
            ////运行时编译方法时，JIT编译器看到这个标志 会把所有根的生存期延长至方法结束
            ////所以JIT编译器任务Main的t变量必须存活至方法结束
            ////在Release模式下  只会输出一次
            ////回收开始时 垃圾回收器首先假定堆中的所有对象都是不可达的（垃圾）
            ////然后垃圾回收器检查应用程序的根，发现在初始化之后，Main方法再也没有使用过变量t，所以分配给t的内存会被回收


            //Console.ReadLine();

            ////t = null; //在Release模式下  输出一次  因为  t = null JIT编译器 优化掉
            //t.Dispose(); //在Release模式下  循环输出  要显示的释放t t才能存活到被释放的一刻
            #endregion


            string str1 = "1234";
            string str2 = "1235";

            GC.Collect();

            //疑问 初始化后 没有再使用Str2  str2  会被回收吗？  应该是不会！ timer比较特殊
            //
            str1 = "1256";

            Console.ReadLine();
        }


        private static void TimerCallback(Object o)
        {
            Console.WriteLine("In TimerCallback：" + DateTime.Now);

            //出于演示目的，强制执行一次垃圾回收
            GC.Collect();
        }
    }

    

}
