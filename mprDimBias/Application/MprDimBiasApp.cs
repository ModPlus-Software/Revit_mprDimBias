namespace mprDimBias.Application
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using Autodesk.Revit.DB;
    using Autodesk.Revit.DB.Events;
    using Autodesk.Revit.UI;
    using Autodesk.Revit.UI.Events;
    using Body;
    using ModPlusAPI;
    using ModPlusAPI.Windows;
    using Work;

    public class MprDimBiasApp : IExternalApplication
    {
        public static DimensionsDilutionUpdater DimensionsDilutionUpdater;
        public static DimensionsModifyDilutionUpdater DimensionsModifyDilutionUpdater;
        public static Dictionary<ElementId, bool> DimsModifiedByUpdater;
        public static UIControlledApplication Application;
        public static double K;
        public static bool IsSyncInWork;
        
        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                Application = application;
                Application.Idling += ApplicationOnIdling;

                DimsModifiedByUpdater = new Dictionary<ElementId, bool>();

                var dimDilWorkVar = 
                    bool.TryParse(UserConfigFile.GetValue("mprDimBias", "DimBiasOnOff"), out var b) && b;

                var dimModifiedDilWorkVar = 
                    bool.TryParse(UserConfigFile.GetValue("mprDimBias", "ModifiedDimBiasOnOff"), out b) && b;

                K = double.TryParse(UserConfigFile.GetValue("mprDimBias", "K"), NumberStyles.Number, CultureInfo.InvariantCulture, out var d)
                    ? d
                    : 0.6;

                DimensionsDilutionUpdater = new DimensionsDilutionUpdater();
                DimensionsModifyDilutionUpdater = new DimensionsModifyDilutionUpdater();
                if (dimDilWorkVar)
                    DimensionsDilution.DimDilutionOn(application.ActiveAddInId, ref DimensionsDilutionUpdater);
                else
                    DimensionsDilution.DimDilutionOff(application.ActiveAddInId, ref DimensionsDilutionUpdater);
                if (dimModifiedDilWorkVar)
                    DimensionsDilution.DimModifiedDilutionOn(application.ActiveAddInId, ref DimensionsModifyDilutionUpdater);
                else
                    DimensionsDilution.DimModifiedDilutionOff(application.ActiveAddInId, ref DimensionsModifyDilutionUpdater);

                // create ribbon tab
                CreateRibbonTab(application);
            }
            catch (Exception exception)
            {
                ExceptionBox.Show(exception);
                return Result.Failed;
            }

            return Result.Succeeded;
        }

        private void ApplicationOnIdling(object sender, IdlingEventArgs e)
        {
            if (sender is UIApplication uiApplication)
            {
                Application.Idling -= ApplicationOnIdling;
                uiApplication.Application.DocumentSynchronizingWithCentral += ApplicationOnDocumentSynchronizingWithCentral;
                uiApplication.Application.DocumentSynchronizedWithCentral += ApplicationOnDocumentSynchronizedWithCentral;
            }
        }

        private void ApplicationOnDocumentSynchronizedWithCentral(object sender, DocumentSynchronizedWithCentralEventArgs e)
        {
            IsSyncInWork = false;
        }

        private void ApplicationOnDocumentSynchronizingWithCentral(object sender, DocumentSynchronizingWithCentralEventArgs e)
        {
            IsSyncInWork = true;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        private void CreateRibbonTab(UIControlledApplication application)
        {
            RibbonPanel panel = null;
            
            const string tabName = "ModPlus";
            ModPlus_Revit.App.RibbonBuilder.CreateModPlusTabIfNoExist(application);
            var rPanels = application.GetRibbonPanels(tabName);
            foreach (RibbonPanel rPanel in rPanels)
            {
                if (rPanel.Name.Equals(Language.TryGetCuiLocalGroupName("Аннотации")))
                {
                    panel = rPanel;
                    break;
                }
            }

            if (panel == null)
                panel = application.CreateRibbonPanel(tabName, Language.TryGetCuiLocalGroupName("Аннотации"));
            var intF = new ModPlusConnector();
            var rid = new PushButtonData(
                intF.Name,
                ConvertLName(Language.GetFunctionLocalName(intF)),
                Assembly.GetExecutingAssembly().Location,
                intF.FullClassName)
            {
                LargeImage = new System.Windows.Media.Imaging.BitmapImage(
                    new Uri("pack://application:,,,/mprDimBias_" + intF.AvailProductExternalVersion + ";component/Resources/mprDimBias_32x32.png"))
            };
            rid.ToolTip = Language.GetFunctionShortDescription(intF);
            rid.LongDescription = Language.GetFunctionFullDescription(intF);
            rid.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, ModPlus_Revit.App.RibbonBuilder.GetHelpUrl(intF.Name)));
            panel.AddItem(rid);
        }

        private static string ConvertLName(string lName)
        {
            if (!lName.Contains(" "))
                return lName;
            if (lName.Length <= 8)
                return lName;
            if (lName.Count(x => x == ' ') == 1)
            {
                return lName.Split(' ')[0] + Environment.NewLine + lName.Split(' ')[1];
            }

            var center = lName.Length * 0.5;
            var nearestDelta = lName.Select((c, i) => new { index = i, value = c }).Where(w => w.value == ' ')
                .OrderBy(x => Math.Abs(x.index - center)).First().index;
            return lName.Substring(0, nearestDelta) + Environment.NewLine + lName.Substring(nearestDelta + 1);
        }
    }
}
