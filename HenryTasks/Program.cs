using System;
using System.Globalization;
using HwrBerlin.Bot;
using HwrBerlin.Bot.Engines;
using static HwrBerlin.Bot.Engines.Robot;

namespace HwrBerlin.HenryTasks
{
    class Program
    {
        /// <summary>
        /// user can press keys for four directions
        /// </summary>
        private enum Direction
        {
            UP,
            DOWN,
            RIGHT,
            LEFT
        }


        private Robot robot = new Robot();
        private Arm arm = new Arm();

        static void Main(string[] args)
        {
            Program program = new Program();

            program.Start();
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
            }

            Console.WriteLine("      ███      ");

            switch (walkMode)
            {
                case WalkMode.BACKWARDS:
                    Console.WriteLine("       |       ");
                    Console.WriteLine("       |       ");
                    Console.WriteLine("       V       ");
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
            if (robot.Enable())
            {
                string consoleInput = "";

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
                        "'task' - starts the task programm");
                    consoleInput = Console.ReadLine();
                    consoleInput = consoleInput.Trim();
                    String[] inpurArray = consoleInput.Split(' ');
                    switch (inpurArray[0])
                    {
                        case "stop":
                            {
                                robot.StopImmediately();
                                break;
                            }
                        case "enable":
                            {
                                robot.Enable();
                                break;
                            }
                        case "disable":
                            {
                                robot.Move(0);
                                robot.Disable();
                                break;
                            }
                        case "exit":
                            {
                                robot.Move(0);
                                robot.Disable();
                                consoleInput = "exit"; // not needed
                                break;
                            }
                        case "movecm":
                            {
                                if (inpurArray.Length > 1)
                                {
                                    if (Int32.TryParse(inpurArray[1], out int j))
                                    {
                                        robot.MoveInCm(j);
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
                            }
                        case "movev":
                            {
                                if (inpurArray.Length > 1)
                                {
                                    // if passed value is "1.5" the result will be j=1.5
                                    // if passed value is "1,5" the result will be j=15
                                    // because the system is written in english
                                    if (Double.TryParse(inpurArray[1], NumberStyles.Number, CultureInfo.InvariantCulture, out double j))
                                    {
                                        robot.Move(j);
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
                            }
                        case "turndg":
                            {
                                if (inpurArray.Length > 1)
                                {
                                    if (Int32.TryParse(inpurArray[1], out int j))
                                    {
                                        robot.TurnInDegrees(j);
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
                            }
                        case "keys":
                            {
                                ConsoleFormatter.Custom(ConsoleColor.DarkGreen,
                                    "'arrow keys' - direct the robot",
                                    "'space' - slow down robot",
                                    "'D' - disable robot",
                                    "'E' - enable robot",
                                    "'X' - terminate keys");
                                consoleInput = "";
                                WalkMode lastWalkMode = robot.CurrentWalkMode;
                                TurnMode lastTurnMode = robot.CurrentTurnMode;
                                Console.Clear();
                                CurrentMode(robot.CurrentWalkMode, robot.CurrentTurnMode);
                                while (consoleInput == "")
                                {
                                    ConsoleKey c;

                                    if (Console.KeyAvailable)
                                    {
                                        c = Console.ReadKey(true).Key;
                                    }
                                    else
                                    {
                                        c = ConsoleKey.Spacebar;
                                    }
                                    #region Keyswitch
                                    switch (c)
                                    {
                                        case ConsoleKey.UpArrow:
                                            ChangeWalkAndTurnMode(Direction.UP);
                                            break;
                                        case ConsoleKey.DownArrow:
                                            ChangeWalkAndTurnMode(Direction.DOWN);
                                            break;
                                        case ConsoleKey.RightArrow:
                                            ChangeWalkAndTurnMode(Direction.RIGHT);
                                            break;
                                        case ConsoleKey.LeftArrow:
                                            ChangeWalkAndTurnMode(Direction.LEFT);
                                            break;
                                        case ConsoleKey.Enter:
                                            robot.StopByMode();
                                            break;
                                        case ConsoleKey.D:
                                            robot.Disable();
                                            break;
                                        case ConsoleKey.E:
                                            robot.Enable();
                                            break;
                                        case ConsoleKey.X:
                                            consoleInput = "exit";
                                            ConsoleFormatter.Info("Terminated keys");
                                            break;
                                    }
                                    #endregion

                                    if (robot.CurrentWalkMode != lastWalkMode || robot.CurrentTurnMode != lastTurnMode)
                                    {
                                        lastWalkMode = robot.CurrentWalkMode;
                                        lastTurnMode = robot.CurrentTurnMode;
                                        Console.Clear();
                                        CurrentMode(robot.CurrentWalkMode, robot.CurrentTurnMode);
                                    }
                                }

                                consoleInput = "";
                                Console.BackgroundColor = ConsoleColor.Black;
                                break;
                            }

                        case "divide":
                            {
                                arm.Divide();
                                break;
                            }
                        case "join":
                            {
                                arm.Join();
                                break;
                            }

                        case "up":
                            {
                                arm.Up();
                                break;
                            }

                        case "down":
                            {
                                arm.Down();
                                break;
                            }
                        case "task":
                            {
                                PracticalTask2b();
                                break;
                            }
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
        /// handles state machines for WalkMode and TurnMode and calles moveByMode()
        /// </summary>
        /// <param name="direction">specified by arrow key that was pressed</param>
        /// <returns>returns always true</returns>
        private bool ChangeWalkAndTurnMode(Direction direction)
        {
            WalkMode currentWalkMode = robot.CurrentWalkMode;
            TurnMode currentTurnMode = robot.CurrentTurnMode;

            switch (direction)
            {
                //pressing up key switches from BACKWARDS to STOP to FORWARDS_SLOW to FORWARDS_MEDIUM to FORWARDS_FAST
                //once reached FORWARDS_FAST, pressing up key again will not have any effect
                case Direction.UP:
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
                    }
                    break;

                //pressing down key switches from FORWARDS_FAST to FORWARDS_MEDIUM to FORWARDS_SLOW to STOP to BACKWARDS
                //once reached BACKWARDS, pressing down key again will not have any effect
                case Direction.DOWN:
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
                    }
                    break;

                //pressing left key switches from RIGHT_HARD to RIGHT_SMOOTH to STRAIGHT to LEFT_SMOOTH to LEFT_HARD
                //once reached LEFT_HARD, pressing left key again will not have any effect
                case Direction.LEFT:
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
                    }
                    break;

                //pressing right key switches from LEFT_HARD to LEFT_SMOOTH to STRAIGHT to RIGHT_SMOOTH to RIGHT_HARD
                //once reached RIGHT_HARD, pressing right key again will not have any effect
                case Direction.RIGHT:
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
                    }
                    break;
            }


            if (currentWalkMode == WalkMode.BACKWARDS)
            {
                //WalkMode.BACKWARDS needs reverse lateral movement calculation
                robot.MoveByMode(currentWalkMode, currentTurnMode, true);
            }
            else
            {
                robot.MoveByMode(currentWalkMode, currentTurnMode, false);
            }


            return true;
        }

        public void PracticalTask2a()
        {
            Console.Clear();
            Console.WriteLine("Roboter fährt zur Küche.");
            robot.TurnInDegrees(-90);
            robot.MoveInCm(200);
            arm.Divide();

            Console.Clear();
            Console.WriteLine("Bitte halten Sie ein Glas Wasser in der Hand des Roboters und drücken ENTER, damit sich die Hand schließt.");
            Console.ReadLine();
            Console.Clear();
            Console.WriteLine("Schließe die Hand.");
            arm.Join();
            Console.Clear();
            Console.WriteLine("Fahre zum Patienten");
            robot.TurnInDegrees(-180);
            robot.MoveInCm(200);
            robot.TurnInDegrees(-90);
            robot.MoveInCm(200);

            Console.Clear();
            Console.WriteLine("ENTER drücke, damit sich die Hand öffnet.");
            Console.ReadLine();
            Console.Clear();
            Console.WriteLine("Öffne Hand");
            arm.Divide();
            Console.Clear();
            Console.WriteLine("Fahre zurück zum Warteplatz.");
            robot.TurnInDegrees(-180);
            robot.MoveInCm(200);
            robot.TurnInDegrees(-180);
        }

        public void PracticalTask2b()
        {
            Console.Clear();
            Console.WriteLine("Roboter fährt zur Küche.");
            robot.MoveInCm(100);
            robot.TurnInDegrees(90);
            robot.MoveInCm(200);
            arm.Divide();

            Console.Clear();
            Console.WriteLine("Bitte halten Sie ein Glas Wasser in der Hand des Roboters und drücken ENTER, damit sich die Hand schließt.");
            Console.ReadLine();
            Console.Clear();
            Console.WriteLine("Schließe die Hand.");
            arm.Join();
            Console.Clear();
            Console.WriteLine("Fahre zum Patienten");
            robot.TurnInDegrees(-180);
            robot.MoveInCm(400);

            Console.Clear();
            Console.WriteLine("ENTER drücke, damit sich die Hand öffnet.");
            Console.ReadLine();
            Console.Clear();
            Console.WriteLine("Öffne Hand");
            arm.Divide();
            Console.Clear();
            Console.WriteLine("Fahre zurück zum Warteplatz.");
            robot.TurnInDegrees(180);
            robot.MoveInCm(200);
            robot.TurnInDegrees(-90);
            robot.MoveInCm(-100);
        }
    }
}
