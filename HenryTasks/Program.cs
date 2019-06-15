using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using HwrBerlin.Bot;
using HwrBerlin.Bot.Engines;
using static HwrBerlin.Bot.Engines.Robot;
using HwrBerlin.Bot.Scanner;
using System.Linq;


namespace HwrBerlin.HenryTasks


{
     internal class Program : AutoDriveProgramHT
    {
        private static Scanner _scanner;
        /// <summary>
        /// user can press keys for four directions
        /// </summary>
        private enum Direction
        {
            Up,
            Down,
            Right,
            Left
        }


        private readonly Robot _robot = new Robot();
        private readonly Arm _arm = new Arm();

        private static void Main()
        {
            var program = new Program();

            program.Start();
        }


        //method fpr printing an array list
        public void printArray<T>(IEnumerable<T> a)
        {
            foreach (var i in a)
            {
                Debug.WriteLine(i);
            }
        }

        /*
         * Scant den Bereich vor ihm
         * Gibt true zurück, wenn etwas vor ihm ist
         * Gibt false zurück, wenn nichts vor ihm ist
         */
        public Boolean scanForObstacles(int treshold, List<int> medianList)
        {

            for (int i = 100; i <= 200; i++)
            {
                // checks every degree right infront of henry (100° angle)
                // if treshold is greater than any degree distance henry stops
                if (treshold > medianList[i])
                {
                    // sets stop 
                    return true;

                }

            }

            return false;

        }


        private void CurrentMode(WalkMode walkMode, TurnMode turnMode)
        {
            switch (walkMode)
            {
                case WalkMode.FORWARDS_FAST:
                    Console.WriteLine("       ^       ");
                    Console.WriteLine("       |       ");
                    Console.WriteLine("       |       ");
                    Console.WriteLine("       |       ");
                    Console.WriteLine("       |       ");
                    Console.WriteLine("       |       ");
                    Console.WriteLine("       |       ");
                    break;
                case WalkMode.FORWARDS_MEDIUM:
                    Console.WriteLine("               ");
                    Console.WriteLine("               ");
                    Console.WriteLine("       ^       ");
                    Console.WriteLine("       |       ");
                    Console.WriteLine("       |       ");
                    Console.WriteLine("       |       ");
                    Console.WriteLine("       |       ");
                    break;
                case WalkMode.FORWARDS_SLOW:
                    Console.WriteLine("               ");
                    Console.WriteLine("               ");
                    Console.WriteLine("               ");
                    Console.WriteLine("               ");
                    Console.WriteLine("       ^       ");
                    Console.WriteLine("       |       ");
                    Console.WriteLine("       |       ");
                    break;
                case WalkMode.BACKWARDS:
                    break;
                case WalkMode.STOP:
                    break;
                default:
                    Console.WriteLine("               ");
                    Console.WriteLine("               ");
                    Console.WriteLine("               ");
                    Console.WriteLine("               ");
                    Console.WriteLine("               ");
                    Console.WriteLine("               ");
                    Console.WriteLine("               ");
                    break;
            }

            Console.WriteLine("      ███      ");

            switch (turnMode)
            {
                case TurnMode.LEFT_HARD:
                    Console.WriteLine(" <----███      ");
                    break;
                case TurnMode.LEFT_SMOOTH:
                    Console.WriteLine("   <--███      ");
                    break;
                case TurnMode.STRAIGHT:
                    Console.WriteLine("      ███      ");
                    break;
                case TurnMode.RIGHT_SMOOTH:
                    Console.WriteLine("      ███--> ");
                    break;
                case TurnMode.RIGHT_HARD:
                    Console.WriteLine("      ███----> ");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(turnMode), turnMode, null);
            }

            Console.WriteLine("      ███      ");

            switch (walkMode)
            {
                case WalkMode.BACKWARDS:
                    Console.WriteLine("       |       ");
                    Console.WriteLine("       |       ");
                    Console.WriteLine("       V       ");
                    break;
                case WalkMode.STOP:
                    break;
                case WalkMode.FORWARDS_SLOW:
                    break;
                case WalkMode.FORWARDS_MEDIUM:
                    break;
                case WalkMode.FORWARDS_FAST:
                    break;
                default:
                    Console.WriteLine("               ");
                    Console.WriteLine("               ");
                    Console.WriteLine("               ");
                    break;
            }
        }

        public void Start()
        {
            _robot.Enable();

            if (_robot != null && _robot.Enable())
            {
                var consoleInput = "";

                while (consoleInput != "exit")
                {
                    ConsoleFormatter.Custom(ConsoleColor.DarkGreen,
                        "'stop' - stop robot,   'exit' - terminate program",
                        "'disable' - disable robot,   'enable' - enable robot",
                        "'movecm <cm>',  'movev <v>'",
                        "'turndg <dg>'",
                        "'keys' - start arrow keys mode",
                        "'up' - arm gets higher",
                        "'down' - arm gets lower",
                        "'divide' - more space between grippers",
                        "'join' - less space between grippers",
                        "'task' - starts the task programm",
                        "'auto' - starts autodrive, henry drives forward and stops if a obstacle is in front of him and waits a given ammount of time",
                        "'testcorridor' - testing check2() Method, ",
                        "'auto3' - starts autodrive",
                        "'auto4' - starts autodrive",
                        "'testauto' - starts testauto, no scanning, only sample data");
                
                    consoleInput = Console.ReadLine();
                    consoleInput = consoleInput.Trim();
                    var inputArray = consoleInput.Split(' ');
                    switch (inputArray[0])
                    {   

                        /* Must-Haves:
                         * auto represents our must haves, these include that Henry stops if a obstacle is
                         * right infront of him and that he waits a certain amount of time. If the obstacle 
                         * disappeared Henry moves forward 
                        */
                        //
                        case "auto":

                            Auto1 instanceAuto_1 = new Auto1();
                            //Looping the method call infinitifely
                            int neverEnding = 1;
                            while (neverEnding == 1) {
                                //Calling the Method
                                instanceAuto_1.Decide();
                            }
                            break;
                        //case for testing the check2() Method in order to check the thresholdlist and comparison with medianlist
                        case "testcorridor":
                            Auto1 instanceAuto1_2 = new Auto1();
                            var thresholdlist = new List<double>();
                            thresholdlist = instanceAuto1_2.generateCorridorList();
                            int forever = 1;
                            while (forever==1){
                            instanceAuto1_2.Check2(thresholdlist);
                            }
                            break;

                         //Case for just executing the Check()-Method and its output
                        case "auto3":
                            Auto1 instanceAuto = new Auto1();
                            int loop_infinitely = 1;
                            while (loop_infinitely == 1) {
                                instanceAuto.Check();
                            }

                            break;
                            
                            //case for executing the output of the decide() Method in the class Auto1
                            case "auto4":
                            //new instance of class Auto1
                            Auto1 instanceAuto2 = new Auto1();
                            //Looping the method call infinitifely
                            int infinity = 1;
                            while (infinity == 1) {
                                //Calling the Method
                                instanceAuto2.Decide();
                            }
                            break;

                            //case for testing: utilizes the methods testListnoObstacle() , testListObstacle() , testCheck() from the class Auto1
                            case "testauto":
                            //create INstanz of class Auto1
                            Auto1 instanceAuto5 = new Auto1();
                            //initialize four lists
                            var list1= new List<int>();
                            var list2= new List<int>();
                            var list3= new List<int>();
                            var list4= new List<int>();
                            //set the initilized lists with the methods testListnoObstacle() and testListIbstacle()
                            //two lists with values > safety_threshold --> lists that do not contain obstacles
                            //two lists with values < safety_threshold --> lists that do contain obstacles
                            list1=instanceAuto5.testListnoObstacle();
                            list2=instanceAuto5.testListObstacle();
                            list3=instanceAuto5.testListnoObstacle();
                            list4=instanceAuto5.testListObstacle();
                            //Output for Debugger
                            Debug.WriteLine("Liste 1 Länge: "+ list1.Count());
                            Debug.WriteLine("Liste 2 Länge: "+ list2.Count());
                            Debug.WriteLine("Liste 3 Länge: "+ list3.Count());
                            Debug.WriteLine("Liste 4 Länge: "+ list4.Count());
                            //Output for Debugger
                            Debug.WriteLine("liste1: "+ list1.ToString());
                            Debug.WriteLine("liste2: "+ list2.ToString());
                            Debug.WriteLine("liste3: "+ list3.ToString());
                            Debug.WriteLine("liste4: "+ list4.ToString());
                            //Looping the program infititely, exit via closing the window
                            int nevereverEnding = 1;
                            while (nevereverEnding == 1) {

                                //Test runs through with 10 iterations for each for loop
                                //each for loop checks the allocated list through with the testCheck() - Method
                                //further output for debugging is directly coded within the testCheck()- Method
                                for (int i = 0; i < 10; i++)
                                {
                                    instanceAuto5.testCheck(list1);
                                }
                                for (int i = 0; i < 10; i++)
                                {
                                    instanceAuto5.testCheck(list2);
                                }
                                for (int i = 0; i < 10; i++)
                                {
                                    instanceAuto5.testCheck(list3);
                                }
                                for (int i = 0; i < 10; i++)
                                {
                                    instanceAuto5.testCheck(list4);
                                }  
                            }
                             break;

                        case "stop":
                            _robot.StopImmediately();
                            break;

                        case "enable":
                            _robot.Enable();
                            break;

                        case "disable":
                            _robot.Move(0);
                            _robot.Disable();
                            break;

                        case "exit":
                            _robot.Move(0);
                            _robot.Disable();
                            consoleInput = "exit"; // not needed
                            break;

                        case "movecm":
                            if (inputArray.Length > 1)
                            {
                                if (int.TryParse(inputArray[1], out var j))
                                {
                                    _robot.MoveInCm(j);
                                }
                                else
                                {
                                    ConsoleFormatter.Warning("Parameter is not valid");
                                }
                            }
                            else
                            {
                                ConsoleFormatter.Warning("Parameter expected");
                            }
                            break;

                        case "movev":
                            if (inputArray.Length > 1)
                            {
                                // if passed value is "1.5" the result will be j=1.5
                                // if passed value is "1,5" the result will be j=15
                                // because the system is written in english
                                if (double.TryParse(inputArray[1], NumberStyles.Number, CultureInfo.InvariantCulture, out var j))
                                {
                                    _robot.Move(j);
                                }
                                else
                                {
                                    ConsoleFormatter.Warning("Parameter is not valid");
                                }
                            }
                            else
                            {
                                ConsoleFormatter.Warning("Parameter expected");
                            }
                            break;

                        case "turndg":
                            if (inputArray.Length > 1)
                            {
                                if (int.TryParse(inputArray[1], out var j))
                                {
                                    _robot.TurnInDegrees(j);
                                }
                                else
                                {
                                    ConsoleFormatter.Warning("Parameter is not valid.");
                                }
                            }
                            else
                            {
                                ConsoleFormatter.Warning("Parameter expected");
                            }
                            break;

                        case "keys":
                            ConsoleFormatter.Custom(ConsoleColor.DarkGreen,
                                "'arrow keys' - direct the robot",
                                "'space' - slow down robot",
                                "'D' - disable robot",
                                "'E' - enable robot",
                                "'X' - terminate keys");
                            consoleInput = "";
                            var lastWalkMode = _robot.CurrentWalkMode;
                            var lastTurnMode = _robot.CurrentTurnMode;
                            Console.Clear();
                            CurrentMode(_robot.CurrentWalkMode, _robot.CurrentTurnMode);
                            while (consoleInput == "")
                            {
                                var c = Console.KeyAvailable ? Console.ReadKey(true).Key : ConsoleKey.Spacebar;
                                #region Keyswitch
                                switch (c)
                                {
                                    case ConsoleKey.UpArrow:
                                        ChangeWalkAndTurnMode(Direction.Up);
                                        break;
                                    case ConsoleKey.DownArrow:
                                        ChangeWalkAndTurnMode(Direction.Down);
                                        break;
                                    case ConsoleKey.RightArrow:
                                        ChangeWalkAndTurnMode(Direction.Right);
                                        break;
                                    case ConsoleKey.LeftArrow:
                                        ChangeWalkAndTurnMode(Direction.Left);
                                        break;
                                    case ConsoleKey.Enter:
                                        _robot.StopByMode();
                                        break;
                                    case ConsoleKey.D:
                                        _robot.Disable();
                                        break;
                                    case ConsoleKey.E:
                                        _robot.Enable();
                                        break;
                                    case ConsoleKey.X:
                                        consoleInput = "exit";
                                        ConsoleFormatter.Info("Terminated keys");
                                        break;
                                }
                                #endregion

                                if (_robot.CurrentWalkMode == lastWalkMode && _robot.CurrentTurnMode == lastTurnMode)
                                    continue;

                                lastWalkMode = _robot.CurrentWalkMode;
                                lastTurnMode = _robot.CurrentTurnMode;
                                Console.Clear();
                                CurrentMode(_robot.CurrentWalkMode, _robot.CurrentTurnMode);
                            }
                            consoleInput = "";
                            Console.BackgroundColor = ConsoleColor.Black;
                            break;

                        case "divide":
                            _arm.Divide();
                            break;

                        case "join":
                            _arm.Join();
                            break;

                        case "up":
                            _arm.Up();
                            break;

                        case "down":
                            _arm.Down();
                            break;

                        case "task":
                            PracticalTask2B();
                            break;
                    }

                    Console.Clear();
                }
            }
            else
            {
                ConsoleFormatter.Error(
                    "Try to enable the robot failed.",
                    "Press enter to close the application.");
                Console.ReadLine();
            }
        }

        /// <summary>
        /// handles state machines for WalkMode and TurnMode and calls moveByMode()
        /// </summary>
        /// <param name="direction">specified by arrow key that was pressed</param>
        /// <returns>returns always true</returns>
        private bool ChangeWalkAndTurnMode(Direction direction)
        {
            var currentWalkMode = _robot.CurrentWalkMode;
            var currentTurnMode = _robot.CurrentTurnMode;

            switch (direction)
            {
                //pressing up key switches from Backwards to STOP to FORWARDS_SLOW to FORWARDS_MEDIUM to FORWARDS_FAST
                //once reached FORWARDS_FAST, pressing up key again will not have any effect
                case Direction.Up:
                    switch (currentWalkMode)
                    {
                        case WalkMode.BACKWARDS:
                            currentWalkMode = WalkMode.STOP;
                            break;
                        case WalkMode.STOP:
                            currentWalkMode = WalkMode.FORWARDS_SLOW;
                            break;
                        case WalkMode.FORWARDS_SLOW:
                            currentWalkMode = WalkMode.FORWARDS_MEDIUM;
                            break;
                        case WalkMode.FORWARDS_MEDIUM:
                            currentWalkMode = WalkMode.FORWARDS_FAST;
                            break;
                        case WalkMode.FORWARDS_FAST:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;

                //pressing down key switches from FORWARDS_FAST to FORWARDS_MEDIUM to FORWARDS_SLOW to STOP to Backwards
                //once reached Backwards, pressing down key again will not have any effect
                case Direction.Down:
                    switch (currentWalkMode)
                    {
                        case WalkMode.FORWARDS_FAST:
                            currentWalkMode = WalkMode.FORWARDS_MEDIUM;
                            break;
                        case WalkMode.FORWARDS_MEDIUM:
                            currentWalkMode = WalkMode.FORWARDS_SLOW;
                            break;
                        case WalkMode.FORWARDS_SLOW:
                            currentWalkMode = WalkMode.STOP;
                            break;
                        case WalkMode.STOP:
                            currentWalkMode = WalkMode.BACKWARDS;
                            break;
                        case WalkMode.BACKWARDS:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;

                //pressing left key switches from RIGHT_HARD to RIGHT_SMOOTH to STRAIGHT to LEFT_SMOOTH to LEFT_HARD
                //once reached LEFT_HARD, pressing left key again will not have any effect
                case Direction.Left:
                    switch (currentTurnMode)
                    {
                        case TurnMode.RIGHT_HARD:
                            currentTurnMode = TurnMode.RIGHT_SMOOTH;
                            break;
                        case TurnMode.RIGHT_SMOOTH:
                            currentTurnMode = TurnMode.STRAIGHT;
                            break;
                        case TurnMode.STRAIGHT:
                            currentTurnMode = TurnMode.LEFT_SMOOTH;
                            break;
                        case TurnMode.LEFT_SMOOTH:
                            currentTurnMode = TurnMode.LEFT_HARD;
                            break;
                        case TurnMode.LEFT_HARD:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;

                //pressing right key switches from LEFT_HARD to LEFT_SMOOTH to STRAIGHT to RIGHT_SMOOTH to RIGHT_HARD
                //once reached RIGHT_HARD, pressing right key again will not have any effect
                case Direction.Right:
                    switch (currentTurnMode)
                    {
                        case TurnMode.LEFT_HARD:
                            currentTurnMode = TurnMode.LEFT_SMOOTH;
                            break;
                        case TurnMode.LEFT_SMOOTH:
                            currentTurnMode = TurnMode.STRAIGHT;
                            break;
                        case TurnMode.STRAIGHT:
                            currentTurnMode = TurnMode.RIGHT_SMOOTH;
                            break;
                        case TurnMode.RIGHT_SMOOTH:
                            currentTurnMode = TurnMode.RIGHT_HARD;
                            break;
                        case TurnMode.RIGHT_HARD:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
            
            _robot.MoveByMode(currentWalkMode, currentTurnMode, currentWalkMode == WalkMode.BACKWARDS);

            return true;
        }

        public void PracticalTask2A()
        {
            Console.Clear();
            Console.WriteLine("Roboter fährt zur Küche.");
            _robot.TurnInDegrees(-90);
            _robot.MoveInCm(200);
            _arm.Divide();

            Console.Clear();
            Console.WriteLine("Bitte halten Sie ein Glas Wasser in der Hand des Roboters und drücken ENTER, damit sich die Hand schließt.");
            Console.ReadLine();
            Console.Clear();
            Console.WriteLine("Schließe die Hand.");
            _arm.Join();
            Console.Clear();
            Console.WriteLine("Fahre zum Patienten");
            _robot.TurnInDegrees(-180);
            _robot.MoveInCm(200);
            _robot.TurnInDegrees(-90);
            _robot.MoveInCm(200);

            Console.Clear();
            Console.WriteLine("ENTER drücke, damit sich die Hand öffnet.");
            Console.ReadLine();
            Console.Clear();
            Console.WriteLine("Öffne Hand");
            _arm.Divide();
            Console.Clear();
            Console.WriteLine("Fahre zurück zum Warteplatz.");
            _robot.TurnInDegrees(-180);
            _robot.MoveInCm(200);
            _robot.TurnInDegrees(-180);
        }

        public void PracticalTask2B()
        {
            Console.Clear();
            Console.WriteLine("Roboter fährt zur Küche.");
            _robot.MoveInCm(100);
            _robot.TurnInDegrees(90);
            _robot.MoveInCm(200);
            _arm.Divide();

            Console.Clear();
            Console.WriteLine("Bitte halten Sie ein Glas Wasser in der Hand des Roboters und drücken ENTER, damit sich die Hand schließt.");
            Console.ReadLine();
            Console.Clear();
            Console.WriteLine("Schließe die Hand.");
            _arm.Join();
            Console.Clear();
            Console.WriteLine("Fahre zum Patienten");
            _robot.TurnInDegrees(-180);
            _robot.MoveInCm(400);

            Console.Clear();
            Console.WriteLine("ENTER drücke, damit sich die Hand öffnet.");
            Console.ReadLine();
            Console.Clear();
            Console.WriteLine("Öffne Hand");
            _arm.Divide();
            Console.Clear();
            Console.WriteLine("Fahre zurück zum Warteplatz.");
            _robot.TurnInDegrees(180);
            _robot.MoveInCm(200);
            _robot.TurnInDegrees(-90);
            _robot.MoveInCm(-100);
        }
    }
}
