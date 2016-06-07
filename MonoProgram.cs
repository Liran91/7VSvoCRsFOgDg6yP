using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Timers;
namespace MonoTest
{
    class Program
    {
        public static void Main()
        {
            int processID = System.Diagnostics.Process.GetCurrentProcess().Id;
            System.Console.WriteLine("Process ID: {0}", processID.ToString());
            Logger logger = new Logger();
            logger.RunLogger();
        }
    }

    public enum eLogLevel
    {
        INFO = 1, DEBUG, ERROR
    }

    public class Logger
    {
        const int k_DefaultMaxNumOfEventsPerMin = 1000;
        const float k_DefaultLoadTestLenInMin = 1;
        const float k_DefaultRampUpPeriodInMin = 0.2f;
        const float k_DefaultTearDownPeriodInMin = 0.2f;
        const int k_SecondInMilisec = 1000;
        const int k_SecondsInAMinute = 60;
        const int k_NoRangeLimit = 0;
        const int k_Second = 1;
        Random m_rand = new Random();
        float m_EventsPerSec = 0;
        int m_MaxNumOfEventsPerMinute;
        float m_TearDownLoggingRateIncrementPerSec;
        float m_RampUpLoggingRateIncrementPerSec;
        StreamWriter m_logFile;

        public void RunLogger()
        {
            char[] delimiterChars = { ' ', ';' };

            ProgramStatusPrinter.PrintInputRequest();

            string inputStr = System.Console.ReadLine();
            string[] tokenizedInputStr = inputStr.Split(delimiterChars);
            List<string> logFilesNameList = new List<string>();
            float loadTestLengthInMinutes;
            float rampUpPeriodInMinutes;
            float tearDownPeriodInMinutes;

            ProgramStatusPrinter.PrintInitializingAndValidatingUserInput();

            m_MaxNumOfEventsPerMinute = (int)CheckIfValidInputString(tokenizedInputStr, 0, k_DefaultMaxNumOfEventsPerMin, k_NoRangeLimit, k_NoRangeLimit);
            loadTestLengthInMinutes = CheckIfValidInputString(tokenizedInputStr, 1, k_DefaultLoadTestLenInMin, 1, 60);
            rampUpPeriodInMinutes = CheckIfValidInputString(tokenizedInputStr, 2, k_DefaultRampUpPeriodInMin, 0, 15);
            tearDownPeriodInMinutes = CheckIfValidInputString(tokenizedInputStr, 3, k_DefaultTearDownPeriodInMin, 0, 15);

            for (int i = 4; i < tokenizedInputStr.Length; i++)
            {
                logFilesNameList.Add(tokenizedInputStr[i]);
            }

            CheckIfFileExtensionsAreValid(logFilesNameList);

            string logFileName;

            if (logFilesNameList.Count > 1)
            {
                logFileName = GetRandomFileNameFromFileNameList(logFilesNameList);
            }
            else
            {
                logFileName = logFilesNameList[0];
            }

            StartLogging(loadTestLengthInMinutes, rampUpPeriodInMinutes, tearDownPeriodInMinutes, logFileName);

        }

        public float CheckIfValidInputString(string[] inputStrArr, int index, float DefaultValue, int minRange, int maxRange)
        {
            float returnedValue;

            if ((inputStrArr.Length - 1 >= index))
            {
                returnedValue = CheckIfValidNumericInput(inputStrArr[index], minRange, maxRange, DefaultValue);
            }
            else
            {
                returnedValue = DefaultValue;
            }

            return returnedValue;
        }

        private void StartLogging(float loadTestLengthInMinutes, float rampUpPeriodInMinutes, float tearDownPeriodInMinutes, string logFileName)
        {

            string path = string.Format(@".\{0}", logFileName);
            m_logFile = new StreamWriter(path, true);
            float sleepDurationBetweenEvents = 0;
            float maximumLoadPeriodInMinutes = loadTestLengthInMinutes - (rampUpPeriodInMinutes + tearDownPeriodInMinutes);

            try
            {
                m_RampUpLoggingRateIncrementPerSec = GetLoggingRateIncrementPerSec(m_MaxNumOfEventsPerMinute, rampUpPeriodInMinutes);
                m_TearDownLoggingRateIncrementPerSec = GetLoggingRateIncrementPerSec(m_MaxNumOfEventsPerMinute, tearDownPeriodInMinutes);


                ProgramStatusPrinter.PrintLoggingToFileStarted();

                m_EventsPerSec = m_RampUpLoggingRateIncrementPerSec;

                System.Timers.Timer LogIntervalTimer = new System.Timers.Timer();
                LogIntervalTimer.Elapsed += new ElapsedEventHandler(WriteToLogOnInterval);
                LogIntervalTimer.Interval = k_SecondInMilisec;
                LogIntervalTimer.Enabled = true;

                System.Timers.Timer EventsPerSecIncTimer = new System.Timers.Timer();
                EventsPerSecIncTimer.Interval = k_SecondInMilisec;
                EventsPerSecIncTimer.Elapsed += new ElapsedEventHandler(IncreaseEventsPerSec);
                EventsPerSecIncTimer.Enabled = true;

                LogIntervalTimer.Start();
                EventsPerSecIncTimer.Start();

                var startTime = DateTime.UtcNow;
                while (DateTime.UtcNow - startTime < TimeSpan.FromSeconds(rampUpPeriodInMinutes * k_SecondsInAMinute))
                {
                    if (m_EventsPerSec < m_MaxNumOfEventsPerMinute)
                    {
                        LogIntervalTimer.Interval = (k_SecondInMilisec / m_EventsPerSec);
                    }
                }

                LogIntervalTimer.Stop();
                EventsPerSecIncTimer.Stop();
                m_EventsPerSec = m_MaxNumOfEventsPerMinute / 60;
                sleepDurationBetweenEvents = (k_SecondInMilisec / m_EventsPerSec);
                LogIntervalTimer.Interval = sleepDurationBetweenEvents;
                LogIntervalTimer.Start();

                startTime = DateTime.UtcNow;
                while (DateTime.UtcNow - startTime < TimeSpan.FromSeconds(maximumLoadPeriodInMinutes * k_SecondsInAMinute))
                {

                }
                LogIntervalTimer.Stop();

                m_EventsPerSec = m_MaxNumOfEventsPerMinute;

                System.Timers.Timer EventsPerSecDecTimer = new System.Timers.Timer();
                EventsPerSecDecTimer.Interval = k_SecondInMilisec;
                EventsPerSecDecTimer.Elapsed += new ElapsedEventHandler(DecreaseEventsPerSec);
                EventsPerSecDecTimer.Enabled = true;


                LogIntervalTimer.Start();
                EventsPerSecDecTimer.Start();
                startTime = DateTime.UtcNow;
                while (DateTime.UtcNow - startTime < TimeSpan.FromSeconds(tearDownPeriodInMinutes * k_SecondsInAMinute))
                {
                    if (m_EventsPerSec > 0)
                    {
                        LogIntervalTimer.Interval = k_SecondInMilisec / m_EventsPerSec;
                    }
                }

                LogIntervalTimer.Stop();
                EventsPerSecDecTimer.Stop();


                m_logFile.Close();

                ProgramStatusPrinter.PrintLoggingDone();
            }

            catch (Exception ex)
            {
                Console.WriteLine("Error! {0}", ex.Message);
                m_logFile.Close();
            }
        }

        private void WriteToLogOnInterval(object source, ElapsedEventArgs e)
        {
            string logString = GenerateLogString();
            m_logFile.WriteLine(logString);
            
            (source as Timer).Start();
        }

        private void IncreaseEventsPerSec(object source, ElapsedEventArgs e)
        {
            (source as Timer).Stop();
            if (m_EventsPerSec < m_MaxNumOfEventsPerMinute)
            {
                m_EventsPerSec += m_RampUpLoggingRateIncrementPerSec;
                if (m_EventsPerSec > m_MaxNumOfEventsPerMinute)
                {
                    m_EventsPerSec = m_MaxNumOfEventsPerMinute;
                }
            }
            (source as Timer).Start();
        }

        private void DecreaseEventsPerSec(object source, ElapsedEventArgs e)
        {
            (source as Timer).Stop();
            if (m_EventsPerSec > 0)
            {
                m_EventsPerSec -= m_TearDownLoggingRateIncrementPerSec;
                if (m_EventsPerSec <= 0)
                {
                    m_EventsPerSec = 0;
                }
            }
            (source as Timer).Start();
        }

        private void WriteLogLineToFile()
        {
                string logString = GenerateLogString();
                m_logFile.WriteLine(logString);
        }

        private string GenerateLogString()
        {
            StringBuilder logString = new StringBuilder("");

            logString.Append(DateTime.Now.ToString());
            logString.Append(" ");

            int randRes = m_rand.Next(1, 4);

            eLogLevel logLevel = (eLogLevel)(randRes);
            string logLevelStr = Enum.GetName(typeof(eLogLevel), logLevel);

            logString.Append(logLevelStr);
            logString.Append(" ");

            string line1 = GenerateRandomLine(128);
            logString.Append(line1);
            logString.Append(string.Format("{0}                    ", Environment.NewLine));

            if (CheckIfToAddLine(15))
            {
                string line2 = GenerateRandomLine(100);
                logString.Append(line2);
                logString.Append(string.Format("{0}                    ", Environment.NewLine));
            }

            if (CheckIfToAddLine(5))
            {
                string line3 = GenerateRandomLine(80);
                logString.Append(line3);
            }
            return logString.ToString();
        }

        private bool CheckIfToAddLine(int chanceToAddLine)
        {
            bool addLine = false;

            int rollResult = m_rand.Next(1, 100);

            if (rollResult >= 1 && rollResult <= chanceToAddLine)
            {
                addLine = true;
            }

            return addLine;
        }

        private string GenerateRandomLine(int lineLength)
        {
            char ch;
            StringBuilder randomLine = new StringBuilder();

            for (int i = 0; i < lineLength; i++)
            {
                int randRes = m_rand.Next(0, 255);
                ch = (char)randRes;
                randomLine.Append(ch);
            }

            return (randomLine.ToString());
        }

        private float GetLoggingRateIncrementPerSec(float maxNumOfEventsPerMinute, float phaseDurationInMinutes)
        {
            float phaseDurationInSecs = phaseDurationInMinutes * 60;
            float loggingRateIncrementPerSec = maxNumOfEventsPerMinute / phaseDurationInSecs;

            if (loggingRateIncrementPerSec <= 0)
            {
                throw new Exception("Logging rate increment per second must be bigger than 0!");
            }

            return loggingRateIncrementPerSec;
        }

        private string GetRandomFileNameFromFileNameList(List<string> logFilesNameList)
        {
            int fileNameIndex = m_rand.Next(0, logFilesNameList.Count - 1);

            return (logFilesNameList[fileNameIndex]);
        }

        private void CheckIfFileExtensionsAreValid(List<string> logFileNameList)
        {
            string extensionStr;

            for (int i = 0; i < logFileNameList.Count; i++)
            {
                int strLen = logFileNameList[i].Length;
                if (strLen >= 4)
                {
                    extensionStr = logFileNameList[i].Substring(strLen - 4);

                    if (extensionStr != ".log" && extensionStr != ".txt")
                    {
                        logFileNameList.Remove(logFileNameList[i].ToString());
                    }
                }
            }


            if (logFileNameList.Count == 0)
            {
                StringBuilder defaultFileName = new StringBuilder();
                Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                defaultFileName.Append(unixTimestamp.ToString());
                defaultFileName.Append(".log");
                logFileNameList.Add(defaultFileName.ToString());
            }

        }

        private float CheckIfValidNumericInput(string input, int minRange, int maxRange, float defaultValue)
        {
            float returnedValue;
            bool validInput = float.TryParse(input, out returnedValue);

            if (minRange == k_NoRangeLimit && maxRange == k_NoRangeLimit)
            {
                if (!validInput || returnedValue < 0)
                {
                    returnedValue = defaultValue;
                }
            }
            else
            {
                if (!validInput || returnedValue > maxRange || returnedValue < minRange)
                {
                    returnedValue = defaultValue;
                }
            }

            return returnedValue;

        }

    }

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
