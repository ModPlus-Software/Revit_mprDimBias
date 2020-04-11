namespace mprDimBias.Body
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Application;
    using Autodesk.Revit.DB;
    using ModPlusAPI;

    public class AdvancedDimension
    {
        private readonly Dimension _dimension;
        private readonly double _textHeight;
        private readonly double _scale;
        private readonly DimInfo _info;
        private readonly bool _isValid;
        private readonly List<List<AdvancedDimensionSegment>> _advancedCorrectSegmentSets;
        private readonly bool _moveDownInsteadSide;

        public AdvancedDimension(Dimension dimension)
        {
            var doc = dimension.Document;
            _dimension = dimension;

            // Получаю высоту текста размера из его типа
            _textHeight = dimension.DimensionType.get_Parameter(BuiltInParameter.TEXT_SIZE).AsDouble();

            // get scale
            _scale = doc.ActiveView.Scale;

            // смещать размер внизу, а не в сторону по возможности
            _moveDownInsteadSide = bool.TryParse(UserConfigFile.GetValue("mprDimBias", "MoveDownInsteadSide"), out var b) && b;

            try
            {
                var advancedSegments = new List<AdvancedDimensionSegment>();
                if (_dimension.DimensionShape == DimensionShape.Linear)
                {
                    // dimension segments
                    var tempDimensionSegments = _dimension.Segments.Cast<DimensionSegment>().ToList();

                    if (tempDimensionSegments.Count == 1)
                    {
                        advancedSegments.Add(new AdvancedDimensionSegment(tempDimensionSegments[0], null, null));
                    }
                    else if (tempDimensionSegments.Count == 2)
                    {
                        advancedSegments.Add(new AdvancedDimensionSegment(
                            tempDimensionSegments[0], null, tempDimensionSegments[1]));
                        advancedSegments.Add(new AdvancedDimensionSegment(
                            tempDimensionSegments[1], tempDimensionSegments[0], null));
                    }
                    else
                    {
                        for (var i = 0; i < tempDimensionSegments.Count; i++)
                        {
                            if (i == 0)
                            {
                                advancedSegments.Add(new AdvancedDimensionSegment(tempDimensionSegments[i], null,
                                    tempDimensionSegments[i + 1]));
                            }
                            else if (i != tempDimensionSegments.Count - 1)
                            {
                                advancedSegments.Add(new AdvancedDimensionSegment(
                                    tempDimensionSegments[i], tempDimensionSegments[i - 1],
                                    tempDimensionSegments[i + 1]));
                            }
                            else
                            {
                                advancedSegments.Add(new AdvancedDimensionSegment(
                                    tempDimensionSegments[i], tempDimensionSegments[i - 1], null));
                            }
                        }
                    }

                    _advancedCorrectSegmentSets = new List<List<AdvancedDimensionSegment>>();
                    advancedSegments.ForEach(s => s.CheckNeedCorrection(_scale, _textHeight));
                    foreach (var segment in advancedSegments)
                    {
                        if (segment.NeedCorrect)
                        {
                            if (!_advancedCorrectSegmentSets.Any())
                                _advancedCorrectSegmentSets.Add(new List<AdvancedDimensionSegment>());
                            _advancedCorrectSegmentSets.Last().Add(segment);
                        }
                        else
                        {
                            _advancedCorrectSegmentSets.Add(new List<AdvancedDimensionSegment>());
                        }
                    }

                    for (var i = _advancedCorrectSegmentSets.Count - 1; i >= 0; i--)
                    {
                        if (_advancedCorrectSegmentSets[i].Count == 0)
                            _advancedCorrectSegmentSets.RemoveAt(i);
                    }

                    if (_dimension.Curve is Line line)
                    {
                        _info = new DimInfo(doc.ActiveView, line.Direction);
                        _isValid = true;
                    }
                    else
                    {
                        _isValid = true;
                    }
                }
                else if (_dimension.DimensionShape == DimensionShape.Radial ||
                         _dimension.DimensionShape == DimensionShape.Diameter)
                {
                    if (_dimension.LeaderEndPosition != null)
                    {
                        var directionVector = _dimension.Origin.Normalize();
                        _info = new DimInfo(doc.ActiveView, directionVector);
                        _isValid = true;
                    }
                    else
                    {
                        _isValid = false;
                    }
                }
                else
                {
                    _isValid = false;
                }
            }
            catch
            {
                _isValid = false;
            }
        }

        /// <summary>
        /// Выполнить смещение текста в размере
        /// </summary>
        /// <returns>True - если смещение было выполнено (размер был изменен)</returns>
        public bool SetMoveForCorrect()
        {
            if (!_isValid)
                return false;

            try
            {
                var modified = false;

                if (_advancedCorrectSegmentSets != null && _advancedCorrectSegmentSets.Count != 0)
                {
                    foreach (var advancedDimensionSegments in _advancedCorrectSegmentSets)
                    {
                        var segmentsArray = new List<List<AdvancedDimensionSegment>>();
                        for (var i = 0; i < advancedDimensionSegments.Count; i++)
                        {
                            if (i == 0 || segmentsArray.Last().Last().AfterSegment == null)
                            {
                                segmentsArray.Add(new List<AdvancedDimensionSegment> { advancedDimensionSegments[i] });
                            }
                            else
                            {
                                segmentsArray.Last().Add(advancedDimensionSegments[i]);
                            }
                        }

                        foreach (var segments in segmentsArray)
                        {
                            CorrectDimTolerance(segments);
                        }
                    }

                    modified = true;
                }
                else
                {
                    if (_dimension.ValueString != null)
                    {
                        var stringLen = _dimension.ValueString.Length * _textHeight * _scale * MprDimBiasApp.OffsetFactor;
                        var value = _dimension.Value;
                        if (stringLen >= value.GetValueOrDefault() && value.HasValue)
                        {
                            modified = true;
                            SimpleMove(stringLen, 1);
                        }
                    }
                }

                return modified;
            }
            catch
            {
                return false;
            }
        }

        private void CorrectDimTolerance(IReadOnlyCollection<AdvancedDimensionSegment> segments)
        {
            var first = segments.FirstOrDefault(x => x.IsFirst);
            if (first != null)
            {
                if (_moveDownInsteadSide && HasFreeSpace(first, first.AfterSegment))
                    ComplexMoveSegment(first, -1, 1);
                else
                    SimpleMoveSegment(first, 1);
            }

            var last = segments.FirstOrDefault(x => x.IsLast);
            if (last != null)
            {
                if (_moveDownInsteadSide && HasFreeSpace(last, last.BeforeSegment))
                    ComplexMoveSegment(last, 1, 1);
                else
                    SimpleMoveSegment(last, -1);
            }

            var middle = segments.Where(x => x.IsMiddle).ToList();

            SetToleranceForMiddle(middle);
        }

        private void SetToleranceForMiddle(IReadOnlyList<AdvancedDimensionSegment> middleSegments)
        {
            var horVector = 1;
            var verticalVector = 0;
            var maxVerticalVector = Math.Max(Convert.ToInt32(Math.Ceiling((double)middleSegments.Count / 3)), 2);
            var upDown = 1;

            // Если всего один размерный сегмент, то вертикальный вектор смещения будет зависеть от ширины соседнего сегмента
            var leftSideProcessedCount = 0;
            for (var i = 0; i < middleSegments.Count; i++)
            {
                if (upDown == 1)
                    leftSideProcessedCount++;

                var currentSegment = middleSegments[i];
                var leftSegment = currentSegment.BeforeSegment;
                var rightSegment = currentSegment.AfterSegment;

                if (leftSegment.Value != null && HasFreeSpace(currentSegment, leftSegment))
                {
                    horVector = 1;
                    if (_moveDownInsteadSide)
                    {
                        verticalVector++;
                        ComplexMoveSegment(currentSegment, 1, 1);
                    }
                    else
                    {
                        if (rightSegment.Value != null && HasFreeSpace(currentSegment, rightSegment))
                            SimpleMoveSegment(currentSegment, 1);
                        else
                            SimpleMoveSegment(currentSegment, -1);
                    }
                }
                else if (rightSegment.Value != null && HasFreeSpace(currentSegment, rightSegment))
                {
                    horVector = -1;
                    if (_moveDownInsteadSide)
                    {
                        ComplexMoveSegment(currentSegment, -1, 1);
                    }
                    else
                    {
                        SimpleMoveSegment(currentSegment, -1);
                    }
                }
                else
                {
                    if (upDown == 1 && Math.Abs(verticalVector) >= maxVerticalVector)
                    {
                        upDown = -1;
                        verticalVector = middleSegments.Count - leftSideProcessedCount;
                        horVector = -1;
                    }
                    else if (i == middleSegments.Count - 1 || rightSegment.Value == null)
                    {
                        if (upDown == 1)
                            verticalVector = 1;
                        else 
                            verticalVector = -1;
                    }
                    else
                    {
                        if (upDown == 1)
                            verticalVector++;
                        else
                            verticalVector--;
                    }
                    
                    ComplexMoveSegment(currentSegment, horVector, verticalVector * upDown);
                }
            }
        }

        private void ComplexMoveSegment(AdvancedDimensionSegment segment, int horVector, int verticalVector)
        {
            segment.Segment.ResetTextPosition();
            var p1 = segment.Segment.TextPosition;
            p1 = GeometryHelpers.MoveByViewCorrectDirComplex(p1, _info, segment.StringLength, _textHeight * _scale * 2 * verticalVector, horVector);
            segment.Segment.TextPosition = p1;
        }

        private void SimpleMove(double stringLen, int vector)
        {
            _dimension.ResetTextPosition();
            var p1 = _dimension.TextPosition;
            p1 = GeometryHelpers.MoveByViewCorrectDir(p1, _info, stringLen, vector);
            _dimension.TextPosition = p1;
        }

        private void SimpleMoveSegment(AdvancedDimensionSegment segment, int vector)
        {
            if (segment.BeforeSegment != null && segment.AfterSegment != null &&
                segment.BeforeSegment.Value.HasValue && segment.AfterSegment.Value.HasValue)
            {
                if (segment.BeforeSegment.Value.Value > segment.AfterSegment.Value.Value)
                    vector *= -1;
            }

            segment.Segment.ResetTextPosition();
            var p1 = segment.Segment.TextPosition;
            p1 = GeometryHelpers.MoveByViewCorrectDir(p1, _info, segment.StringLength, vector);
            segment.Segment.TextPosition = p1;
        }

        private bool HasFreeSpace(AdvancedDimensionSegment segmentToMove, DimensionSegment segmentToCheck)
        {
            var stringLength = segmentToCheck.ValueString.Length * _scale * _textHeight;
            if (segmentToMove.Value * 2 < (segmentToCheck.Value - stringLength) / 2)
                return true;

            return false;
        }
    }
}