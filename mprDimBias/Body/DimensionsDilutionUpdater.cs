﻿namespace mprDimBias.Body
{
    using System;
    using Application;
    using Autodesk.Revit.DB;
    using ModPlusAPI.Windows;

    public class DimensionsDilutionUpdater : IUpdater
    {
        private static UpdaterId _updaterId;
        
        public DimensionsDilutionUpdater()
        {
            _updaterId = new UpdaterId(new AddInId(new ModPlusConnector().AddInId), new Guid("d6ead746-4376-4484-b900-5f63d191476f"));
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

            foreach (var elementId in data.GetAddedElementIds())
            {
                if (doc.GetElement(elementId) is Dimension dimension)
                {
                    try
                    {
                        var equalityParameter = dimension.get_Parameter(BuiltInParameter.DIM_DISPLAY_EQ);
                        if (dimension is SpotDimension || 
                            (equalityParameter != null && equalityParameter.AsInteger() == 2))
                            continue;
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
            return "DimBiasUpdater";
        }

        /// <inheritdoc/>
        public string GetAdditionalInformation()
        {
            return string.Empty;
        }
    }
}