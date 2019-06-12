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
        public static Robot _robot = new Robot();
        public static Scanner _scanner = new Scanner();
        public static int velocity = 1;
        public static int safety_threshold = 700;

        //METHODS FOR AUTONOMOUS DRIVING FUNCTIONALITY

        //1. SCAN
        //this method utilizes the class scanner.cs directly by calling on  the methods GetDataList() as well as MedianFilter()
        //The try-catch block aids in case the received list from those called methods has a lenght of 0
        public List<int> Scan(){
            //initialize local medianList
            var medianList = new List<int>();
            try { 
                medianList=_scanner.MedianFilter(_scanner.GetDataList());
             }
                catch (Exception e)
                {
                    if (e is IndexOutOfRangeException)
                    {

                        Debug.WriteLine(e.Message);
                        Debug.WriteLine("Länge MedianListe: "+ medianList.Count());
                        //continue;
                        //break;
                    }
                }
            return medianList;

         }
        //2. CHECK
        //This method uses the output of the SCAN()-Method above as input
        //boolean method, returns the value allocated with the two robot-states: drive or stop
        //if no obstacle occurs within the defined range, the boolean drive is set to true 
        //if an obstacle occurs within the defined range, the boolean drive is set to false
        //for the initial start of the robot and program, the boolsche variable is set to false in order to maintain a stop until the frist scan has been initited 
        public Boolean Check()
        {
            Debug.WriteLine("Entering Check-Method");
            Boolean drive = false;
            // checks every degree right infront of henry (100° angle)
            // if treshold is greater than any degree distance henry stops
            var medianList = new List<int>();
            medianList = Scan();
            Debug.WriteLine("Länge MedianListe: "+medianList.Count());
            for (int i = 100; i <= 200; i++)
            {
                Debug.WriteLine("Check-Method: Entering For Loop");
                if(medianList.Count == 0){
                    Debug.WriteLine(" Liste ist leer :(");
                    //break;
                    //continue;
                    return drive;
                }
                else if (safety_threshold > medianList[i])
                {
                    // sets stop
                    Debug.WriteLine(medianList[i]);
                    Debug.WriteLine("drive == false");
                    drive = false;
                    return drive;
                }
                else if (safety_threshold <= medianList[i])
                {
                    Debug.WriteLine("drive == true");
                    drive = true;
                    Debug.WriteLine(medianList[i]);
                    return drive;
                }
            }
            return drive;
        }
        //2 B) 
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
                    //printArray(medianList);
                }
            }
            return drive;
        }
        //3. DECIDE
        //Based on the output value of the method CHECK(), this following method sets the robot into the repsective modus, either stop or drive
        public void Decide()
        {
            // velocity that henry drives
            //stop and drive mode are represented by the integer values 0 and 1
            int drive_mode = 1;
            int stop_mode = 0;

            _robot.Enable();
            if (_robot != null && _robot.Enable())
            {
                //Calling the method Check() to allocate the boolsche value correctly from the scan data
                Boolean drive = Check();
                //setting the robot into the repsective velocity according to the above input from the method call
                if (drive == false)
                {
                    // sets velocity to zero so that Henry stops
                    _robot.Move(stop_mode);
                    //return;
                }
                else if (drive == true)
                {
                    //sets velocity to 1 so that henry drives
                    _robot.Move(drive_mode);
                }
            }
        }

        //method fpr printing an array list
        /*public void printArray<T>(IEnumerable<T> a)
        {
            foreach (var i in a)
            {
                Debug.WriteLine(i);
            }
        }*/

            //4. TEST
            //The following methods have been implemented in order to test the logik and structure of the above methods
            //fake lists are beeing created within the methods testListnoObstacle() and testlistObstacle()
            //fake list with values > safety_threshold, no obstacles
        public List<int> testListnoObstacle (){
            var filltestList = new List<int>();
            for(int i =0; i<=270;i++){
                //i=700;
                filltestList.Add(700);
}
            return filltestList;

}
        //fake list with values < safety_threshold, obstacles are implied
        public List<int> testListObstacle (){
               var filltestList2 = new List<int>();
            for (int i=0;i<=270;i++){
                //i=699;
                filltestList2.Add(699);
                //return testList;
}
            return filltestList2;

}
        public Boolean testCheck(List<int> testList)
        {
            //_robot = new Robot();
            //_scanner = new Scanner();
            Boolean drive = false;

            for (int i = 100; i <= 200; i++)
            {
                if(testList.Count == 0){
                    Debug.WriteLine(" Liste ist leer :(");
                    continue;
                }
                if (safety_threshold > testList[i])
                {
                    // sets stop
                    Debug.WriteLine(testList[i]);
                    Debug.WriteLine("drive == false");
                    drive = false;
                    return drive;
                }
                else if (safety_threshold <= testList[i])
                {
                    Debug.WriteLine("drive == true");
                    drive = true;
                    Debug.WriteLine(testList[i]);
                    //printArray(medianList);
                    return drive;
                }
            }
            return drive;
        }
    }
}
