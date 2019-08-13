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
        // initializing objects and variables:
        public static Robot _robot = new Robot();
        public static Scanner _scanner = new Scanner();
        // velocity is in km/h
        public static int velocity = 1;
        // threshold is in mm
        public static int safety_threshold = 700;

        // turns left if the value is 1 and right if the value is 2
        // used in checkLeftOrRight() and set/reset to zero again in driveLeftOrRight() 
        int twiddle = 0;

        /// <summary>
        /// This method utilizes the class scanner.cs directly by calling on the methods GetDataList() as well as MedianFilter().
        /// The try-catch block aids in case the received list from those called methods has a lenght of 0, i.e. when the scanner port is blocked.
        /// </summary>
        /// <returns> Returns a list of distances from the scanner. Scanning errors will be corrected via the use of the median correction. </returns>
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
        /// This method uses the output of the Scan()-Method (MedianList) as input.
        /// Boolean method, returns the value allocated with the two robot-states: either drive or stop.
        /// If no obstacle occurs within the defined range, the boolean drive is set to true.
        /// If an obstacle occurs within the defined range, the boolean drive is set to false.
        /// For the initial start of the robot and program, the boolean variable is set to false in order to maintain a stop until the frist scan has been initiated.
        /// </summary>
        /// <returns> Returns the boolean variable drive. True means there is no obstacle. The robot can safely frive forward. 
        /// False means there is an obstacle so the robot has to stop immediately. </returns>
        public Boolean Check()
        {
            var medianList = new List<int>();

            // calling Scan() method to retrieve the medianList
            medianList = Scan();

            Boolean drive = false;

            // Checking the retrieved list between index of 100 and 200 -> the area infront of the robot
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
        /// Getting a list with 181 values for a safe corridor. In this corridor the robot can safely turn around.
        /// </summary>
        /// <returns> Returns a double array list with values in which the robot can safely move. </returns>
        public List<double> generateCorridorList()
        {
            var thresholdlist = new List<double>();
            var reverseThresholdlist = new List<double>();

            // turning radius of the Robot = 44,35 cm
            // adding an extra 10 cm safety distance to the radius on both sides
            // safety_radius = 44,35cm + 10cm + 10cm = 64.35cm -> in MM = 643.5mm
            double safety_radius = 600;

            // initializing the values for the corridor
            // adding the safety_radius itself as value for 0 degrees, as the calculation starts at 1 degree
            thresholdlist.Add(safety_radius);

            // calculation for values in list with index 1-90
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

            // adding values in list for index 90-179, the values are mirrowed from the first 89 entries in the same list
            for (int i = 179; i >= 90; i--)
            {
                thresholdlist.Add(thresholdlist[i-89]);
            }
            // adding the safety_radius itself as value for 181 degrees, same as for degree 0, respective position 0 within the list
            thresholdlist.Add(thresholdlist[0]);

            // returning complete list of corridor-distances
            return thresholdlist;
        }

        /// <summary>
        /// Implementation for the real threshold distances (thresholds depending on degree).
        /// This method does a comparison between the scan data (i.e. medianList) and the thresholdlist (see method generateCorridorList()).
        /// </summary>
        /// <returns> Returns the boolean variable drive. True means there is no obstacle in our defined safe corridor. 
        /// The robot can safely frive forward and turn around. False means there is an obstacle in the corridor, so the robot has to stop 
        /// immediately and can not safely turn. </returns>
        public Boolean Check2()
        {
            // set drive = false as default for each new entry into this method
            Boolean drive = false;

            // get medianlist via calling Scan() method after declaration of local list
            List<int> medianList = new List<int>();
            medianList = Scan();

            // get thresholdlist via calling generateCorridorList() Method after the declaration of the local list
            List<double> thresholdlist = new List<double>();
            thresholdlist = generateCorridorList();

            // check comparison: compare the values from thresholdlist with the values from the respective degree in the medianlist
            // iterator for thresholdlist starting at index 0
            int i = 0;

            // iterator for medianList starting at index 45 (we ignore the first 45 values, because the robot sees itself in them)
            int j = 45;

            while (i < thresholdlist.Count()-1 && j < medianList.Count()-1)
            {
             /* Further debugging output if needed, helpful for reverse engineering the decision making process in the code concerning the boolean value for drive
                Debug.WriteLine("current index thresholdlist: "+ i);
                Debug.WriteLine("lenght of thresholdlist: " + thresholdlist.Count());
                Debug.WriteLine("current index medianList: " + j);
                Debug.WriteLine("length of medianList: " + medianList.Count()); */

                // the Check comparison algorithm to set the boolean drive according to the output of comparison
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
            // Return the previously set boolean variable drive
            return drive;
        }

        /// <summary>
        /// Based on the output value of the method Check(), this following method sets the robot into the respective driving mode, either stop or drive.
        /// However this method is only suitable for deciding if the robot has to stop or if he can drive. E.g. if an obstacle is in front of him.
        /// This method can not guarantee, that he can safely turn! For this use the method Decide_basedonthresholdlist().
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
                // calling the method Check() to allocate the boolean value correctly from the scan data
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
                    // sets velocity to 1 so that Henry drives
                    _robot.Move(drive_mode);
                }
            }
        }

        /// <summary>
        /// Based on the output value of the method Check2(), this following method sets the robot into the respective driving mode, either stop or drive.
        /// This method also makes sure, that the robot will stop if there is an obstacle in the safety corridor. Meaning that he can always safely turn around.
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
                // calling the method Check2() to allocate the boolsche value correctly from the scan data
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
                    // sets velocity to 1 so that Henry drives
                    _robot.Move(drive_mode);
                }
            }
        }

         /// <summary>
         /// Generates a random number. Based on the number he either turns left or right.
         /// If the random number is 1 Henry turns 45° to the left.
         /// If the random number is 2 Henry turns 45° to the right.
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
        /// Henry drives forward as long as there is space. If there is an obstacle in front of him or in his safety corridor the method 
        /// randomLeftOrRight() is called to turn him randomly.
        /// </summary>
        public void randomDriveLeftOrRight()
        {
            int drive_mode = 1;
            int stop_mode = 0;

            _robot.Enable();
            if (_robot != null && _robot.Enable())
            {
                // calling the method Check2() to allocate the boolean value correctly from the scan data
                Boolean drive = Check2();

                // setting the robot into the repsective velocity according to the above input from the method call
                if (drive == false)
                {
                    // sets velocity to zero so that Henry stops
                    _robot.Move(stop_mode);
                    randomLeftOrRight();
                    // drive = true;
                    return;
                }
                else if (drive == true)
                {
                    // sets velocity to 1 so that Henry drives
                    _robot.Move(drive_mode);
                }
            }
        }

        /// <summary>
        /// Decides where to move based on the scanner data. The area in front of him that the scanner sees is divided into two 90 degree areas.
        /// He turns in the direction with less obstacles and aligns itself with the degree that has the furthest distance.
        /// We also make sure that he has to turn in one direction as long as he cannot drive forward. With this we make sure that he cannot 
        /// get stuck in a loop on certain occasions.
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

            // values for the right side (0 to 90 degrees, respective 46 to 136 degrees)
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

            // values for left side (90 to 180 degrees, respective 137 to 226)
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

                  // if he has turned to the right before he has to turn further to the right to not get stuck in a loop
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
        /// Henry drives forward. If there is an obstacle he calls the checkLeftOrRight() method to check where he has to
        /// turn in ordner to drive forward again.
        /// </summary>
        public void driveLeftOrRight()
        {
            int drive_mode = 1;
            int stop_mode = 0;

            _robot.Enable();
            if (_robot != null && _robot.Enable())
            {
                // calling the method Check2() to allocate the boolean value correctly from the scan data
                Boolean drive = Check2();

                // setting the robot into the respective velocity according to the above input from the method call
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
                    // sets velocity to 1 so that Henry drives
                    _robot.Move(drive_mode);
                }
            }
        }

        /// <summary>
        /// Method for printing an array list in the debug command line.
        /// </summary>
        /// <param name="a"> Array that you want to be printed. </param>
        public void printArray<T>(IEnumerable<T> a)
        {
            foreach (var i in a)
            {
                Debug.WriteLine(i);
            }
        }

        /// <summary>
        /// The following methods have been implemented in order to test the logic and structure of the above methods.
        /// Fake lists are beeing created within the methods testListnoObstacle() and testlistObstacle().
        /// Fake list with values > safety_threshold, no obstacles.
        /// </summary>
        /// <returns> Returns a filled list of the length 270 with the values of the safety threshold.</returns>
        public List<int> testListnoObstacle()
        {

            var filltestList = new List<int>();

            for(int i =0; i <= 270; i++)
            {
                filltestList.Add(safety_threshold);
            }
            return filltestList;

        }

        /// <summary>
        /// Generates a fake list where the values are smaller than the safety_threshold. So that the list is full of obstacles for the logic.
        /// </summary>
        /// <returns> Returns a filled list of the length 270 with the values of the (safety threshold -1).</returns>
        public List<int> testListObstacle()
        {
            var filltestList2 = new List<int>();
            for (int i = 0; i <= 270; i++)
            {
                filltestList2.Add((safety_threshold - 1));
            }
            return filltestList2;

        }
        
        /// <summary>
        /// Method to check Logic, testlists with fake data are utilized.
        /// The methods testListnoObstacle() and testListObstacle() will be called first, so as to call the below method afterwards 
        /// including the generated list as parameter.
        /// </summary>
        /// <param name = "testlist"> Array that shall be run in the logic of the Check() Method. </param>
        /// <returns> Returns the boolean variable drive. True means there is no obstacle. The robot can safely frive forward. 
        /// False means there is an obstacle so the robot has to stop immediately. </returns>
        public Boolean testCheck(List<int> testList)
        {
            Boolean drive = false;
            // checking the lists values from index 100 to 200
            for (int i = 100; i <= 200; i++)
            {
                if(testList.Count == 0)
                {
                    Debug.WriteLine("arraylist is empty :(");
                    continue;
                }

                // safety threshold is bigger than the value in the testlist --> obstacle
                if (safety_threshold > testList[i])
                {
                    // sets stop
                    Debug.WriteLine(testList[i]);
                    Debug.WriteLine("drive == false");
                    drive = false;
                }
                // safety threshold is smaller or equal than the value in the testlist --> no obstacle
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
