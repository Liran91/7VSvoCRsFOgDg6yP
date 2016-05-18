using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace LiranHPAssigment
{
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
        Random m_rand = new Random();
        StreamWriter m_logFile;

        public void RunLogger()
        {           
            char[] delimiterChars = { ' ', ';' };

            ProgramStatusPrinter.PrintInputRequest();

            string inputStr = System.Console.ReadLine();
            string[] tokenizedInputStr = inputStr.Split(delimiterChars);
            List<string> logFilesNameList = new List<string>();
            int maxNumOfEventsPerMinute;
            float loadTestLengthInMinutes;
            float rampUpPeriodInMinutes;
            float tearDownPeriodInMinutes;

            ProgramStatusPrinter.PrintInitializingAndValidatingUserInput();

            maxNumOfEventsPerMinute = (int)CheckIfValidInputString(tokenizedInputStr,0, k_DefaultMaxNumOfEventsPerMin, k_NoRangeLimit, k_NoRangeLimit);
            loadTestLengthInMinutes = CheckIfValidInputString(tokenizedInputStr,1, k_DefaultLoadTestLenInMin, 1, 60);
            rampUpPeriodInMinutes = CheckIfValidInputString(tokenizedInputStr, 2, k_DefaultRampUpPeriodInMin, 0, 15);
            tearDownPeriodInMinutes = CheckIfValidInputString(tokenizedInputStr,3, k_DefaultTearDownPeriodInMin, 0, 15);

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

            StartLogging(maxNumOfEventsPerMinute, loadTestLengthInMinutes, rampUpPeriodInMinutes, tearDownPeriodInMinutes, logFileName);

        }

       public float CheckIfValidInputString(string[] inputStrArr,int index, float DefaultValue,int minRange,int maxRange)
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

        private void StartLogging(int maxNumOfEventsPerMinute, float loadTestLengthInMinutes, float rampUpPeriodInMinutes, float tearDownPeriodInMinutes, string logFileName)
        {
            string path = string.Format(@".\{0}", logFileName);
            m_logFile = File.AppendText(path);

            float maximumLoadPeriodInMinutes = loadTestLengthInMinutes - (rampUpPeriodInMinutes + tearDownPeriodInMinutes);
            float rampUpLoggingRateIncrementPerSec = GetLoggingRateIncrementPerSec(maxNumOfEventsPerMinute, rampUpPeriodInMinutes);
            float tearDownLoggingRateIncrementPerSec = GetLoggingRateIncrementPerSec(maxNumOfEventsPerMinute, tearDownPeriodInMinutes);
            float eventsPerMin = 0;
            float numOfTimesToLogPerSec;

            ProgramStatusPrinter.PrintLoggingToFileStarted();

            var startTime = DateTime.UtcNow;
            while (DateTime.UtcNow - startTime < TimeSpan.FromSeconds(rampUpPeriodInMinutes*k_SecondsInAMinute))
            {
                eventsPerMin += rampUpLoggingRateIncrementPerSec;

                numOfTimesToLogPerSec = (int)(eventsPerMin / k_SecondsInAMinute);

                for (int i = 0; i < numOfTimesToLogPerSec; i++)
                {
                    string logString = GenerateLogString();
                    m_logFile.WriteLine(logString);
                }

                System.Threading.Thread.Sleep(k_SecondInMilisec);
            }

            numOfTimesToLogPerSec = (float)(maxNumOfEventsPerMinute / k_SecondsInAMinute);
            int LogFrequency = (int)(k_SecondInMilisec / numOfTimesToLogPerSec);

            startTime = DateTime.UtcNow;
            while (DateTime.UtcNow - startTime < TimeSpan.FromSeconds(maximumLoadPeriodInMinutes * k_SecondsInAMinute))
            {
                string logString = GenerateLogString();
                m_logFile.WriteLine(logString);

                System.Threading.Thread.Sleep(LogFrequency);
            }

            startTime = DateTime.UtcNow;
            eventsPerMin = maxNumOfEventsPerMinute;
            while (DateTime.UtcNow - startTime < TimeSpan.FromSeconds(tearDownPeriodInMinutes * k_SecondsInAMinute))
            {

                eventsPerMin -= tearDownLoggingRateIncrementPerSec;

                numOfTimesToLogPerSec = (int)(eventsPerMin / k_SecondsInAMinute);

                for (int i = 0; i < numOfTimesToLogPerSec; i++)
                {
                    string logString = GenerateLogString();
                    m_logFile.WriteLine(logString);
                }

                System.Threading.Thread.Sleep(k_SecondInMilisec);
            }

            m_logFile.Close();

            ProgramStatusPrinter.PrintLoggingDone();
        }

        private string GenerateLogString()
        {
            StringBuilder logString = new StringBuilder();

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

}
