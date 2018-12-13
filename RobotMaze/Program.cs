using System;
using System.Collections.Generic;

namespace RobotMaze
{
    class Program
    {
        static void Main(string[] args)
        {
            var r = new Robot();
            r.TestHistory();
            Console.ReadKey();
        }
    }
    public class Robot
    {
        /// <summary>
        /// how many centimeters a robot can walk per step (in centimeters)
        /// </summary>
        public int UnitOfMovement { get; set; } = 20;
        //public List<Directions> DirectionsHistory { get; } = new List<Directions>();
        public List<List<RelativeDirections>> DecisionsHistory { get; } = new List<List<RelativeDirections>>();

        //all distances are measured in CM by the sensors
        //all units should be multiplied by UnitOfMovement to get actual distance

        public bool IsRunning { get; set; } = true;
        public List<RelativeDirections> DiscoverSurroundings()
        {
            var l = new List<RelativeDirections>();
            if (IsLeftWalkable()) l.Add(RelativeDirections.Left);
            if (IsRightWalkable()) l.Add(RelativeDirections.Right);
            if (IsFrontWalkable()) l.Add(RelativeDirections.Front);
            return l;
        }
        //keep walking in the maze forever, until the robot is stopped manually
        public RelativeDirections RemoveLastDecision(List<List<RelativeDirections>> history)
        {
            if (history.Count == 0)
            {
                return RelativeDirections.Front;
            }
            var lastL = history[history.Count - 1];
            var last = lastL[lastL.Count - 1];
            if (lastL.Count > 0)
            {
                lastL.RemoveAt(lastL.Count - 1);
            }
            return last;
        }
        public void DiscoverMaze()
        {
            UpdateSensors();
            DecisionsHistory.Add(DiscoverSurroundings());
            while (IsRunning)
            {
                if (DecisionsHistory.Count == 0)
                {
                    //closed maze !!
                    Console.WriteLine("Closed maze, don't know where to go");
                    return;
                }
                var d = DecisionsHistory[DecisionsHistory.Count - 1];
                if (d.Count != 0)
                {
                    //after taking a decision
                    //check if it will lead to cicles
                    var zeroDispIndex = HasZeroDisplacementIndex(DecisionsHistory, out var dir);
                    if (zeroDispIndex)
                    {
                        //bad decision !, but it wasn't executed yet
                        RemoveLastDecision(DecisionsHistory);
                        if (DecisionsHistory[DecisionsHistory.Count - 1].Count == 0)
                        {
                            DecisionsHistory.RemoveAt(DecisionsHistory.Count - 1);
                        }
                    }
                    else
                    {
                        var lastDir = d[d.Count - 1];
                        //good decision, move !
                        ChangeDirectionAndMove(lastDir);
                        UpdateSensors();
                        DecisionsHistory.Add(DiscoverSurroundings());
                    }
                }
                else
                {
                    //current decision is a closed road, but unfortionatly, it already moved
                    //go back, then change direction
                    DecisionsHistory.RemoveAt(DecisionsHistory.Count - 1);
                    var parentD = DecisionsHistory[DecisionsHistory.Count - 1];

                }
            }
        }

        public void TestHistory()
        {
            var histoy = new List<List<RelativeDirections>>
            {
                new List<RelativeDirections>() { RelativeDirections.Front},
                new List<RelativeDirections>() { RelativeDirections.Right },
                new List<RelativeDirections>() { RelativeDirections.Front },

                new List<RelativeDirections>() { RelativeDirections.Left },
                new List<RelativeDirections>() { RelativeDirections.Right },
                new List<RelativeDirections>() { RelativeDirections.Left },

                new List<RelativeDirections>() { RelativeDirections.Left },
                new List<RelativeDirections>() { RelativeDirections.Front },
                new List<RelativeDirections>() { RelativeDirections.Front },

                new List<RelativeDirections>() { RelativeDirections.Left },
                new List<RelativeDirections>() { RelativeDirections.Front },
                new List<RelativeDirections>() { RelativeDirections.Front }
            };
            bool has = HasZeroDisplacementIndex(histoy, out var dir);
            Console.WriteLine($"Current Absolute Direction ({dir}), Has zero displacement ? {has}");
        }

        public bool HasZeroDisplacementIndex(List<List<RelativeDirections>> decisions, out AbsoluteDirections curAbs)
        {
            int HSum = 0;
            int VSum = 0;
            curAbs = AbsoluteDirections.Up;
            for (int i = 0; i < decisions.Count; i++)
            {
                var curDList = decisions[i];
                var end = curDList[curDList.Count - 1];
                switch (curAbs)
                {
                    case AbsoluteDirections.Right:
                        switch (end)
                        {
                            case RelativeDirections.Right: curAbs = AbsoluteDirections.Down; VSum--; break;
                            case RelativeDirections.Left: curAbs = AbsoluteDirections.Up; VSum++; break;
                            case RelativeDirections.Front: HSum++; break;
                            case RelativeDirections.Backward: HSum--; break;
                            default: break;
                        }
                        break;
                    case AbsoluteDirections.Left:
                        switch (end)
                        {
                            case RelativeDirections.Right: curAbs = AbsoluteDirections.Up; VSum++; break;
                            case RelativeDirections.Left: curAbs = AbsoluteDirections.Down; VSum--; break;
                            case RelativeDirections.Front: HSum--; break;
                            case RelativeDirections.Backward: HSum++; break;
                            default: break;
                        }
                        break;
                    case AbsoluteDirections.Up:
                        switch (end)
                        {
                            case RelativeDirections.Right: curAbs = AbsoluteDirections.Right; HSum++; break;
                            case RelativeDirections.Left: curAbs = AbsoluteDirections.Left; HSum--; break;
                            case RelativeDirections.Front: VSum++; break;
                            case RelativeDirections.Backward: VSum--; break;
                            default: break;
                        }

                        break;
                    case AbsoluteDirections.Down:
                        switch (end)
                        {
                            case RelativeDirections.Right: curAbs = AbsoluteDirections.Left; HSum--; break;
                            case RelativeDirections.Left: curAbs = AbsoluteDirections.Right; HSum++; break;
                            case RelativeDirections.Front: VSum--; break;
                            case RelativeDirections.Backward: VSum++; break;
                            default: break;
                        }
                        break;
                    default:
                        break;
                }
                if (HSum == 0 && VSum == 0)
                {
                    return true;
                }
            }
            return false;
        }
        public RelativeDirections GetInverse(RelativeDirections d)
        {
            switch (d)
            {
                case RelativeDirections.Right:
                    return RelativeDirections.Left;
                case RelativeDirections.Left:
                    return RelativeDirections.Right;
                case RelativeDirections.Front:
                    return RelativeDirections.Backward;
                case RelativeDirections.Backward:
                    return RelativeDirections.Front;
                default:
                    return RelativeDirections.Front;
            }
        }


        //Distance to the right 
        public int CurrentDistanceR { get; set; }
        public int CurrentUnitR => CurrentDistanceR / UnitOfMovement;

        //Distance to the left 
        public int CurrentDistanceL { get; set; }
        public int CurrentUnitL => CurrentDistanceL / UnitOfMovement;

        //Distance to the front
        public int CurrentDistanceF { get; set; }
        public int CurrentUnitF => CurrentDistanceF / UnitOfMovement;


        public bool IsLeftWalkable()
        {
            return CurrentUnitL > 0;
        }
        public bool IsRightWalkable()
        {
            return CurrentUnitR > 0;
        }
        public bool IsFrontWalkable()
        {
            return CurrentUnitF > 0;
        }

        //robot-specific things
        public void UpdateSensors()
        {
            CurrentDistanceR = 0;//value from sensor here
            CurrentDistanceL = 0;//value from sensor here
            CurrentDistanceF = 0;//value from sensor here
        }

        public void ChangeDirectionAndMove(RelativeDirections d)
        {
            //make motors move robot 1 step in Direction (d)
            //1 step = UnitOfMovement cm
            switch (d)
            {
                case RelativeDirections.Right:
                    //turn right then move forward
                    break;
                case RelativeDirections.Left:
                    //turn left then move forward
                    break;
                case RelativeDirections.Front:
                    //move forward
                    break;
                case RelativeDirections.Backward:
                    //move backward
                    break;
                default:
                    break;
            }
        }
    }
    public enum RelativeDirections : byte
    {
        Right, //inverse of right is left
        Left, //inverse of left is right
        Front, //inverse of front is Backward
        Backward //inverse of Backward is front
    }
    public enum AbsoluteDirections : byte
    {
        Up,
        Down,
        Right,
        Left
    }

}
