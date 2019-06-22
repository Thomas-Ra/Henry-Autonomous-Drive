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
        public static int safety_threshold = 600;
                    // bei 1 dreht nach links, bei 2 dreht nach rechts
            int rechts = 0;

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
                /*for(int i =0; i<=medianList.Count()-1; i++){ 
                Debug.WriteLine("Index: "+ i +" --> "+ medianList[i]);
                 }*/
                 var medianList = new List<int>();
                 medianList= Scan();
                 Boolean drive = false;
                for (int i = 100; i <= 200; i++)
                {
                Debug.WriteLine("Check-Method: Entering For Loop");
                if(medianList.Count == 0){
                    Debug.WriteLine(" Liste ist leer :(");
                }
                else if (safety_threshold > medianList[i])
                {
                    // sets stop
                    Debug.WriteLine("Aktueller Wert aus MedianListe "+medianList[i]);
                    Debug.WriteLine("Aktueller Index aus MedianListe: "+i);
                    Debug.WriteLine("drive == false");
                    drive = false;
                    return drive;
                }
                else if (safety_threshold <= medianList[i])
                {
                    //Debug.WriteLine("drive == true");
                    drive = true;
                    Debug.WriteLine("Aktueller Wert aus MedianListe "+medianList[i]);
                    Debug.WriteLine("Aktueller Index aus MedianListe: "+i);
                }
            }
            Debug.WriteLine(drive.ToString());
            return drive;
        }

        /**
         * getting a list with 181 values for a corridor
         **/
        public List<double> generateCorridorList()
        {
            var thresholdlist = new List<double>();
            var reverseThresholdlist = new List<double>();
            //Turning Radius of Robot = 44,35
            // Adding an extra ten cm safety distance to the radius on both sides
            //safety_radius=44,35 + 10 + 10 = 64.35 -> in MM = 643.5
            double safety_radius = 600;
            //initializing of values for the corridor
            //adding the safety_radius itself as value for 0 degrees, as the calculation starts at 1 degree
            thresholdlist.Add(safety_radius);
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
            //adding the value for the safety_threshold to the list for the respective 90 degrees
            thresholdlist.Add(safety_threshold);
            for (int i = 179; i >= 90; i--)
            {
                thresholdlist.Add(thresholdlist[i-89]);
            }
            ////adding the safety_radius itself as value for 181 degrees, same as for degree 0, respective Position 0 within the list
            thresholdlist.Add(thresholdlist[0]);

            //reversing list
            //reverseThresholdlist.Add(thresholdlist.Reverse().ToDouble());
            // adding reversed list
            //thresholdlist.Add(reverseThresholdlist);
            //return the list
            return thresholdlist;
        }


        //2 B) 
        //Implementation for the real threshold distances (variable, depending on degree)
        //needs checking for right calculation!
        public Boolean Check2()
        {
            Debug.WriteLine("Entering Check2 Method");
            //set drive = false as default
            Boolean drive = false;
            //get medianlist via scan() method after declaration of local list
            List<int> medianList = new List<int>();
            medianList = Scan();
            //get Thresholdlist via generateCorridor() Method
            List<double> thresholdlist = new List<double>();
            thresholdlist = generateCorridorList();
            //Check: compare values from thresholdlist with the values fro, the repsecrtive degree in medianlist
            int i = 0;
            int j = 45;
            while (i < thresholdlist.Count()-1 && j < medianList.Count()-1)
            {
             /*   Debug.WriteLine("Aktueller Index Thresholdliste "+ i);
                Debug.WriteLine("Länge Thresholdliste= "+thresholdlist.Count());
                Debug.WriteLine("Aktueller Index MedianListe "+j);
                Debug.WriteLine("Länge MedianListe= "+ medianList.Count()); */
                //Check Algorithm to set the boolean drive according to the output of comparison
                 if (thresholdlist[i] > medianList[j])
                {
                    // sets stop
                 /*   Debug.WriteLine("Aktueller Index Thresholdliste "+ j);
                    Debug.WriteLine("Aktueller Index Medianliste "+ i);
                    Debug.WriteLine("Aktueller Wert aus MedianListe" + medianList[j]);
                    Debug.WriteLine("drive == false"); */
                    drive = false;
                    return drive;
                }
                else if (thresholdlist[i] <= medianList[j])
                {
                  //  Debug.WriteLine("drive == true");
                    drive = true;
                  /*  Debug.WriteLine("Aktueller Index Thresholdliste "+ j);
                    Debug.WriteLine("Aktueller Index Medianliste "+ i);
                    Debug.WriteLine("Aktueller Wert aus MedianListe" + medianList[j]);    */  
                 }
                i++;
                j++;
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

        //3. DECIDE#2 (calling Check2())
        //Based on the output value of the method CHECK(), this following method sets the robot into the repsective modus, either stop or drive
        public void Decide_basedonthresholdlist()
        {
            // velocity that henry drives
            //stop and drive mode are represented by the integer values 0 and 1
            int drive_mode = 1;
            int stop_mode = 0;

            _robot.Enable();
            if (_robot != null && _robot.Enable())
            {
                //Calling the method Check2() to allocate the boolsche value correctly from the scan data
                Boolean drive = Check2();
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

         /// <summary>
         /// Generates random number. Based on the number he turns left or right.
         /// If random number is 1 Henry turns 45° to the left.
         /// If random number is 2 Henry turns 45° to the right.
         /// </summary>
        public void randomLeftOrRight() {

            Random rnd = new Random();

            int zufallszahl = 0;
            int ergebnis = 0;

            zufallszahl = rnd.Next(1, 3);

            if (zufallszahl == 1){

                _robot.TurnInDegrees(45);
            }
            if (zufallszahl == 2){

                _robot.TurnInDegrees(-45);
            }
           
        }
            /// <summary>
            /// drives forward as long as there is room. If there is an obstacle the method 
            /// randomLeftOrRight is called
            /// </summary>
        public void randomDriveLeftOrRight(){

            int drive_mode = 1;
            int stop_mode = 0;

            _robot.Enable();
            if (_robot != null && _robot.Enable())
            {
                //Calling the method Check2() to allocate the boolsche value correctly from the scan data
                Boolean drive = Check2();
                //setting the robot into the repsective velocity according to the above input from the method call
                if (drive == false)
                {
                    // sets velocity to zero so that Henry stops
                    _robot.Move(stop_mode);
                    randomLeftOrRight();
                    drive = true;
                }
                else if (drive == true)
                {
                    //sets velocity to 1 so that henry drives
                    _robot.Move(drive_mode);
                }
            }


        }


        /// <summary>
        /// Decides where to move. Turns henry either a certain ammount of degrees left or right.
        /// </summary>
        public void checkLeftOrRight(){

            int left = 0;
            int leftDistance = 0;
            int leftFurthestIndex = 0;
            int right = 0;
            int rightDistance = 0;
            int rightFurthestIndex = 0;

            List<int> medianList = new List<int>();
            medianList = Scan();

            //hier sind die rechten werte 
            for(int i = 46; i <= 136; i++){

                // searches for the furthest distance and its index.
                if(rightDistance < medianList[i]){
                     
                    //only sets longest distance if it is longer than the safety threshold
                     if(medianList[i] > safety_threshold){
                        // saves the furthest distance thus far. If there is a bigger one it saves them.
                        rightDistance = medianList[i];
                        // saves the index from the longest distance to use it when Henry has to turn
                        rightFurthestIndex = i * -1;
                        Debug.WriteLine("Längste Distanz rechts: " + leftDistance + " Gradzahl zum rechts drehen: " + leftFurthestIndex);
                     }
                }

                if(medianList[i] < safety_threshold){

                    right++;
                }
            }

            // hier ist links
            for(int i = 137; i <= 226; i++){

                 // searches for the furthest distance and its index.
                if(leftDistance < medianList[i]){

                    //only sets longest distance if it is longer than the safety threshold
                    if(medianList[i] > safety_threshold){
                        // saves the furthest distance thus far. If there is a bigger one it saves them.
                        leftDistance = medianList[i];
                        // saves the index from the longest distance to use it when Henry has to turn
                        leftFurthestIndex = i - 90;
                        Debug.WriteLine("Längste Distanz links: " + rightDistance + " Gradzahl zum links drehen: " + rightFurthestIndex);
                    }
                }

                // if there is an obstacle it gets counted
                if(medianList[i] < safety_threshold){

                    left++;
                }
            }

            // when there are more objects on the right side, henry turns left 
            if(right > left){

                if(rechts == 0){

                    _robot.TurnInDegrees(leftFurthestIndex);
                    rechts = 1;
                } else if (rechts == 1){

                    _robot.TurnInDegrees(30);
                } else if (rechts == 2){

                    _robot.TurnInDegrees(-30);
                }
                
            // when there are more obstacles on the left side, henry turns right
            } else if( right < left){

                if(rechts == 0){

                    _robot.TurnInDegrees(leftFurthestIndex);
                    rechts = 2;
                } else if (rechts == 1){

                    _robot.TurnInDegrees(30);
                } else if (rechts == 2){

                    _robot.TurnInDegrees(-30);
                }

            } else if ( right == left){

                 _robot.TurnInDegrees(180);
            // if the furthest distance is empty 
            } else if( rightFurthestIndex < safety_threshold){

                _robot.TurnInDegrees(-180);
            }
            else if( leftFurthestIndex < safety_threshold){

                _robot.TurnInDegrees(180);
            }

        }

         /// <summary>
         /// Henry drives forward. If there is an obstacle he calls the checkLeftOrRight Method to check where to
         /// turn and drives forward again.
         /// </summary>
        public void driveLeftOrRight(){

            int drive_mode = 1;
            int stop_mode = 0;

            _robot.Enable();
            if (_robot != null && _robot.Enable())
            {
                //Calling the method Check2() to allocate the boolsche value correctly from the scan data
                Boolean drive = Check2();
                //setting the robot into the repsective velocity according to the above input from the method call
                if (drive == false)
                {
                    // sets velocity to zero so that Henry stops
                    _robot.Move(stop_mode);
                    checkLeftOrRight();
                    drive = true;
                }
                else if (drive == true)
                {
                    rechts = 0;
                    //sets velocity to 1 so that henry drives
                    _robot.Move(drive_mode);
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


/*
             for(int i =0; i<=medianList.Count()-1; i++){ 
                Debug.WriteLine("Index: "+ i +" --> "+ medianList[i]);
             }
            for (int i = 100; i <= 200; i++)
            {
                Debug.WriteLine("Check-Method: Entering For Loop");
                if(medianList.Count == 0){
                    Debug.WriteLine(" Liste ist leer :(");
                    //return drive;
                }
                else if (safety_threshold > medianList[i])
                {
                    // sets stop
                    Debug.WriteLine("Aktueller Wert aus MedianListe "+medianList[i]);
                    Debug.WriteLine("Aktueller Index aus MedianListe: "+i);
                    //Debug.WriteLine("drive == false");
                    drive = false;
                    //return drive;
                }
                else if (safety_threshold <= medianList[i])
                {
                    //Debug.WriteLine("drive == true");
                    drive = true;
                    Debug.WriteLine("Aktueller Wert aus MedianListe "+medianList[i]);
                    Debug.WriteLine("Aktueller Index aus MedianListe: "+i);
                    //return drive;
                }
            }
            Debug.WriteLine(drive.ToString());
            return drive;
     */