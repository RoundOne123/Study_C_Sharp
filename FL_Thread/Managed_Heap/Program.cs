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
    #region 托管堆基础
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
    #endregion

    #region 代：性能提升
    /*
     * CLR的GC是基于代的垃圾回收器
     *  对代码的假设：1、2、3
     * 
     * 托管堆在初始化时不包含对象
     *      CLR初始化时为第0代选择一个预算容量（以KB为单位）
     *      第一次垃圾回收后，第0代就不包含任何对象了。新对象会分配到第0代中。
     *      开始垃圾回收时，垃圾回收器必须决定检查哪些代
     *      【CLR初始化时】会为第0代对象【选择预算】，事实上，它还必须为第1代选择预算
     *      （选择预算是为每一代划分最大的能分配多少空间的对象 如果超过这个预算就要进行垃圾回收）
     *      由于第1代占用的内存远少于预算，所以垃圾回收器只检查第0代中的对象
     *          根据假设第0代包含更多垃圾的可能性很大，能回收更多内存，
     *      
     *      提升垃圾回收性能的点：
     *          a.忽略第1代中的对象显然能提升垃圾回收器的性能。
     *          b.对性能有更大提振作用的是：现在不必遍历托管堆中的对象。
     *              如果根或对象（所有代的？）对象引用了老一代对象：
     *                  垃圾回收器就可以忽略老对象内部的所有引用（此时可能存在的问题就是 
     *                  老对象又引用了新对象，下面会讲），能在更短的时间内构造好【可达对象图】
     *              如果老对象的字段引用新对象：
     *                  为了确保对老对象的已更新字段进行检查，垃圾回收器利用JIT编译器内部的一个机制。
     *                  这个机制在对象的引用字段发生变化时，会设置一个对应的【位标志】。
     *                  这样，垃圾回收器就知道自上一次垃圾回收以来，哪些老对象已被写入。
     *                  只有字段发生变化的老对象才需检查是否引用了第0代中的任何新对象。
     *      
     *      在某次垃圾回收的时候，如果第0代和第1代都达到了预算 则 第1代和第0代都会进行垃圾回收
     *      回收的结果是：第1代的幸存者提升至第2代，第0代的幸存者提升至第1代，第0代再次空出来
     *      
     *      托管堆只支持三代：0、1、2 没有第3代
     *      
     *      CLR初始化时，会为每一代选择预算，然而CLR垃圾回收器是自调节的
     *      基于垃圾回收的结果，垃圾回收器可能增大或减小这些代的预算，从而提升应用程序的总体性能。
     *          即，垃圾回收器会根据应用程序要求的内存负载来自动优化。
     *      
     *                  
     */
    #endregion

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


    /*
     * 示例自定义类CGNotfication类在第0代或第2代回收时引发一个事件。
     * 可以通过这个类方便的检测应用程序，更好的理解应用程序的内存使用情况。
     * 看不太懂
     */

    public static class GCNotification
    {
        private static Action<Int32> s_gcDone = null;   //事件的字段

        public static event Action<Int32> GCDone {
            add
            {
                if(s_gcDone == null)
                {
                    new GenObject(0);
                    new GenObject(2); //？？？？这个写法是什么意思？
                }
                s_gcDone += value;
            }
            remove
            {
                s_gcDone -= value;
            }
        }

        //sealed 密封类 防止其它类继承此类
        private sealed class GenObject
        {
            private Int32 m_generation;
            public GenObject(Int32 generation)
            {
                m_generation = generation;
            }

            ~GenObject()
            {
                if(GC.GetGeneration(this) >= m_generation)
                {
                    Action<Int32> temp = Volatile.Read(ref s_gcDone);
                    if (temp != null)
                        temp(m_generation);
                }

                if((s_gcDone != null) 
                    && !AppDomain.CurrentDomain.IsFinalizingForUnload() 
                    && !Environment.HasShutdownStarted)
                {
                    if (m_generation == 0)
                        new GenObject(0);
                    else
                        GC.ReRegisterForFinalize(this);
                }
                else
                {
                    /*放过对象，让其被回收*/
                }
            }
        }

    }
}
