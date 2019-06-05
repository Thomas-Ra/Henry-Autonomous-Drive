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
        //Initializing objects and variables:
        public static Robot _robot;
        public static Scanner _scanner;
        public static int velocity = 1;
        public static int safety_threshold = 700;

        //METHODS FOR AUTONOMOUS DRIVING FUNCTIONALITY
        //1. SCAN
        public Boolean ScanAndCheck()
        {
            _robot = new Robot();
            _scanner = new Scanner();
            Boolean drive = false;
            var medianList = new List<int>();

            //for (int i = 46; i <= 224; i++)
            //For Testing we use values from 100 to 200 degrees in front of the robot
            for (int i = 100; i <= 200; i++)
            {
                try
                {
                    medianList = _scanner.MedianFilter(_scanner.GetDataList());

                }
                catch (Exception e)
                {
                    if (e is IndexOutOfRangeException)
                    {

                        Debug.WriteLine(e.Message);
                        continue;
                    }
                }
                // checks every degree right infront of henry (100° angle)
                // if treshold is greater than any degree distance henry stops
                //threshold with whole list of different distances
                // if thresholdlist > medianList[i]){}
                if (safety_threshold > medianList[i])
                {
                    // sets stop
                    Debug.WriteLine(medianList[i]);
                    Debug.WriteLine("drive == false");
                    drive = false;
                    return drive;
                }
                else if (safety_threshold < medianList[i])
                {
                    Debug.WriteLine("drive == true");
                    drive = true;
                    Debug.WriteLine(medianList[i]);
                    //printArray(medianList);
                    return drive;
                }
            }
            return drive;
        }
        //Implementation for the real threshold distances (variable, depending on degree)
        //needs checking for right calculation!
        public Boolean ScanAndCheck2()
        {
            _robot = new Robot();
            _scanner = new Scanner();
            Boolean drive = false;
            var medianList = new List<int>();
            var thresholdlist = new List<double>();
            for (int i = 1; i <= 89; i++)
            {
                double rt = 54.35;
                double threshold = i / rt;
                if (threshold > safety_threshold)
                {
                    threshold = safety_threshold;
                    thresholdlist.Add(threshold);
                }
                else
                    thresholdlist.Add(i);
            }
            medianList = _scanner.MedianFilter(_scanner.GetDataList());
            for (int i = 46; i <= 224; i++)
            {
                // checks every degree right infront of henry (100° angle)
                // if treshold is greater than any degree distance henry stops
                //threshold with whole list of different distances
                // if thresholdlist > medianList[i]){}
                if (thresholdlist[i] > medianList[i])
                {
                    // sets stop 
                    Debug.WriteLine("drive =false");
                    //Debug.WriteLine(medianList);

                }
                else
                {
                    //drive is allowed
                    Debug.WriteLine("drive = true");
                    drive = true;
                    printArray(medianList);
                }
            }
            return drive;
        }
        //2. DECIDE
        public void decideStopOrDrive()
        {
            _robot = new Robot();
            _scanner = new Scanner();
            // velocity that henry drives
            int velocity = 1;
            // list for the median values of the scanner
            var medianList = new List<int>();
            _robot.Enable();

            if (_robot != null && _robot.Enable())
            {
                Debug.WriteLine("Länge medianList: " + medianList.Count);
                Auto1 instanceAuto1 = new Auto1();

                Boolean drive = instanceAuto1.ScanAndCheck();
                if (drive == false)
                {
                    // sets velocity to zero so that Henry stops
                    _robot.Move(0);
                    //return;
                }
                else if (drive == true)
                {
                    //sets velocity to 1so that henry drives
                    _robot.Move(velocity);
                }


            }

        }

        //method fpr printing an array list
        public void printArray<T>(IEnumerable<T> a)
        {
            foreach (var i in a)
            {
                Debug.WriteLine(i);
            }
        }
    }
}

