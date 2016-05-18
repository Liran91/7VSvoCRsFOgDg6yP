using System;
using System.Collections.Generic;
using System.Text;
using System.IO

namespace LiranGiladHPEInterviewAssigment
{
    enum eLogLevel
    {
        INFO = 1, DEBUG, ERROR
    }

    class Program
    {
        const float k_DefaultMaxNumOfEventsPerMin = 1000;
        const float k_DefaultLoadTestLenInMin = 1;
        const float k_DefaultRampUpPeriodInMin = 0.2f;
        const float k_DefaultTearDownPeriodInMin = 0.2f;
        const int k_NoRangeLimit = 0;
        Random rand = new Random();

        public void Main()
        {
            char[] delimiterChars = { ' ', ';' };
            string inputStr = System.Console.ReadLine();
            string[] tokenizedInputStr = inputStr.Split(delimiterChars);
            List<string> logFilesNameList = new List<string>();

            float MaxNumOfEventsPerMin = CheckIfInputIsValid(tokenizedInputStr[0], k_NoRangeLimit, k_NoRangeLimit, k_DefaultMaxNumOfEventsPerMin);
            float loadTestLenInMin = CheckIfInputIsValid(tokenizedInputStr[1], 1, 60, k_DefaultLoadTestLenInMin);
            float rampUpPeriodInMin = CheckIfInputIsValid(tokenizedInputStr[2], 0, 15, k_DefaultRampUpPeriodInMin);
            float tearDownPeriodInMin = CheckIfInputIsValid(tokenizedInputStr[3], 0, 15, k_DefaultTearDownPeriodInMin);

            int remainingStringsInStrArr = tokenizedInputStr.Length - 4;

            for (int i = 4; i < remainingStringsInStrArr; i++)
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

            RunLogger(MaxNumOfEventsPerMin, loadTestLenInMin, rampUpPeriodInMin, tearDownPeriodInMin, logFileName);
        }


        public void RunLogger(float maxNumOfEventsPerMin, float loadTestLenInMin, float rampUpPeriodInMin, float tearDownPeriodInMin, string logFileName )
        {
            FileStream fileStream = new FileStream(logFileName, FileMode.Create);

            float rampUpLoggingRate = GetLoggingRateDuringPhase(maxNumOfEventsPerMin, rampUpPeriodInMin);
            float tearDownLoggingRate = GetLoggingRateDuringPhase(maxNumOfEventsPerMin, tearDownPeriodInMin);

            var startTime = DateTime.UtcNow;
            while(DateTime.UtcNow - startTime < TimeSpan.FromMinutes(loadTestLenInMin))
            {

            }

        }

        public string GenerateLogString()
        {
            StringBuilder logString = new StringBuilder();

            logString.Append(DateTime.Now.ToString());
            logString.Append(" ");

            eLogLevel logLevel = (eLogLevel)rand.Next(1,3);
            string logLevelStr = Enum.GetName(typeof(eLogLevel),logLevel);

            logString.Append(logLevelStr);
            logString.Append(" ");

            string line1 = GenerateRandomLine(128);
            logString.Append(line1);
            logString.Append(string.Format(@"{0}\t\t\t\t\t", Environment.NewLine));

            if(CheckIfToAddLine(15))
            {
                string line2 = GenerateRandomLine(100);
                logString.Append(line2);
                logString.Append(string.Format(@"{0}\t\t\t\t\t", Environment.NewLine));
            }
            
            if(CheckIfToAddLine(5))
            {
                string line3 = GenerateRandomLine(80);
                logString.Append(line3);
            }

            return logString.ToString();
        }

        public bool CheckIfToAddLine(int chanceToAddLine)
        {
            bool addLine = false;

            int rollResult = rand.Next(1, 100);

            if(rollResult >= 1 && rollResult <= chanceToAddLine)
            {
                addLine = true;
            }

            return addLine;
        }

        public string GenerateRandomLine(int lineLength)
        {
            char ch;
            StringBuilder randomLine = new StringBuilder();

            for (int i = 0; i < lineLength; i++)
            {
                ch = (char)rand.Next(0, 255);
                randomLine.Append(ch);
            }

            return (randomLine.ToString());
        }

        public float GetLoggingRateDuringPhase(float maxNumOfEventsPerMin, float phaseDurationInMin)
        {
            float phaseDurationInSecs = phaseDurationInMin * 60;
            float loggingRate = phaseDurationInSecs / maxNumOfEventsPerMin;

            return loggingRate;
        }

        public string GetRandomFileNameFromFileNameList(List<string> logFilesNameList)
        {
            int fileNameIndex = rand.Next(0, logFilesNameList.Count - 1);

            return (logFilesNameList[fileNameIndex]);
        }

        public void CheckIfFileExtensionsAreValid(List<string> logFileNameList)
        {
            string extensionStr;

            foreach (string fileName in logFileNameList)
            {
                int strLen = fileName.Length;
                extensionStr = fileName.Substring(strLen - 4);

                if(extensionStr != ".log" && extensionStr != ".txt")
                {
                    logFileNameList.Remove(fileName);
                }
            }

            if(logFileNameList.Count == 0)
            {
                StringBuilder defaultFileName = new StringBuilder();
                defaultFileName.Append(DateTime.Now.ToString());
                defaultFileName.Append(".log");
                logFileNameList.Add(defaultFileName.ToString());
            }

        }

        public float CheckIfInputIsValid(string input,int minRange, int maxRange, float defaultValue)
        {
           float returnedValue;
           bool validInput = float.TryParse(System.Console.ReadLine(), out returnedValue);

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

