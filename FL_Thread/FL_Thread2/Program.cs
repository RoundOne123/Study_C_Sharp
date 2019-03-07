using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
using System.Threading;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;

namespace FL_Thread2
{
    class Program
    {
        static void Main(string[] args)
        {
            //FontAndBackgroundThread();
            //AsynCallMethodByThreadInThreadPool();
            //ControlContentFlow();
            //CancellationDemo.Go();
            //TaskResultDemo.Go();

            //ParallelExampleThree();

            //TaskExampleOne();

            ParentAndChild();

            Console.ReadKey();
        }

        #region 前台线程和后台线程
        static void FontAndBackgroundThread()
        {
            //创建新线程（默认前台线程）
            Thread t = new Thread(Worker);

            t.IsBackground = false;//设为后台线程
            t.Start();  //启动线程

            //t 为前台线程，则需要等到所有前台线程结束 该程序才会结束 这里即要等到 t 5s
            //t 为后台线程，则当前没前台线程 所有程序立即结束

            Console.WriteLine("Returning from Main");
        }

        private static void Worker()
        {
            Thread.Sleep(5000);

            Console.WriteLine("Returning from Worker");
        }
        #endregion

        #region 线程池中的线程异步调用方法
        static void AsynCallMethodByThreadInThreadPool()
        {
            Console.WriteLine("Main thread: queuing an asynchronous operation"); //1
            ThreadPool.QueueUserWorkItem(ComputeBoundOp, 5);
            Console.WriteLine("Main thread: Doing other work here...");//2  2和3 顺序可能互调
            Thread.Sleep(5000);
            Console.WriteLine("Hit <Enter> to end this program...");//4
            Console.ReadLine();
        }

        private static void ComputeBoundOp(Object state)
        {
            Console.WriteLine("In ComputeBoundOp: state = {0}", state);//3
            Thread.Sleep(1000);
        }
        #endregion

        #region 调用ThreadPool.QueueUserWorkItem时阻止上下文的流动
        static void ControlContentFlow()
        {
            //将数据放入 主线程的逻辑调用上下文中
            CallContext.LogicalSetData("Name", "Jeffrey");

            //初始化要由一个线程池线程做的一些工作
            //线程池线程要访问逻辑调用上下文数据
            ThreadPool.QueueUserWorkItem(state => Console.WriteLine("Name={0}", CallContext.LogicalGetData("Name")));

            //阻止Main线程的执行上下文的流动
            ExecutionContext.SuppressFlow();

            //初始化要由线程池做的工作
            //线程池线程不能访问逻辑调用上下文
            ThreadPool.QueueUserWorkItem(state => Console.WriteLine("Name={0}", CallContext.LogicalGetData("Name")));

            //恢复Main线程的执行上下文的流动
            //以免将来使用更多的线程池线程
            ExecutionContext.RestoreFlow();

            //...

            Console.ReadLine();

            //Result：
            //Name=Jeffrey
            //Name =
        }
        #endregion

        #region 为自己的计算限制操作添加取消能力
        internal static class CancellationDemo
        {
            public static void Go()
            {
                CancellationTokenSource cts = new CancellationTokenSource();

                ThreadPool.QueueUserWorkItem(o =>  Count(cts.Token, 1000));

                Console.WriteLine("Press <Enter> to cancel the operation.");
                Console.ReadLine();
                cts.Cancel();

                Console.ReadLine();
            }
        }

        private static void Count(CancellationToken token, Int32 countTo)
        {
            for(Int32 count = 0; count < countTo; count++)
            {
                if (token.IsCancellationRequested)
                {
                    Console.WriteLine("Count is cancelled");
                    break;
                }
                Console.WriteLine(count);
                Thread.Sleep(200);  //出于演示目的浪费一些时间
            }

            Console.WriteLine("Count is done");
        }
        #endregion

        #region 任务 -- 等待任务完成并获取结果
        internal static class TaskResultDemo
        {
            public static void Go()
            {
                Task<Int32> t = new Task<Int32>(n => Sum((Int32)n), 1000000000);

                t.Start();  //启动任务

                t.Wait();   //显式等待任务完成  
                //Task.WaitAny();
                Console.WriteLine("The sum is " + t.Result);
                Console.ReadKey();
            }


            private static Int32 Sum(Int32 n)
            {
                Int32 sum = 0;
                for (; n > 0; n--)
                    checked { sum+= n; }   //防止n太大 太大时会抛出异常    
                //这里的异常会被“吞噬”并存储到一个集合中  而线程池线程可以返回到线程池中
                //调用 Wait方法或者Result属性时  这些成员会抛出一个System.AggregateException对象（封装了异常对象的一个集合）
                return sum;
            }
        }
        #endregion

        #region 任务 -- 取消任务
        internal static class CancelTaskDemo
        {
            public static void Go()
            {
                CancellationTokenSource cts = new CancellationTokenSource();

                Task<Int32> t = new Task<Int32>( () => Sum(cts.Token, 1000000000), cts.Token);

                cts.Cancel();   //这是异步请求，Task可能已经完成了

                try
                {
                    Console.WriteLine("The sum is: " + t.Result);
                }catch(AggregateException x)
                {
                    x.Handle(e => e is OperationCanceledException);
                    Console.WriteLine("Sum was canceled");
                }
            }

            private static Int32 Sum(CancellationToken ct, Int32 n)
            {
                Int32 sum = 0;
                for(; n > 0; n--)
                {
                    //在取消标志引用的CancellationTokenSource上调用Cancel
                    //下面这行代码就会抛出OperationCanceledException
                    ct.ThrowIfCancellationRequested();

                    checked { sum += n; }
                }
                return sum;
            }
        }
        #endregion


        private static void ParallelExampleThree()
        {
            ParallelLoopResult result = Parallel.For(10, 40, async (int i, ParallelLoopState pls) =>
            {
                Console.WriteLine("i : {0}, task: {1}", i, Task.CurrentId);
                //await Task.Delay(10);
                if (i > 15)
                    pls.Break();
            });
            Console.WriteLine("Is completed: {0}", result.IsCompleted);
            Console.WriteLine("lowest break iteration: {0}", result.LowestBreakIteration);
        }


        /// <summary>
        /// 任务相关
        /// </summary>
        static object taskMethodLock = new object();
        static void TaskMethod(object title)
        {
            lock (taskMethodLock)
            {
                Console.WriteLine(title);
                Console.WriteLine("Task id : {0}, thread: {1}", Task.CurrentId == null ? "no task" : Task.CurrentId.ToString(), Thread.CurrentThread.ManagedThreadId);

                Console.WriteLine("is pooled thread : {0}", Thread.CurrentThread.IsThreadPoolThread);
                Console.WriteLine("is background thread: {0}", Thread.CurrentThread.IsBackground);
                Console.WriteLine();
            }
        }

        /// <summary>
        /// 同步任务
        /// </summary>
        private static void TaskExampleOne()
        {
            TaskMethod("Just the main thread");
            var t1 = new Task(TaskMethod, "run sync");
            t1.RunSynchronously();
        }


        /// <summary>
        /// 任务层次结构
        /// </summary>
        static void ParentAndChild()
        {
            var parent = new Task(ParentTask);
            parent.Start();
            Thread.Sleep(2000);  //sleep 的作用 就是让当前的线程等待一段时间
            Console.WriteLine(parent.Status);
            Thread.Sleep(4000);
            Console.WriteLine(parent.Status);
        }

        static void ParentTask()
        {
            Console.WriteLine("task id {0}", Task.CurrentId);
            var child = new Task(ChildTask, TaskCreationOptions.AttachedToParent);
            child.Start();
            Thread.Sleep(1000);
            Console.WriteLine("parent started child");
        }

        static void ChildTask()
        {
            Console.WriteLine("child");
            Thread.Sleep(5000);
            Console.WriteLine("child finished");
        }

    }
}
