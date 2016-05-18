using System;
using System.Collections.Generic;
using System.Text;

namespace LiranHPAssigment
{
    public class Program
    {
        public static void Main()
        {
            int processID = System.Diagnostics.Process.GetCurrentProcess().Id;
            System.Console.WriteLine("Process ID: {0}", processID.ToString());
            Logger logger = new Logger();
            logger.RunLogger();
        }
    }
}