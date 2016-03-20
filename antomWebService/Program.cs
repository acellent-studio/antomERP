using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace antom.WebApi
{
    static class Program
    {
        /// <summary>
        /// 應用程式的主要進入點。
        /// </summary>
        static void Main(string[] args)
        {
#if DEBUG
            var svc = new WebService();
            Console.WriteLine("Starting service...");
            svc.InternalStart(args);

            Console.WriteLine("Press any key to stop service...");
            Console.ReadLine();
            //Console.WriteLine("Stopping service...");
#else
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new WebService()
            };
            ServiceBase.Run(ServicesToRun);
#endif
        }
    }
}
