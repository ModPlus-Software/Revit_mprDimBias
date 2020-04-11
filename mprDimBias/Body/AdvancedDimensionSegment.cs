namespace mprDimBias.Body
{
    using Application;
    using Autodesk.Revit.DB;

    /// <summary>
    /// Сегмент размера
    /// </summary>
    public class AdvancedDimensionSegment
    {
        private readonly string _valueString;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdvancedDimensionSegment"/> class.
        /// </summary>
        /// <param name="segment">Исходный сегмент размера Revit <see cref="DimensionSegment"/></param>
        /// <param name="beforeSegment">Предшествующий сегмент</param>
        /// <param name="afterSegment">Последующий сегмент</param>
        public AdvancedDimensionSegment(DimensionSegment segment, DimensionSegment beforeSegment, DimensionSegment afterSegment)
        {
            Segment = segment;

            BeforeSegment = beforeSegment;
            AfterSegment = afterSegment;

            if (Segment.Value.HasValue)
            {
                Value = Segment.Value.Value;
                _valueString = Segment.ValueString;
            }
        }

        /// <summary>
        /// Исходный сегмент размера Revit <see cref="DimensionSegment"/>
        /// </summary>
        public DimensionSegment Segment { get; }

        /// <summary>
        /// Измеренная длина сегмента
        /// </summary>
        public double Value { get; }

        /// <summary>
        /// Последующий сегмент
        /// </summary>
        public DimensionSegment AfterSegment { get; }

        /// <summary>
        /// Предшествующий сегмент
        /// </summary>
        public DimensionSegment BeforeSegment { get; }

        /// <summary>
        /// Сегмент является первым в размере
        /// </summary>
        public bool IsFirst => BeforeSegment == null;

        /// <summary>
        /// Сегмент является последним в размере
        /// </summary>
        public bool IsLast => AfterSegment == null;

        /// <summary>
        /// Сегмент является средним
        /// </summary>
        public bool IsMiddle => !IsFirst && !IsLast;

        /// <summary>
        /// Требуется смещение текста для текущего сегмента
        /// </summary>
        public bool NeedCorrect { get; private set; }

        /// <summary>
        /// Длина строки с учетом масштаба, размера текста и коэффициента смещения
        /// </summary>
        public double StringLength { get; set; }

        /// <summary>
        /// Выполнить проверку возможности и необходимости смещения текста для сегмента
        /// </summary>
        /// <param name="scale">Масштаб вида</param>
        /// <param name="textSize">Размер текста</param>
        public void CheckNeedCorrection(double scale, double textSize)
        {
            if (Segment.IsTextPositionAdjustable())
            {
                StringLength = _valueString.Length * textSize * scale * MprDimBiasApp.OffsetFactor;
                NeedCorrect = StringLength >= Value;
            }
            else
            {
                NeedCorrect = false;
            }
        }
    }
}