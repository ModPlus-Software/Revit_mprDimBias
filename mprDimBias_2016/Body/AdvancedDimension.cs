using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using mprDimBias.Application;

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
                AdvancedSegments.Add(new AdvancedDimensionSegment(tempDimensionSegments[0], null, tempDimensionSegments[1]));
                AdvancedSegments.Add(new AdvancedDimensionSegment(tempDimensionSegments[1], tempDimensionSegments[0], null));
            }
            else
            {
                for (var i = 0; i < tempDimensionSegments.Count; i++)
                {
                    if (i == 0)
                    {
                        AdvancedSegments.Add(new AdvancedDimensionSegment(tempDimensionSegments[i], null, tempDimensionSegments[i + 1]));
                    }
                    else if (i != tempDimensionSegments.Count - 1)
                    {
                        AdvancedSegments.Add(new AdvancedDimensionSegment(tempDimensionSegments[i], tempDimensionSegments[i - 1], tempDimensionSegments[i + 1]));
                    }
                    else
                    {
                        AdvancedSegments.Add(new AdvancedDimensionSegment(tempDimensionSegments[i], tempDimensionSegments[i - 1], null));
                    }
                }
            }
            AdvancedSegments.ForEach(s => s.SetCorrectStatus(TextHeight, Scale));
            AdvancedCorrectSegments =
                (from x in AdvancedSegments
                 where x.NeedCorect
                 select x).ToList();
            Info = new DimInfo(doc.ActiveView, (Dimension.Curve as Line)?.Direction);
        }
        #endregion

        #region Fields

        public Dimension Dimension;
        /// <summary>Высота текста размера</summary>
        public double TextHeight;

        /// <summary>Масштаб текущего вида</summary>
        public double Scale;

        public DimInfo Info;

        #endregion

        #region Parameters

        public List<AdvancedDimensionSegment> AdvancedSegments { get; set; }
        public List<AdvancedDimensionSegment> AdvancedCorrectSegments { get; set; }

        #endregion

        #region Public Methods

        public void SetMoveForCorrect()
        {
            if (AdvancedCorrectSegments.Count != 0)
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
            }
            else
            {
                if (Dimension.ValueString != null)
                {
                    double stringLen = Dimension.ValueString.Length * TextHeight * Scale * MprDimBiasApp.K;
                    double? value = Dimension.Value;
                    if ((stringLen >= value.GetValueOrDefault() ? value.HasValue : false))
                    {
                        SimpleMove(stringLen, 1);
                    }
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
            for (int i = 0; i < middle.Count; i++)
            {
                if (Math.Abs(vertVector) >= maxVertVector)
                {
                    siegth *= -1;
                    vertVector = 1;
                }
                else
                {
                    vertVector++;
                }
                this.ComplexMoveSegm(middle[i], horVector, vertVector * siegth, i);
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

        #endregion
    }
}