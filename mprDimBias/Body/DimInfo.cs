namespace mprDimBias.Body
{
    using Autodesk.Revit.DB;

    public struct DimInfo
    {
        public DimDirection DimensDir { get; set; }

        /// <summary>
        /// Вектор направления размерной линии
        /// </summary>
        public XYZ Direction { get; set; }

        public int DirectionDigit { get; set; }

        public XYZ ViewDir { get; set; }

        public int ViewDirDigit { get; set; }

        public DimDirection ViewDirDir { get; set; }

        public DimDirection ViewRight { get; set; }

        public int ViewRightDigit { get; set; }

        public XYZ ViewRightDirection { get; set; }

        public DimDirection ViewUp { get; set; }

        public int ViewUpDigit { get; set; }

        public XYZ ViewUpDirection { get; set; }

        public DimInfo(View view, XYZ dimDir)
        {
            this = new DimInfo();
            Direction = dimDir;
            DirectionDigit = GeometryHelpers.IsAboveDir(Direction) ? -1 : 1;
            ViewDir = view.ViewDirection;
            ViewDirDigit = GeometryHelpers.IsAboveDir(ViewDir) ? -1 : 1;
            ViewUpDirection = view.UpDirection;
            ViewRightDirection = view.RightDirection;
            ViewUpDigit = GeometryHelpers.IsAboveDir(ViewUpDirection) ? -1 : 1;
            ViewRightDigit = GeometryHelpers.IsAboveDir(ViewRightDirection) ? -1 : 1;
            ViewUp = GeometryHelpers.GetDirectionFromVector(ViewUpDirection);
            ViewRight = GeometryHelpers.GetDirectionFromVector(ViewRightDirection);
            DimensDir = GeometryHelpers.GetDirectionFromVector(dimDir);
            ViewDirDir = GeometryHelpers.GetDirectionFromVector(ViewDir);
        }
    }
}
