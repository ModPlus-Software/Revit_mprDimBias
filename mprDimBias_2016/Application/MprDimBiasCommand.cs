using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using mprDimBias.View;

namespace mprDimBias.Application
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class MprDimBiasCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var settings = new DimBiasSettings(commandData.Application);
            settings.ShowDialog();
            return Result.Succeeded;
        }
    }
}
