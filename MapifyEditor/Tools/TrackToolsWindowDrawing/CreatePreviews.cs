using Mapify.Editor.Utils;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static Mapify.Editor.Tools.ToolEnums;

#if UNITY_EDITOR
namespace Mapify.Editor.Tools
{
    public partial class TrackToolsWindow
    {
        private List<PreviewPointCache> _newCache = new List<PreviewPointCache>();
        private List<PreviewPointCache> _nextCache = new List<PreviewPointCache>();
        private List<PreviewPointCache> _backCache = new List<PreviewPointCache>();

        private void CreatePiecePreviews()
        {
            ClearPreviews();
            DoNullCheck();

            if (!_isOpen)
            {
                return;
            }

            if (_drawNewPreview)
            {
                AttachPoint ap = new AttachPoint(
                    _currentParent ? _currentParent.position : Vector3.zero,
                    _currentParent ? _currentParent.forward : Vector3.forward);

                ap.Handle = ap.Position - ap.Handle;

                if (CheckGrade(ap.GetGrade()))
                {
                    _newCache.Add(new PreviewPointCache(ap));
                    _newCache[0].Tooltip = PreviewPointCache.NewString;
                }
            }

            switch (_selectionType)
            {
                case SelectionType.Track:
                    CacheTrack(CurrentTrack);
                    break;
                case SelectionType.BezierPoint:
                    if (!Mathf.Approximately(CurrentPoint.handle1.sqrMagnitude, 0) &&
                        CheckGrade(CurrentPoint.GetGradeBackwards()))
                    {
                        _nextCache.Add(new PreviewPointCache(
                            new AttachPoint(CurrentPoint.position, CurrentPoint.globalHandle1)));
                        _nextCache[0].Tooltip = PreviewPointCache.NextString;
                    }
                    if (!Mathf.Approximately(CurrentPoint.handle2.sqrMagnitude, 0) &&
                        CheckGrade(CurrentPoint.GetGradeForwards()))
                    {
                        _backCache.Add(new PreviewPointCache(
                            new AttachPoint(CurrentPoint.position, CurrentPoint.globalHandle2)));
                        _backCache[0].Tooltip = PreviewPointCache.BackString;
                    }
                    break;
                case SelectionType.Switch:
                    CacheTrack(CurrentSwitch.ThroughTrack);
                    if (IsAllowedCreation(CurrentSwitch.DivergingTrack, false))
                    {
                        _nextCache.Add(new PreviewPointCache(
                            new AttachPoint(CurrentSwitch.DivergingTrack.Curve.Last().position, CurrentSwitch.DivergingTrack.Curve.Last().globalHandle1)));
                        _nextCache[_nextCache.Count - 1].Tooltip = PreviewPointCache.DivString;
                    }
                    break;
                case SelectionType.Turntable:
                    CacheTrack(CurrentTurntable.Track);
                    break;
                default:
                    break;
            }

            switch (_currentPiece)
            {
                case TrackPiece.Straight:
                    foreach (var cache in _newCache)
                    {
                        cache.Lines = new Vector3[][] { TrackToolsCreator.Previews.PreviewStraight(
                            cache.Attach.Position, cache.Attach.Handle,
                            _length, _endGrade, out cache.Points, _sampleCount) };
                    }
                    foreach (var cache in _nextCache)
                    {
                        cache.Lines = new Vector3[][] { TrackToolsCreator.Previews.PreviewStraight(
                            cache.Attach.Position, cache.Attach.Handle,
                            _length, _endGrade, out cache.Points, _sampleCount) };
                    }
                    foreach (var cache in _backCache)
                    {
                        cache.Lines = new Vector3[][] { TrackToolsCreator.Previews.PreviewStraight(
                            cache.Attach.Position, cache.Attach.Handle,
                            _length, _endGrade, out cache.Points, _sampleCount) };
                    }
                    break;
                case TrackPiece.Curve:
                    foreach (var cache in _newCache)
                    {
                        cache.Lines = new Vector3[][] { TrackToolsCreator.Previews.PreviewArcCurve(
                            cache.Attach.Position, cache.Attach.Handle,
                            _orientation, _radius, _arc, _maxArcPerPoint, _endGrade, out cache.Points, _sampleCount) };
                    }
                    foreach (var cache in _nextCache)
                    {
                        cache.Lines = new Vector3[][] { TrackToolsCreator.Previews.PreviewArcCurve(
                            cache.Attach.Position, cache.Attach.Handle,
                            _orientation, _radius, _arc, _maxArcPerPoint, _endGrade, out cache.Points, _sampleCount) };
                    }
                    foreach (var cache in _backCache)
                    {
                        cache.Lines = new Vector3[][] { TrackToolsCreator.Previews.PreviewArcCurve(
                            cache.Attach.Position, cache.Attach.Handle,
                            _orientation, _radius, _arc, _maxArcPerPoint, _endGrade, out cache.Points, _sampleCount) };
                    }
                    break;
                case TrackPiece.Switch:
                    if (LeftSwitch && RightSwitch)
                    {
                        foreach (var cache in _newCache)
                        {
                            cache.Lines = TrackToolsCreator.Previews.PreviewSwitch(GetCurrentSwitchPrefab(),
                                cache.Attach.Position, cache.Attach.Handle,
                                _connectingPoint, _sampleCount);
                        }
                        foreach (var cache in _nextCache)
                        {
                            cache.Lines = TrackToolsCreator.Previews.PreviewSwitch(GetCurrentSwitchPrefab(),
                                cache.Attach.Position, cache.Attach.Handle,
                                _connectingPoint, _sampleCount);
                        }
                    foreach (var cache in _backCache)
                        {
                            cache.Lines = TrackToolsCreator.Previews.PreviewSwitch(GetCurrentSwitchPrefab(),
                                cache.Attach.Position, cache.Attach.Handle,
                                _connectingPoint, _sampleCount);
                        }
                    }
                    break;
                case TrackPiece.Yard:
                    if (LeftSwitch && RightSwitch)
                    {
                        foreach (var cache in _newCache)
                        {
                            cache.Lines = TrackToolsCreator.Previews.PreviewYard(LeftSwitch, RightSwitch,
                                cache.Attach.Position, cache.Attach.Handle,
                                _orientation, _trackDistance, _yardOptions, _sampleCount);
                        }
                        foreach (var cache in _nextCache)
                        {
                            cache.Lines = TrackToolsCreator.Previews.PreviewYard(LeftSwitch, RightSwitch,
                                cache.Attach.Position, cache.Attach.Handle,
                                _orientation, _trackDistance, _yardOptions, _sampleCount);
                        }
                        foreach (var cache in _backCache)
                        {
                            cache.Lines = TrackToolsCreator.Previews.PreviewYard(LeftSwitch, RightSwitch,
                                cache.Attach.Position, cache.Attach.Handle,
                                _orientation, _trackDistance, _yardOptions, _sampleCount);
                        }
                    }
                    break;
                case TrackPiece.Turntable:
                    foreach (var cache in _newCache)
                    {
                        cache.Lines = TrackToolsCreator.Previews.PreviewTurntable(
                            cache.Attach.Position, cache.Attach.Handle,
                            _turntableOptions, _sampleCount);
                    }
                    foreach (var cache in _nextCache)
                    {
                        cache.Lines = TrackToolsCreator.Previews.PreviewTurntable(
                            cache.Attach.Position, cache.Attach.Handle,
                            _turntableOptions, _sampleCount);
                    }
                    foreach (var cache in _backCache)
                    {
                        cache.Lines = TrackToolsCreator.Previews.PreviewTurntable(
                            cache.Attach.Position, cache.Attach.Handle,
                            _turntableOptions, _sampleCount);
                    }
                    break;
                case TrackPiece.Special:
                    SpecialPreviews();
                    break;
                default:
                    break;
            }
        }

        private void SpecialPreviews()
        {
            switch (_currentSpecial)
            {
                case SpecialTrackPiece.Buffer:
                    break;
                case SpecialTrackPiece.SwitchCurve:
                    if (LeftSwitch && RightSwitch)
                    {
                        foreach (var cache in _newCache)
                        {
                            cache.Lines = new Vector3[1][];
                            System.Array.Copy(TrackToolsCreator.Previews.PreviewSwitch(GetCurrentSwitchPrefab(),
                                cache.Attach.Position, cache.Attach.Handle,
                                _connectingPoint, _sampleCount), 1, cache.Lines, 0, 1);
                        }
                        foreach (var cache in _nextCache)
                        {
                            cache.Lines = new Vector3[1][];
                            System.Array.Copy(TrackToolsCreator.Previews.PreviewSwitch(GetCurrentSwitchPrefab(),
                                cache.Attach.Position, cache.Attach.Handle,
                                _connectingPoint, _sampleCount), 1, cache.Lines, 0, 1);
                        }
                        foreach (var cache in _backCache)
                        {
                            cache.Lines = new Vector3[1][];
                            System.Array.Copy(TrackToolsCreator.Previews.PreviewSwitch(GetCurrentSwitchPrefab(),
                                cache.Attach.Position, cache.Attach.Handle,
                                _connectingPoint, _sampleCount), 1, cache.Lines, 0, 1);
                        }
                    }
                    break;
                case SpecialTrackPiece.Connect2:
                    {
                        BezierPoint p0 = null;
                        BezierPoint p1 = null;

                        switch (_selectionType)
                        {
                            case SelectionType.Track:
                                p0 = _useHandle2Start ? _selectedTracks[0].Curve[0] : _selectedTracks[0].Curve.Last();
                                if (_selectedTracks.Length > 1)
                                {
                                    p1 = _useHandle2End ? _selectedTracks[1].Curve[0] : _selectedTracks[1].Curve.Last();
                                }
                                break;
                            case SelectionType.BezierPoint:
                                p0 = _selectedPoints[0];
                                if (_selectedPoints.Length > 1)
                                {
                                    p1 = _selectedPoints[1];
                                }
                                break;
                            case SelectionType.None:
                            default:
                                break;
                        }

                        if (p0)
                        {
                            AttachPoint a0 = new AttachPoint(p0.position,
                                _useHandle2Start ? p0.globalHandle2 : p0.globalHandle1);

                            _nextCache.Add(new PreviewPointCache(a0));
                            _nextCache[0].Points = new Vector3[] { a0.Position,
                                MathHelper.MirrorAround(a0.Handle, a0.Position) };

                            _nextCache[0].Lines = new Vector3[][] { _nextCache[0].Points };
                            _nextCache[0].DrawButton = false;
                        }

                        if (p0 && p1)
                        {
                            AttachPoint a1 = new AttachPoint(p1.position,
                                _useHandle2End ? p1.globalHandle2 : p1.globalHandle1);

                            _backCache.Add(new PreviewPointCache(a1));
                            _backCache[0].Points = new Vector3[] { a1.Position,
                                MathHelper.MirrorAround(a1.Handle, a1.Position) };

                            _backCache[0].Lines = new Vector3[][] { _backCache[0].Points };
                            _backCache[0].DrawButton = false;

                            _newCache[0].Lines = new Vector3[][] { TrackToolsCreator.Previews.PreviewConnect2(
                                p0.position,
                                _useHandle2Start ? p0.globalHandle2 : p0.globalHandle1,
                                p1.position,
                                _useHandle2End ? p1.globalHandle2 : p1.globalHandle1,
                                _lengthMultiplier,
                                _sampleCount) };
                        }
                    }
                    break;
                case SpecialTrackPiece.Crossover:
                    if (LeftSwitch && RightSwitch)
                    {
                        foreach (var cache in _newCache)
                        {
                            cache.Lines = TrackToolsCreator.Previews.PreviewCrossover(GetCurrentSwitchPrefab(),
                                cache.Attach.Position, cache.Attach.Handle,
                                _orientation, _trackDistance, _isTrailing, _switchDistance, _sampleCount);
                        }
                        foreach (var cache in _nextCache)
                        {
                            cache.Lines = TrackToolsCreator.Previews.PreviewCrossover(GetCurrentSwitchPrefab(),
                                cache.Attach.Position, cache.Attach.Handle,
                                _orientation, _trackDistance, _isTrailing, _switchDistance, _sampleCount);
                        }
                        foreach (var cache in _backCache)
                        {
                            cache.Lines = TrackToolsCreator.Previews.PreviewCrossover(GetCurrentSwitchPrefab(),
                                cache.Attach.Position, cache.Attach.Handle,
                                _orientation, _trackDistance, _isTrailing, _switchDistance, _sampleCount);
                        }
                    }
                    break;
                case SpecialTrackPiece.ScissorsCrossover:
                    if (LeftSwitch && RightSwitch)
                    {
                        foreach (var cache in _newCache)
                        {
                            cache.Lines = TrackToolsCreator.Previews.PreviewScissorsCrossover(LeftSwitch, RightSwitch,
                                cache.Attach.Position, cache.Attach.Handle,
                                _orientation, _trackDistance, _switchDistance, _sampleCount);
                        }
                        foreach (var cache in _nextCache)
                        {
                            cache.Lines = TrackToolsCreator.Previews.PreviewScissorsCrossover(LeftSwitch, RightSwitch,
                                cache.Attach.Position, cache.Attach.Handle,
                                _orientation, _trackDistance, _switchDistance, _sampleCount);
                        }
                        foreach (var cache in _backCache)
                        {
                            cache.Lines = TrackToolsCreator.Previews.PreviewScissorsCrossover(LeftSwitch, RightSwitch,
                                cache.Attach.Position, cache.Attach.Handle,
                                _orientation, _trackDistance, _switchDistance, _sampleCount);
                        }
                    }
                    break;
                case SpecialTrackPiece.DoubleSlip:
                    if (LeftSwitch && RightSwitch)
                    {
                        foreach (var cache in _newCache)
                        {
                            cache.Lines = TrackToolsCreator.Previews.PreviewDoubleSlip(LeftSwitch, RightSwitch,
                                cache.Attach.Position, cache.Attach.Handle,
                                _orientation, _crossAngle, _sampleCount);
                        }
                        foreach (var cache in _nextCache)
                        {
                            cache.Lines = TrackToolsCreator.Previews.PreviewDoubleSlip(LeftSwitch, RightSwitch,
                                cache.Attach.Position, cache.Attach.Handle,
                                _orientation, _crossAngle, _sampleCount);
                        }
                        foreach (var cache in _backCache)
                        {
                            cache.Lines = TrackToolsCreator.Previews.PreviewDoubleSlip(LeftSwitch, RightSwitch,
                                cache.Attach.Position, cache.Attach.Handle,
                                _orientation, _crossAngle, _sampleCount);
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        private void ClearPreviews()
        {
            _newCache.Clear();
            _nextCache.Clear();
            _backCache.Clear();

            TrackToolsCreator.Previews.CachedYard = null;
        }

        private void CacheTrack(Track t)
        {
            if (IsAllowedCreation(t, false))
            {
                _nextCache.Add(new PreviewPointCache(
                    new AttachPoint(t.Curve.Last().position, t.Curve.Last().globalHandle1)));
                _nextCache[0].Tooltip = PreviewPointCache.NextString;
            }
            if (IsAllowedCreation(t, true))
            {
                _backCache.Add(new PreviewPointCache(
                    new AttachPoint(t.Curve[0].position, t.Curve[0].globalHandle2)));
                _backCache[0].Tooltip = PreviewPointCache.BackString;
            }
        }

        private void CreateEditingPreviews()
        {
            switch (_editingMode)
            {
                case EditingMode.InsertPoint:

                    break;
                default:
                    break;
            }
        }
    }
}
#endif
