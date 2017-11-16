using System;
using Autodesk.Revit.DB;
using mprDimBias.Work;
using ModPlusAPI.Windows;

namespace mprDimBias.Body
{
    public class DimensionsDilutionUpdater : IUpdater
    {
        private static UpdaterId _updaterId;

        public DimensionsDilutionUpdater()
        {
            _updaterId = new UpdaterId(new AddInId(new Interface().AddInId), new Guid("d6ead746-4376-4484-b900-5f63d191476f"));
        }
        public void Execute(UpdaterData data)
        {
            Document doc = data.GetDocument();
            foreach (ElementId elementId in data.GetAddedElementIds())
            {
                if (doc.GetElement(elementId) is Dimension dimension)
                {
                    try
                    {
                        DimensionsDilution.DoDilution(dimension, doc);
                    }
                    catch (Exception exception)
                    {
                        ExceptionBox.Show(exception);
                    }
                }
            }
        }

        public UpdaterId GetUpdaterId()
        {
            return _updaterId;
        }

        public ChangePriority GetChangePriority()
        {
            return ChangePriority.FloorsRoofsStructuralWalls; // ??????????????????????????????????
        }

        public string GetUpdaterName()
        {
            return "DimBiasUpdater";
        }

        public string GetAdditionalInformation()
        {
            return string.Empty;
        }
        
    }
}