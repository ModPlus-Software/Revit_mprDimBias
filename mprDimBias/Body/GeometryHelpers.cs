namespace mprDimBias.Body
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Autodesk.Revit.DB;

    public static class GeometryHelpers
    {
        public static DimDirection GetDirectionFromVector(XYZ vec)
        {
            var dirs = new List<double>
            {
                Math.Abs(vec.X),
                Math.Abs(vec.Y),
                Math.Abs(vec.Z)
            };
            return (DimDirection)dirs.IndexOf(dirs.Max());
        }

        public static bool IsAboveDir(XYZ direction)
        {
            return Math.Round(direction.X, 1) < 0 || Math.Round(direction.Y, 1) < 0 || !(Math.Round(direction.Z, 1) >= 0);
        }

        public static XYZ MoveByViewCorrectDir(XYZ p1, DimInfo info, double stringLen, int vector)
        {
            if (info.ViewDirDigit != info.DirectionDigit || info.DirectionDigit >= 0)
            {
                stringLen *= vector * -1;
            }
            else
            {
                stringLen *= vector;
                if (info.ViewDirDir != DimDirection.Z)
                {
                    if (info.ViewRigthDigit < 0 || info.ViewUpDigit < 0)
                    {
                        stringLen *= info.ViewRigthDigit == info.ViewUpDigit ? 1 : -1;
                    }
                }
            }

            if (info.Direction.Z < 0)
            {
                stringLen *= -1;
            }

            return MoveXyzByVector(p1, stringLen, info.Direction);
        }

        public static XYZ MoveByViewCorrectDirComplex(
            XYZ p1, DimInfo info, double stringLen, double textHeight, int vector)
        {
            if (info.ViewDirDigit != info.DirectionDigit || info.DirectionDigit >= 0)
            {
                stringLen *= vector * -1;
            }
            else
            {
                stringLen *= vector;
                if (info.ViewDirDir != DimDirection.Z)
                {
                    if (info.ViewRigthDigit < 0 || info.ViewUpDigit < 0)
                    {
                        stringLen *= info.ViewRigthDigit == info.ViewUpDigit ? 1 : -1;
                    }
                }
            }

            if (info.Direction.Z < 0)
            {
                stringLen *= -1;
            }

            var perpendicular = info.Direction.CrossProduct(info.ViewDir);
            
            return MoveXyzByVector(p1, stringLen, textHeight, info.Direction, perpendicular);
        }

        private static XYZ MoveXyzByVector(XYZ p1, double lenX, XYZ vec)
        {
            return new XYZ(p1.X + (lenX * vec.X), p1.Y + (lenX * vec.Y), p1.Z + (lenX * vec.Z));
        }

        private static XYZ MoveXyzByVector(XYZ p1, double lenX, double lenY, XYZ horiz, XYZ vert)
        {
            p1 = MoveXyzByVector(p1, lenX, horiz);
            p1 = MoveXyzByVector(p1, lenY, vert);
            return p1;
        }
    }
}
