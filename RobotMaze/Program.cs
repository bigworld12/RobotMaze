using System;
using System.Collections.Generic;

namespace RobotMaze
{
    class Program
    {
        static void Main(string[] args)
        {
            var r = new Robot();
            r.DiscoverMaze();
            Console.ReadKey();
        }
    }
    //describes actual robot
    public class Robot
    {        
        public List<List<RelativeDirections>> DecisionsHistory { get; } = new List<List<RelativeDirections>>();
        //all distances are measured in CM by the sensors
        //all units should be multiplied by UnitOfMovement to get actual distance

        
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
                    var zeroDispIndex = HasZeroDisplacement(DecisionsHistory, out AbsoluteDirections absDir);//absDir is useless, just for testing
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
                    //go back, then change direction (invert the last decision)
                    DecisionsHistory.RemoveAt(DecisionsHistory.Count - 1);
                    var parentD = DecisionsHistory[DecisionsHistory.Count - 1];
                    var lastD = parentD[parentD.Count - 1];
                    parentD.RemoveAt(parentD.Count - 1);

                    MoveBackwardThenInverse(lastD);
                }
            }
        }



        public bool HasZeroDisplacement(List<List<RelativeDirections>> decisions, out AbsoluteDirections curAbs)
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
                            default: break;
                        }
                        break;
                    case AbsoluteDirections.Left:
                        switch (end)
                        {
                            case RelativeDirections.Right: curAbs = AbsoluteDirections.Up; VSum++; break;
                            case RelativeDirections.Left: curAbs = AbsoluteDirections.Down; VSum--; break;
                            case RelativeDirections.Front: HSum--; break;
                            default: break;
                        }
                        break;
                    case AbsoluteDirections.Up:
                        switch (end)
                        {
                            case RelativeDirections.Right: curAbs = AbsoluteDirections.Right; HSum++; break;
                            case RelativeDirections.Left: curAbs = AbsoluteDirections.Left; HSum--; break;
                            case RelativeDirections.Front: VSum++; break;

                            default: break;
                        }

                        break;
                    case AbsoluteDirections.Down:
                        switch (end)
                        {
                            case RelativeDirections.Right: curAbs = AbsoluteDirections.Left; HSum--; break;
                            case RelativeDirections.Left: curAbs = AbsoluteDirections.Right; HSum++; break;
                            case RelativeDirections.Front: VSum--; break;
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

        public int CurrentUnitR() { return CurrentDistanceR / UnitOfMovement; }
        public int CurrentUnitL() { return CurrentDistanceL / UnitOfMovement; }

        public int CurrentUnitF() { return CurrentDistanceF / UnitOfMovement; }

        public bool IsLeftWalkable()
        {
            return CurrentUnitL() > 0;
        }
        public bool IsRightWalkable()
        {
            return CurrentUnitR() > 0;
        }
        public bool IsFrontWalkable()
        {
            return CurrentUnitF() > 0;
        }



        // Robot-specific stuff
        public bool IsRunning { get; set; } = true; //just for testing, should always be true
        //how many centimeters a robot can walk per step (in centimeters)        
        public int UnitOfMovement { get; set; } = 20; //keep changing this untill you get best results

        //Distance to the left         
        public int CurrentDistanceL { get; set; }
        //Distance to the right         
        public int CurrentDistanceR { get; set; }
        //Distance to the front
        public int CurrentDistanceF { get; set; }
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
                    //move forward, without changing direction
                    break;
                default:
                    break;
            }
        }
        public void MoveBackwardThenInverse(RelativeDirections d)
        {
            switch (d)
            {
                case RelativeDirections.Right:
                    //move backward then turn left
                    break;
                case RelativeDirections.Left:
                    //move backward then turn right
                    break;
                case RelativeDirections.Front:
                    //move backward, without changing direction
                    break;
                default:
                    break;
            }
        }
    }
    public enum RelativeDirections : byte
    {
        Right = 0, //inverse of right is left
        Left = 1, //inverse of left is right
        Front = 2 //inverse of front is Backward        
    }
    public enum AbsoluteDirections : byte
    {
        Up = 0,
        Down = 1,
        Right = 2,
        Left = 3
    }
}
