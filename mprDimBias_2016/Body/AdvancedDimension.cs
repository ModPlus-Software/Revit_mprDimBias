using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using mprDimBias.Application;
using ModPlusAPI.Windows;

namespace mprDimBias.Body
{
    public class AdvancedDimension
    {
        #region Constructor

        public AdvancedDimension(Dimension dimension, Document doc)
        {
            AdvancedSegments = new List<AdvancedDimensionSegment>();
            Dimension = dimension;
            // Получаю высоту текста размера из его типа
            TextHeight = dimension.DimensionType.get_Parameter(BuiltInParameter.TEXT_SIZE).AsDouble();
            // get scale
            Scale = doc.ActiveView.Scale;

            if (Dimension.DimensionShape == DimensionShape.Linear)
            {
                // dimension segments
                List<DimensionSegment> tempDimensionSegments = new List<DimensionSegment>();
                foreach (DimensionSegment segment in Dimension.Segments)
                {
                    tempDimensionSegments.Add(segment);
                }
                if (tempDimensionSegments.Count == 1)
                {
                    AdvancedSegments.Add(new AdvancedDimensionSegment(tempDimensionSegments[0], null, null));
                }
                else if (tempDimensionSegments.Count == 2)
                {
                    AdvancedSegments.Add(new AdvancedDimensionSegment(tempDimensionSegments[0], null,
                        tempDimensionSegments[1]));
                    AdvancedSegments.Add(new AdvancedDimensionSegment(tempDimensionSegments[1],
                        tempDimensionSegments[0], null));
                }
                else
                {
                    for (var i = 0; i < tempDimensionSegments.Count; i++)
                    {
                        if (i == 0)
                        {
                            AdvancedSegments.Add(new AdvancedDimensionSegment(tempDimensionSegments[i], null,
                                tempDimensionSegments[i + 1]));
                        }
                        else if (i != tempDimensionSegments.Count - 1)
                        {
                            AdvancedSegments.Add(new AdvancedDimensionSegment(tempDimensionSegments[i],
                                tempDimensionSegments[i - 1], tempDimensionSegments[i + 1]));
                        }
                        else
                        {
                            AdvancedSegments.Add(new AdvancedDimensionSegment(tempDimensionSegments[i],
                                tempDimensionSegments[i - 1], null));
                        }
                    }
                }
                AdvancedSegments.ForEach(s => s.SetCorrectStatus(TextHeight, Scale));
                AdvancedCorrectSegments =
                (from x in AdvancedSegments
                 where x.NeedCorrect
                 select x).ToList();
                if (Dimension.Curve is Line line)
                {
                    Info = new DimInfo(doc.ActiveView, line.Direction);
                    IsValid = true;
                }
                else IsValid = true;
            }
            else if (Dimension.DimensionShape == DimensionShape.Radial || Dimension.DimensionShape == DimensionShape.Diameter)
            {
                if (Dimension.LeaderEndPosition != null)
                {
                    var directionVector = Dimension.Origin.Normalize();
                    Info = new DimInfo(doc.ActiveView, directionVector);
                    IsValid = true;
                }
                else IsValid = false;
            }
            else IsValid = false;
        }
        #endregion

        #region Fields

        public Dimension Dimension;
        /// <summary>Высота текста размера</summary>
        public double TextHeight;

        /// <summary>Масштаб текущего вида</summary>
        public double Scale;

        public DimInfo Info;

        public bool IsValid;

        #endregion

        #region Parameters

        public List<AdvancedDimensionSegment> AdvancedSegments { get; set; }
        public List<AdvancedDimensionSegment> AdvancedCorrectSegments { get; set; }

        #endregion

        #region Public Methods

        public void SetMoveForCorrect(out bool modified)
        {
            modified = false;
            if (!IsValid) return;

            if (AdvancedCorrectSegments != null && AdvancedCorrectSegments.Count != 0)
            {
                List<List<AdvancedDimensionSegment>> tempPl = new List<List<AdvancedDimensionSegment>>()
                {
                    new List<AdvancedDimensionSegment> {AdvancedCorrectSegments[0]}
                };
                for (int i = 1; i < AdvancedCorrectSegments.Count; i++)
                {
                    if (tempPl.Last().Last().AfterSegment ==
                        null)
                    {
                        tempPl.Add(new List<AdvancedDimensionSegment>() { AdvancedCorrectSegments[i] });
                    }
                    else
                    {
                        tempPl.Last().Add(AdvancedCorrectSegments[i]);
                    }
                }
                foreach (List<AdvancedDimensionSegment> sets in tempPl)
                {
                    CorrectDimTolerance(sets);
                }
                modified = true;
            }
            else
            {
                if (Dimension.ValueString != null)
                {
                    //bool checkByTextLenght = false;
                    //if (Dimension.Origin != null && Dimension.LeaderEndPosition != null &&
                    //    Dimension.TextPosition != null)
                    //{
                    //    // Три вектора (стороны треугольника). Нужно получить три угла
                    //    // если все углы меньше 90 (или хоть один равен 90), значит текст
                    //    // расположен "внутри" размера
                    //    var vec1 = Dimension.Origin - Dimension.LeaderEndPosition;
                    //    var vec2 = Dimension.TextPosition - Dimension.Origin;
                    //    var vec3 = Dimension.TextPosition - Dimension.LeaderEndPosition;
                    //    var ang1 = vec1.AngleTo(vec2) * 180 / Math.PI;
                    //    var ang2 = vec2.AngleTo(vec3) * 180 / Math.PI;
                    //    var ang3 = vec3.AngleTo(vec1) * 180 / Math.PI;
                    //    if (ang3 <= 90.0 && ang2 <= 90.0 && ang1 <= 90.0)
                    //        checkByTextLenght = true;
                    //}
                    //else checkByTextLenght = true;

                    //if (checkByTextLenght)
                    //{
                        double stringLen = Dimension.ValueString.Length * TextHeight * Scale * MprDimBiasApp.K;
                        double? value = Dimension.Value;
                        if (stringLen >= value.GetValueOrDefault() && value.HasValue)
                        {
                            modified = true;
                            SimpleMove(stringLen, 1);
                        }
                    //}
                }
            }
        }

        #endregion

        #region Private Methods

        private void CorrectDimTolerance(List<AdvancedDimensionSegment> sets)
        {
            AdvancedDimensionSegment last = sets.FirstOrDefault(x => x.IsLast);
            if (last != null)
            {
                SimpleMoveSegm(last, -1);
            }
            AdvancedDimensionSegment first = sets.FirstOrDefault(x => x.IsFirst);
            if (first != null)
            {
                this.SimpleMoveSegm(first, 1);
            }
            List<AdvancedDimensionSegment> middle = (
                from x in sets
                where !x.IsFirst && !x.IsLast
                select x).ToList();

            SetToleranceForMidle(middle);
        }

        private void SetToleranceForMidle(List<AdvancedDimensionSegment> middle)
        {
            int horVector = 1;
            int vertVector = 0;
            int maxVertVector = Math.Max(Convert.ToInt32(Math.Ceiling((double)middle.Count / 3)), 2);
            int siegth = 1;
            // Если всего один размерный сегмент, то вертикальный вектор смещения будет зависеть от ширины соседнего сегмента
            for (int i = 0; i < middle.Count; i++)
            {
                var leftSegment = middle[i].BeforeSegment;
                var rightSegment = middle[i].AfterSegment;
                //if (rightSegment.Value != null && rightSegment.Value > middle[0].Value * 8)
                if (rightSegment.Value != null && HasFreeSpace(middle[0], rightSegment))
                {
                    horVector = -1;
                    ComplexMoveSegm(middle[i], horVector, vertVector * siegth, i);
                }
                //else if (leftSegment.Value != null && leftSegment.Value > middle[0].Value * 8)
                else if (leftSegment.Value != null && HasFreeSpace(middle[0], leftSegment))
                {
                    horVector = 1;
                    ComplexMoveSegm(middle[i], horVector, vertVector * siegth, i);
                }
                else
                {
                    if (Math.Abs(vertVector) >= maxVertVector)
                    {
                        siegth *= -1;
                        vertVector = 1;
                    }
                    else
                        vertVector++;

                    if (rightSegment.Value != null && leftSegment.Value != null)
                    {
                        if (rightSegment.Value > leftSegment.Value)
                            horVector = -1;
                        else horVector = 1;
                    }

                    ComplexMoveSegm(middle[i], horVector, vertVector * siegth, i);
                }
            }
        }
        private void ComplexMoveSegm(AdvancedDimensionSegment segm, int horVector, int vertVector, int number)
        {
            segm.Segment.ResetTextPosition();
            XYZ p1 = segm.Segment.TextPosition;
            p1 = GeometryHelpers.MoveByViewCorrectDirComplex(p1, Info, segm.StringLenght, TextHeight * Scale * 2 * vertVector, horVector, number);
            segm.Segment.TextPosition = p1;
        }
        private void SimpleMove(double stringLen, int vector)
        {
            Dimension.ResetTextPosition();
            XYZ p1 = Dimension.TextPosition;
            p1 = GeometryHelpers.MoveByViewCorrectDir(p1, Info, stringLen, vector);
            Dimension.TextPosition = p1;
        }
        private void SimpleMoveSegm(AdvancedDimensionSegment segm, int vector)
        {
            segm.Segment.ResetTextPosition();
            XYZ p1 = segm.Segment.TextPosition;
            p1 = GeometryHelpers.MoveByViewCorrectDir(p1, Info, segm.StringLenght, vector);
            segm.Segment.TextPosition = p1;
        }

        private bool HasFreeSpace(AdvancedDimensionSegment segmentToMove, DimensionSegment segmentToCheck)
        {
            var stringLenght = segmentToCheck.ValueString.Length * Scale * TextHeight;
            if (segmentToMove.Value * 2 < (segmentToCheck.Value - stringLenght) / 2) return true;
            else return false;
        }

        #endregion
    }
}