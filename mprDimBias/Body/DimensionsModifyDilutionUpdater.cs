namespace mprDimBias.Body
{
    using System;
    using Application;
    using Autodesk.Revit.DB;
    using ModPlusAPI.Windows;
    using Work;

    public class DimensionsModifyDilutionUpdater : IUpdater
    {
        private static UpdaterId _updaterId;

        public DimensionsModifyDilutionUpdater()
        {
            _updaterId = new UpdaterId(new AddInId(new ModPlusConnector().AddInId), new Guid("25877119-32d3-4c0b-8782-33afd1ccbe05"));
        }

        /// <inheritdoc/>
        public void Execute(UpdaterData data)
        {
            var doc = data.GetDocument();
            if (doc?.ActiveView == null)
                return;
            if (doc.IsFamilyDocument)
                return;
            if (MprDimBiasApp.IsSyncInWork)
                return;

            foreach (var elementId in data.GetModifiedElementIds())
            {
                if (doc.GetElement(elementId) is Dimension dimension)
                {
                    var equalityParameter = dimension.get_Parameter(BuiltInParameter.DIM_DISPLAY_EQ);
                    if (dimension is SpotDimension || 
                        (equalityParameter != null && equalityParameter.AsInteger() == 2))
                        continue;
                    if (MprDimBiasApp.DimsModifiedByUpdater.ContainsKey(elementId))
                    {
                        if (!MprDimBiasApp.DimsModifiedByUpdater[elementId])
                        {
                            try
                            {
                                var modified = new AdvancedDimension(dimension).SetMoveForCorrect();
                                MprDimBiasApp.DimsModifiedByUpdater[elementId] = modified;
                            }
                            catch (Exception exception)
                            {
                                ExceptionBox.Show(exception);
                            }
                        }
                        else
                        {
                            MprDimBiasApp.DimsModifiedByUpdater[elementId] = false;
                        }
                    }
                    else
                    {
                        try
                        {
                            var modified = new AdvancedDimension(dimension).SetMoveForCorrect();
                            if (!MprDimBiasApp.DimsModifiedByUpdater.ContainsKey(elementId))
                                MprDimBiasApp.DimsModifiedByUpdater.Add(elementId, modified);
                        }
                        catch (Exception exception)
                        {
                            ExceptionBox.Show(exception);
                        }
                    }
                }
            }
        }

        /// <inheritdoc/>
        public UpdaterId GetUpdaterId()
        {
            return _updaterId;
        }

        /// <inheritdoc/>
        public ChangePriority GetChangePriority()
        {
            return ChangePriority.FloorsRoofsStructuralWalls; // ??????????????????????????????????
        }

        /// <inheritdoc/>
        public string GetUpdaterName()
        {
            return "ModifiedDimBiasUpdater";
        }

        /// <inheritdoc/>
        public string GetAdditionalInformation()
        {
            return string.Empty;
        }
    }
}