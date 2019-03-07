using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;

namespace FL_Http
{

    /*
     * System.Net 名称空间通常与叫高层的操作有关，
     * 比如下载和上传文件，使用HTTP和其他协议进行Web请求等
     * System.Net.Sockets名称空间包含的类通常与较低层的操作有关
     * 对于直接使用套接字或TCP/IP之类的协议的操作非常有用（这些类中的方法与Windows套接字API函数非常类似
     * 
     * HttpClient类用于发送http请求，接收请求的响应
     * 异步调用Web服务
     * 
     * 标题  作用是什么？？？？
     * 
     * HttpContent
     * 
     * HttpClient类可以把HttpMessageHandler作为其构造函数的参数，这样就可以定制请求
     */

    class Program
    {
        static void Main(string[] args)
        {
            //AsyncGetDataDemo();
            HttpClientMessageHandlerRequestDemo();

            Console.ReadKey();
        }

        #region 异步调用Web服务 遍历标题
        private static void AsyncGetDataDemo()
        {
            Console.WriteLine("In main before call to GetData!");
            GetData();
            Console.WriteLine("Back in main after call to GetData!");
            Console.ReadKey();
        }

        //异步调用Web服务
        //如何在响应和请求中遍历标题
        private static async void GetData()
        {
            //是线程安全的 一个hc对象可以用于处理多个请求
            //每个hc实例都维护它自己的线程池，所以hc实例之间的请求会被隔离
            HttpClient httpClient = new HttpClient();

            //设置标题值后，标题和标题值会与这个HttpClient实例发送的每个请求一起发送
            //内容将以json格式返回 默认是XML格式
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json;odata=verbose");

            Console.WriteLine("Request Headers: ");
            EnumerateHeaders(httpClient.DefaultRequestHeaders);  //hc的header属性
            Console.WriteLine();

            //表示包含标题、状态和内容的响应
            HttpResponseMessage response = null;

            //await + GetAsync => 所以Main方法可以在GetAsync方法调用Web服务的同时执行完毕
            response = await httpClient.GetAsync("http://services.odata.org/Northwind/Northwind.svc/Regions");

            if (response.IsSuccessStatusCode)
            {

                Console.WriteLine("Respose Headers: ");
                EnumerateHeaders(response.Headers); //response的headers属性

                Console.WriteLine("Response Status Code: " + response.StatusCode + " " + response.ReasonPhrase);
                //ReadAsStringAsync 的获取返回内容的字符串表示
                //是一个异步调用 但本例未使用异步调用功能 所以调用Result方法会阻塞该线程
                string responseBodyAsText = response.Content.ReadAsStringAsync().Result;


                Console.WriteLine("Respose.Content.Headers: ");
                EnumerateHeaders(response.Content.Headers); //response的headers属性

                Console.WriteLine("Received payload of " + responseBodyAsText.Length + " characters");
                //Console.WriteLine(responseBodyAsText);
            }
        }

        private static void EnumerateHeaders(HttpHeaders headers)
        {
            foreach (var header in headers)
            {
                var value = "";
                foreach (var val in header.Value)
                {
                    value = val + " ";
                }
                Console.WriteLine("Header: " + header.Key + "   Value: " + value);
            }
        }
        #endregion

        //添加定制的处理流程
        private static void HttpClientMessageHandlerRequestDemo()
        {
            GetData_2();
        }

        private static void GetData_2()
        {
            HttpClient httpClient = new HttpClient(new MessageHandler("error"));
            HttpResponseMessage response = null;
            Console.WriteLine();

            response = httpClient.GetAsync("http://services.odata.org/Northwind/Northwind.svc/Regions").Result;
            Console.WriteLine(response.StatusCode);
        }
    }

    public class MessageHandler : HttpClientHandler
    {
        string displayMessage = "";
        public MessageHandler(string message)
        {
            displayMessage = message;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            Console.WriteLine("In DisplayMessageHandler " + displayMessage);

            if(displayMessage == "error")
            {
                var response = new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);
                var tsc = new TaskCompletionSource<HttpResponseMessage>();
                tsc.SetResult(response);
                return tsc.Task;
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
    /*
     * 26.3 把输出结果显示为HTML页面
     *      在Windows Forms应用程序中使用内置的WebBrowser控件（封装了COM对象）：
     *      
     *      使用编程功能，从代码中调用Internet Explorer实例：
     *          通过编程（Process），打开Internet Explorer进程 导航到指定的网页
     *          这样的做法会把IE作为单独窗口打开，不能控制浏览器
     *          
     * 26.3.1 从应用程序中进行简单的Web浏览   （见FL_Browser）
     * 
     * 
     */
}
