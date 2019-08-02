using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HwrBerlin.Bot.Engines;
using HwrBerlin.Bot.Scanner;
using System.Diagnostics;
using System.Globalization;
using HwrBerlin.Bot;
using static HwrBerlin.Bot.Engines.Robot;

namespace HwrBerlin.HenryTasks
{
    public class Auto1
    {
        // Initializing objects and variables:
        public static Robot _robot = new Robot();
        public static Scanner _scanner = new Scanner();
        // velocity is in km/h
        public static int velocity = 1;
        // threshold is in mm
        public static int safety_threshold = 700;

        // turns left if 1 and right if the value is 2
        int twiddle = 0;

        // METHODS FOR AUTONOMOUS DRIVING FUNCTIONALITY

        /// <summary>
        /// 1. SCAN
        /// this method utilizes the class scanner.cs directly by calling on  the methods GetDataList() as well as MedianFilter()
        /// The try-catch block aids in case the received list from those called methods has a lenght of 0
        /// </summary>
        public List<int> Scan()
        {

            // initialize local medianList
            var medianList = new List<int>();

            try 
            { 
                medianList=_scanner.MedianFilter(_scanner.GetDataList());
             }
                catch (Exception e)
                {
                    if (e is IndexOutOfRangeException)
                   {
                        Debug.WriteLine(e.Message);
                        Debug.WriteLine("length medianList: "+ medianList.Count());
                    }
                }
            return medianList;
         }

        /// <summary>
        /// 2. CHECK
        /// This method uses the output of the SCAN()-Method above as input
        /// boolean method, returns the value allocated with the two robot-states: drive or stop
        /// if no obstacle occurs within the defined range, the boolean drive is set to true 
        /// if an obstacle occurs within the defined range, the boolean drive is set to false
        /// for the initial start of the robot and program, the boolsche variable is set to false in order to maintain a stop until the frist scan has been initited 
        /// </summary>
        public Boolean Check()
        {
            var medianList = new List<int>();

            // Calling scan() Method to retrieve MedianList
            medianList= Scan();

            Boolean drive = false;

            // Checking the retrieved list between index of 100 and 200
            for (int i = 100; i <= 200; i++)
            {
                Debug.WriteLine("Check-Method: Entering For Loop");

                if(medianList.Count == 0)
                {
                    Debug.WriteLine(" list is empty :(");
                }
                else if (safety_threshold > medianList[i])
                {
                    // sets stop
                    Debug.WriteLine("current value from medianList: " + medianList[i]);
                    Debug.WriteLine("current index from medianList: " + i);
                    Debug.WriteLine("drive == false");
                    drive = false;
                    return drive;
                }
                else if (safety_threshold <= medianList[i])
                {
                    Debug.WriteLine("drive == true");
                    Debug.WriteLine("current value from medianList: " + medianList[i]);
                    Debug.WriteLine("current index from medianList: " + i);
                    drive = true;
                }
            }
            Debug.WriteLine(drive.ToString());
            return drive;
        }

        /// <summary>
        /// getting a list with 181 values for a corridor
        /// </summary>
        public List<double> generateCorridorList()
        {
            var thresholdlist = new List<double>();
            var reverseThresholdlist = new List<double>();

            // Turning Radius of Robot = 44,35 cm
            // Adding an extra 10 cm safety distance to the radius on both sides
            // safety_radius = 44,35 + 10 + 10 = 64.35 -> in MM = 643.5
            double safety_radius = 600;

            // initializing the values for the corridor
            // adding the safety_radius itself as value for 0 degrees, as the calculation starts at 1 degree
            thresholdlist.Add(safety_radius);

            // Calculation for values in list with index 1-90
            for (int i = 1; i <= 90; i++)
            {
                double threshold =  safety_radius / Math.Cos((Math.PI * i / 180.0)) ;
                if (threshold > safety_threshold)
                {
                    threshold = safety_threshold;
                    thresholdlist.Add(threshold);
                }
                else
                    thresholdlist.Add(threshold);
            }

            // adding the value for the safety_threshold to the list for the respective 90 degrees
            thresholdlist.Add(safety_threshold);

            // Adding values in list for index 90-179, the values are mirrowed from the first 89 entries in the same list
            for (int i = 179; i >= 90; i--)
            {
                thresholdlist.Add(thresholdlist[i-89]);
            }
            // adding the safety_radius itself as value for 181 degrees, same as for degree 0, respective Position 0 within the list
            thresholdlist.Add(thresholdlist[0]);

            // Returning complete list of corridor-distances
            return thresholdlist;
        }

        /// <summary>
        /// 2 B) 
        /// Implementation for the real threshold distances (variable, depending on degree)
        /// needs checking for right calculation!
        /// This method implies a comparison between the actual scan data (i.e. medianList) and the thresholdlist (see method above for calculation)
        /// </summary>
        public Boolean Check2()
        {
            // set drive = false as default for each new entry into this method
            Boolean drive = false;

            // get medianlist via calling scan() method after declaration of local list
            List<int> medianList = new List<int>();
            medianList = Scan();

            // get Thresholdlist via calling generateCorridor() Method after declaration of local list
            List<double> thresholdlist = new List<double>();
            thresholdlist = generateCorridorList();

            // Check: compare values from thresholdlist with the values from the repsecrtive degree in medianlist
            // iterator for thresholdlist starting at index 0
            int i = 0;

            // iterator for medianList starting at index 45
            int j = 45;

            while (i < thresholdlist.Count()-1 && j < medianList.Count()-1)
            {
             /* Further debugging output if needed, helpful for reverse engineering the decision making process in code concerning the boolean value for drive
                Debug.WriteLine("current index thresholdlist: "+ i);
                Debug.WriteLine("lenght of thresholdlist: " + thresholdlist.Count());
                Debug.WriteLine("current index medianList: " + j);
                Debug.WriteLine("length of medianList: " + medianList.Count()); */

                // Check Algorithm to set the boolean drive according to the output of comparison
                 if (thresholdlist[i] > medianList[j])
                {
                 // sets stop
                 /* Debug.WriteLine("current index thresholdlist: "+ j);
                    Debug.WriteLine("current index medianlist: "+ i);
                    Debug.WriteLine("current value from medianlist: " + medianList[j]); */

                    Debug.WriteLine("drive == false");
                    drive = false;
                    return drive;
                }
                if (thresholdlist[i] <= medianList[j])
                {
                    Debug.WriteLine("drive == true");
                    drive = true; /*
                    Debug.WriteLine("current index thresholdlist: "+ j);
                    Debug.WriteLine("current index medianlist: "+ i);
                    Debug.WriteLine("current value from medianlist: " + medianList[j]); */   
                 }
                i++;
                j++;
            }
            // Return the previously set boolsche variable drive
            return drive;
        }

        /// <summary>
        /// 3. DECIDE
        /// Based on the output value of the method CHECK(), this following method sets the robot into the repsective modus, either stop or drive
        /// </summary>
        public void Decide()
        {
            // velocity that henry drives
            // stop and drive mode are represented by the integer values 0 and 1
            int drive_mode = 1;
            int stop_mode = 0;

            _robot.Enable();
            if (_robot != null && _robot.Enable())
            {
                // Calling the method Check() to allocate the boolsche value correctly from the scan data
                Boolean drive = Check();

                // setting the robot into the repsective velocity according to the above input from the method call
                if (drive == false)
                {
                    // sets velocity to zero so that Henry stops
                    _robot.Move(stop_mode);
                    return;
                }
                else if (drive == true)
                {
                    // sets velocity to 1 so that henry drives
                    _robot.Move(drive_mode);
                }
            }
        }

        /// <summary>
        /// 3. DECIDE#2 (calling Check2())
        /// Based on the output value of the method CHECK()2, this following method sets the robot into the repsective modus, either stop or drive
        /// </summary>
        public void Decide_basedonthresholdlist()
        {
            // velocity that henry drives
            // stop and drive mode are represented by the integer values 0 and 1
            int drive_mode = 1;
            int stop_mode = 0;

            _robot.Enable();
            if (_robot != null && _robot.Enable())
            {
                // Calling the method Check2() to allocate the boolsche value correctly from the scan data
                Boolean drive = Check2();

                // setting the robot into the repsective velocity according to the above input from the method call
                if (drive == false)
                {
                    // sets velocity to zero so that Henry stops
                    _robot.Move(stop_mode);
                    return;
                }
                if (drive == true)
                {
                    // sets velocity to 1 so that henry drives
                    _robot.Move(drive_mode);
                }
            }
        }

         /// <summary>
         /// Generates random number. Based on the number he turns left or right.
         /// If random number is 1 Henry turns 45° to the left.
         /// If random number is 2 Henry turns 45° to the right.
         /// </summary>
        public void randomLeftOrRight()
        {

            Random rnd = new Random();

            int random = 0;

            random = rnd.Next(1, 3);

            if (random == 1)
            {
                Debug.WriteLine("Turn left");
                _robot.TurnInDegrees(45);
            }
            if (random == 2)
            {
                 Debug.WriteLine("Turn right");
                _robot.TurnInDegrees(-45);
            }
           
        }

        /// <summary>
        /// drives forward as long as there is room. If there is an obstacle the method 
        /// randomLeftOrRight is called
        /// </summary>
        public void randomDriveLeftOrRight()
        {

            int drive_mode = 1;
            int stop_mode = 0;

            _robot.Enable();
            if (_robot != null && _robot.Enable())
            {
                // Calling the method Check2() to allocate the boolsche value correctly from the scan data
                Boolean drive = Check2();

                // setting the robot into the repsective velocity according to the above input from the method call
                if (drive == false)
                {
                    // sets velocity to zero so that Henry stops
                    _robot.Move(stop_mode);
                    randomLeftOrRight();
                    drive = true;
                }
                else if (drive == true)
                {
                    // sets velocity to 1 so that henry drives
                    _robot.Move(drive_mode);
                }
            }
        }

        /// <summary>
        /// Decides where to move. Turns henry either a certain ammount of degrees left or right.
        /// </summary>
        public void checkLeftOrRight()
        {

            int left = 0;
            int leftDistance = 0;
            int leftFurthestIndex = 0;

            int right = 0;
            int rightDistance = 0;
            int rightFurthestIndex = 0;

            List<int> medianList = new List<int>();
            medianList = Scan();

            // Values for the right side (0 to 90 degrees, respective 46 to 136 degrees
            for(int i = 46; i <= 136; i++)
            {

                // searches for the furthest distance and its index
                if(rightDistance < medianList[i])
                {
                    // only sets longest distance if it is longer than the safety threshold
                     if(medianList[i] > safety_threshold)
                     {
                        // saves the furthest distance thus far. If there is a bigger one it saves them.
                        rightDistance = medianList[i];
                        // saves the index from the longest distance to use it when Henry has to turn, *-1 needs to be negative to turn right
                        rightFurthestIndex = i * -1;
                      Debug.WriteLine("longest distance right: " + rightDistance + " degree to turn right " + rightFurthestIndex);
                     }
                }

                if(medianList[i] < safety_threshold)
                {

                    right++;
                }
            }

            // Values for Left side, degree 90 to 180, repsective 137 to 226
            for(int i = 137; i <= 226; i++)
            {

                 // searches for the furthest distance and its index.
                if(leftDistance < medianList[i])
                {

                    // only sets longest distance if it is longer than the safety threshold
                    if(medianList[i] > safety_threshold)
                    {

                        // saves the furthest distance thus far. If there is a bigger one it saves that one instead.
                        leftDistance = medianList[i];
                        // saves the index from the longest distance to use it when Henry has to turn
                        leftFurthestIndex = i - 90;
                        Debug.WriteLine("furthest distance to the left: " + leftDistance + " degrees to turn left: " + leftFurthestIndex);
                    }
                }

                // if there is an obstacle it gets counted
                if(medianList[i] < safety_threshold)
                {
                    left++;
                }
            }

            // when there are more objects on the right side, henry turns left 
            if(right > left)
            {

                if(twiddle == 0)
                {

                    _robot.TurnInDegrees(leftFurthestIndex);
                    // set twiddle to 1, to save that he turned left before
                    twiddle = 1;
                    Debug.WriteLine("turns left to following degree: " + leftFurthestIndex);

                    // if he turned left before he has to turn further to the left to not get stuck in a loop
                } else if (twiddle == 1)
                {
                    _robot.TurnInDegrees(30);
                     Debug.WriteLine("turns to the left, because he turned left before. Value of twiddle: " + twiddle);

                  // if he has turned to the right before he has to turn further to the right
                } else if (twiddle == 2)
                {
                    _robot.TurnInDegrees(-30);
                    Debug.WriteLine("turns left, turned left just before. Value of twiddle: " + twiddle);
                }
                
            // when there are more obstacles on the left side, henry turns right
            } else if( right < left)
            {

                if(twiddle == 0)
                {
                    _robot.TurnInDegrees(rightFurthestIndex);
                    twiddle = 2;
                    Debug.WriteLine("turns left to the following degree: " + rightFurthestIndex);

                } else if (twiddle == 1)
                {
                    _robot.TurnInDegrees(30);
                     Debug.WriteLine("turns to the right, has turned left before. Value of twiddle: " + twiddle);

                } else if (twiddle == 2)
                {
                    _robot.TurnInDegrees(-30);
                    Debug.WriteLine("turns right, turned right before, Value of twiddle: " + twiddle);

                }

            } else if ( right == left)
            {
                 _robot.TurnInDegrees(180);

            // if the furthest distance is empty 
            } else if( rightFurthestIndex < safety_threshold)
            {
                _robot.TurnInDegrees(-180);
            }
            else if( leftFurthestIndex < safety_threshold)
            {
                _robot.TurnInDegrees(180);
            }

        }

        /// <summary>
        /// Henry drives forward. If there is an obstacle he calls the checkLeftOrRight Method to check where to
        /// turn and drives forward again.
        /// </summary>
        public void driveLeftOrRight()
        {
            int drive_mode = 1;
            int stop_mode = 0;

            _robot.Enable();
            if (_robot != null && _robot.Enable())
            {
                // Calling the method Check2() to allocate the boolsche value correctly from the scan data
                Boolean drive = Check2();

                // setting the robot into the repsective velocity according to the above input from the method call
                if (drive == false)
                {
                    // sets velocity to zero so that Henry stops
                    _robot.Move(stop_mode);
                    checkLeftOrRight();
                    drive = true;
                }
                else if (drive == true)
                {
                    twiddle = 0;
                    // sets velocity to 1 so that henry drives
                    _robot.Move(drive_mode);
                }
            }
        }

        /// <summary>
        /// method fpr printing an array list
        /// </summary>
        public void printArray<T>(IEnumerable<T> a)
        {
            foreach (var i in a)
            {
                Debug.WriteLine(i);
            }
        }

        /// <summary>
        /// 4. TEST
        /// The following methods have been implemented in order to test the logik and structure of the above methods
        /// fake lists are beeing created within the methods testListnoObstacle() and testlistObstacle()
        /// fake list with values > safety_threshold, no obstacles
        /// </summary>
        public List<int> testListnoObstacle ()
        {

            var filltestList = new List<int>();

            for(int i =0; i<=270;i++)
            {
                filltestList.Add(safety_threshold);
            }
            return filltestList;

        }

        /// <summary>
        /// fake list with values < safety_threshold, obstacles are implied
        /// </summary>
        public List<int> testListObstacle ()
        {
            var filltestList2 = new List<int>();
            for (int i=0;i<=270;i++)
            {
                filltestList2.Add(( safety_threshold - 1));
            }
            return filltestList2;

        }
        
        /// <summary>
        /// Method to check Logic, testlists with fake data are utilized. This method has a list as parameter
        /// The above methods for testlists will be called first, so as to call the below method afterwards including the generated list as parameter
        /// </summary>
        public Boolean testCheck(List<int> testList)
        {
            Boolean drive = false;
            // Checking the lists values from index 100 to 200
            for (int i = 100; i <= 200; i++)
            {
                if(testList.Count == 0)
                {
                    Debug.WriteLine("arraylist is empty :(");
                    continue;
                }

                // Safety Threshold is bigger than the value in the testlist --> obstacle
                if (safety_threshold > testList[i])
                {
                    // sets stop
                    Debug.WriteLine(testList[i]);
                    Debug.WriteLine("drive == false");
                    drive = false;
                }
                // Safety Threshold is smaller or equal than the value in the testlist --> no onstacle
                else if (safety_threshold <= testList[i])
                {
                    Debug.WriteLine("drive == true");
                    drive = true;
                    Debug.WriteLine(testList[i]);
                    return drive;
                }
            }
            return drive;
        }
    }
}