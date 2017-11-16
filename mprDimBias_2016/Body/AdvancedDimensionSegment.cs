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

        public bool NeedCorect { get; set; }

        public XYZ PosXyz { get; set; }

        public DimensionSegment Segment { get; set; }

        public double StringLenght { get; set; }

        public double Value { get; set; }

        public string ValueString { get; set; }

#region Public Methods

        public void SetCorrectStatus(double scale, double textSize)
        {
            StringLenght = ValueString.Length * textSize * scale * MprDimBiasApp.K;
            NeedCorect = StringLenght >= Value;
        }

#endregion
    }
}