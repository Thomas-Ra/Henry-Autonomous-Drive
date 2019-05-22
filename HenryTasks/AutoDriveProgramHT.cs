using HwrBerlin.Bot.Engines;
using HwrBerlin.Bot.Scanner;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace HwrBerlin.HenryTasks

{
    public class AutoDriveProgramHT
    {
        private static Robot _robot;
        private static Scanner _scanner;
        private static int velocity = 1;

        /* private static int _problemSide;
        private static int _problemSideRange;
        private static int _maxProblems; */

        public void stopIfObstacle()
        {
            Debug.WriteLine("entering stopIfObstacle()");
            _robot = new Robot();
            _scanner = new Scanner();

            // velocity that henry drives
            int velocity = 1;
            // reference variable for calling the methods from the scanner class
            var instanceScanner = new Scanner();
            // list for the median values of the scanner
            var medianList = new List<int>();

            // set on true if a certain distance is undershot (unterschritten)
            Boolean stop = false;
            // treshold, if undershot, henry stops; distance in mm
            //To-DO:
            //Threshold must be amended towareds the calculation of every degree and its respektive Distance to the corridor-border
            int treshold = 700;

            // the ammount of time henry waits 
            // it takes 46 miliseconds to check 100° in the for loop
            // therefore we expect that it takes about 50 ms for one complete loop -> 2 loops per second
            // waiting time: ammount of seconds*2
            int wait = 40;

            // counts the ammount of cycles/Iterations
            int elapsedCycles = 0;

            // checks if henry can drive again
            Boolean driveAgain = false;

            _robot.Enable();

            if (_robot != null && _robot.Enable())
            {

                while (stop == false)
                {
                    // with every iteration of the while loop an actual list is fetched for the distances
                    medianList.Clear();

                    try
                    {
                        medianList = instanceScanner.MedianFilter(instanceScanner.GetDataList());

                    }
                    catch (Exception e)
                    {
                        if(e is IndexOutOfRangeException){

                            Debug.WriteLine(e.Message);
                            continue;
                        }
                    }

                    if(!(medianList.Count >= 250 && medianList.Count <= 280))
                    {
                        Debug.WriteLine("medianList nicht 271 Elemente groß");
                        Debug.WriteLine("Länge medianList: " + medianList.Count);
                        continue;
                    }

                    Debug.WriteLine("Länge medianList: " + medianList.Count);
                    //changed from 100 to 46 and 200 to 2224 ro exclude the degrees where the scanner sees the robot
                    //but include all relebant degrees for obstacle scanning
                    for (int i = 46; i <= 224; i++)
                    {
                        // checks every degree right infront of henry (100° angle)
                        // if treshold is greater than any degree distance henry stops
                        if (treshold > medianList[i])
                        {
                            // sets stop 
                            stop = true;
                            // sets velocity to zero so that Henry stops
                            _robot.Move(0);
                            
                        }

                    }
                    // we need to check if the boolean var is set on true or not
                    // if we wouldnt check he would stop for a second and drive forwards again
                    if (stop == false)
                    {
                        // henry drives forward at the velocity if stop is on false
                        _robot.Move(velocity);

                        // if stop = true Henry has detected an obstacle
                        // we wait for a given ammount of time 
                        // if the obstacle is away he drives forward again
                    }

                }

            }

            //FollowTheWall();
            //vorher: else
            // Console.ReadLine();
            Debug.WriteLine("vor rekursion");

            stop = false;
            medianList.Clear();
            stopIfObstacle();
        }

    }

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
