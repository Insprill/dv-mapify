using System;
using System.Collections.Generic;
using System.Text;

namespace Mapify.Editor.Tools
{
    public static class ToolEnums
    {
        public enum TrackOrientation
        {
            Left,
            Right
        }

        public enum SwitchType
        {
            Vanilla,
            Custom
        }

        public enum SwitchPoint
        {
            Joint,
            Through,
            Diverging
        }

        public enum SelectionType
        {
            None,
            Track,
            BezierPoint,
            Switch,
            Turntable
        }

        public enum CreationMode
        {
            Freeform,
            Piece
        }

        public enum TrackPiece
        {
            Straight,
            Curve,
            Switch,
            Yard,
            Turntable,
            Special
        }

        public enum SpecialTrackPiece
        {
            Buffer,
            SwitchCurve,
            Connect2,
            Crossover,
            ScissorsCrossover,
            DoubleSlip
        }

        public enum EditingMode
        {
            Merge,
            MatchTerrain,
            InsertPoint
        }

        public static TrackOrientation FlipOrientation(TrackOrientation orientation)
        {
            if (orientation == TrackOrientation.Left)
            {
                return TrackOrientation.Right;
            }

            return TrackOrientation.Left;
        }

        public static SwitchPoint NextPoint(SwitchPoint switchPoint)
        {
            if (switchPoint == SwitchPoint.Joint)
            {
                return SwitchPoint.Through;
            }
            if (switchPoint == SwitchPoint.Through)
            {
                return SwitchPoint.Diverging;
            }

            return SwitchPoint.Joint;
        }

        public static Switch.StandSide FlipStand(Switch.StandSide standSide)
        {
            if (standSide == Switch.StandSide.THROUGH)
            {
                return Switch.StandSide.DIVERGING;
            }

            return Switch.StandSide.THROUGH;
        }
    }
}
