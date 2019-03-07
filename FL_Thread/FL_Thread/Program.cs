using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace FL_Thread
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Main thread: starting a dedicated thread to do an asynchronous operation");
            Thread dedicatedThread = new Thread(ComputeBoundOp);
            dedicatedThread.Start();

            Console.WriteLine("Main thread: Doing other work here...");
            Thread.Sleep(1000);                    //主线程等待十秒

            dedicatedThread.Join();             //这个join的作用 造成当前的线程阻塞 直到dedicatedThread代表的线程销毁或者终止
            Console.WriteLine("Hit <Enter> to end this program...");
            Console.ReadLine();
        }

       private static void ComputeBoundOp(Object state)
        {
            Console.WriteLine("In ComputeBounOp : state={0}", state);
            Thread.Sleep(5000);     //另一个线程等待1秒

            Console.WriteLine("XXXX");
        }
    }
}
