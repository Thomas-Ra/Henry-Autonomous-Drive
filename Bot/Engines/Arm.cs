using System;
using System.IO;

namespace HwrBerlin.Bot.Engines
{
    public class Arm
    {
        /// <summary>
        /// Engine object is needed to use engine methods
        /// </summary>
        private Engine engine;

        /// <summary>
        /// armPosition is an object of ArmPos
        /// </summary>
        private ArmPos armPosition;

        /// <summary>
        /// handPosition is an object of HandPos
        /// </summary>
        private HandPos handPosition;

        /// <summary>
        /// predefined velocity for arm; can also be slower
        /// </summary>
        private const int VELOCITY_ARM = 120000;

        /// <summary>
        /// predefined velocity for hand; can also be slower
        /// </summary>
        private const int VELOCITY_HAND = 20000;


        /// <summary>
        /// Arm should not move up, when arm is already at highest position (TOP);
        /// Arm should not move down, when arm is already at lowest position (BOTTOM);
        /// Arm can move, when arm is between TOP and BOTTOM
        /// </summary>
        public enum ArmPos
        {
            TOP,
            BETWEEN,
            BOTTOM
        }

        /// <summary>
        /// Grappers of hand should not join, when hand is already CLOSED;
        /// Grappers of hand should not divide, when hand is already OPEN;
        /// Grappers of hand can join or divide, when hand is between OPEN and CLOSED
        /// </summary>
        public enum HandPos
        {
            OPEN,
            BETWEEN,
            CLOSED
        }

        /// <summary>
        /// constructor for Arm;
        /// creates engine object, gets Arm- und HandPosition
        /// </summary>
        /// <exception cref="Exception">thrown, when connection to arm fails</exception>
        public Arm()
        {
            //connect to engine of type ARM
            engine = new Engine(Engine.EngineType.ARM);

            //position of arm must be known before moving the first time
            GetArmPositionFromFile();

            //position of hand must be known before moving the first time
            GetHandPositionFromFile();
        }

        /// <summary>
        /// gets ArmPosition from file
        /// </summary>
        /// <returns>bool that indicates, if operation was successfull</returns>
        private bool GetArmPositionFromFile()
        {
            try
            {
                //get data from file, using StreamReader
                using (StreamReader sr = new StreamReader("arm_position.dat"))
                {
                    armPosition = (ArmPos)Enum.Parse(typeof(ArmPos), sr.ReadToEnd());
                }
                //returns true, if getArmPositionFromFile() was successfull
                return true;
            }
            catch
            {
                ArmPosition = ArmPos.BETWEEN;
            }
            //returns false, if getArmPositionFromFile() fails
            return false;
        }

        /// <summary>
        /// gets HandPosition from file
        /// </summary>
        /// <returns>bool that indicates, if operation was successfull</returns>
        private bool GetHandPositionFromFile()
        {
            try
            {
                //get data from file, using StreamReader
                using (StreamReader sr = new StreamReader("hand_position.dat"))
                {
                    handPosition = (HandPos)Enum.Parse(typeof(HandPos), sr.ReadToEnd());
                }
                //returns true, if getHandPositionFromFile() was successfull
                return true;
            }
            catch
            {
                HandPosition = HandPos.BETWEEN;
            }
            //returns false, if getHandPositionFromFile() fails
            return false;
        }

        /// <summary>
        /// getter and setter for ArmPosition; needed to indicate, if arm can move or has already reached limit
        /// </summary>
        private ArmPos ArmPosition
        {
            get { return armPosition; }
            set
            {
                //set arm position to passed value
                armPosition = value;

                //save data to file, using StreamWriter
                using (StreamWriter file = new StreamWriter("arm_position.dat", false))
                {
                    file.WriteLine(armPosition);
                }
            }
        }

        /// <summary>
        /// getter and setter for HandPosition; needed to indicate, if hand can move or has already reached limit
        /// </summary>
        private HandPos HandPosition
        {
            get { return handPosition; }
            set
            {
                //set hand position to passed value
                handPosition = value;
                //save data to file, using StreamWriter
                using (StreamWriter file = new StreamWriter("hand_position.dat", false))
                {
                    file.WriteLine(handPosition);
                }
            }
        }

        /// <summary>
        /// moves grappers of hand away from each other until enter is pressed or limit is reached
        /// </summary>
        /// <returns>bool that indicates, if operation was successfull</returns>
        public bool Divide()
        {
            //result is false by default; result is used as return statement
            bool result = false;

            //if hand is not at limit and is able to divide
            if (HandPosition != HandPos.OPEN)
            {
                //motor needs to be enabled before being able to move
                engine.Enable();

                //grappers should move
                //MoveToPosition works better than MoveWithVelocity here
                //MoveToPosition2 is needed as motor 2 is used for join and divide
                engine.MoveToPosition2(2000);

                //position of hand has changed from OPEN to BETWEEN
                HandPosition = HandPos.BETWEEN;

                //stop is set false while loop should run
                bool stop = false;

                //if engine has no disable or fault state and nobody has pressed a key to stop, motor should move
                //as motor 2 is operating, use states ending with ...2
                while (!engine.DisableState2 && !engine.FaultState2 && !stop)
                {
                    //if any key was pressed
                    if (Console.KeyAvailable)

                        //if pressed key was enter
                        if (Console.ReadKey(true).Key == ConsoleKey.Enter)
                            //state of stop is true; while loop will not start again
                            stop = true;
                }

                //while loop is left when key was pressed or when limit is reached

                //if somebody pressed enter to stop hand movement
                if (stop)
                    //motors are stopped
                    engine.StopImmediately();

                //if limit is reached
                else
                    //hand position is open
                    HandPosition = HandPos.OPEN;

                //process passed successfully
                result = true;
            }

            //divide() returns true if divide() was successfull
            return result;
        }

        /// <summary>
        /// moves grappers of hand towards each other until enter is pressed or limit is reached
        /// </summary>
        /// <returns>bool that indicates, if operation was successfull</returns>
        public bool Join()
        {
            //result is false by default; result is used as return statement
            bool result = false;

            //if hand is not at limit and is able to divide
            if (HandPosition != HandPos.CLOSED)
            {
                //motor needs to be enabled before being able to move
                engine.Enable();

                //grappers should move
                //MoveToPosition works better than MoveWithVelocity here
                engine.MoveToPosition2(-2000);

                //position of hand has changed from OPEN to BETWEEN
                HandPosition = HandPos.BETWEEN;

                //stop is set false while loop should run
                bool stop = false;

                //if engine has no disable or fault state and nobody has pressed a key to stop, motor should move
                //as motor 2 is operating, use states ending with ...2
                while (!engine.DisableState2 && !engine.FaultState2 && !stop)
                {
                    //listenForEnterPressed();

                    //if any key was pressed
                    if (Console.KeyAvailable)

                        //if pressed key was enter
                        if (Console.ReadKey(true).Key == ConsoleKey.Enter)
                            //state of stop is true; while loop will not start again
                            stop = true;
                }

                //while loop is left when key was pressed or when limit is reached

                //if somebody pressed enter to stop hand movement
                if (stop)
                    //motors are stopped
                    engine.StopImmediately();
                else
                    //hand position is closed
                    HandPosition = HandPos.CLOSED;

                //process passed successfully
                result = true;
            }
            //join() returns true if join() was successfull
            return result;
        }



        /// <summary>
        /// moves arm upwards until enter is pressed or limit is reached
        /// </summary>
        /// <returns>bool that indicates, if operation was successfull</returns>
        public bool Up()
        {
            //result is false by default; result is used as return statement
            bool result = false;

            //if arm is not at limit and is able to raise
            if (ArmPosition != ArmPos.TOP)
            {
                //motor needs to be enabled before being able to move
                engine.Enable();

                //arm should move
                //MoveToPosition works better than MoveWithVelocity here
                engine.MoveToPosition1(-2000);

                //position of arm has changed from TOP to BETWEEN
                ArmPosition = ArmPos.BETWEEN;

                //stop is set false while loop should run
                bool stop = false;

                //if engine has no disable or fault state and nobody has pressed a key to stop, motor should move
                //as motor 1 is operating, use states ending with ...1
                while (!engine.DisableState1 && !engine.FaultState1 && !stop)
                {
                    //if any key was pressed
                    if (Console.KeyAvailable)

                        //if pressed key was enter
                        if (Console.ReadKey(true).Key == ConsoleKey.Enter)
                            //state of stop is true; while loop will not start again
                            stop = true;
                }

                //while loop is left when key was pressed or when limit is reached

                //if somebody pressed enter to stop arm movement
                if (stop)
                    //motors are stopped
                    engine.StopImmediately();
                else
                    //arm position is top
                    ArmPosition = ArmPos.TOP;

                //process passed successfully
                result = true;
            }
            //join() returns true if join() was successfull
            return result;
        }

        /// <summary>
        /// moves arm downwards until enter is pressed or limit is reached
        /// </summary>
        /// <returns>bool that indicates, if operation was successfull</returns>
        public bool Down()
        {
            //result is false by default; result is used as return statement
            bool result = false;

            //if arm is not at limit and is able to descent
            if (ArmPosition != ArmPos.BOTTOM)
            {
                //motor needs to be enabled before being able to move
                engine.Enable();

                //arm should move
                //MoveToPosition works better than MoveWithVelocity here
                engine.MoveToPosition1(2000);

                //position of arm has changed from BOTTOM to BETWEEN
                ArmPosition = ArmPos.BETWEEN;

                //stop is set false while loop should run
                bool stop = false;

                //if engine has no disable or fault state and nobody has pressed a key to stop, motor should move
                //as motor 1 is operating, use states ending with ...1
                while (!engine.DisableState1 && !engine.FaultState1 && !stop)
                {
                    //if any key was pressed
                    if (Console.KeyAvailable)

                        //if pressed key was enter
                        if (Console.ReadKey(true).Key == ConsoleKey.Enter)
                            //state of stop is true; while loop will not start again
                            stop = true;
                }

                //while loop is left when key was pressed or when limit is reached

                //if somebody pressed enter to stop arm movement
                if (stop)
                    //motors are stopped
                    engine.StopImmediately();
                else
                    //arm position is bottom
                    ArmPosition = ArmPos.BOTTOM;

                //process passed successfully
                result = true;
            }
            //join() returns true if join() was successfull
            return result;
        }
    }
}
