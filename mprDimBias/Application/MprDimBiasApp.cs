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
        private static UIControlledApplication _application;

        /// <summary>
        /// Идентификаторы размеров, смещенных при работе текущего плагина
        /// </summary>
        public static Dictionary<ElementId, bool> DimsModifiedByUpdater { get; private set; }

        /// <summary>
        /// Коэффициент смещения
        /// </summary>
        public static double OffsetFactor { get; set; }

        /// <summary>
        /// Is synchronization with central model in work
        /// </summary>
        public static bool IsSyncInWork { get; private set; }

        /// <summary>
        /// Static instance of <see cref="DimensionsDilutionUpdater"/>
        /// </summary>
        public static DimensionsDilutionUpdater DimensionsDilutionUpdater { get; private set; }

        /// <summary>
        /// Static instance of <see cref="DimensionsModifyDilutionUpdater"/>
        /// </summary>
        public static DimensionsModifyDilutionUpdater DimensionsModifyDilutionUpdater { get; private set; }

        /// <inheritdoc/>
        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                _application = application;
                _application.Idling += ApplicationOnIdling;

                DimsModifiedByUpdater = new Dictionary<ElementId, bool>();

                var dimDilWorkVar =
                    bool.TryParse(UserConfigFile.GetValue("mprDimBias", "DimBiasOnOff"), out var b) && b;

                var dimModifiedDilWorkVar =
                    bool.TryParse(UserConfigFile.GetValue("mprDimBias", "ModifiedDimBiasOnOff"), out b) && b;

                OffsetFactor = double.TryParse(UserConfigFile.GetValue("mprDimBias", "K"), NumberStyles.Number, CultureInfo.InvariantCulture, out var d)
                    ? d
                    : 0.6;

                DimensionsDilutionUpdater = new DimensionsDilutionUpdater();
                DimensionsModifyDilutionUpdater = new DimensionsModifyDilutionUpdater();
                if (dimDilWorkVar)
                    DimensionsDilution.DimDilutionOn(DimensionsDilutionUpdater);
                else
                    DimensionsDilution.DimDilutionOff(DimensionsDilutionUpdater);
                if (dimModifiedDilWorkVar)
                    DimensionsDilution.DimModifiedDilutionOn(DimensionsModifyDilutionUpdater);
                else
                    DimensionsDilution.DimModifiedDilutionOff(DimensionsModifyDilutionUpdater);

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

        /// <inheritdoc />
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        private static void ApplicationOnIdling(object sender, IdlingEventArgs e)
        {
            if (sender is UIApplication uiApplication)
            {
                _application.Idling -= ApplicationOnIdling;
#if !R2017 && !R2018 && !R2019 && !R2020
                uiApplication.Application.DocumentReloadingLatest += ApplicationOnDocumentReloadingLatest;
                uiApplication.Application.DocumentReloadedLatest += ApplicationOnDocumentReloadedLatest;
#endif
                uiApplication.Application.DocumentSynchronizingWithCentral += ApplicationOnDocumentSynchronizingWithCentral;
                uiApplication.Application.DocumentSynchronizedWithCentral += ApplicationOnDocumentSynchronizedWithCentral;
            }
        }

#if !R2017 && !R2018 && !R2019 && !R2020
        private static void ApplicationOnDocumentReloadedLatest(object sender, DocumentReloadedLatestEventArgs e)
        {
            IsSyncInWork = false;
        }

        private static void ApplicationOnDocumentReloadingLatest(object sender, DocumentReloadingLatestEventArgs e)
        {
            IsSyncInWork = true;
        }
#endif

        private static void ApplicationOnDocumentSynchronizedWithCentral(object sender, DocumentSynchronizedWithCentralEventArgs e)
        {
            IsSyncInWork = false;
        }

        private static void ApplicationOnDocumentSynchronizingWithCentral(object sender, DocumentSynchronizingWithCentralEventArgs e)
        {
            IsSyncInWork = true;
        }

        private static void CreateRibbonTab(UIControlledApplication application)
        {
            var panel = ModPlus_Revit.App.RibbonBuilder.GetOrCreateRibbonPanel(
                application,
                "ModPlus",
                Language.TryGetCuiLocalGroupName("Аннотации"));

            var intF = new ModPlusConnector();

            var rid = new PushButtonData(
                intF.Name,
                ConvertLName(Language.GetFunctionLocalName(intF)),
                Assembly.GetExecutingAssembly().Location,
                intF.FullClassName)
            {
                LargeImage = new System.Windows.Media.Imaging.BitmapImage(
                    new Uri(
                        $"pack://application:,,,/mprDimBias_{intF.AvailProductExternalVersion};component/Resources/mprDimBias_32x32.png"))
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
