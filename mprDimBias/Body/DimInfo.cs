namespace mprDimBias.Body
{
    using Autodesk.Revit.DB;

    public struct DimInfo
    {
        public DimDirection DimensDir { get; set; }

        public XYZ Direction { get; set; }

        public int DirectionDigit { get; set; }

        public XYZ ViewDir { get; set; }

        public int ViewDirDigit { get; set; }

        public DimDirection ViewDirDir { get; set; }

        public DimDirection ViewRigth { get; set; }

        public int ViewRigthDigit { get; set; }

        public XYZ ViewRigthDirection { get; set; }

        public DimDirection ViewUp { get; set; }

        public int ViewUpDigit { get; set; }

        public XYZ ViewUpDirection { get; set; }

        public DimInfo(Autodesk.Revit.DB.View view, XYZ dimDir)
        {
            this = new DimInfo();
            Direction = dimDir;
            DirectionDigit = GeometryHelpers.IsAboveDir(Direction) ? -1 : 1;
            ViewDir = view.ViewDirection;
            ViewDirDigit = GeometryHelpers.IsAboveDir(ViewDir) ? -1 : 1;
            ViewUpDirection = view.UpDirection;
            ViewRigthDirection = view.RightDirection;
            ViewUpDigit = GeometryHelpers.IsAboveDir(ViewUpDirection) ? -1 : 1;
            ViewRigthDigit = GeometryHelpers.IsAboveDir(ViewRigthDirection) ? -1 : 1;
            ViewUp = GeometryHelpers.GetDirFromVector(ViewUpDirection);
            ViewRigth = GeometryHelpers.GetDirFromVector(ViewRigthDirection);
            DimensDir = GeometryHelpers.GetDirFromVector(dimDir);
            ViewDirDir = GeometryHelpers.GetDirFromVector(ViewDir);
        }
    }
}
