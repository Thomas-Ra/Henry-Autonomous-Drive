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
                var medianList = new List<int>();
                //Calling scan() Method to retrieve MedianList
                 medianList= Scan();
                 Boolean drive = false;
                //Checking the retrieved list between index of 100 and 200
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
            //Turning Radius of Robot = 44,35 cm
            // Adding an extra 10 cm safety distance to the radius on both sides
            //safety_radius=44,35 + 10 + 10 = 64.35 -> in MM = 643.5
            double safety_radius = 600;
            //initializing the values for the corridor
            //adding the safety_radius itself as value for 0 degrees, as the calculation starts at 1 degree
            thresholdlist.Add(safety_radius);
            //Calculation for values in list with index 1-90
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
            //Adding values in list for index 90-179, the values are mirrowed from the first 89 entries in the same list
            for (int i = 179; i >= 90; i--)
            {
                thresholdlist.Add(thresholdlist[i-89]);
            }
            ////adding the safety_radius itself as value for 181 degrees, same as for degree 0, respective Position 0 within the list
            thresholdlist.Add(thresholdlist[0]);
            //Returning complete list of corridor-distances
            return thresholdlist;
        }


        //2 B) 
        //Implementation for the real threshold distances (variable, depending on degree)
        //needs checking for right calculation!
        //This method implies a comparison between the actual scan data (i.e. medianList) and the thresholdlist (see method above for calculation)
        public Boolean Check2()
        {
            //set drive = false as default for each new entry into this method
            Boolean drive = false;
            //get medianlist via calling scan() method after declaration of local list
            List<int> medianList = new List<int>();
            medianList = Scan();
            //get Thresholdlist via calling generateCorridor() Method after declaration of local list
            List<double> thresholdlist = new List<double>();
            thresholdlist = generateCorridorList();
            //Check: compare values from thresholdlist with the values from the repsecrtive degree in medianlist
            //iterator for thresholdlist starting at index 0
            int i = 0;
            //iterator for medianList starting at index 45
            int j = 45;
            while (i < thresholdlist.Count()-1 && j < medianList.Count()-1)
            {
             /*Further debugging output if needed, helpful for reverse engineering the decision making process in code concerning the boolean value for drive
                Debug.WriteLine("Aktueller Index Thresholdliste "+ i);
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
            //Return the previously set boolsche variable drive
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
        //Based on the output value of the method CHECK()2, this following method sets the robot into the repsective modus, either stop or drive
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
                Debug.WriteLine("Turn left");
                _robot.TurnInDegrees(45);
            }
            if (zufallszahl == 2){
                 Debug.WriteLine("Turn right");
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

            //Values for the right side (0 to 90 degrees, respective 46 to 136 degrees
            for(int i = 46; i <= 136; i++){

                // searches for the furthest distance and its index
                if(rightDistance < medianList[i]){
                     
                    //only sets longest distance if it is longer than the safety threshold
                     if(medianList[i] > safety_threshold){
                        // saves the furthest distance thus far. If there is a bigger one it saves them.
                        rightDistance = medianList[i];
                        // saves the index from the longest distance to use it when Henry has to turn
                        rightFurthestIndex = i * -1;
                      Debug.WriteLine("Längste Distanz Rechts: " + rightDistance + " Gradzahl zum Rechts drehen: " + rightFurthestIndex);
                     }
                }

                if(medianList[i] < safety_threshold){

                    right++;
                }
            }

            // Values for Left side, degree 90 to 180, repsective 137 to 226
            for(int i = 137; i <= 226; i++){

                 // searches for the furthest distance and its index.
                if(leftDistance < medianList[i]){

                    //only sets longest distance if it is longer than the safety threshold
                    if(medianList[i] > safety_threshold){
                        // saves the furthest distance thus far. If there is a bigger one it saves that one instead.
                        leftDistance = medianList[i];
                        // saves the index from the longest distance to use it when Henry has to turn
                        leftFurthestIndex = i - 90;
                        Debug.WriteLine("Längste Distanz Links: " + leftDistance + " Gradzahl zum links drehen: " + leftFurthestIndex);
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
                    // setzen rechts auf 1, um abzuspeichern, dass er sich vorher nach links gedreht hat
                    rechts = 1;
                    Debug.WriteLine("Dreht nach links zum " + leftFurthestIndex);
                    // wenn er sich vorher nach links gedreht hat, dreht er sich weiter nach links, um nicht in einen loop zu kommen
                } else if (rechts == 1){

                    _robot.TurnInDegrees(30);
                     Debug.WriteLine("Dreht nach links, hat sich gerade nach links gedreht, variable steht auf  " + rechts);
                    // sollte er sich vorher nach rechts gedreht haben, dreht er sich wieder nach rechts
                } else if (rechts == 2){

                    _robot.TurnInDegrees(-30);
                    Debug.WriteLine("Dreht nach links, hat sich gerade nach rechts gedreht, variable steht auf  " + rechts);
                }
                
            // when there are more obstacles on the left side, henry turns right
            } else if( right < left){

                if(rechts == 0){

                    _robot.TurnInDegrees(rightFurthestIndex);
                    rechts = 2;
                    Debug.WriteLine("Dreht nach rechts zum " + rightFurthestIndex);

                } else if (rechts == 1){

                    _robot.TurnInDegrees(30);
                     Debug.WriteLine("Dreht nach rechts, hat sich gerade nach links gedreht, variable steht auf  " + rechts);
                } else if (rechts == 2){

                    _robot.TurnInDegrees(-30);
                    Debug.WriteLine("Dreht nach rechts, hat sich gerade nach rechts gedreht, variable steht auf  " + rechts);
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
        //Method to check Logic, testlists with fake data are utilized. This method has a list as parameter
        //The above methods for testlists will be called first, so as to call the below method afterwards including the generated list as parameter
        public Boolean testCheck(List<int> testList)
        {
            Boolean drive = false;
            //Checking the lists values from index 100 to 200
            for (int i = 100; i <= 200; i++)
            {
                if(testList.Count == 0){
                    Debug.WriteLine(" Liste ist leer :(");
                    continue;
                }
                //Safety Threshold is bigger than the value in the testlist --> onstacle
                if (safety_threshold > testList[i])
                {
                    // sets stop
                    Debug.WriteLine(testList[i]);
                    Debug.WriteLine("drive == false");
                    drive = false;
                }
                //Safety Threshold is smaller or equal than the value in the testlist --> no onstacle
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

