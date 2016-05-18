using System;
using System.Collections.Generic;
using System.Text;

namespace LiranGiladHPEInterviewAssigment
{
    class Program
    {
        const float k_DefaultMaxNumOfEventsPerMin = 1000;
        const float k_DefaultLoadTestLenInMin = 1;
        const float k_DefaultRampUpPeriodInMin = 0.2f;
        const float k_DefaultTearDownPeriodInMin = 0.2f;
        const int k_NoRangeLimit = 0;

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

            int remainingStringsInInputStr = tokenizedInputStr.Length - 4;

            for (int i = 4; i < remainingStringsInInputStr; i++)
            {
                logFilesNameList.Add(tokenizedInputStr[i]);
            }

            CheckIfFileExtensionsAreValid(logFilesNameList);

            RunLogger(MaxNumOfEventsPerMin, loadTestLenInMin, rampUpPeriodInMin, tearDownPeriodInMin, logFilesNameList);
        }

        public void RunLogger(float MaxNumOfEventsPerMin, float loadTestLenInMin, float rampUpPeriodInMin, float tearDownPeriodInMin, List<string> logFileNameList )
        {

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

