using System;
using System.Globalization;
using Autodesk.Revit.DB;
using mprDimBias.Application;
using ModPlusAPI.Windows;

namespace mprDimBias.Body
{
    public class AdvancedDimensionSegment
    {
#region Constructor

        public AdvancedDimensionSegment()
        {
            IsFirst = false;
            IsLast = false;
            
        }

        public AdvancedDimensionSegment(DimensionSegment segment, object beforeSegment, object afterSegment)
        {
            IsFirst = !(beforeSegment is DimensionSegment);
            IsLast = !(afterSegment is DimensionSegment);
            Segment = segment;

            if (!IsFirst)
                BeforeSegment = beforeSegment as DimensionSegment;
            else BeforeSegment = null;

            if(!IsLast)
                AfterSegment = afterSegment as DimensionSegment;
            else AfterSegment = null;

            if (Segment.Value.HasValue)
            {
                Value = Segment.Value.Value;
                ValueString = Segment.ValueString;
            }
            PosXyz = Segment.TextPosition;
        }
#endregion
        public DimensionSegment AfterSegment { get; set; }

        public DimensionSegment BeforeSegment { get; set; }

        public bool IsFirst { get; set; }

        public bool IsLast { get; set; }

        public bool NeedCorrect { get; set; }

        public XYZ PosXyz { get; set; }

        public DimensionSegment Segment { get; set; }

        public double StringLenght { get; set; }

        public double Value { get; set; }

        public string ValueString { get; set; }

#region Public Methods

        public void SetCorrectStatus(double scale, double textSize)
        {
            //bool checkByTextLenght = false;
            //if (Segment.Origin != null && Segment.LeaderEndPosition != null && Segment.TextPosition != null)
            //{
            //    // Три вектора (стороны треугольника). Нужно получить три угла
            //    // если все углы меньше 90 (или хоть один равен 90), значит текст
            //    // расположен "внутри" размера
            //    var vec1 = Segment.Origin - Segment.LeaderEndPosition;
            //    var vec2 = Segment.TextPosition - Segment.Origin;
            //    var vec3 = Segment.TextPosition - Segment.LeaderEndPosition;
            //    var ang1 = vec1.AngleTo(vec2) * 180 / Math.PI;
            //    var ang2 = vec2.AngleTo(vec3) * 180 / Math.PI;
            //    var ang3 = vec3.AngleTo(vec1) * 180 / Math.PI;
            //    if (ang3 <= 90.0 && ang2 <= 90.0 && ang1 <= 90.0)
            //        checkByTextLenght = true;
            //}
            //else checkByTextLenght = true;

            //if (checkByTextLenght)
            //{
            if (Segment.IsTextPositionAdjustable())
            {
                StringLenght = ValueString.Length * textSize * scale * MprDimBiasApp.K;
                NeedCorrect = StringLenght >= Value;
            }
            else NeedCorrect = false;
            //}
        }
#endregion
    }
}