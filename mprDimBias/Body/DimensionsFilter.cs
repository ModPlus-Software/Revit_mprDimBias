namespace mprDimBias.Body
{
    using System;
    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI.Selection;

    /// <summary>
    /// Dimension selection filter
    /// </summary>
    public class DimensionsFilter : ISelectionFilter
    {
        /// <inheritdoc />
        public bool AllowElement(Element elem)
        {
            if (elem is Dimension dimension)
            {
                var equalityParameter = dimension.get_Parameter(BuiltInParameter.DIM_DISPLAY_EQ);
                if (dimension is SpotDimension || 
                    (equalityParameter != null && equalityParameter.AsInteger() == 2))
                    return false;
                    
                return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public bool AllowReference(Reference reference, XYZ position)
        {
            throw new NotImplementedException();
        }
    }
}