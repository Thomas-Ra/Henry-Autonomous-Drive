using System;

namespace HwrBerlin.Bot.Engines
{
    class Robot
    {
        /// <summary>
        /// robot can move forwards with three different velocities, stop and move backwards
        /// </summary>
        public enum WalkMode
        {
            BACKWARDS = -1,
            STOP = 0,
            FORWARDS_SLOW = 1,
            FORWARDS_MEDIUM = 2,
            FORWARDS_FAST = 3
        }

        /// <summary>
        /// robot can turn left and right in two intensities or move straight
        /// </summary>
        public enum TurnMode
        {
            LEFT_HARD = -2,
            LEFT_SMOOTH = -1,
            STRAIGHT = 0,
            RIGHT_SMOOTH = 1,
            RIGHT_HARD = 2
        }

        /// <summary>
        /// Engine object is needed to use engine methods
        /// </summary>
        private Engine engine;

        /// <summary>
        /// Walkmode object is needed to set speed and direction of movement
        /// </summary>
        private WalkMode currentWalkMode;

        /// <summary>
        /// TurnMode object is needed to set intensity of lateral movement
        /// </summary>
        private TurnMode currentTurnMode;

        /// <summary>
        /// constructor for robot;
        /// creates engine object; sets walkMode and turnMode to default (no movement)
        /// </summary>
        public Robot()
        {
            engine = new Engine(Engine.EngineType.ROBOT);
            currentWalkMode = WalkMode.STOP;
            currentTurnMode = TurnMode.STRAIGHT;
        }

        /// <summary>
        /// CurrentWalkMode defines the walk mode the robot has
        /// </summary>
        public WalkMode CurrentWalkMode
        {
            get { return currentWalkMode; }
        }

        /// <summary>
        /// CurrentTurnMode defines the turn mode the robot has
        /// </summary>
        public TurnMode CurrentTurnMode
        {
            get { return currentTurnMode; }
        }

        /// <summary>
        /// uses method from engine object
        /// </summary>
        /// <returns> <see cref="Engine.Enable()"/></returns>
        public bool Enable()
        {
            return engine.Enable();
        }

        /// <summary>
        /// uses method from engine object
        /// </summary>
        /// <returns> <see cref="Engine.Disable()"/></returns>
        public void Disable()
        {
            engine.Disable();
        }

        /// <summary>
        /// uses method from engine object
        /// </summary>
        /// <returns> <see cref="Engine.StopImmediately()"/></returns>
        public void StopImmediately()
        {
            engine.StopImmediately();
        }

        /// <summary>
        /// converts argument from double to int to use method from engine object
        /// </summary>
        /// <param name="velocity"></param>
        /// <returns><see cref="Engine.MoveWithVelocity()"/></returns>
        public bool Move(double velocity)
        {
            Int32 vel = (int)Math.Round((velocity * 1000000), 0);

            return engine.MoveWithVelocity(vel, vel);
        }

        /// <summary>
        /// converts arguments from double to int to use method from engine object
        /// </summary>
        /// <param name="velocity1">velocity for left motor epos1</param>
        /// <param name="velocity2">velocity for right motor epos2</param>
        /// <returns><see cref="Engine.MoveWithVelocity()"/></returns>
        public bool Move(double velocity1, double velocity2)
        {
            Int32 vel1 = (int)Math.Round((velocity1 * 1000000), 0);
            Int32 vel2 = (int)Math.Round((velocity2 * 1000000), 0);

            return engine.MoveWithVelocity(vel1, vel2);
        }

        /// <summary>
        /// multiplies argument (distance in cm) with -2000 to make robot move given distance
        /// </summary>
        /// <param name="distance">distance in cm for wheels</param>
        public void MoveInCm(int distance)
        {
            engine.MoveToPosition(distance * -2000);

            //robot should move until at least one motor has a negative MovementState
            while (!engine.MovementState1 || !engine.MovementState2) { }
        }



        /// <summary>
        /// makes robot turn the amount of degrees, passed by call
        /// </summary>
        /// <param name="degree">degrees that the robot should turn - negative value: turn right</param>
        public void TurnInDegrees(int degree)
        {
            //both wheels turn, one forwards, one backwards
            engine.MoveToPosition(degree * 700, degree * -700);

            //robot should move until at least one motor has a negative MovementState
            while (!engine.MovementState1 || !engine.MovementState2) { }
        }



        /// <summary>
        /// stop movement by pressing enter when navigating with arrow keys
        /// </summary>
        /// <returns>returns always true</returns>
        public bool StopByMode()
        {
            currentWalkMode = WalkMode.STOP;
            currentTurnMode = TurnMode.STRAIGHT;

            //calling moveByMode with WalkMode.STOP and TurnMode.STRAIGHT makes robot stop moving
            MoveByMode(currentWalkMode, currentTurnMode);

            return true;
        }

        /// <summary>
        /// makes robot move into specified direction; enter stops process
        /// </summary>
        /// <param name="walkMode">specifies speed and backwards/forwards</param>
        /// <param name="turnMode">specifies intensity of lateral movement</param>
        /// <param name="reverseMode">true for driving backwards</param>
        /// <returns>returns always true</returns>
        private bool MoveByMode(WalkMode walkMode, TurnMode turnMode, bool reverseMode = false)
        {
            //velocity for left wheel
            double vel1 = (int)walkMode;
            //velocity for right wheel
            double vel2 = (int)walkMode;

            //if driving backwards
            if (reverseMode)
            {
                //both wheels get same velocity first; if driving to right side, right wheel should turn faster, left one slower
                vel1 = vel1 - (((double)turnMode) / 2);
                vel2 = vel2 + (((double)turnMode) / 2);
            }
            //if driving forwards
            else
            {
                //both wheels get same velocity first; if driving to right side, left wheel should turn faster, right one slower
                vel1 = vel1 + (((double)turnMode) / 2);
                vel2 = vel2 - (((double)turnMode) / 2);
            }

            //call Move() with calculated values
            Move(vel1, vel2);

            return true;
        }
    }
}
