using HwrBerlin.Bot.Engines;
using HwrBerlin.Bot.Scanner;
using System;
using System.Collections.Generic;

namespace HwrBerlin.HenryAutoDrive
{
    class Program
    {
        static Robot robot;
        static Scanner scanner;
        static Random rnd;

        static void Main(string[] args)
        {
            robot = new Robot();
            scanner = new Scanner();
            rnd = new Random();
        }

        static void AutoDrive()
        {
            //distances that cause an immediate stop
            //TODO: List has to be filled with concrete values, 49 is just for testing & lower than turnDistances
            List<int> stopDistances = new List<int>();
            for (int i = 0; i < 181; i++)
            {
                stopDistances.Add(300);
            }
            //distances that cause a turn
            //TODO: List has to be filled with concrete values, 50 is just for testing
            List<int> turnDistances = new List<int>();
            for (int i = 0; i < 181; i++)
            {
                turnDistances.Add(500);
            }
            List<int> scanData = new List<int>();
            scanData = scanner.MedianFilter(scanner.GetDataList());
            //as long as stop distances are not reached and no stop key is pressed
            while ((!CompareList(scanData, stopDistances)) || !Console.KeyAvailable)
            {
                //as long as there are no turn distances reached
                while (!CompareList(scanData, turnDistances))
                {
                    robot.Move(1);
                    scanData = scanner.GetDataList();

                }
                if (CompareList(scanData, turnDistances))
                {
                    robot.Move(0);
                    int degrees = rnd.Next(10, 90);
                    int sign = rnd.Next(0, 1);
                    if (sign == 1)
                    {
                        degrees = degrees * (-1);
                    }
                    robot.TurnInDegrees(degrees);
                    scanData = scanner.GetDataList();
                }
            }
        }

        static bool CompareList(List<int> first, List<int> second)
        {
            //which element do we look at
            int i = 0;
            //how many problems are detected in comparison, there are unwanted peaks that are not detected by the median filter
            int count = 0;
            bool result = false;
            //loop through lists and compare if there are problems
            foreach (int data in first)
            {
                if (!(data >= second[i]))
                {
                    count++;
                }
                i++;
            }
            //if there are more problems than expected from measure failures than set result to "problem" (true)
            if (count >= 10)
            {
                result = true;
            }
            Console.WriteLine(count);
            return result;
        }
    }
}
