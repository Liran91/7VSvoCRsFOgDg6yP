using System;
using System.Collections.Generic;
using System.Text;

namespace LiranGiladHPEInterviewAssigment
{
    class Program
    {
        public void Main()
        {
            const float k_DefaultMaxNumOfEventsPerMin = 1000;
            const float k_DefaultLoadTestLenInMin = 1;
            const float k_DefaultRampUpPeriodInMin = 0.2f;
            const float k_DefaultTearDownPeriodInMin = 0.2f;
            const int k_NoRangeLimit = 0;
            DateTime timeAndDate = DateTime.Now;

            float MaxNumOfEventsPerMin = GetValidInput(k_NoRangeLimit, k_NoRangeLimit, k_DefaultMaxNumOfEventsPerMin);
            float loadTestLenInMin = GetValidInput(1, 60, k_DefaultMaxNumOfEventsPerMin);
            float rampUpPeriodInMin = GetValidInput(1, 60, k_DefaultMaxNumOfEventsPerMin);
            float tearDownPeriodInMin = GetValidInput(1, 60, k_DefaultMaxNumOfEventsPerMin);


        }

        public float GetValidInput(int minRange, int maxRange, float defaultValue)
        {
            float returnedValue;

            if (minRange == 0 && minRange == 0)
            {
                System.Console.WriteLine(@"Please enter a numeric value:
(any non-numeric or input outside the designated range will result in the default value being used)");
                float.TryParse(System.Console.ReadLine(), out returnedValue);
            }
            else
            {
                System.Console.WriteLine(@"Please enter a value between {0} and {1}:
(any non-numeric or input outside the designated range will result in the default value being used)", minRange, maxRange);
                float.TryParse(System.Console.ReadLine(), out returnedValue);

                if (returnedValue > maxRange || returnedValue < minRange)
                {
                    returnedValue = defaultValue;
                }
            }

            return returnedValue;

        }
    }
}

