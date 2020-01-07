namespace mprDimBias.Body
{
    using Application;
    using Autodesk.Revit.DB;

    public class AdvancedDimensionSegment
    {
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
            else
                BeforeSegment = null;

            if (!IsLast)
                AfterSegment = afterSegment as DimensionSegment;
            else
                AfterSegment = null;

            if (Segment.Value.HasValue)
            {
                Value = Segment.Value.Value;
                ValueString = Segment.ValueString;
            }

            PosXyz = Segment.TextPosition;
        }

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

        public void SetCorrectStatus(double scale, double textSize)
        {
            if (Segment.IsTextPositionAdjustable())
            {
                StringLenght = ValueString.Length * textSize * scale * MprDimBiasApp.K;
                NeedCorrect = StringLenght >= Value;
            }
            else
            {
                NeedCorrect = false;
            }
        }
    }
}