using System;
using System.Linq;
using System.Reflection;
using System.Windows.Media.Imaging;
using Autodesk.Revit.UI;
using mprDimBias.Body;
using mprDimBias.Work;
using ModPlusAPI;
using ModPlusAPI.Windows;

namespace mprDimBias.Application
{
    public class MprDimBiasApp : IExternalApplication
    {
        public static DimensionsDilutionUpdater DimensionsDilutionUpdater = null;
        public static UIControlledApplication Application;
        public static double K;

        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                Application = application;
                var dimDilWorkVar = !bool.TryParse(UserConfigFile.GetValue(UserConfigFile.ConfigFileZone.Settings,
                                        "mprDimBias", "DimBiasOnOff"), out var b) || b;
                K = double.TryParse(UserConfigFile.GetValue(UserConfigFile.ConfigFileZone.Settings,
                    "mprDimBias", "K").Replace(',', '.'), out var d)
                    ? d
                    : 0.6;

                DimensionsDilutionUpdater = new DimensionsDilutionUpdater();
                if (dimDilWorkVar)
                    DimensionsDilution.DimDilutionOn(application.ActiveAddInId, ref DimensionsDilutionUpdater);
                else DimensionsDilution.DimDilutionOff(application.ActiveAddInId, ref DimensionsDilutionUpdater);
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

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        private void CreateRibbonTab(UIControlledApplication application)
        {
            RibbonPanel panel = null;
            var rPanels = application.GetRibbonPanels("ModPlus");
            foreach (RibbonPanel rPanel in rPanels)
            {
                if (rPanel.Name.Equals("Аннотации"))
                {
                    panel = rPanel;
                    break;
                }
            }
            if(panel == null)
                panel = application.CreateRibbonPanel("ModPlus", "Аннотации");
            var intF = new Interface();
            PushButtonData rid = new PushButtonData(
                intF.Name,
                ConvertLName(intF.LName),
                Assembly.GetExecutingAssembly().Location,
                intF.FullClassName)
            {
                LargeImage = new BitmapImage(new Uri("pack://application:,,,/mprDimBias_" +
                                                     intF.AvailProductExternalVersion +
                                                     ";component/Resources/mprDimBias_32x32.png"))
            };
            panel.AddItem(rid);
        }
        private static string ConvertLName(string lName)
        {
            if (!lName.Contains(" ")) return lName;
            if (lName.Length <= 8) return lName;
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
