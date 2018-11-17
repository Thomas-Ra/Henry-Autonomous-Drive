using EposCmd.Net;
using EposCmd.Net.DeviceCmdSet.Operation;
using System;

namespace HwrBerlin.Bot.Engines
{
    public class Engine
    {
        /// <summary>
        /// DeviceManager for USB communication
        /// </summary>
        private DeviceManager connector;

        //Two engines
        /// <summary>
        /// Device comes from library;
        /// use Device to configurate motors
        /// </summary>
        private Device epos1;

        /// <summary>
        /// Device comes from library;
        /// use Device to configurate motors
        /// </summary>
        private Device epos2;

        //Two modes each engine
        /// <summary>
        /// motor 1 operates in <see cref="ProfilePositionMode"/>
        /// </summary>
        private ProfilePositionMode ppm1;

        /// <summary>
        /// motor 2 operates in <see cref="ProfilePositionMode"/>
        /// </summary>
        private ProfilePositionMode ppm2;

        /// <summary>
        /// motor 1 operates in <see cref="ProfileVelocityMode"/>
        /// </summary>
        private ProfileVelocityMode pvm1;

        /// <summary>
        /// motor 2 operates in <see cref="ProfileVelocityMode"/>
        /// </summary>
        private ProfileVelocityMode pvm2;

        //State Machines
        /// <summary>
        /// handels states of motor 1 to see, when motor is in fault state, enabled or disabled
        /// </summary>
        private StateMachine sm1;
        /// <summary>
        /// handels states of motor 2 to see, when motor is in fault state, enabled or disabled
        /// </summary>
        private StateMachine sm2;

        //Motion Info
        /// <summary>
        /// object from library to get information about MotionState of motor 1
        /// </summary>
        private MotionInfo mi1;
        /// <summary>
        /// object from library to get information about MotionState of motor 2
        /// </summary>
        private MotionInfo mi2;

        /// <summary>
        /// Maximal velocity is used as upper limit for every engine
        /// Maximal velocity multiplied with -1 is used as lower limit
        /// </summary>
        private const int MAX_VELOCITY = 12000000 - 1;

        /// <summary>
        /// deceleration needed for SetPositionProfile
        /// </summary>
        private readonly uint profileDeceleration;

        /// <summary>
        /// acceleration needed for SetPositionProfile
        /// </summary>
        private readonly uint profileAcceleration;

        /// <summary>
        /// object of EngineMode
        /// </summary>
        private EngineMode engineMode;


        /// <summary>
        /// selects wheter robot or arm should be used
        /// </summary>
        public enum EngineType
        {
            ROBOT,
            ARM
        }

        /// <summary>
        /// motors can be moved with a given velocity or to a given position;
        /// select EngineMode depending on the way the motor should move (with velocity or to position)
        /// </summary>
        public enum EngineMode
        {
            VELOCITY,
            POSITION
        }


        /// <summary>
        /// constructor for Engine
        /// </summary>
        /// <param name="engineType">
        /// select robot to drive;
        /// select arm to move arm
        /// </param>
        /// <exception cref="DeviceException">thrown, when USB connection is lost</exception>
        /// <exception cref="Exception">thrown, when a process in constructor fails</exception>
        public Engine(EngineType engineType)
        {
            try
            {
                switch (engineType)
                {
                    case EngineType.ROBOT:
                        //connect to motor to drive
                        connector = new DeviceManager("EPOS2", "MAXON SERIAL V2", "USB", "USB0");
                        //motor to drive needs acceleration and deceleration
                        profileAcceleration = 3000;
                        ProfileDeceleration = 3000;
                        break;
                    case EngineType.ARM:
                        //connect to motor to move arm
                        connector = new DeviceManager("EPOS", "MAXON_RS232", "RS232", "COM4");
                        //motor to move arm does not need acceleration and deceleration
                        profileAcceleration = 0;
                        ProfileDeceleration = 0;
                        break;
                }

                //get baudrate info
                uint b = connector.Baudrate;

                //set connection properties
                connector.Baudrate = b;
                connector.Timeout = 500;

                epos1 = connector.CreateDevice(Convert.ToUInt16(1));
                epos2 = connector.CreateDevice(Convert.ToUInt16(2));

                //ProfilePositionMode and ProfileVelocityMode are assigned to objects from the library
                ppm1 = epos1.Operation.ProfilePositionMode;
                ppm2 = epos2.Operation.ProfilePositionMode;
                pvm1 = epos1.Operation.ProfileVelocityMode;
                pvm2 = epos2.Operation.ProfileVelocityMode;


                //StateMachines are assigned to objects from the library
                sm1 = epos1.Operation.StateMachine;
                sm2 = epos2.Operation.StateMachine;

                //MotionInfo is assigned to an object from the library
                mi1 = epos1.Operation.MotionInfo;
                mi2 = epos2.Operation.MotionInfo;

                //motors may be disabled
                Enable();
                //motors should initialize in VelocityMode
                ActivateVelocityMode();

            }
            //an error may occur
            catch (Exception e)
            {
                ConsoleFormatter.Error("Failed to connect to the engine.", "Type: " + engineType.ToString());
                throw e;
            }
        }

        /// <summary>
        /// ProfileAcceleration defines, how fast motor starts to move
        /// </summary>
        public uint ProfileAcceleration { get; set; }

        /// <summary>
        /// ProfileDeceleration defines, how fast motor stops moving
        /// </summary>
        public uint ProfileDeceleration { get; set; }

        /// <summary>
        /// get position of motor 1
        /// </summary>
        public int Position1
        {
            get { return mi1.GetPositionIs(); }
        }

        /// <summary>
        /// get position of motor 2
        /// </summary>
        public int Position2
        {
            get { return mi2.GetPositionIs(); }
        }


        /// <summary>
        /// get velocity of motor 1
        /// </summary>
        public int Velocity1
        {
            get { return mi1.GetVelocityIs(); }
        }

        /// <summary>
        /// get velocity of motor 2
        /// </summary>
        public int Velocity2
        {
            get { return mi2.GetVelocityIs(); }
        }

        /// <summary>
        /// get movement state of motor 1
        /// </summary>
        public bool MovementState1
        {
            get
            {
                bool state = false;
                mi1.GetMovementState(ref state);
                return state;
            }
        }

        /// <summary>
        /// get movement state of motor 2
        /// </summary>
        public bool MovementState2
        {
            get
            {
                bool state = false;
                mi2.GetMovementState(ref state);
                return state;
            }
        }


        /// <summary>
        /// motor 1 is disabled, when upper/lower limit is reached or motor is unable to move
        /// </summary>
        public bool DisableState1
        {
            get { return sm1.GetDisableState(); }
        }

        /// <summary>
        /// motor 2 is disabled, when upper/lower limit is reached or motor is unable to move
        /// </summary>
        public bool DisableState2
        {
            get { return sm2.GetDisableState(); }
        }

        /// <summary>
        /// true, if an error occured with motor 1
        /// </summary>
        public bool FaultState1
        {
            get { return sm1.GetFaultState(); }
        }

        /// <summary>
        /// true, if an error occured with motor 2
        /// </summary>
        public bool FaultState2
        {
            get { return sm2.GetFaultState(); }
        }

        /// <summary>
        /// clears states and enables motors
        /// </summary>
        /// <returns>bool that indicates, if operation was successfull</returns>
        public bool Enable()
        {
            try
            {
                //reset fault state of motor 1
                if (sm1.GetFaultState())
                {
                    sm1.ClearFault();
                }

                //enable motor 1
                sm1.SetEnableState();


                //reset fault state of motor 2
                if (sm2.GetFaultState())
                {
                    sm2.ClearFault();
                }

                //enable motor 2
                sm2.SetEnableState();


                //returns true if Enable() was successful
                return true;
            }
            //an error may occur
            catch (Exception e)
            {
                ConsoleFormatter.Error("Failed to activate the engine.", "Message: " + e.Message);
                //returns false if Enable() fails
                return false;
            }

        }

        /// <summary>
        /// clears states and disables motors
        /// </summary>
        /// <returns>bool that indicates, if operation was successfull</returns>
        public bool Disable()
        {
            //stops wheels
            StopImmediately();

            try
            {
                //reset fault state of motor 1
                if (sm1.GetFaultState())
                {
                    sm1.ClearFault();
                }

                //reset fault state of motor 2
                if (sm2.GetFaultState())
                {
                    sm2.ClearFault();
                }

                //reset disable state of motor 1
                if (!sm1.GetDisableState())
                {
                    sm1.SetDisableState();
                }

                //reset disable state of motor 2
                if (!sm2.GetDisableState())
                {
                    sm2.SetDisableState();
                }

                //returns true, if Disable() was successfull
                return true;
            }
            //an error may occur
            catch (Exception e)
            {
                ConsoleFormatter.Error("Failed to deactivate the engine.", "Message: " + e.Message);
                //returns false, if Disable() fails
                return false;
            }
        }

        /// <summary>
        /// emergency stop
        /// </summary>
        /// <returns>bool that indicates, if operation was successfull</returns>
        public bool StopImmediately()
        {
            try
            {
                //motor must be in PositionMode to set velocity of wheels to 0
                ActivatePositionMode();

                //velocity of motors is set to 0
                ppm1.HaltPositionMovement();
                ppm2.HaltPositionMovement();

                //returns true, if StopImmediately() was successfull
                return true;
            }
            //an error may occur
            catch (Exception e)
            {
                ConsoleFormatter.Error("Failed to stop the engine.", "Message: " + e.Message);
                //returns false, if StopImmediately() fails
                return false;
            }
        }

        /// <summary>
        /// sets engine into <see cref="PositionMode"/>
        /// </summary>
        /// <returns>bool that indicates, if operation was successfull</returns>
        private bool ActivatePositionMode()
        {
            //if engine is not in PositionMode, set PositionMode
            if (engineMode != EngineMode.POSITION)
            {
                //set PositionMode for both motors seperately
                ppm1.ActivateProfilePositionMode();
                ppm2.ActivateProfilePositionMode();

                //PostionProfile is only set, when acceleration and deceleration are not 0
                if (profileAcceleration != 0 && profileDeceleration != 0)
                {
                    ppm1.SetPositionProfile(1000000, profileAcceleration, profileDeceleration);
                    ppm2.SetPositionProfile(1000000, profileAcceleration, profileDeceleration);
                }
                //set state machine to EngineMode.POSITION
                engineMode = EngineMode.POSITION;

                //returns true, if ActivatePositionMode() was successfull
                return true;
            }

            //returns false, if ActivatePositionMode() fails
            return false;
        }

        /// <summary>
        /// sets engine into <see cref="VelocityMode"/>
        /// </summary>
        /// <returns>bool that indicates, if operation was successfull</returns>
        private bool ActivateVelocityMode()
        {
            //if engine is not in VelocityMode, set VelocityMode
            if (engineMode != EngineMode.VELOCITY)
            {
                //set VelocityMode for both motors seperately
                pvm1.ActivateProfileVelocityMode();
                pvm2.ActivateProfileVelocityMode();

                //PostionProfile is only set, when acceleration and deceleration are not 0
                if (profileAcceleration != 0 && profileDeceleration != 0)
                {
                    pvm1.SetVelocityProfile(ProfileAcceleration, ProfileDeceleration);
                    pvm2.SetVelocityProfile(ProfileAcceleration, ProfileDeceleration);
                }
                //set state machine to EngineMode.VELOCITY
                engineMode = EngineMode.VELOCITY;

                //returns true, if ActivateVelocityMode() was successfull
                return true;
            }

            //returns false if ActivateVelocityMode() fails
            return false;
        }
        /// <summary>
        /// makes both wheels turn until the given distance is reached
        /// </summary>
        /// <param name="distance">distance in cm, already multiplied by -2000</param>
        /// <returns>bool that indicates, if operation was successfull</returns>
        public bool MoveToPosition(int distance)
        {
            try
            {
                //PositonMode is needed to let robot drive a given distance
                ActivatePositionMode();

                //both wheels should turn
                ppm1.MoveToPosition(distance, false, true);
                ppm2.MoveToPosition(distance, false, true);

                //returns true, if MoveToPosition() was successfull
                return true;
            }
            //an error may occur
            catch (Exception e)
            {
                ConsoleFormatter.Error("Failed to move for " + distance + "cm", "Message: " + e.Message);
                //returns false if MovetoPosition() fails
                return false;
            }
        }

        /// <summary>
        /// makes both wheels turn until seperatly given distances are reached
        /// </summary>
        /// <param name="distance1">distance in cm for wheel 1, already multiplied by -2000</param>
        /// <param name="distance2">distance in cm for wheel 2, already multiplied by -2000</param>
        /// <returns>bool that indicates, if operation was successfull</returns>
        public bool MoveToPosition(int distance1, int distance2)
        {
            try
            {
                //PositonMode is needed to let robot drive a given distance
                ActivatePositionMode();
                //both wheels should turn
                ppm1.MoveToPosition(distance1, false, true);
                ppm2.MoveToPosition(distance2, false, true);

                //returns true, if MoveToPosition() was successfull
                return true;
            }
            //an error may occur
            catch (Exception e)
            {
                ConsoleFormatter.Error("Failed to move for " + distance1 + "cm and " + distance2 + "cm", "Message: " + e.Message);
                //returns false if MovetoPosition() fails
                return false;
            }
        }

        /// <summary>
        /// makes wheel 1 turn until given distance is reached
        /// </summary>
        /// <param name="distance">distance in cm for wheel 1, already multiplied by -2000</param>
        /// <returns>bool that indicates, if operation was successfull</returns>
        public bool MoveToPosition1(int distance)
        {
            try
            {
                //PositonMode is needed to let robot drive a given distance
                ActivatePositionMode();
                //wheel 1 should turn
                ppm1.MoveToPosition(distance * -2000, false, true);

                //returns true, if MoveToPosition() was successfull
                return true;
            }
            //an error may occur
            catch (Exception e)
            {
                ConsoleFormatter.Error("Failed to move for " + distance + "cm for engine 1.", "Message: " + e.Message);
                //returns false if MovetoPosition1() fails
                return false;
            }
        }

        /// <summary>
        /// makes wheel 2 turn until given distance is reached
        /// </summary>
        /// <param name="distance">distance in cm for wheel 2, already multiplied by -2000</param>
        /// <returns>bool that indicates, if operation was successfull</returns>
        public bool MoveToPosition2(int distance)
        {
            try
            {
                //PositonMode is needed to let robot drive a given distance
                ActivatePositionMode();
                //wheel 1 should turn
                ppm2.MoveToPosition(distance * -2000, false, true);

                //returns true, if MoveToPosition() was successfull
                return true;
            }
            //an error may occur
            catch (Exception e)
            {
                ConsoleFormatter.Error("Failed to move for " + distance + "cm for engine 2.", "Message: " + e.Message);
                //returns false if MovetoPosition1() fails
                return false;
            }
        }

        /// <summary>
        /// makes motors move with given velocity
        /// </summary>
        /// <param name="v">velocity for both motors</param>
        /// <returns>bool that indicates, if operation was successfull</returns>
        public bool MoveWithVelocity(Int32 v)
        {
            try
            {
                //velocity should be between specified borders
                //motors will not move with to high or to low velocity
                if (v >= -MAX_VELOCITY && v <= MAX_VELOCITY)
                {
                    //VelocityMode is needed to let robot drive with a given velocity
                    ActivateVelocityMode();
                    //both wheels should turn
                    pvm1.MoveWithVelocity(v * -1);
                    pvm2.MoveWithVelocity(v * -1);
                    //returns true, if MoveWithVelocity() was successfull
                    return true;
                }
            }
            //an error may occur
            catch (Exception e)
            {
                ConsoleFormatter.Error("Failed to move with velocity " + v, "Message: " + e.Message);
            }
            //returns false if MoveWithVelocity() fails
            return false;
        }

        /// <summary>
        /// makes motors move with given velocities
        /// </summary>
        /// <param name="v1">velocity for left motor epos1</param>
        /// <param name="v2">velocity for right motor epos2</param>
        /// <returns>bool that indicates, if operation was successfull</returns>
        public bool MoveWithVelocity(Int32 v1, Int32 v2)
        {
            try
            {
                //velocities should be between specified borders
                //motors will not move with to high or to low velocities
                if (v1 >= -MAX_VELOCITY && v1 <= MAX_VELOCITY && v2 >= -MAX_VELOCITY && v2 <= MAX_VELOCITY)
                {
                    //VelocityMode is needed to let robot drive with a given velocity
                    ActivateVelocityMode();

                    //negative velocities will make robot move forwards
                    //both wheels should turn
                    pvm1.MoveWithVelocity(v1 * -1);
                    pvm2.MoveWithVelocity(v2 * -1);

                    //returns true, if MoveWithVelocity() was successfull
                    return true;
                }
            }
            //an error may occur
            catch (Exception e)
            {
                ConsoleFormatter.Error("Failed to move with velocity " + v1 + " and " + v2, "Message: " + e.Message);
            }
            //returns false if MoveWithVelocity() fails
            return false;
        }


        /// <summary>
        /// makes left motor move with given velocity
        /// </summary>
        /// <param name="v1">velocity for the left motor epos1</param>
        /// <exception cref="DeviceException">thrown, when USB connection is lost</exception>
        /// <exception cref="Exception">thrown, when <see cref="ActivateVelocityMode"/> or <see cref="pvm1.MoveWithVelocity"/> fails</exception>
        /// <returns>bool that indicates, if operation was successfull</returns>
        public bool MoveWithVelocity1(Int32 v1)
        {
            try
            {
                //velocity should be between specified borders
                //motor will not move with to high or to low velocity
                if (v1 >= -MAX_VELOCITY && v1 <= MAX_VELOCITY)
                {
                    //VelocityMode is needed to let robot drive with a given velocity
                    ActivateVelocityMode();

                    //negative velocity will make wheel move forwards
                    pvm1.MoveWithVelocity(v1 * -1);

                    //returns true if operation was successfull
                    return true;
                }
            }
            //an error may occur
            catch (Exception e)
            {
                ConsoleFormatter.Error("Failed to move with velocity " + v1 + " for engine 1", "Message: " + e.Message);
            }

            //returns false if an error was thrown
            return false;
        }

        /// <summary>
        /// makes right motor move with given velocity
        /// </summary>
        /// <param name="v2">velocity for the right motor epos2</param>
        /// <exception cref="DeviceException">thrown, when USB connection is lost</exception>
        /// <exception cref="Exception">thrown, when <see cref="ActivateVelocityMode"/> or <see cref="pvm1.MoveWithVelocity"/> fails</exception>
        /// <returns>bool that indicates, if operation was successfull</returns>
        public bool MoveWithVelocity2(Int32 v2)
        {
            try
            {
                //velocity should be between specified borders
                //motor will not move with to high or to low velocity
                if (v2 >= -MAX_VELOCITY && v2 <= MAX_VELOCITY)
                {
                    //VelocityMode is needed to let robot drive with a given velocity
                    ActivateVelocityMode();

                    //negative velocity will make wheel move forwards
                    pvm2.MoveWithVelocity(v2 * -1);

                    //returns true if operation was successfull
                    return true;
                }
            }
            //an error may occur
            catch (Exception e)
            {
                ConsoleFormatter.Error("Failed to move with velocity " + v2 + " for engine 2", "Message: " + e.Message);
            }

            //returns false if an error was thrown
            return false;
        }
    }
}
