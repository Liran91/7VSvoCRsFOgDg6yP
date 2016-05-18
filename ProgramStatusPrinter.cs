using System;
using System.Collections.Generic;
using System.Text;

namespace LiranHPAssigment
{
   public static class ProgramStatusPrinter
    {
       public static void PrintInputRequest()
       {
           System.Console.WriteLine("{0} Awaiting user input: ", DateTime.Now.ToString());
       }

       public static void PrintInitializingAndValidatingUserInput()
       {
         System.Console.WriteLine("{0} Initializing logger variables and validing input...", DateTime.Now.ToString());
       }

       public static void PrintLoggingToFileStarted()
       {
           System.Console.WriteLine("{0} Logging to file started, please wait...", DateTime.Now.ToString());
       }

       public static void PrintLoggingDone()
       {
           System.Console.WriteLine("{0} Logging operation concluded successfully!", DateTime.Now.ToString());
       }
    }
}
