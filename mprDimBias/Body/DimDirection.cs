using System.Diagnostics.CodeAnalysis;

namespace mprDimBias.Body
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum DimDirection
    {
        X = 0,
        Y = 1,
        Z = 2,
        UX,
        UY,
        UZ,
        Default
    }
}
