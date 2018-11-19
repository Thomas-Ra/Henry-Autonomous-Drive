using HwrBerlin.Bot.Engines;
using HwrBerlin.Bot.Scanner;
using System;
using System.Collections.Generic;

namespace HwrBerlin.HenryAutoDrive
{
    internal class Program
    {
        private static Robot _robot;
        private static Scanner _scanner;
        private static Random _rnd;

        private static void Main()
        {
            _robot = new Robot();
            _scanner = new Scanner();
            _rnd = new Random();

            AutoDrive();
        }

        private static void AutoDrive()
        {
            //distances that cause an immediate stop
            //TODO: List has to be filled with concrete values, 49 is just for testing & lower than turnDistances
            var stopDistances = new List<int>();
            for (var i = 0; i < 181; i++)
                stopDistances.Add(300);

            //distances that cause a turn
            //TODO: List has to be filled with concrete values, 50 is just for testing
            var turnDistances = new List<int>();
            for (var i = 0; i < 181; i++)
                turnDistances.Add(500);

            var scanData = _scanner.MedianFilter(_scanner.GetDataList());
            //as long as stop distances are not reached and no stop key is pressed
            while ((!CompareList(scanData, stopDistances)) || !Console.KeyAvailable)
            {
                //as long as there are no turn distances reached
                while (!CompareList(scanData, turnDistances))
                {
                    _robot.Move(1);
                    scanData = _scanner.GetDataList();
                }

                if (!CompareList(scanData, turnDistances))
                    continue;

                _robot.Move(0);
                var degrees = _rnd.Next(10, 90);
                var sign = _rnd.Next(0, 1);
                if (sign == 1)
                    degrees = degrees * (-1);
                _robot.TurnInDegrees(degrees);
                scanData = _scanner.GetDataList();
            }
        }

        private static bool CompareList(IEnumerable<int> first, IReadOnlyList<int> second)
        {
            //which element do we look at
            var i = 0;
            //how many problems are detected in comparison, there are unwanted peaks that are not detected by the median filter
            var count = 0;
            var result = false;
            //loop through lists and compare if there are problems
            foreach (var data in first)
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
