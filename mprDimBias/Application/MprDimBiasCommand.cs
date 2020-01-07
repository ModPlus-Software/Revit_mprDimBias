namespace mprDimBias.Application
{
    using Autodesk.Revit.Attributes;
    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;
    using View;

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class MprDimBiasCommand : IExternalCommand
    {
        /// <inheritdoc />
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var settings = new DimBiasSettings(commandData.Application);
            settings.ShowDialog();
            return Result.Succeeded;
        }
    }
}
