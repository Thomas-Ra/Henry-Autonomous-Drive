using EposCmd.Net;
using EposCmd.Net.DeviceCmdSet.Operation;
using System;

namespace HwrBerlin.Bot.Engines
{
    /// <summary>
    /// provides functions to control directly a EPOS engine
    /// </summary>
    public class Engine
    {
        /// <summary>
        /// DeviceManager for USB communication
        /// </summary>
        private readonly DeviceManager _connector;

        //Two engines
        /// <summary>
        /// Device comes from library;
        /// use Device to configure motors
        /// </summary>
        private readonly Device _epos1;

        /// <summary>
        /// Device comes from library;
        /// use Device to configure motors
        /// </summary>
        private readonly Device _epos2;

        //Two modes each engine
        /// <summary>
        /// motor 1 operates in <see cref="ProfilePositionMode"/>
        /// </summary>
        private readonly ProfilePositionMode _ppm1;

        /// <summary>
        /// motor 2 operates in <see cref="ProfilePositionMode"/>
        /// </summary>
        private readonly ProfilePositionMode _ppm2;

        /// <summary>
        /// motor 1 operates in <see cref="ProfileVelocityMode"/>
        /// </summary>
        private readonly ProfileVelocityMode _pvm1;

        /// <summary>
        /// motor 2 operates in <see cref="ProfileVelocityMode"/>
        /// </summary>
        private readonly ProfileVelocityMode _pvm2;

        //State Machines
        /// <summary>
        /// handles states of motor 1 to see, when motor is in fault state, enabled or disabled
        /// </summary>
        private readonly StateMachine _sm1;
        /// <summary>
        /// handles states of motor 2 to see, when motor is in fault state, enabled or disabled
        /// </summary>
        private readonly StateMachine _sm2;

        //Motion Info
        /// <summary>
        /// object from library to get information about MotionState of motor 1
        /// </summary>
        private readonly MotionInfo _mi1;
        /// <summary>
        /// object from library to get information about MotionState of motor 2
        /// </summary>
        private readonly MotionInfo _mi2;

        /// <summary>
        /// Maximal velocity is used as upper limit for every engine
        /// Maximal velocity multiplied with -1 is used as lower limit
        /// </summary>
        private const int MaxVelocity = 12000000 - 1;

        /// <summary>
        /// deceleration needed for SetPositionProfile
        /// </summary>
        public uint ProfileDeceleration;

        /// <summary>
        /// acceleration needed for SetPositionProfile
        /// </summary>
        public uint ProfileAcceleration;

        /// <summary>
        /// object of EngineMode
        /// </summary>
        private EngineMode _engineMode;


        /// <summary>
        /// selects whether robot or arm should be used
        /// </summary>
        public enum EngineType
        {
            Robot,
            Arm
        }

        /// <summary>
        /// motors can be moved with a given velocity or to a given position;
        /// select EngineMode depending on the way the motor should move (with velocity or to position)
        /// </summary>
        public enum EngineMode
        {
            Velocity,
            Position
        }


        /// <summary>
        /// constructor for Engine
        /// </summary>
        /// <param name="engineType">
        /// select robot to drive;
        /// select arm to move arm
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">thrown, if the engine type could not found</exception>
        /// <exception cref="Exception">thrown, when a process in constructor fails</exception>
        public Engine(EngineType engineType)
        {
            try
            {
                switch (engineType)
                {
                    case EngineType.Robot:
                        //connect to motor to drive
                        _connector = new DeviceManager("EPOS2", "MAXON SERIAL V2", "USB", "USB0");
                        //motor to drive needs acceleration and deceleration
                        ProfileAcceleration = 3000;
                        ProfileDeceleration = 3000;
                        break;
                    case EngineType.Arm:
                        //connect to motor to move arm
                        _connector = new DeviceManager("EPOS", "MAXON_RS232", "RS232", "COM4");
                        //motor to move arm does not need acceleration and deceleration
                        ProfileAcceleration = 0;
                        ProfileDeceleration = 0;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(engineType), engineType, null);
                }

                //get baud rate info
                var b = _connector.Baudrate;

                //set connection properties
                _connector.Baudrate = b;
                _connector.Timeout = 500;

                _epos1 = _connector.CreateDevice(Convert.ToUInt16(1));
                _epos2 = _connector.CreateDevice(Convert.ToUInt16(2));

                //ProfilePositionMode and ProfileVelocityMode are assigned to objects from the library
                _ppm1 = _epos1.Operation.ProfilePositionMode;
                _ppm2 = _epos2.Operation.ProfilePositionMode;
                _pvm1 = _epos1.Operation.ProfileVelocityMode;
                _pvm2 = _epos2.Operation.ProfileVelocityMode;


                //StateMachines are assigned to objects from the library
                _sm1 = _epos1.Operation.StateMachine;
                _sm2 = _epos2.Operation.StateMachine;

                //MotionInfo is assigned to an object from the library
                _mi1 = _epos1.Operation.MotionInfo;
                _mi2 = _epos2.Operation.MotionInfo;

                //motors may be disabled
                Enable();
                //motors should initialize in VelocityMode
                ActivateVelocityMode();

            }
            //an error may occur
            catch (Exception)
            {
                ConsoleFormatter.Error("Failed to connect to the engine.",
                                       "Type: " + engineType,
                                       "Help: Check if the cable is plugged in properly.");
                throw;
            }
        }

        /// <summary>
        /// get position of motor 1
        /// </summary>
        public int Position1 => _mi1.GetPositionIs();

        /// <summary>
        /// get position of motor 2
        /// </summary>
        public int Position2 => _mi2.GetPositionIs();


        /// <summary>
        /// get velocity of motor 1
        /// </summary>
        public int Velocity1 => _mi1.GetVelocityIs();

        /// <summary>
        /// get velocity of motor 2
        /// </summary>
        public int Velocity2 => _mi2.GetVelocityIs();

        /// <summary>
        /// get movement state of motor 1
        /// </summary>
        public bool MovementState1
        {
            get
            {
                var state = false;
                _mi1.GetMovementState(ref state);
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
                var state = false;
                _mi2.GetMovementState(ref state);
                return state;
            }
        }


        /// <summary>
        /// motor 1 is disabled, when upper/lower limit is reached or motor is unable to move
        /// </summary>
        public bool DisableState1 => _sm1.GetDisableState();

        /// <summary>
        /// motor 2 is disabled, when upper/lower limit is reached or motor is unable to move
        /// </summary>
        public bool DisableState2 => _sm2.GetDisableState();

        /// <summary>
        /// true, if an error occured with motor 1
        /// </summary>
        public bool FaultState1 => _sm1.GetFaultState();

        /// <summary>
        /// true, if an error occured with motor 2
        /// </summary>
        public bool FaultState2 => _sm2.GetFaultState();

        /// <summary>
        /// clears states and enables motors
        /// </summary>
        /// <returns>bool that indicates, if operation was successful</returns>
        public bool Enable()
        {
            try
            {
                //reset fault state of motor 1
                if (_sm1.GetFaultState())
                {
                    _sm1.ClearFault();
                }

                //enable motor 1
                _sm1.SetEnableState();


                //reset fault state of motor 2
                if (_sm2.GetFaultState())
                {
                    _sm2.ClearFault();
                }

                //enable motor 2
                _sm2.SetEnableState();


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
        /// <returns>bool that indicates, if operation was successful</returns>
        public bool Disable()
        {
            //stops wheels
            StopImmediately();

            try
            {
                //reset fault state of motor 1
                if (_sm1.GetFaultState())
                {
                    _sm1.ClearFault();
                }

                //reset fault state of motor 2
                if (_sm2.GetFaultState())
                {
                    _sm2.ClearFault();
                }

                //reset disable state of motor 1
                if (!_sm1.GetDisableState())
                {
                    _sm1.SetDisableState();
                }

                //reset disable state of motor 2
                if (!_sm2.GetDisableState())
                {
                    _sm2.SetDisableState();
                }

                //returns true, if Disable() was successful
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
        /// <returns>bool that indicates, if operation was successful</returns>
        public bool StopImmediately()
        {
            try
            {
                //motor must be in PositionMode to set velocity of wheels to 0
                ActivatePositionMode();

                //velocity of motors is set to 0
                _ppm1.HaltPositionMovement();
                _ppm2.HaltPositionMovement();

                //returns true, if StopImmediately() was successful
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
        /// <returns>bool that indicates, if operation was successful</returns>
        private bool ActivatePositionMode()
        {
            //if engine is not in PositionMode, set PositionMode
            if (_engineMode == EngineMode.Position)
                return false;
            
            //set PositionMode for both motors separately
            _ppm1.ActivateProfilePositionMode();
            _ppm2.ActivateProfilePositionMode();

            //PostionProfile is only set, when acceleration and deceleration are not 0
            if (ProfileAcceleration != 0 && ProfileDeceleration != 0)
            {
                _ppm1.SetPositionProfile(1000000, ProfileAcceleration, ProfileDeceleration);
                _ppm2.SetPositionProfile(1000000, ProfileAcceleration, ProfileDeceleration);
            }
            //set state machine to EngineMode.POSITION
            _engineMode = EngineMode.Position;

            //returns true, if ActivatePositionMode() was successful
            return true;
        }

        /// <summary>
        /// sets engine into <see cref="VelocityMode"/>
        /// </summary>
        /// <returns>bool that indicates, if operation was successful</returns>
        private bool ActivateVelocityMode()
        {
            //if engine is not in VelocityMode, set VelocityMode
            if (_engineMode == EngineMode.Velocity)
                return false;

            //set VelocityMode for both motors seperately
            _pvm1.ActivateProfileVelocityMode();
            _pvm2.ActivateProfileVelocityMode();

            //PositionProfile is only set, when acceleration and deceleration are not 0
            if (ProfileAcceleration != 0 && ProfileDeceleration != 0)
            {
                _pvm1.SetVelocityProfile(ProfileAcceleration, ProfileDeceleration);
                _pvm2.SetVelocityProfile(ProfileAcceleration, ProfileDeceleration);
            }

            //set state machine to EngineMode.VELOCITY
            _engineMode = EngineMode.Velocity;

            //returns true, if ActivateVelocityMode() was successful
            return true;
        }

        /// <summary>
        /// makes both wheels turn until the given distance is reached
        /// </summary>
        /// <param name="distance">distance in cm, already multiplied by -2000</param>
        /// <returns>bool that indicates, if operation was successful</returns>
        public bool MoveToPosition(int distance)
        {
            try
            {
                //PositionMode is needed to let robot drive a given distance
                ActivatePositionMode();

                //both wheels should turn
                _ppm1.MoveToPosition(distance, false, true);
                _ppm2.MoveToPosition(distance, false, true);

                //returns true, if MoveToPosition() was successful
                return true;
            }
            //an error may occur
            catch (Exception e)
            {
                ConsoleFormatter.Error("Failed to move for " + distance + "cm", "Message: " + e.Message);
                //returns false if MoveToPosition() fails
                return false;
            }
        }

        /// <summary>
        /// makes both wheels turn until separately given distances are reached
        /// </summary>
        /// <param name="distance1">distance in cm for wheel 1, already multiplied by -2000</param>
        /// <param name="distance2">distance in cm for wheel 2, already multiplied by -2000</param>
        /// <returns>bool that indicates, if operation was successful</returns>
        public bool MoveToPosition(int distance1, int distance2)
        {
            try
            {
                //PositionMode is needed to let robot drive a given distance
                ActivatePositionMode();

                //both wheels should turn
                _ppm1.MoveToPosition(distance1, false, true);
                _ppm2.MoveToPosition(distance2, false, true);

                //returns true, if MoveToPosition() was successful
                return true;
            }
            //an error may occur
            catch (Exception e)
            {
                ConsoleFormatter.Error("Failed to move for " + distance1 + "cm and " + distance2 + "cm", "Message: " + e.Message);
                //returns false if MoveToPosition() fails
                return false;
            }
        }

        /// <summary>
        /// makes wheel 1 turn until given distance is reached
        /// </summary>
        /// <param name="distance">distance in cm for wheel 1, already multiplied by -2000</param>
        /// <returns>bool that indicates, if operation was successful</returns>
        public bool MoveToPosition1(int distance)
        {
            try
            {
                //PositionMode is needed to let robot drive a given distance
                ActivatePositionMode();
                //wheel 1 should turn
                _ppm1.MoveToPosition(distance * -2000, false, true);

                //returns true, if MoveToPosition() was successful
                return true;
            }
            //an error may occur
            catch (Exception e)
            {
                ConsoleFormatter.Error("Failed to move for " + distance + "cm for engine 1.", "Message: " + e.Message);
                //returns false if MoveToPosition1() fails
                return false;
            }
        }

        /// <summary>
        /// makes wheel 2 turn until given distance is reached
        /// </summary>
        /// <param name="distance">distance in cm for wheel 2, already multiplied by -2000</param>
        /// <returns>bool that indicates, if operation was successful</returns>
        public bool MoveToPosition2(int distance)
        {
            try
            {
                //PositionMode is needed to let robot drive a given distance
                ActivatePositionMode();
                //wheel 1 should turn
                _ppm2.MoveToPosition(distance * -2000, false, true);

                //returns true, if MoveToPosition() was successful
                return true;
            }
            //an error may occur
            catch (Exception e)
            {
                ConsoleFormatter.Error("Failed to move for " + distance + "cm for engine 2.", "Message: " + e.Message);
                //returns false if MoveToPosition1() fails
                return false;
            }
        }

        /// <summary>
        /// makes motors move with given velocity
        /// </summary>
        /// <param name="v">velocity for both motors</param>
        /// <returns>bool that indicates, if operation was successful</returns>
        public bool MoveWithVelocity(int v)
        {
            try
            {
                //velocity should be between specified borders
                //motors will not move with to high or to low velocity
                if (v >= -MaxVelocity && v <= MaxVelocity)
                {
                    //VelocityMode is needed to let robot drive with a given velocity
                    ActivateVelocityMode();
                    //both wheels should turn
                    _pvm1.MoveWithVelocity(v * -1);
                    _pvm2.MoveWithVelocity(v * -1);
                    //returns true, if MoveWithVelocity() was successful
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
        /// <returns>bool that indicates, if operation was successful</returns>
        public bool MoveWithVelocity(int v1, int v2)
        {
            try
            {
                //velocities should be between specified borders
                //motors will not move with to high or to low velocities
                if (v1 >= -MaxVelocity && v1 <= MaxVelocity && v2 >= -MaxVelocity && v2 <= MaxVelocity)
                {
                    //VelocityMode is needed to let robot drive with a given velocity
                    ActivateVelocityMode();

                    //negative velocities will make robot move forwards
                    //both wheels should turn
                    _pvm1.MoveWithVelocity(v1 * -1);
                    _pvm2.MoveWithVelocity(v2 * -1);

                    //returns true, if MoveWithVelocity() was successful
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
        /// <exception cref="Exception">thrown, when <see cref="ActivateVelocityMode"/> or <see cref="_pvm1.MoveWithVelocity"/> fails</exception>
        /// <returns>bool that indicates, if operation was successful</returns>
        public bool MoveWithVelocity1(int v1)
        {
            try
            {
                //velocity should be between specified borders
                //motor will not move with to high or to low velocity
                if (v1 >= -MaxVelocity && v1 <= MaxVelocity)
                {
                    //VelocityMode is needed to let robot drive with a given velocity
                    ActivateVelocityMode();

                    //negative velocity will make wheel move forwards
                    _pvm1.MoveWithVelocity(v1 * -1);

                    //returns true if operation was successful
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
        /// <exception cref="Exception">thrown, when <see cref="ActivateVelocityMode"/> or <see cref="_pvm1.MoveWithVelocity"/> fails</exception>
        /// <returns>bool that indicates, if operation was successful</returns>
        public bool MoveWithVelocity2(int v2)
        {
            try
            {
                //velocity should be between specified borders
                //motor will not move with to high or to low velocity
                if (v2 >= -MaxVelocity && v2 <= MaxVelocity)
                {
                    //VelocityMode is needed to let robot drive with a given velocity
                    ActivateVelocityMode();

                    //negative velocity will make wheel move forwards
                    _pvm2.MoveWithVelocity(v2 * -1);

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
