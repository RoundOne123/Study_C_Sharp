using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace FL_Hash
{
    class Program
    {
        static void Main(string[] args)
        {
            Hashtable ht = Hashtable.Synchronized(new Hashtable());
            Dictionary<string, string> dic = new Dictionary<string, string>();

            HashSet<string> hs = new HashSet<string>();
            ArrayList al = new ArrayList();
            List<int> list = new List<int>();
            Console.WriteLine(list.Capacity);
            list.Add(1);
            Console.WriteLine(list.Capacity);

            Console.ReadLine();
        }




    }
}
