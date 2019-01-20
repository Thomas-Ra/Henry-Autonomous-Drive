using System;
using System.IO;

namespace HwrBerlin.Bot.Engines
{
    /// <summary>
    /// provides function to control the arm
    /// </summary>
    public class Arm
    {
        /// <summary>
        /// Engine object is needed to use engine methods
        /// </summary>
        private readonly Engine _engine;

        /// <summary>
        /// armPosition is an object of ArmPos
        /// </summary>
        private ArmPos _armPosition;

        /// <summary>
        /// handPosition is an object of HandPos
        /// </summary>
        private HandPos _handPosition;

        /// <summary>
        /// Arm should not move up, when arm is already at highest position (TOP);
        /// Arm should not move down, when arm is already at lowest position (BOTTOM);
        /// Arm can move, when arm is between TOP and BOTTOM
        /// </summary>
        public enum ArmPos
        {
            Top,
            Between,
            Bottom
        }

        /// <summary>
        /// Grappers of hand should not join, when hand is already CLOSED;
        /// Grappers of hand should not divide, when hand is already OPEN;
        /// Grappers of hand can join or divide, when hand is between OPEN and CLOSED
        /// </summary>
        public enum HandPos
        {
            Open,
            Between,
            Closed
        }

        /// <summary>
        /// constructor for Arm;
        /// creates engine object, gets Arm- und HandPosition
        /// </summary>
        /// <exception cref="Exception">thrown, when connection to arm fails</exception>
        public Arm()
        {
            //connect to engine of type ARM
            _engine = new Engine(Engine.EngineType.Arm);

            //position of arm must be known before moving the first time
            GetArmPositionFromFile();

            //position of hand must be known before moving the first time
            GetHandPositionFromFile();
        }

        /// <summary>
        /// gets ArmPosition from file
        /// </summary>
        /// <returns>bool that indicates, if operation was successful</returns>
        private void GetArmPositionFromFile()
        {
            try
            {
                //get data from file, using StreamReader
                using (var sr = new StreamReader("arm_position.dat"))
                {
                    _armPosition = (ArmPos)Enum.Parse(typeof(ArmPos), sr.ReadToEnd());
                }
            }
            catch
            {
                ArmPosition = ArmPos.Between;
            }
        }

        /// <summary>
        /// gets HandPosition from file
        /// </summary>
        /// <returns>bool that indicates, if operation was successful</returns>
        private void GetHandPositionFromFile()
        {
            try
            {
                //get data from file, using StreamReader
                using (var sr = new StreamReader("hand_position.dat"))
                {
                    _handPosition = (HandPos)Enum.Parse(typeof(HandPos), sr.ReadToEnd());
                }
            }
            catch
            {
                HandPosition = HandPos.Between;
            }
        }

        /// <summary>
        /// getter and setter for ArmPosition; needed to indicate, if arm can move or has already reached limit
        /// </summary>
        private ArmPos ArmPosition
        {
            get => _armPosition;
            set
            {
                //set arm position to passed value
                _armPosition = value;

                //save data to file, using StreamWriter
                using (var file = new StreamWriter("arm_position.dat", false))
                {
                    file.WriteLine(_armPosition);
                }
            }
        }

        /// <summary>
        /// getter and setter for HandPosition; needed to indicate, if hand can move or has already reached limit
        /// </summary>
        private HandPos HandPosition
        {
            get => _handPosition;
            set
            {
                //set hand position to passed value
                _handPosition = value;
                //save data to file, using StreamWriter
                using (var file = new StreamWriter("hand_position.dat", false))
                {
                    file.WriteLine(_handPosition);
                }
            }
        }

        /// <summary>
        /// moves grappers of hand away from each other until enter is pressed or limit is reached
        /// </summary>
        /// <returns>bool that indicates, if operation was successful</returns>
        public bool Divide()
        {
            //if hand is not at limit and is able to divide
            if (HandPosition == HandPos.Open)
                return false;

            //motor needs to be enabled before being able to move
            _engine.Enable();

            //grappers should move
            //MoveToPosition works better than MoveWithVelocity here
            //MoveToPosition2 is needed as motor 2 is used for join and divide
            _engine.MoveToPosition2(2000);

            //position of hand has changed from OPEN to BETWEEN
            HandPosition = HandPos.Between;

            //stop is set false while loop should run
            var stop = false;

            //if engine has no disable or fault state and nobody has pressed a key to stop, motor should move
            //as motor 2 is operating, use states ending with ...2
            while (!_engine.DisableState2 && !_engine.FaultState2 && !stop)
            {
                //if any key was pressed
                if (!Console.KeyAvailable) continue;

                if (Console.ReadKey(true).Key == ConsoleKey.Enter)
                    //state of stop is true; while loop will not start again
                    stop = true;
            }

            //while loop is left when key was pressed or when limit is reached

            //if somebody pressed enter to stop hand movement
            if (stop)
                //motors are stopped
                _engine.StopImmediately();

            //if limit is reached
            else
                //hand position is open
                HandPosition = HandPos.Open;

            //divide() returns true if divide() was successful
            return true;
        }

        /// <summary>
        /// moves grappers of hand towards each other until enter is pressed or limit is reached
        /// </summary>
        /// <returns>bool that indicates, if operation was successful</returns>
        public bool Join()
        {
            //if hand is not at limit and is able to divide
            if (HandPosition == HandPos.Closed)
                return false;

            //motor needs to be enabled before being able to move
            _engine.Enable();

            //grappers should move
            //MoveToPosition works better than MoveWithVelocity here
            _engine.MoveToPosition2(-2000);

            //position of hand has changed from OPEN to BETWEEN
            HandPosition = HandPos.Between;

            //stop is set false while loop should run
            var stop = false;

            //if engine has no disable or fault state and nobody has pressed a key to stop, motor should move
            //as motor 2 is operating, use states ending with ...2
            while (!_engine.DisableState2 && !_engine.FaultState2 && !stop)
            {
                //if any key was pressed
                if (!Console.KeyAvailable) continue;

                if (Console.ReadKey(true).Key == ConsoleKey.Enter)
                    //state of stop is true; while loop will not start again
                    stop = true;
            }

            //while loop is left when key was pressed or when limit is reached

            //if somebody pressed enter to stop hand movement
            if (stop)
                //motors are stopped
                _engine.StopImmediately();
            else
                //hand position is closed
                HandPosition = HandPos.Closed;

            //join() returns true if join() was successful
            return true;
        }



        /// <summary>
        /// moves arm upwards until enter is pressed or limit is reached
        /// </summary>
        /// <returns>bool that indicates, if operation was successful</returns>
        public bool Up()
        {
            //if arm is not at limit and is able to raise
            if (ArmPosition == ArmPos.Top)
                return false;
            
            //motor needs to be enabled before being able to move
            _engine.Enable();

            //arm should move
            //MoveToPosition works better than MoveWithVelocity here
            _engine.MoveToPosition1(-2000);

            //position of arm has changed from TOP to BETWEEN
            ArmPosition = ArmPos.Between;

            //stop is set false while loop should run
            var stop = false;

            //if engine has no disable or fault state and nobody has pressed a key to stop, motor should move
            //as motor 1 is operating, use states ending with ...1
            while (!_engine.DisableState1 && !_engine.FaultState1 && !stop)
            {
                //if any key was pressed
                if (!Console.KeyAvailable) continue;

                if (Console.ReadKey(true).Key == ConsoleKey.Enter)
                    //state of stop is true; while loop will not start again
                    stop = true;
            }

            //while loop is left when key was pressed or when limit is reached

            //if somebody pressed enter to stop arm movement
            if (stop)
                //motors are stopped
                _engine.StopImmediately();
            else
                //arm position is top
                ArmPosition = ArmPos.Top;
            
            //join() returns true if join() was successful
            return true;
        }

        /// <summary>
        /// moves arm downwards until enter is pressed or limit is reached
        /// </summary>
        /// <returns>bool that indicates, if operation was successful</returns>
        public bool Down()
        {
            //if arm is not at limit and is able to descent
            if (ArmPosition == ArmPos.Bottom)
                return false;
            
            //motor needs to be enabled before being able to move
            _engine.Enable();

            //arm should move
            //MoveToPosition works better than MoveWithVelocity here
            _engine.MoveToPosition1(2000);

            //position of arm has changed from BOTTOM to BETWEEN
            ArmPosition = ArmPos.Between;

            //stop is set false while loop should run
            var stop = false;

            //if engine has no disable or fault state and nobody has pressed a key to stop, motor should move
            //as motor 1 is operating, use states ending with ...1
            while (!_engine.DisableState1 && !_engine.FaultState1 && !stop)
            {
                //if any key was pressed
                if (!Console.KeyAvailable) continue;

                if (Console.ReadKey(true).Key == ConsoleKey.Enter)
                    //state of stop is true; while loop will not start again
                    stop = true;
            }

            //while loop is left when key was pressed or when limit is reached

            //if somebody pressed enter to stop arm movement
            if (stop)
                //motors are stopped
                _engine.StopImmediately();
            else
                //arm position is bottom
                ArmPosition = ArmPos.Bottom;
            
            //join() returns true if join() was successful
            return true;
        }
    }
}
