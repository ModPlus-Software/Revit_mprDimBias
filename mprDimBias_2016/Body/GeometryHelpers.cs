namespace mprDimBias.Body
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Autodesk.Revit.DB;

    public static class GeometryHelpers
    {
        public static double GetViewPlanCutPlaneElevation(ViewPlan viewPlan, Document doc)
        {
            var planViewRange = viewPlan.GetViewRange();
            return planViewRange.GetOffset(PlanViewPlane.CutPlane) + viewPlan.GenLevel.Elevation;
        }

        #region Dimensions Dilution

        public static DimDirection GetDirFromVector(XYZ vec)
        {
            var dirs = new List<double>
            {
                Math.Abs(vec.X),
                Math.Abs(vec.Y),
                Math.Abs(vec.Z)
            };
            return (DimDirection) dirs.IndexOf(dirs.Max());
        }

        public static bool IsAboveDir(XYZ wallDir)
        {
            return (Math.Round(wallDir.X, 1) < 0 || Math.Round(wallDir.Y, 1) < 0
                ? false
                : Math.Round(wallDir.Z, 1) >= 0)
                ? false
                : true;
        }

        public static XYZ MoveByViewCorrectDir(XYZ p1, DimInfo info, double stringLen, int vector)
        {
            XYZ p2 = null;
            if (info.ViewDirDigit != info.DirectionDigit ? true : info.DirectionDigit >= 0)
            {
                stringLen *= vector * -1;
            }
            else
            {
                stringLen *= vector;
                if (info.ViewDirDir != DimDirection.Z)
                {
                    if (info.ViewRigthDigit < 0 ? true : info.ViewUpDigit < 0)
                    {
                        stringLen = stringLen * (info.ViewRigthDigit == info.ViewUpDigit ? 1 : -1);
                    }
                }
            }

            if (info.Direction.Z < 0)
            {
                stringLen *= -1;
            }

            p2 = MoveXyzByVector(p1, stringLen, info.Direction);
            return p2;
        }

        public static XYZ MoveByViewCorrectDirComplex(XYZ p1, DimInfo info, double stringLen, double textHegth,
            int vector, int number)
        {
            XYZ p2 = null;
            if (info.ViewDirDigit != info.DirectionDigit ? true : info.DirectionDigit >= 0)
            {
                stringLen *= vector * -1;
            }
            else
            {
                stringLen *= vector;
                if (info.ViewDirDir != DimDirection.Z)
                {
                    if (info.ViewRigthDigit < 0 ? true : info.ViewUpDigit < 0)
                    {
                        stringLen = stringLen * (info.ViewRigthDigit == info.ViewUpDigit ? 1 : -1);
                    }
                }
            }

            if (info.Direction.Z < 0)
            {
                stringLen *= -1;
            }

            var perp = VectorVectorMult(info.Direction, info.ViewDir);
            p2 = MoveXyzByVector(p1, stringLen, textHegth, info.Direction, perp);
            return p2;
        }

        private static XYZ MoveXyzByVector(XYZ p1, double lenX, XYZ vec)
        {
            var xYZ = new XYZ(p1.X + (lenX * vec.X), p1.Y + (lenX * vec.Y), p1.Z + (lenX * vec.Z));
            return xYZ;
        }

        private static XYZ MoveXyzByVector(XYZ p1, double lenX, double lenY, XYZ horiz, XYZ vert)
        {
            p1 = MoveXyzByVector(p1, lenX, horiz);
            p1 = MoveXyzByVector(p1, lenY, vert);
            return p1;
        }

        private static XYZ VectorVectorMult(XYZ v1, XYZ v2)
        {
            var x = (v1.Y * v2.Z) + (v2.Y * v1.Z);
            var y = -((v1.X * v2.Z) + (v1.Z * v2.X));
            var z = Math.Abs(v1.X * v2.Y) + Math.Abs(v1.Y * v2.X);
            return new XYZ(x, y, z);
        }

        #endregion
    }
}
