using HwrBerlin.Bot.Engines;
using HwrBerlin.Bot.Scanner;
using System;
using System.Collections.Generic;
using System.Diagnostics;


namespace HwrBerlin.HenryAutoDrive
{
    public class AutoDriveProgram
    {
        private static Robot _robot;
        private static Scanner _scanner;
        private static int velocity = 1;

        /* private static int _problemSide;
        private static int _problemSideRange;
        private static int _maxProblems; */

        private static void Main()
        {
            _robot = new Robot();
            _scanner = new Scanner();

            int velocity = 1;
            // reference variable for calling the methods from the scanner class
            var instanceScanner = new Scanner();
            // list for the median values of the scanner
            var medianList = new List<int>();

            // set on true if a certain distance is undershot (unterschritten)
            Boolean stop = false;
            // treshold, if undershot, when henry has to stop in mm
            int treshold = 700;

            _robot.Enable();

            if (_robot != null && _robot.Enable())
            {

                while (stop == false)
                {
                    // with every iteration of the while loop an actual list is fetched for the distances
                    medianList.Clear();
                    medianList = instanceScanner.MedianFilter(instanceScanner.GetDataList());

                    for (int i = 100; i <= 200; i++)
                    {
                        // checks every degree right infront of henry 
                        // if treshold is greater than any degree distance henry stops
                        if (treshold > medianList[i])
                        {
                            // sets stop 
                            stop = true;
                            _robot.StopImmediately();
                        }

                    }
                    // we need to check if the boolean var is set on true or not
                    // if we wouldnt check he would stop for a second and drive forwards
                    if (stop == false)
                    {
                        // henry drives forward at the velocity if stop is on false
                        _robot.Move(velocity);

                    }


                }

            }




            //FollowTheWall();
            //vorher: else
            Console.ReadLine();
         }

         /*
         private static void AutoDrive()
         {
             //distances that cause an immediate stop
             var stopDistances = new List<int>();
             for (var i = 0; i < 39; i++)
                 stopDistances.Add(0);
             for (var i = 0; i < 193; i++)
                 stopDistances.Add(300);
             for (var i = 0; i < 39; i++)
                 stopDistances.Add(0);
             //distances that cause a turn
             var turnDistances = new List<int>();
             for (var i = 0; i < 39; i++)
                 turnDistances.Add(0);
             for (var i = 0; i < 193; i++)
                 turnDistances.Add(500);
             for (var i = 0; i < 39; i++)
                 turnDistances.Add(0);
             //distances free for random left
             var freeDistancesLeft = new List<int>();
             for (var i = 0; i < 39; i++)
                 freeDistancesLeft.Add(0);
             for (var i = 0; i < 97; i++)
                 freeDistancesLeft.Add(550);
             for (var i = 0; i < 96; i++)
                 freeDistancesLeft.Add(500);
             for (var i = 0; i < 39; i++)
                 freeDistancesLeft.Add(0);
             //distances free for random right
             var freeDistancesRight = new List<int>();
             for (var i = 0; i < 39; i++)
                 freeDistancesRight.Add(0);
             for (var i = 0; i < 96; i++)
                 freeDistancesRight.Add(500);
             for (var i = 0; i < 97; i++)
                 freeDistancesRight.Add(550);
             for (var i = 0; i < 39; i++)
                 freeDistancesRight.Add(0);

             _problemSide = 0;
             _problemSideRange = 150000;
             _maxProblems = 10;

             while (!Console.KeyAvailable)
             {

                 var sensorData = _scanner.MedianFilter(_scanner.GetDataList());

                 string s = "[";
                 foreach (var l in sensorData)
                 {
                     s += l + ",";
                 }
                 s += "]";

                 var currentWalkMode = Robot.WalkMode.FORWARDS_SLOW;
                 var currentTurnMode = Robot.TurnMode.STRAIGHT;

                 //if stop distances reached drive backwards
                 if (CompareList(sensorData, stopDistances))
                 {
                     currentWalkMode = Robot.WalkMode.BACKWARDS;
                 }
                 else
                 {
                     //if turn distances not reached drive forwards
                     if (!CompareList(sensorData, turnDistances))
                     {
                         if (!CompareList(sensorData, freeDistancesLeft) || !CompareList(sensorData, freeDistancesRight))
                         {
                             currentTurnMode = sensorData.GetRange(45, 90).Average() > sensorData.GetRange(136, 90).Average() ? Robot.TurnMode.LEFT_SMOOTH : Robot.TurnMode.RIGHT_SMOOTH;
                         }
                     }
                     else
                     {
                         currentWalkMode = Robot.WalkMode.STOP;
                         //if there are more problems on the one side turn to the other side
                         currentTurnMode = _problemSide < 0 ? Robot.TurnMode.RIGHT_SMOOTH : Robot.TurnMode.LEFT_SMOOTH;
                     }
                 }

                 if (_robot.CurrentWalkMode != currentWalkMode || _robot.CurrentTurnMode != currentTurnMode)
                     _robot.MoveByMode(currentWalkMode, currentTurnMode);
             }
         }

         private static void FollowTheWall()
         {
             var stopDistances = new List<int>();
             for (var i = 0; i < 39; i++)
                 stopDistances.Add(0);
             for (var i = 0; i < 193; i++)
                 stopDistances.Add(300);
             for (var i = 0; i < 39; i++)
                 stopDistances.Add(0);
             //distances that cause a turn
             var turnDistances = new List<int>();
             for (var i = 0; i < 39; i++)
                 turnDistances.Add(0);
             for (var i = 0; i < 193; i++)
                 turnDistances.Add(500);
             for (var i = 0; i < 39; i++)
                 turnDistances.Add(0);
             //distances free for random left
             var freeDistancesLeft = new List<int>();
             for (var i = 0; i < 39; i++)
                 freeDistancesLeft.Add(0);
             for (var i = 0; i < 97; i++)
                 freeDistancesLeft.Add(550);
             for (var i = 0; i < 96; i++)
                 freeDistancesLeft.Add(500);
             for (var i = 0; i < 39; i++)
                 freeDistancesLeft.Add(0);
             //distances free for random right
             var freeDistancesRight = new List<int>();
             for (var i = 0; i < 39; i++)
                 freeDistancesRight.Add(0);
             for (var i = 0; i < 96; i++)
                 freeDistancesRight.Add(500);
             for (var i = 0; i < 97; i++)
                 freeDistancesRight.Add(550);
             for (var i = 0; i < 39; i++)
                 freeDistancesRight.Add(0);

             _problemSide = 0;
             _problemSideRange = 150000;
             _maxProblems = 5;

             var wallLeft = false;

             while (!Console.KeyAvailable)
             {
                 var currentWalkMode = Robot.WalkMode.FORWARDS_SLOW;
                 var currentTurnMode = Robot.TurnMode.STRAIGHT;

                 var sensorData = _scanner.MedianFilter(_scanner.GetDataList());

                 //if stop distances reached drive backwards
                 if (CompareList(sensorData, stopDistances))
                 {
                     currentWalkMode = Robot.WalkMode.BACKWARDS;
                 }
                 //if turn distances reached
                 else
                 {
                     if (!CompareList(sensorData, turnDistances))
                     {
                         if (wallLeft)
                         {
                             if (!CompareList(sensorData, freeDistancesLeft))
                             {
                                 currentWalkMode = Robot.WalkMode.STOP;
                                 currentTurnMode = Robot.TurnMode.LEFT_SMOOTH;
                             }
                         }
                         else
                         {
                             if (!CompareList(sensorData, freeDistancesRight))
                             {
                                 currentWalkMode = Robot.WalkMode.STOP;
                                 currentTurnMode = Robot.TurnMode.RIGHT_SMOOTH;
                             }
                         }
                     }
                     else
                     {
                         currentWalkMode = Robot.WalkMode.STOP;
                         //if more problems on the left turn right
                         if (_problemSide < 0)
                         {
                             currentTurnMode = Robot.TurnMode.RIGHT_SMOOTH;
                             wallLeft = true;
                         }
                         //if more problems on the right turn left
                         else
                         {
                             currentTurnMode = Robot.TurnMode.LEFT_SMOOTH;
                             wallLeft = false;
                         }
                     }
                 }

                 if (_robot.CurrentWalkMode != currentWalkMode || _robot.CurrentTurnMode != currentTurnMode)
                     _robot.MoveByMode(currentWalkMode, currentTurnMode);
             }
         }

         private static bool CompareList(IList<int> data, IList<int> limits)
         {
             var max = data.Count > limits.Count ? limits.Count : data.Count;
             //how many problems are detected in comparison
             var count = 0;
             //loop through lists and compare if there are problems
             for (var i = 0; i < max; i++)
             {
                 if (data[i] >= limits[i])
                     continue;
                 count++;
                 if (i < 135)
                 {
                     if (_problemSide > -_problemSideRange)
                     {
                         _problemSide--;
                     }
                 }
                 else
                 {
                     if (_problemSide < _problemSideRange)
                     {
                         _problemSide++;
                     }
                 }
             }
             //if there are more problems than expected from measure failures than true (there are problems)
             return count >= _maxProblems;
         }*/
            }
}
