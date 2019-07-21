﻿using System;
using Autodesk.Revit.DB;
using mprDimBias.Application;
using mprDimBias.Work;
using ModPlusAPI.Windows;

namespace mprDimBias.Body
{
    public class DimensionsDilutionUpdater : IUpdater
    {
        private static UpdaterId _updaterId;
        
        public DimensionsDilutionUpdater()
        {
            _updaterId = new UpdaterId(new AddInId(new ModPlusConnector().AddInId), new Guid("d6ead746-4376-4484-b900-5f63d191476f"));
        }

        public void Execute(UpdaterData data)
        {
            Document doc = data.GetDocument();
            if(doc?.ActiveView == null)
                return;
            if(doc.IsFamilyDocument)
                return;
            if (MprDimBiasApp.IsSyncInWork)
                return;

            foreach (ElementId elementId in data.GetAddedElementIds())
            {
                if (doc.GetElement(elementId) is Dimension dimension)
                {
                    try
                    {
                        var equalityParameter = dimension.get_Parameter(BuiltInParameter.DIM_DISPLAY_EQ);
                        if (dimension is SpotDimension || 
                            (equalityParameter != null && equalityParameter.AsInteger() == 2))
                            continue;
                        DimensionsDilution.DoDilution(dimension, doc, out bool modified);
                        if(!MprDimBiasApp.DimsModifiedByUpdater.ContainsKey(elementId))
                            MprDimBiasApp.DimsModifiedByUpdater.Add(elementId, modified);
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

    public class DimensionsModifyDilutionUpdater : IUpdater
    {
        private static UpdaterId _updaterId;

        public DimensionsModifyDilutionUpdater()
        {
            _updaterId = new UpdaterId(new AddInId(new ModPlusConnector().AddInId), new Guid("25877119-32d3-4c0b-8782-33afd1ccbe05"));
        }
        public void Execute(UpdaterData data)
        {
            Document doc = data.GetDocument();
            if (doc?.ActiveView == null)
                return;
            if (doc.IsFamilyDocument)
                return;
            if (MprDimBiasApp.IsSyncInWork)
                return;

            foreach (ElementId elementId in data.GetModifiedElementIds())
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
                                DimensionsDilution.DoDilution(dimension, doc, out bool modified);
                                MprDimBiasApp.DimsModifiedByUpdater[elementId] = modified;
                            }
                            catch (Exception exception)
                            {
                                ExceptionBox.Show(exception);
                            }
                        }
                        else MprDimBiasApp.DimsModifiedByUpdater[elementId] = false;
                    }
                    else
                    {
                        try
                        {
                            DimensionsDilution.DoDilution(dimension, doc, out bool modified);
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
            return "ModifiedDimBiasUpdater";
        }

        public string GetAdditionalInformation()
        {
            return string.Empty;
        }

    }
}