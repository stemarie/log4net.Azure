using System;
using System.Threading;

namespace log4net.Azure.console
{
    class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static void Main(string[] args)
        {
            Log.Info("test");
            try
            {
                throw new Exception("throw a message!");
            }
            catch (Exception ex)
            {
                Log.Error("Test exception", ex);
            }
            for (int i = 0; i < 2; i++)
            {
                Thread.Sleep(1000);
                Console.WriteLine("Waiting {0}", i);
            }
        }
    }
}
