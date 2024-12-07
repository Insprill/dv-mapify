using Mapify.Editor.Tools.OptionData;
using Mapify.Editor.Utils;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static Mapify.Editor.Tools.ToolEnums;

namespace Mapify.Editor.Tools
{
    /// <summary>
    /// Class with methods to instantiate track pieces of various shapes and sizes.
    /// </summary>
    public static partial class TrackToolsCreator
    {
        public static Track GetEmptyTrack()
        {
            return new GameObject().AddComponent<Track>();
        }

        public static Track GetEmptyTrack(string name, Transform parent, Vector3 position)
        {
            Track t = GetEmptyTrack();

            t.name = name;
            t.transform.parent = parent;
            t.transform.position = position;

            return t;
        }

        // Bézier generation.
        // Separate functions so they can be used for previews, avoiding duplicating the code.
        /// <summary>
        /// Creates a cubic bezier curve that is straight.
        /// </summary>
        /// <param name="attachPoint">Attachment point for the track.</param>
        /// <param name="handlePosition">Handle of the attachment point for the track.</param>
        /// <param name="length">Horizontal length for the track.</param>
        /// <param name="endGrade">Grade at the end of the track.</param>
        /// <returns>
        /// An array of cubic beziers.
        /// </returns>
        public static SimpleBezier[] GenerateStraightBezier(Vector3 attachPoint, Vector3 handlePosition, float length, float endGrade)
        {
            // Create in 2D.
            Vector2 p0 = attachPoint.Flatten();
            Vector2 dir = (p0 - handlePosition.Flatten()).normalized;
            Vector2 p3 = p0 + dir * length;

            Vector2 p1 = Vector2.Lerp(p0, p3, MathHelper.OneThird);
            Vector2 p2 = Vector2.Lerp(p3, p0, MathHelper.OneThird);

            // Determine the grade at the start.
            float startGrade = MathHelper.GetGrade(handlePosition, attachPoint);

            // To create a smooth change in grade.
            float heightDif = TrackToolsHelper.CalculateHeightDifference(startGrade, endGrade, length);
            float handleLength = length * MathHelper.OneThird;

            // Return in 3D.
            return new SimpleBezier[] { new SimpleBezier(p0.To3D(attachPoint.y),
                    p1.To3D(attachPoint.y + startGrade * handleLength),
                    p2.To3D(attachPoint.y + heightDif - endGrade * handleLength),
                    p3.To3D(attachPoint.y + heightDif)) };
        }

        /// <summary>
        /// Creates one or more bezier curves that aproximate a circle.
        /// </summary>
        /// <param name="attachPoint">Attachment point for the track.</param>
        /// <param name="handlePosition">Handle of the attachment point for the track.</param>
        /// <param name="orientation">Whether the curve turns left or right.</param>
        /// <param name="radius">Radius in meters.</param>
        /// <param name="arc">Arc in degrees.</param>
        /// <param name="maxArc">Maximum arc allowed before the curve needs to be divided into multiple parts.</param>
        /// <param name="endGrade">The grade at the end of the track.</param>
        /// <returns>
        /// An array of cubic beziers.
        /// </returns>
        public static SimpleBezier[] GenerateCurveBeziers(Vector3 attachPoint, Vector3 handlePosition,
            TrackOrientation orientation, float radius, float arc, float maxArc, float endGrade)
        {
            bool isLeft = orientation == TrackOrientation.Left;

            // Create in 2D.
            Vector2 current = attachPoint.Flatten();
            Vector2 dir = (current - handlePosition.Flatten()).normalized;
            Vector2 pivot = current + (isLeft ? MathHelper.RotateCCW(dir) : MathHelper.RotateCW(dir)) * radius;

            // Split the bezier for smoother arcs.
            int arcCount = Mathf.CeilToInt(arc / maxArc);
            float actualArc = arc / arcCount;
            float arcLength = radius * actualArc * Mathf.Deg2Rad;

            // Length of the handle to approximate the arc as best as possible.
            float handleLength = MathHelper.ArcToBezierHandleLength(actualArc * Mathf.Deg2Rad) * radius;

            actualArc = isLeft ? actualArc * Mathf.Deg2Rad : actualArc * -Mathf.Deg2Rad;

            // There's one more control than the number of arcs, so reuse the variable.
            arcCount++;
            (Vector2 Back, Vector2 Here, Vector2 Next)[] controls = new (Vector2, Vector2, Vector2)[arcCount];

            // Create controls and their respective handles.
            for (int i = 0; i < arcCount; i++)
            {
                controls[i] = (current - dir * handleLength, current, current + dir * handleLength);
                // Rotate control around pivot to become the next control.
                current = MathHelper.RotateAround(current, actualArc, pivot);
                dir = MathHelper.Rotate(dir, actualArc);
            }

            arcCount--;

            // Determine the grade at the start.
            float startGrade = MathHelper.GetGrade(handlePosition, attachPoint);

            SimpleBezier[] curves = new SimpleBezier[arcCount];
            Vector3 p0, p1, p2, p3;
            float height = attachPoint.y;
            float nextgrade;
            float angle = Mathf.Atan(startGrade);
            float angleStep = (Mathf.Atan(endGrade) - angle) / (arcCount);

            // Move from 2D to 3D.
            for (int i = 0; i < arcCount; i++)
            {
                p0 = controls[i].Here.To3D(height);
                p1 = controls[i].Next.To3D(height + startGrade * handleLength);

                // Apply change to next point.
                angle += angleStep;
                nextgrade = Mathf.Tan(angle);
                height += TrackToolsHelper.CalculateHeightDifference(startGrade, nextgrade, arcLength);
                startGrade = nextgrade;

                p2 = controls[i + 1].Back.To3D(height - nextgrade * handleLength);
                p3 = controls[i + 1].Here.To3D(height);

                curves[i] = new SimpleBezier(p0, p1, p2, p3);
            }

            return curves;
        }

        // Normal pieces

        // Straights.
        /// <summary>
        /// Instantiates a straight <see cref="Track"/> and returns it.
        /// </summary>
        /// <param name="parent">The parent <see cref="Transform"/> for the new track.</param>
        /// <param name="attachPoint">Attachment point for the track.</param>
        /// <param name="handlePosition">Handle of the attachment point for the track.</param>
        /// <param name="length">Horizontal length for the track.</param>
        /// <param name="endGrade">Grade at the end of the track.</param>
        /// <returns>The instantiated <see cref="Track"/>.</returns>
        public static Track CreateStraight(Transform parent, Vector3 attachPoint, Vector3 handlePosition,
            float length, float endGrade)
        {
            Track t = GetEmptyTrack($"[Straight][{length}m]", parent, attachPoint);
            BezierPoint bp;

            // Generate bezier points.
            var curves = GenerateStraightBezier(attachPoint, handlePosition, length, endGrade);

            // Assign the points to the curve.
            bp = t.Curve.AddPointAt(curves[0].P0);
            bp.handleStyle = BezierPoint.HandleStyle.Broken;
            bp.globalHandle2 = curves[0].P1;

            bp = t.Curve.AddPointAt(curves[0].P3);
            bp.handleStyle = BezierPoint.HandleStyle.Broken;
            bp.globalHandle1 = curves[0].P2;

            return t;
        }

        /// <summary>
        /// Instantiates a straight <see cref="Track"/> between 2 points and returns it.
        /// </summary>
        /// <param name="parent">The parent <see cref="Transform"/> for the new track.</param>
        /// <param name="p0">The starting point.</param>
        /// <param name="p1">The ending point.</param>
        /// <returns>The instantiated <see cref="Track"/>.</returns>
        public static Track CreateStraight2Point(Transform parent, Vector3 p0, Vector3 p1)
        {
            Track t = GetEmptyTrack($"[Straight][{(p1 - p0).HorizontalMagnitude()}m]", parent, p0);
            BezierPoint bp;

            // Assign the points to the curve.
            bp = t.Curve.AddPointAt(p0);
            bp.handleStyle = BezierPoint.HandleStyle.Broken;
            bp.globalHandle2 = Vector3.Lerp(p0, p1, MathHelper.OneThird);

            bp = t.Curve.AddPointAt(p1);
            bp.handleStyle = BezierPoint.HandleStyle.Broken;
            bp.globalHandle1 = Vector3.Lerp(p1, p0, MathHelper.OneThird);

            return t;
        }

        // Curves.
        /// <summary>
        /// Instantiates a <see cref="Track"/> that approximates a circular arc and returns it.
        /// </summary>
        /// <param name="parent">The parent <see cref="Transform"/> for the new track.</param>
        /// <param name="attachPoint">Attachment point for the track.</param>
        /// <param name="handlePosition">Handle of the attachment point for the track.</param>
        /// <param name="orientation">Whether the curve turns left or right.</param>
        /// <param name="radius">Radius in meters.</param>
        /// <param name="arc">Arc in degrees.</param>
        /// <param name="maxArc">Maximum arc allowed before the curve needs to be divided into multiple parts.</param>
        /// <param name="endGrade">The grade at the end of the track.</param>
        /// <returns>The instantiated <see cref="Track"/>.</returns>
        public static Track CreateArcCurve(Transform parent, Vector3 attachPoint, Vector3 handlePosition,
            TrackOrientation orientation, float radius, float arc, float maxArc, float endGrade)
        {
            Track t = GetEmptyTrack($"[Arc Curve {orientation}][R{radius}m][{arc}°]", parent, attachPoint);

            var curves = GenerateCurveBeziers(attachPoint, handlePosition, orientation, radius, arc, maxArc, endGrade);

            BezierPoint bp = t.Curve.AddPointAt(attachPoint);
            bp.handleStyle = BezierPoint.HandleStyle.Broken;

            for (int i = 0; i < curves.Length; i++)
            {
                bp.globalHandle2 = curves[i].P1;
                bp = t.Curve.AddPointAt(curves[i].P3);
                bp.handleStyle = BezierPoint.HandleStyle.Connected;
                bp.globalHandle1 = curves[i].P2;
            }

            t.Curve.Last().handleStyle = BezierPoint.HandleStyle.Broken;
            t.Curve.Last().handle2 = Vector3.zero;

            return t;
        }

        // Switches.
        /// <summary>
        /// Instantiates a <see cref="Switch"/>.
        /// </summary>
        /// <param name="leftPrefab">Prefab of a <see cref="Switch"/> with diverging track to the left.</param>
        /// <param name="rightPrefab">Prefab of a <see cref="Switch"/> with diverging track to the right.</param>
        /// <param name="parent">The parent <see cref="Transform"/> for the new switch.</param>
        /// <param name="attachPoint">Attachment point for the track.</param>
        /// <param name="handlePosition">Handle of the attachment point for the track.</param>
        /// <param name="orientation">Wether the diverging track should exit to the left or right.</param>
        /// <param name="connectingPoint">Which of the 3 exits of the switch is connected to the attachment point.</param>
        /// <returns>The instantiated <see cref="Track"/>.</returns>
        /// <remarks>
        /// Derail Valley switches are static assets, and their tracks cannot be changed.
        /// <para>The switches are also always made at a grade of <b>0%</b>.</para>
        /// </remarks>
        public static Switch CreateVanillaSwitch(Switch leftPrefab, Switch rightPrefab, Transform parent, Vector3 attachPoint, Vector3 handlePosition,
            TrackOrientation orientation, SwitchPoint connectingPoint)
        {
            // Create switch object.
            Switch s = Object.Instantiate(orientation == TrackOrientation.Left ? leftPrefab : rightPrefab, parent);
            s.gameObject.name = $"[Switch {orientation}]";
            // Helper variables.
            Vector3 pivot;
            Quaternion rot;
            Quaternion rotRoot;

            // Rotate the switch based on the connecting point.
            if (connectingPoint == SwitchPoint.Joint)
            {
                rotRoot = Quaternion.identity;
                pivot = Vector3.zero;
            }
            else
            {
                BezierPoint bp;

                if (connectingPoint == SwitchPoint.Through)
                {
                    bp = s.GetThroughPoint();
                }
                else
                {
                    bp = s.GetDivergingPoint();
                }

                rotRoot = Quaternion.Inverse(Quaternion.LookRotation(bp.globalHandle1 - bp.position));
                pivot = bp.position;
            }

            rot = Quaternion.LookRotation(attachPoint - handlePosition);

            s.transform.position = rot * (rotRoot * -pivot) + attachPoint;
            s.transform.rotation = rot * rotRoot;

            return s;
        }

        /// <summary>
        /// Creates a switch without the limitations of the base game switches
        /// </summary>
        public static CustomSwitch CreateCustomSwitch(Transform parent, Vector3 attachPoint, Vector3 handlePosition, int switchBranchesCount,
            int connectingPoint, float radius, float arc, float endGrade)
        {
            //TODO connectingPoint

            var switchObject = new GameObject($"[Switch w/ {switchBranchesCount} branches]");
            switchObject.transform.position = attachPoint;

            var switchComponent = switchObject.AddComponent<CustomSwitch>();
            //TODO
            switchComponent.defaultBranch = 0;
            switchComponent.standSide = CustomSwitch.StandSide.LEFT;

            var tracks = new Track[switchBranchesCount];
            var length = radius * arc * Mathf.Deg2Rad;

            for (int branchIndex = 0; branchIndex < switchBranchesCount; branchIndex++)
            {
                if (switchBranchesCount % 2 == 1 && branchIndex == (switchBranchesCount-1) / 2)
                {
                    //middle track
                    tracks[branchIndex] = CreateStraight(switchObject.transform, attachPoint, handlePosition, length, endGrade);
                    continue;
                }

                TrackOrientation trackOrientation;
                float thisRadius;

                if (branchIndex < switchBranchesCount / 2.0)
                {
                    //left of center
                    trackOrientation = TrackOrientation.Left;
                    thisRadius = (branchIndex + 1) * radius;
                }
                else
                {
                    //right of center
                    trackOrientation = TrackOrientation.Right;
                    thisRadius = (switchBranchesCount - branchIndex) * radius;
                }

                var thisArc = length / thisRadius * Mathf.Rad2Deg;
                tracks[branchIndex] = CreateArcCurve(switchObject.transform, attachPoint, handlePosition, trackOrientation, thisRadius, thisArc, 360, endGrade);
            }

            switchComponent.SetTracks(tracks);
            return switchComponent;
        }

        // Yards.
        /// <summary>
        /// Creates a yard with similar shape to the ones present in the base game.
        /// </summary>
        /// <param name="leftPrefab">Prefab of a <see cref="Switch"/> with diverging track to the left.</param>
        /// <param name="rightPrefab">Prefab of a <see cref="Switch"/> with diverging track to the right.</param>
        /// <param name="parent">The parent <see cref="Transform"/> for the new track.</param>
        /// <param name="attachPoint">Attachment point for the first switch.</param>
        /// <param name="handlePosition">Handle of the attachment point for the first switch.</param>
        /// <param name="orientation">Which side the first diverging track should exit to.</param>
        /// <param name="trackDistance">The distance between the sidings.</param>
        /// <param name="mainSideTracks">Number of tracks to the first diverging side (minimum of 1).</param>
        /// <param name="otherSideTracks">Number of tracks to the opposite side to the diverging track.</param>
        /// <param name="half">Whether the yard should only have an exit on one side.</param>
        /// <param name="alternateSides">Whether both ends should have diverging tracks to the same side.</param>
        /// <param name="minimumLength">The smallest length of one of the straight sections of a siding.</param>
        /// <param name="stationId">The ID of the station this yard belongs to.</param>
        /// <param name="yardId">The ID of this yard.</param>
        /// <param name="startingTrackId">The starting number of the sidings.</param>
        /// <param name="reverseNumbers">Whether the track numbers should increase FROM the starting ID or decrease TO the starting ID.</param>
        /// <param name="sidings">An array with all sidings.</param>
        /// <returns>An array with the <see cref="Switch"/>es at each end of the yard.</returns>
        /// <remarks>
        /// For yards with the same number of tracks on both sides, it is recommended to set <paramref name="alternateSides"/> to <c>true</c>.
        /// This allows for yards with a shorter overall length, and with similar lengths on both sides. If the number of sides is different,
        /// it is a good idea to set it to <c>false</c> instead, to maximise track length on the side with more tracks. It is also recommended
        /// to have more tracks on the side defined by <paramref name="orientation"/> for the same reason.
        /// <para>
        /// The <paramref name="stationId"/> is not the name of the station. Using the base game as example, Oil Well North is <c>OWN</c>, and
        /// Sawmill is <c>SW</c>.
        /// </para>
        /// <para>
        /// The <paramref name="yardId"/> is the letter of this yard. It is recommended to start with <c>B</c>, leaving <c>A</c> to be used for
        /// tracks not belonging to any yard in a station.
        /// </para>
        /// <para>
        /// The <paramref name="startingTrackId"/> is the lowest track number in the yard. Normally this value is <c>1</c>, but in situations where
        /// 2 yards are combined into one, it might be necessary to start the 2nd yard on a higher number (in the base game, the Harbour's D yard
        /// is an example of this, where tracks D6O and D7L are separate from the rest).
        /// The <paramref name="reverseNumbers"/> option uses <paramref name="startingTrackId"/> as the lowest number. A start value of 3, and 5
        /// total tracks, will use the numbers 3, 4, 5, 6, and 7 always, but the order will be reversed.
        /// </para>
        /// </remarks>
        /// <seealso cref="CreateVanillaSwitch"/>
        public static Switch[] CreateYard(Switch leftPrefab, Switch rightPrefab, Transform parent, Vector3 attachPoint, Vector3 handlePosition,
            TrackOrientation orientation, float trackDistance, int mainSideTracks, int otherSideTracks, bool half, bool alternateSides, float minimumLength,
            string stationId, char yardId, byte startingTrackId, bool reverseNumbers, out Track[] sidings)
        {
            if (half)
            {
                return CreateHalfYard(leftPrefab, rightPrefab, parent, attachPoint, handlePosition, orientation, trackDistance,
                    mainSideTracks, otherSideTracks, minimumLength, stationId, yardId, startingTrackId, reverseNumbers,
                    out sidings);
            }
            else
            {
                return CreateFullYard(leftPrefab, rightPrefab, parent, attachPoint, handlePosition, orientation, trackDistance,
                    mainSideTracks, otherSideTracks, alternateSides, minimumLength, stationId, yardId, startingTrackId, reverseNumbers,
                    out sidings);
            }
        }

        /// <summary>
        /// Creates a yard with similar shape to the ones present in the base game.
        /// </summary>
        /// <param name="leftPrefab">Prefab of a <see cref="Switch"/> with diverging track to the left.</param>
        /// <param name="rightPrefab">Prefab of a <see cref="Switch"/> with diverging track to the right.</param>
        /// <param name="trackPrefab">The base track prefab.</param>
        /// <param name="parent">The parent <see cref="Transform"/> for the new track.</param>
        /// <param name="attachPoint">Attachment point for the first switch.</param>
        /// <param name="handlePosition">Handle of the attachment point for the first switch.</param>
        /// <param name="orientation">Which side the first diverging track should exit to.</param>
        /// <param name="trackDistance">The distance between the sidings.</param>
        /// <param name="yardOptions">Settings for the creation of the yard.</param>
        /// <param name="sidings">An array with all sidings.</param>
        /// <returns>An array with the <see cref="Switch"/>es at each end of the yard.</returns>
        /// <seealso cref="CreateYard(Switch, Switch, Track, Transform, Vector3, Vector3, TrackOrientation, float, int, int, bool, float, string, char, byte, bool)"/>
        public static Switch[] CreateYard(Switch leftPrefab, Switch rightPrefab, Transform parent, Vector3 attachPoint, Vector3 handlePosition,
            TrackOrientation orientation, float trackDistance, YardOptions yardOptions, out Track[] sidings)
        {
            return CreateYard(leftPrefab, rightPrefab, parent, attachPoint, handlePosition, orientation, trackDistance,
                yardOptions.TracksMainSide, yardOptions.TracksOtherSide, yardOptions.Half, yardOptions.AlternateSides, yardOptions.MinimumLength,
                yardOptions.StationId, yardOptions.YardId, yardOptions.StartTrackId, yardOptions.ReverseNumbers, out sidings);
        }

        private static Switch[] CreateFullYard(Switch leftPrefab, Switch rightPrefab, Transform parent, Vector3 attachPoint, Vector3 handlePosition,
            TrackOrientation orientation, float trackDistance, int mainSideTracks, int otherSideTracks, bool alternateSides, float minimumLength,
            string stationId, char yardId, byte startingTrackId, bool reverseNumbers, out Track[] sidings)
        {
            if (mainSideTracks < 1)
            {
                throw new System.ArgumentOutOfRangeException(nameof(mainSideTracks), "Main side tracks must be at least 1.");
            }

            Vector3 dir = (attachPoint - handlePosition).normalized;
            Switch start;
            Switch end;
            Switch s;
            Track t;

            // Points to connect each yard track.
            List<BezierPoint> side1 = new List<BezierPoint>();
            List<BezierPoint> side2 = new List<BezierPoint>();
            List<BezierPoint> side3 = new List<BezierPoint>();
            List<BezierPoint> side4 = new List<BezierPoint>();
            Track[] merge1 = new Track[0];
            Track[] merge2 = new Track[0];
            Track[] merge3 = new Track[0];
            Track[] merge4 = new Track[0];
            List<Track> storageTracks = new List<Track>();
            BezierPoint mid;

            // Create an empty gameobject to be the parent of the entry side.
            GameObject startObj = new GameObject();
            startObj.transform.position = attachPoint;

            // Create the switches for the first side.
            side1 = CreateSwitchSprawl(leftPrefab, rightPrefab, startObj.transform, attachPoint, handlePosition,
                orientation, mainSideTracks, trackDistance, out s, out merge1);
            mid = s.GetThroughPoint();
            start = s;

            // Check if the other side has any tracks to make.
            if (otherSideTracks > 0)
            {
                // Create a straight to separate the switches and then the other side.
                t = CreateStraight(startObj.transform, mid.position, mid.globalHandle1,
                    TrackToolsHelper.CalculateYardMidSwitchDistance(trackDistance), 0);
                side2 = CreateSwitchSprawl(leftPrefab, rightPrefab, startObj.transform,
                    t.Curve[1].position, t.Curve[1].globalHandle1, FlipOrientation(orientation), otherSideTracks,
                    trackDistance, out s, out merge2);
                mid = s.GetThroughPoint();
            }

            // Store the start of the middle track before we do the other side.
            BezierPoint midStart = mid;

            // Create an empty gameobject to be the parent of the entry side.
            GameObject endObj = new GameObject();
            endObj.transform.position = attachPoint;

            // Repeat the same thing for the other side. Depending if alternate sides is set,
            // start from the opposite side instead of the main side.
            // There's no need to alternate if there's no tracks on the other side.
            if (alternateSides && otherSideTracks > 0)
            {
                side4 = CreateSwitchSprawl(leftPrefab, rightPrefab, endObj.transform, attachPoint, attachPoint + dir,
                    orientation, otherSideTracks, trackDistance, out s, out merge4);
                mid = s.GetThroughPoint();
                // Final switch, return it later.
                end = s;

                // Create a straight to separate the switches and then the other side.
                t = CreateStraight(endObj.transform, mid.position, mid.globalHandle1,
                    TrackToolsHelper.CalculateYardMidSwitchDistance(trackDistance), 0);
                side3 = CreateSwitchSprawl(leftPrefab, rightPrefab, endObj.transform,
                    t.Curve[1].position, t.Curve[1].globalHandle1, FlipOrientation(orientation), mainSideTracks,
                    trackDistance, out s, out merge3);
                mid = s.GetThroughPoint();
            }
            else
            {
                side3 = CreateSwitchSprawl(leftPrefab, rightPrefab, endObj.transform, attachPoint, attachPoint + dir,
                    FlipOrientation(orientation), mainSideTracks, trackDistance, out s, out merge3);
                mid = s.GetThroughPoint();
                // Final switch, return it later.
                end = s;

                // Check if the other side has any tracks to make.
                if (otherSideTracks > 0)
                {
                    // Create a straight to separate the switches and then the other side.
                    t = CreateStraight(endObj.transform, mid.position, mid.globalHandle1,
                        TrackToolsHelper.CalculateYardMidSwitchDistance(trackDistance), 0);
                    side4 = CreateSwitchSprawl(leftPrefab, rightPrefab, endObj.transform,
                        t.Curve[1].position, t.Curve[1].globalHandle1, orientation, otherSideTracks, trackDistance, out s, out merge4);
                    mid = s.GetThroughPoint();
                }
            }

            // Create an empty GameObject to be the parent of the whole yard.
            GameObject yardObj = new GameObject($"[Yard {stationId} {yardId}]")
            {
                transform =
                {
                    parent = parent,
                    position = attachPoint,
                    rotation = Quaternion.LookRotation(attachPoint - handlePosition)
                }
            };

            float dist;

            // Check the distance along the direction of the yard and if there's
            // not enough distance, move the whole yard half further away.
            // Main side.
            for (int i = side1.Count - 1; i >= 0; i--)
            {
                dist = Vector3.Dot(side3[i].position - side1[i].position, dir);

                if (dist < minimumLength)
                {
                    endObj.transform.position -= dir * (dist - minimumLength);
                }
            }

            // Middle track.
            dist = Vector3.Dot(mid.position - midStart.position, dir);

            if (dist < minimumLength)
            {
                endObj.transform.position -= dir * (dist - minimumLength);
            }

            // Other side.
            for (int i = 0; i < side2.Count; i++)
            {
                dist = Vector3.Dot(side4[i].position - side2[i].position, dir);

                if (dist < minimumLength)
                {
                    endObj.transform.position -= dir * (dist - minimumLength);
                }
            }

            // Connect the 2 sides.
            System.Func<byte, byte> change;
            if (reverseNumbers)
            {
                change = (i => (byte)(i - 1));
                startingTrackId = (byte)(startingTrackId + mainSideTracks + otherSideTracks);
            }
            else
            {
                change = (i => (byte)(i + 1));
            }

            // Main side.
            for (int i = side1.Count - 1; i >= 0; i--)
            {
                if ((side1[i].position - side3[i].position).sqrMagnitude > 0)
                {
                    t = CreateStraight2Point(yardObj.transform, side1[i].position, side3[i].position);
                }
                else
                {
                    t = null;
                }

                // Merge the first with the curves so it reaches the switch.
                if (i == side1.Count - 1)
                {
                    t = TrackToolsEditor.MergeTracks(new Track[] { merge1[0], merge1[1], t, merge3[1], merge3[0] }, 0.01f, false)[0];
                    t.transform.parent = yardObj.transform;
                }

                AssignYardProperties(t, stationId, yardId, startingTrackId);
                startingTrackId = change(startingTrackId);
                storageTracks.Add(t);
            }

            // Middle track.
            t = CreateStraight2Point(yardObj.transform, midStart.position, mid.position);
            AssignYardProperties(t, stationId, yardId, startingTrackId);
            startingTrackId = change(startingTrackId);
            storageTracks.Add(t);

            // Other side.
            for (int i = 0; i < side2.Count; i++)
            {
                if ((side2[i].position - side4[i].position).sqrMagnitude > 0)
                {
                    t = CreateStraight2Point(yardObj.transform, side2[i].position, side4[i].position);
                }
                else
                {
                    t = null;
                }

                // Merge the last with the curves so it reaches the switch.
                if (i == side2.Count - 1)
                {
                    t = TrackToolsEditor.MergeTracks(new Track[] { merge2[0], merge2[1], t, merge4[1], merge4[0] }, 0.01f, false)[0];
                    t.transform.parent = yardObj.transform;
                }

                AssignYardProperties(t, stationId, yardId, startingTrackId);
                startingTrackId = change(startingTrackId);
                storageTracks.Add(t);
            }

            // Kinda ugly, moving all children from both sides to the main yard object,
            // but it simplifies yard length.
            startObj.transform.ReparentAllChildren(yardObj.transform);
            endObj.transform.ReparentAllChildren(yardObj.transform);

            Object.DestroyImmediate(startObj);
            Object.DestroyImmediate(endObj);

            sidings = storageTracks.ToArray();
            return new Switch[] { start, end };
        }

        private static Switch[] CreateHalfYard(Switch leftPrefab, Switch rightPrefab, Transform parent, Vector3 attachPoint, Vector3 handlePosition,
            TrackOrientation orientation, float trackDistance, int mainSideTracks, int otherSideTracks, float minimumLength,
            string stationId, char yardId, byte startingTrackId, bool reverseNumbers, out Track[] sidings)
        {
            if (mainSideTracks < 1)
            {
                throw new System.ArgumentOutOfRangeException(nameof(mainSideTracks), "Main side tracks must be at least 1.");
            }

            Vector3 dir = (attachPoint - handlePosition).normalized;
            Vector3 rotDir = MathHelper.RotateCW(dir.Flatten()).To3D(0);

            // Side direction of the yard.
            if (orientation == TrackOrientation.Left)
            {
                rotDir = -rotDir;
            }

            Switch start;
            Switch s;
            Track t;

            // Points to connect each yard track.
            List<BezierPoint> side1 = new List<BezierPoint>();
            List<BezierPoint> side2 = new List<BezierPoint>();
            Vector3[] side3;
            Vector3[] side4;
            Track[] merge1 = new Track[0];
            Track[] merge2 = new Track[0];
            List<Track> storageTracks = new List<Track>();
            BezierPoint mid;

            // Create an empty gameobject to be the parent of the entry side.
            GameObject startObj = new GameObject();
            startObj.transform.position = attachPoint;

            // Create the switches for the first side.
            side1 = CreateSwitchSprawl(leftPrefab, rightPrefab, startObj.transform, attachPoint, handlePosition,
                orientation, mainSideTracks, trackDistance, out s, out merge1);
            mid = s.GetThroughPoint();
            start = s;

            // Check if the other side has any tracks to make.
            if (otherSideTracks > 0)
            {
                // Create a straight to separate the switches and then the other side.
                t = CreateStraight(startObj.transform, mid.position, mid.globalHandle1,
                    TrackToolsHelper.CalculateYardMidSwitchDistance(trackDistance), 0);
                side2 = CreateSwitchSprawl(leftPrefab, rightPrefab, startObj.transform,
                    t.Curve[1].position, t.Curve[1].globalHandle1, FlipOrientation(orientation), otherSideTracks,
                    trackDistance, out s, out merge2);
                mid = s.GetThroughPoint();
            }

            // Create an empty GameObject to be the parent of the whole yard.
            GameObject yardObj = new GameObject($"[Yard {stationId} {yardId}]")
            {
                transform =
                {
                    parent = parent,
                    position = attachPoint,
                    rotation = Quaternion.LookRotation(attachPoint - handlePosition)
                }
            };

            // Create the attach points for the other side.
            side3 = new Vector3[side1.Count];

            for (int i = 0; i < side3.Length; i++)
            {
                side3[i] = attachPoint + rotDir * trackDistance * (i + 1);
            }

            Vector3 midEnd = attachPoint;

            side4 = new Vector3[side2.Count];

            for (int i = 0; i < side4.Length; i++)
            {
                side4[i] = attachPoint - rotDir * trackDistance * (i + 1);
            }

            // Function to move everything to keep them aligned.
            void MoveDistance(Vector3 distance)
            {
                for (int i = 0; i < side3.Length; i++)
                {
                    side3[i] -= distance;
                }

                midEnd -= distance;

                for (int i = 0; i < side4.Length; i++)
                {
                    side4[i] -= distance;
                }
            }

            float dist;

            // Check the distance along the direction of the yard and if there's
            // not enough distance, move the whole yard half further away.
            // Main side.
            for (int i = side1.Count - 1; i >= 0; i--)
            {
                dist = Vector3.Dot(side3[i] - side1[i].position, dir);

                if (dist < minimumLength)
                {
                    MoveDistance(dir * (dist - minimumLength));
                }
            }

            // Middle track.
            dist = Vector3.Dot(midEnd - mid.position, dir);

            if (dist < minimumLength)
            {
                MoveDistance(dir * (dist - minimumLength));
            }

            // Other side.
            for (int i = 0; i < side2.Count; i++)
            {
                dist = Vector3.Dot(side4[i] - side2[i].position, dir);

                if (dist < minimumLength)
                {
                    MoveDistance(dir * (dist - minimumLength));
                }
            }

            // Connect the 2 sides.
            System.Func<byte, byte> change;
            if (reverseNumbers)
            {
                change = (i => (byte)(i - 1));
                startingTrackId = (byte)(startingTrackId + mainSideTracks + otherSideTracks);
            }
            else
            {
                change = (i => (byte)(i + 1));
            }

            // Main side.
            for (int i = side1.Count - 1; i >= 0; i--)
            {
                if ((side1[i].position - side3[i]).sqrMagnitude > 0)
                {
                    t = CreateStraight2Point(yardObj.transform, side1[i].position, side3[i]);
                }
                else
                {
                    t = null;
                }

                // Merge the first with the curves so it reaches the switch.
                if (i == side1.Count - 1)
                {
                    t = TrackToolsEditor.MergeTracks(new Track[] { merge1[0], merge1[1], t }, 0.01f, false)[0];
                    t.transform.parent = yardObj.transform;
                }

                AssignYardProperties(t, stationId, yardId, startingTrackId);
                startingTrackId = change(startingTrackId);
                storageTracks.Add(t);
            }

            // Middle track.
            t = CreateStraight2Point(yardObj.transform, midEnd, mid.position);
            AssignYardProperties(t, stationId, yardId, startingTrackId);
            startingTrackId = change(startingTrackId);
            storageTracks.Add(t);

            // Other side.
            for (int i = 0; i < side2.Count; i++)
            {
                if ((side2[i].position - side4[i]).sqrMagnitude > 0)
                {
                    t = CreateStraight2Point(yardObj.transform, side2[i].position, side4[i]);
                }
                else
                {
                    t = null;
                }

                // Merge the last with the curves so it reaches the switch.
                if (i == side2.Count - 1)
                {
                    t = TrackToolsEditor.MergeTracks(new Track[] { merge2[0], merge2[1], t }, 0.01f, false)[0];
                    t.transform.parent = yardObj.transform;
                }

                AssignYardProperties(t, stationId, yardId, startingTrackId);
                startingTrackId = change(startingTrackId);
                storageTracks.Add(t);
            }

            // Kinda ugly, moving all children from both sides to the main yard object,
            // but it simplifies yard length.
            startObj.transform.ReparentAllChildren(yardObj.transform);

            Object.DestroyImmediate(startObj);

            sidings = storageTracks.ToArray();
            return new Switch[] { start };
        }

        // Switches on each side of the yard.
        private static List<BezierPoint> CreateSwitchSprawl(Switch leftPrefab, Switch rightPrefab, Transform parent,
            Vector3 attachPoint, Vector3 handlePosition, TrackOrientation orientation, int sideTracks, float trackDistance, out Switch start,
            out Track[] endMerge)
        {
            // Starting switch.
            // Switch stands are all on the outside of the yard.
            start = CreateVanillaSwitch(leftPrefab, rightPrefab, parent, attachPoint, handlePosition, orientation, SwitchPoint.Joint);
            start.standSide = Switch.StandSide.DIVERGING;
            start.defaultState = Switch.StandSide.THROUGH;
            Switch s = start;
            BezierPoint now = s.GetDivergingPoint();

            // Points where each track will be.
            List<BezierPoint> points = new List<BezierPoint>();
            Track t;

            // Flip sides.
            orientation = FlipOrientation(orientation);

            // Lengths of the connecting tracks so that the yard tracks themselves
            // are spaced at the set distance.
            float length = TrackToolsHelper.CalculateLengthFromDistanceYardCentre(leftPrefab, trackDistance);
            float sideL = TrackToolsHelper.CalculateLengthFromDistanceYardSides(leftPrefab, trackDistance);

            // Create a new switch for each extra track.
            for (int i = 1; i < sideTracks; i++)
            {
                t = CreateStraight(parent, now.position, now.globalHandle1, length, 0);
                s = CreateVanillaSwitch(leftPrefab, rightPrefab, parent, t.Curve[1].position, t.Curve[1].globalHandle1,
                    orientation, SwitchPoint.Joint);
                now = s.GetThroughPoint();
                points.Add(s.GetDivergingPoint());
                s.standSide = Switch.StandSide.THROUGH;
                s.defaultState = Switch.StandSide.THROUGH;
                length = sideL;
            }

            // Final track gets no switch.
            // Also store the final 2 tracks (straight and curve) to be merged with the siding, to increase its size.
            endMerge = new Track[2];
            t = CreateStraight(parent, now.position, now.globalHandle1, length, 0);
            endMerge[0] = t;
            t = CreateSwitchCurve(leftPrefab, rightPrefab, parent, t.Curve[1].position, t.Curve[1].globalHandle1,
                orientation, SwitchPoint.Joint);
            endMerge[1] = t;
            points.Add(t.Curve[1]);

            return points;
        }

        // Assign the properties to the yard track.
        private static void AssignYardProperties(Track t, string stationId, char yardId, byte trackId)
        {
            t.name = $"[Siding {yardId}{trackId}S][{t.Curve.length}m]";
            t.trackId = trackId;
            t.yardId = yardId;
            t.stationId = stationId;
            t.trackType = TrackType.Storage;
            // Colour isn't updated until OnValidate() is called so force it to be updated.
            t.Curve.drawColor = Track.COLOR_STORAGE;
        }

        // Turntables.
        /// <summary>
        /// Instantiates a <see cref="Turntable"/> and returns it.
        /// </summary>
        /// <param name="turntablePrefab">The prefab of the turntable.</param>
        /// <param name="trackPrefab">The base track prefab.</param>
        /// <param name="parent">The parent <see cref="Transform"/> for the new track.</param>
        /// <param name="attachPoint">Attachment point for the track.</param>
        /// <param name="handlePosition">Handle of the attachment point for the track.</param>
        /// <param name="radius">The radius of the turntable's track.</param>
        /// <param name="depth">How deep the turntable should be spawned to align the track.</param>
        /// <param name="rotationOffset">The turntable's rotation in relation to the attachment point.</param>
        /// <param name="tracksOffset">The offset rotation of the exit tracks.</param>
        /// <param name="angleBetweenExits">The angle between each exit track.</param>
        /// <param name="exitTrackCount">The number of exit tracks.</param>
        /// <param name="exitTrackLength">The length of the exit tracks.</param>
        /// <param name="exitTracks">All the exit tracks created.</param>
        /// <returns>The instantiated <see cref="Turntable"/>.</returns>
        public static Turntable CreateTurntable(Turntable turntablePrefab, Transform parent, Vector3 attachPoint, Vector3 handlePosition,
            float radius, float depth, float rotationOffset, float tracksOffset, float angleBetweenExits, int exitTrackCount, float exitTrackLength, out Track[] exitTracks)
        {
            // Helper variables for the turntable.
            Vector3 dir = (attachPoint - handlePosition).normalized;
            Vector3 pivot = attachPoint + dir * radius;

            // Create the turntable.
            Turntable tt = Object.Instantiate(turntablePrefab);
            tt.name = "[Turntable]";
            tt.transform.parent = parent;
            tt.transform.position = pivot - new Vector3(0, depth, 0);
            tt.transform.rotation = Quaternion.AngleAxis(rotationOffset, Vector3.up) * Quaternion.LookRotation(dir);

            // Helper variables for the exit tracks.
            Quaternion rotRoot = Quaternion.AngleAxis(rotationOffset, Vector3.up);
            Vector3 rotDir = Quaternion.AngleAxis(tracksOffset, Vector3.up) * (rotRoot * dir);
            Quaternion rot = Quaternion.AngleAxis(angleBetweenExits, Vector3.up);

            exitTracks = new Track[exitTrackCount];

            for (int i = 0; i < exitTrackCount; i++)
            {
                // Create a flat straight track for each exit.
                exitTracks[i] = CreateStraight(parent, pivot + rotDir * radius, pivot - rotDir,
                    exitTrackLength, 0);
                exitTracks[i].name = $"[Exit Track {i} {exitTrackLength}m]";
                rotDir = rot * rotDir;
            }

            return tt;
        }

        /// <summary>
        /// Instantiates a <see cref="Turntable"/> and returns it.
        /// </summary>
        /// <param name="turntablePrefab">The prefab of the turntable.</param>
        /// <param name="trackPrefab">The base track prefab.</param>
        /// <param name="parent">The parent <see cref="Transform"/> for the new track.</param>
        /// <param name="attachPoint">Attachment point for the track.</param>
        /// <param name="handlePosition">Handle of the attachment point for the track.</param>
        /// <param name="turntableOptions">Settings for the creation of the turntable.</param>
        /// <param name="exitTracks">All the exit tracks created.</param>
        /// <returns>The instantiated <see cref="Turntable"/>.</returns>
        public static Turntable CreateTurntable(Turntable turntablePrefab, Transform parent, Vector3 attachPoint, Vector3 handlePosition,
            TurntableOptions turntableOptions, out Track[] exitTracks)
        {
            return CreateTurntable(turntablePrefab, parent, attachPoint, handlePosition, turntableOptions.TurntableRadius,
                turntableOptions.TurntableDepth, turntableOptions.RotationOffset, turntableOptions.TracksOffset, turntableOptions.AngleBetweenExits,
                turntableOptions.ExitTrackCount, turntableOptions.ExitTrackLength, out exitTracks);
        }

        // Special pieces

        /// <summary>
        /// Instantiates a <see cref="BufferStop"/> and returns it.
        /// </summary>
        /// <param name="prefab">Buffer prefab.</param>
        /// <param name="parent">The parent <see cref="Transform"/> for the buffer.</param>
        /// <param name="attachPoint">Attachment point for the track.</param>
        /// <param name="handlePosition">Handle of the attachment point for the track.</param>
        /// <returns>The instantiated <see cref="BufferStop"/>.</returns>
        public static BufferStop CreateBuffer(BufferStop prefab, Transform parent, Vector3 attachPoint, Vector3 handlePosition)
        {
            BufferStop buffer = Object.Instantiate(prefab);
            buffer.transform.parent = parent;
            buffer.transform.position = attachPoint;
            buffer.transform.rotation = Quaternion.LookRotation(handlePosition - attachPoint);
            buffer.gameObject.name = "[Buffer Stop]";

            return buffer;
        }

        /// <summary>
        /// Instantiates the diverging track of a <see cref="Switch"/> only.
        /// </summary>
        /// <param name="leftPrefab">Prefab of a <see cref="Switch"/> with diverging track to the left.</param>
        /// <param name="rightPrefab">Prefab of a <see cref="Switch"/> with diverging track to the right.</param>
        /// <param name="parent">The parent <see cref="Transform"/> for the new track.</param>
        /// <param name="attachPoint">Attachment point for the track.</param>
        /// <param name="handlePosition">Handle of the attachment point for the track.</param>
        /// <param name="orientation">Which side the curve turns to.</param>
        /// <returns>The instantiated <see cref="Track"/>.</returns>
        public static Track CreateSwitchCurve(Switch leftPrefab, Switch rightPrefab, Transform parent, Vector3 attachPoint, Vector3 handlePosition,
            TrackOrientation orientation, SwitchPoint connectingPoint)
        {
            Track t;
            if (orientation == TrackOrientation.Left)
            {
                t = Object.Instantiate(leftPrefab.DivergingTrack);
            }
            else
            {
                t = Object.Instantiate(rightPrefab.DivergingTrack);
            }
            Switch s = orientation == TrackOrientation.Left ? leftPrefab : rightPrefab;

            Vector3 offset = -t.Curve[0].localPosition;

            t.Curve[0].localPosition += offset;
            t.Curve[1].localPosition += offset;

            // Helper variables.
            Vector3 pivot;
            Quaternion rot;
            Quaternion rotRoot;

            // Rotate the switch based on the connecting point.
            if (connectingPoint == SwitchPoint.Joint)
            {
                rotRoot = Quaternion.identity;
                pivot = Vector3.zero;
            }
            else
            {
                BezierPoint bp;

                if (connectingPoint == SwitchPoint.Through)
                {
                    bp = s.GetThroughPoint();
                }
                else
                {
                    bp = s.GetDivergingPoint();
                }

                rotRoot = Quaternion.Inverse(Quaternion.LookRotation(bp.globalHandle1 - bp.position));
                pivot = bp.position;
            }

            rot = Quaternion.LookRotation(attachPoint - handlePosition);

            t.transform.parent = parent;
            t.transform.position = rot * (rotRoot * -pivot) + attachPoint;
            t.transform.rotation = rot * rotRoot;
            t.name = "[Diverging Curve]";

            return t;
        }

        /// <summary>
        /// Instantiates a <see cref="Track"/> that smoothly connects 2 <see cref="BezierPoint"/>.
        /// </summary>
        /// <param name="parent">The parent <see cref="Transform"/> for the new track.</param>
        /// <param name="p0">The starting point of the track.</param>
        /// <param name="p1">The ending point of the track.</param>
        /// <param name="useHandle2Start">Which of <paramref name="p0"/>'s handles to use.</param>
        /// <param name="useHandle2End">Which of <paramref name="p1"/>'s handles to use.</param>
        /// <param name="lengthMultiplier">The multiplier for the final handle length.</param>
        /// <returns>The instantiated <see cref="Track"/>.</returns>
        public static Track CreateConnect2Point(Transform parent, BezierPoint p0, BezierPoint p1,
            bool useHandle2Start, bool useHandle2End, float lengthMultiplier)
        {
            return CreateConnect2Point(parent, p0.position, p1.position,
                useHandle2Start ? p0.globalHandle2 : p0.globalHandle1,
                useHandle2End ? p1.globalHandle2 : p1.globalHandle1,
                lengthMultiplier);
        }

        /// <summary>
        /// Instantiates a <see cref="Track"/> that smoothly connects 2 points.
        /// </summary>
        /// <param name="parent">The parent <see cref="Transform"/> for the new track.</param>
        /// <param name="p0">The starting point of the track.</param>
        /// <param name="p1">The ending point of the track.</param>
        /// <param name="h0">The handle at the starting point.</param>
        /// <param name="h1">The handle at the ending point.</param>
        /// <param name="lengthMultiplier">The multiplier for the final handle length.</param>
        /// <returns>The instantiated <see cref="Track"/>.</returns>
        public static Track CreateConnect2Point(Transform parent, Vector3 p0, Vector3 p1,
            Vector3 h0, Vector3 h1, float lengthMultiplier)
        {
            // One third of the distance is a good base length for smoothing.
            float length = (p0 - p1).magnitude * MathHelper.OneThird * lengthMultiplier;
            // Pick the correct handles.
            h0 = (h0 - p0).normalized * length;
            h1 = (h1 - p1).normalized * length;

            BezierPoint bp;
            Track t = GetEmptyTrack();
            t.transform.parent = parent;
            t.transform.position = p0;

            // Assign the points to the curve.
            bp = t.Curve.AddPointAt(p0);
            bp.handleStyle = BezierPoint.HandleStyle.Broken;
            bp.globalHandle2 = p0 - h0;

            bp = t.Curve.AddPointAt(p1);
            bp.handleStyle = BezierPoint.HandleStyle.Broken;
            bp.globalHandle1 = p1 - h1;

            t.gameObject.name = $"[Connecting Track {t.GetHorizontalLength():F3}m]";

            return t;
        }

        /// <summary>
        /// Creates a crossover.
        /// </summary>
        /// <param name="leftPrefab">Prefab of a <see cref="Switch"/> with diverging track to the left.</param>
        /// <param name="rightPrefab">Prefab of a <see cref="Switch"/> with diverging track to the right.</param>
        /// <param name="parent">The parent <see cref="Transform"/> for the new track.</param>
        /// <param name="attachPoint">Attachment point for the track.</param>
        /// <param name="handlePosition">Handle of the attachment point for the track.</param>
        /// <param name="orientation">The side of the parallel track.</param>
        /// <param name="trackDistance">The distance between the parallel tracks.</param>
        /// <param name="trailing">Whether to the crossover is in front or comes from behind.</param>
        /// <param name="switchDistance">The distance between the switches on the same track.</param>
        /// <returns>An array with the <see cref="Switch"/> at the attachment point (index <c>0</c>) and the other <see cref="Switch"/> (index <c>1</c>).</returns>
        public static Switch[] CreateCrossover(Switch leftPrefab, Switch rightPrefab, Transform parent,
            Vector3 attachPoint, Vector3 handlePosition, TrackOrientation orientation, float trackDistance, bool trailing,
            float switchDistance)
        {
            GameObject crossObj = new GameObject($"[Crossover {orientation}]");
            crossObj.transform.parent = parent;
            crossObj.transform.position = attachPoint;

            SwitchPoint sp;

            if (trailing)
            {
                sp = SwitchPoint.Through;
            }
            else
            {
                sp = SwitchPoint.Joint;
            }

            Switch s1 = CreateVanillaSwitch(leftPrefab, rightPrefab, crossObj.transform,
                attachPoint, handlePosition, orientation, sp);
            BezierPoint bp1 = s1.GetDivergingPoint();

            Vector3 point = s1.GetThroughPoint().position;
            Vector3 dir = (point - s1.GetJointPoint().position).normalized;

            Vector3 offset = (orientation == TrackOrientation.Left ?
                MathHelper.RotateCCW(dir.Flatten()) :
                MathHelper.RotateCW(dir.Flatten())).To3D(0) * trackDistance;

            point = point + (dir * switchDistance) + offset;

            Switch s2 = CreateVanillaSwitch(leftPrefab, rightPrefab, crossObj.transform,
                point, point - dir, orientation, SwitchPoint.Through);
            BezierPoint bp2 = s2.GetDivergingPoint();

            CreateConnect2Point(crossObj.transform, bp1, bp2, false, false, 1.0f);

            return new Switch[] { s1, s2 };
        }

        /// <summary>
        /// Creates a scissors crossover (2 crossovers in the same place).
        /// </summary>
        /// <param name="leftPrefab">Prefab of a <see cref="Switch"/> with diverging track to the left.</param>
        /// <param name="rightPrefab">Prefab of a <see cref="Switch"/> with diverging track to the right.</param>
        /// <param name="parent">The parent <see cref="Transform"/> for the new track.</param>
        /// <param name="attachPoint">Attachment point for the track.</param>
        /// <param name="handlePosition">Handle of the attachment point for the track.</param>
        /// <param name="orientation">The side of the parallel track.</param>
        /// <param name="trackDistance">The distance between the parallel tracks.</param>
        /// <param name="switchDistance">The distance between the switches on the same track.</param>
        /// <returns>
        /// An array of <see cref="Switch"/>es in the following order: [0] attach [1] opposite to 0 [2] next to 0 [3] opposite to 2.
        /// </returns>
        public static Switch[] CreateScissorsCrossover(Switch leftPrefab, Switch rightPrefab, Transform parent,
            Vector3 attachPoint, Vector3 handlePosition, TrackOrientation orientation, float trackDistance, float switchDistance)
        {
            // Create 2 crossovers offset from eachother.
            var c1 = CreateCrossover(leftPrefab, rightPrefab, parent, attachPoint, handlePosition, orientation,
                trackDistance, false, switchDistance);

            Vector3 dir = (attachPoint - handlePosition).normalized;

            Vector3 offset = (orientation == TrackOrientation.Left ?
                MathHelper.RotateCCW(dir.Flatten()) :
                MathHelper.RotateCW(dir.Flatten())).To3D(0) * trackDistance;

            var c2 = CreateCrossover(leftPrefab, rightPrefab, parent, attachPoint + offset, handlePosition + offset,
                FlipOrientation(orientation), trackDistance, false, switchDistance);

            // Reparent the 2nd crossover's pieces to the first, and rename.
            var t1 = c1[0].transform.parent;
            var t2 = c2[0].transform.parent;

            t2.ReparentAllChildren(t1);
            Object.DestroyImmediate(t2.gameObject);
            t1.gameObject.name = "[Scissors Crossover]";

            // Join the 2 crossovers.
            CreateConnect2Point(t1, c1[0].GetThroughPoint(), c2[1].GetThroughPoint(), false, false, 1.0f);
            CreateConnect2Point(t1, c2[0].GetThroughPoint(), c1[1].GetThroughPoint(), false, false, 1.0f);

            // Return all 4 switches.
            return new Switch[] { c1[0], c1[1], c2[0], c2[1] };
        }

        /// <summary>
        /// Creates a double slip.
        /// </summary>
        /// <param name="leftPrefab">Prefab of a <see cref="Switch"/> with diverging track to the left.</param>
        /// <param name="rightPrefab">Prefab of a <see cref="Switch"/> with diverging track to the right.</param>
        /// <param name="parent">The parent <see cref="Transform"/> for the new track.</param>
        /// <param name="attachPoint">Attachment point for the track.</param>
        /// <param name="handlePosition">Handle of the attachment point for the track.</param>
        /// <param name="orientation">The side to which the first switch turns to.</param>
        /// <param name="crossAngle">The angle at which the 2 middle tracks cross eachother.</param>
        /// <returns>
        /// An array of <see cref="Switch"/>es in the following order: [0] attach [1] diverging attach [2] opposite to 0 [3] opposite to 1.
        /// </returns>
        public static Switch[] CreateDoubleSlip(Switch leftPrefab, Switch rightPrefab, Transform parent,
            Vector3 attachPoint, Vector3 handlePosition, TrackOrientation orientation, float crossAngle)
        {
            // Create the parent object.
            GameObject obj = new GameObject("[Double Slip]");
            obj.transform.parent = parent;
            obj.transform.position = attachPoint;

            // Double slips use the switch radius for the curve.
            float radius = TrackToolsHelper.CalculateSwitchRadius(leftPrefab);
            // The minimum angle of the slip is double that of a single switch.
            float minAngle = TrackToolsHelper.CalculateSwitchAngle(leftPrefab) * Mathf.Rad2Deg;
            float arc = Mathf.Clamp(crossAngle - (minAngle * 2.0f), 0.1f, 90.0f - (minAngle * 2.0f));
            BezierPoint bp;

            // First side.
            // Creates a switch, a curve, and then another switch connected through its diverging track.
            Switch s00 = CreateVanillaSwitch(leftPrefab, rightPrefab, obj.transform, attachPoint, handlePosition, orientation, SwitchPoint.Joint);
            bp = s00.GetDivergingPoint();
            bp = CreateArcCurve(obj.transform, bp.position, bp.globalHandle1, orientation, radius, arc, 180.0f, 0).Curve.Last();
            Switch s01 = CreateVanillaSwitch(leftPrefab, rightPrefab, obj.transform, bp.position, bp.globalHandle1,
                FlipOrientation(orientation), SwitchPoint.Diverging);

            // Calculate the middle crossover's position by interesecting the 2 through tracks.
            bp = s00.GetJointPoint();
            Vector3 dir = bp.globalHandle2 - bp.position;
            bp = s01.GetJointPoint();
            Vector3 mid = MathHelper.LineLineIntersection(
                attachPoint.Flatten(), (attachPoint + dir).Flatten(),
                bp.position.Flatten(), bp.globalHandle2.Flatten()).To3D(attachPoint.y);
            Vector3 next = MathHelper.MirrorAround(attachPoint, mid);

            // Repeat the process for the other side.
            Switch s10 = CreateVanillaSwitch(leftPrefab, rightPrefab, obj.transform, next, next + dir, orientation, SwitchPoint.Joint);
            bp = s10.GetDivergingPoint();
            bp = CreateArcCurve(obj.transform, bp.position, bp.globalHandle1, orientation, radius, arc, 180.0f, 0).Curve.Last();
            Switch s11 = CreateVanillaSwitch(leftPrefab, rightPrefab, obj.transform, bp.position, bp.globalHandle1,
                FlipOrientation(orientation), SwitchPoint.Diverging);

            CreateConnect2Point(obj.transform, s00.GetThroughPoint(), s10.GetThroughPoint(), false, false, 1.0f);
            CreateConnect2Point(obj.transform, s01.GetThroughPoint(), s11.GetThroughPoint(), false, false, 1.0f);

            return new Switch[] { s00, s01, s10, s11 };
        }

        /// <summary>
        /// Instantiates a <see cref="Track"/> from a cubic bezier.
        /// </summary>
        /// <param name="parent">The parent <see cref="Transform"/> for the new track.</param>
        /// <param name="p0">The starting point of the track.</param>
        /// <param name="p1">The handle at the starting point.</param>
        /// <param name="p2">The handle at the ending point.</param>
        /// <param name="p3">The ending point of the track.</param>
        /// <returns>The instantiated <see cref="Track"/>.</returns>
        public static Track CreateBezier(Transform parent, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            BezierPoint bp;
            Track t = GetEmptyTrack();
            t.transform.parent = parent;
            t.transform.position = p0;

            // Assign the points to the curve.
            bp = t.Curve.AddPointAt(p0);
            bp.handleStyle = BezierPoint.HandleStyle.Broken;
            bp.globalHandle2 = p1;

            bp = t.Curve.AddPointAt(p3);
            bp.handleStyle = BezierPoint.HandleStyle.Broken;
            bp.globalHandle1 = p2;

            t.gameObject.name = $"[Bezier Track {t.GetHorizontalLength():F3}m]";

            return t;
        }
    }
}
