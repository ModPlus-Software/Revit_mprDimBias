namespace mprDimBias.View
{
    using System.Collections.Generic;
    using Autodesk.Revit.Exceptions;
    using Autodesk.Revit.UI;
    using Autodesk.Revit.UI.Selection;
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Windows;
    using Autodesk.Revit.DB;
    using mprDimBias.Application;
    using mprDimBias.Work;
    using ModPlusAPI;
    using ModPlusAPI.Windows;

    public partial class DimBiasSettings
    {
        private readonly UIApplication _uiApplication;
        private const string LangItem = "mprDimBias";
        public DimBiasSettings(UIApplication uiApplication)
        {
            _uiApplication = uiApplication;
            InitializeComponent();
            Title = ModPlusAPI.Language.GetItem(LangItem, "h1");
        }

        private void ChkOnOffDimBias_OnChecked(object sender, RoutedEventArgs e)
        {
            UserConfigFile.SetValue(UserConfigFile.ConfigFileZone.Settings, "mprDimBias", "DimBiasOnOff", true.ToString(), true);
            AddInId addInId = new AddInId(new ModPlusConnector().AddInId);
            DimensionsDilution.DimDilutionOn(addInId,
                ref MprDimBiasApp.DimensionsDilutionUpdater);
        }

        private void ChkOnOffDimBias_OnUnchecked(object sender, RoutedEventArgs e)
        {
            UserConfigFile.SetValue(UserConfigFile.ConfigFileZone.Settings, "mprDimBias", "DimBiasOnOff", false.ToString(), true);
            AddInId addInId = new AddInId(new ModPlusConnector().AddInId);
            DimensionsDilution.DimDilutionOff(addInId,
                ref MprDimBiasApp.DimensionsDilutionUpdater);
        }
        private void ChkOnOffDimModifyBias_Checked(object sender, RoutedEventArgs e)
        {
            UserConfigFile.SetValue(UserConfigFile.ConfigFileZone.Settings, "mprDimBias", "ModifiedDimBiasOnOff", true.ToString(), true);
            AddInId addInId = new AddInId(new ModPlusConnector().AddInId);
            DimensionsDilution.DimModifiedDilutionOn(addInId,
                ref MprDimBiasApp.DimensionsModifyDilutionUpdater);
        }
        private void ChkOnOffDimModifyBias_Unchecked(object sender, RoutedEventArgs e)
        {
            UserConfigFile.SetValue(UserConfigFile.ConfigFileZone.Settings, "mprDimBias", "ModifiedDimBiasOnOff", false.ToString(), true);
            AddInId addInId = new AddInId(new ModPlusConnector().AddInId);
            DimensionsDilution.DimModifiedDilutionOff(addInId,
                ref MprDimBiasApp.DimensionsModifyDilutionUpdater);
        }

        private void BtOk_OnClick(object sender, RoutedEventArgs e)
        {
            if (TbK.Value != null)
            {
                if (TbK.Value.Value < 0.1 || TbK.Value.Value > 1.0)
                {
                    ModPlusAPI.Windows.MessageBox.Show(ModPlusAPI.Language.GetItem(LangItem, "h5"), MessageBoxIcon.Alert);
                    return;
                }
                MprDimBiasApp.K = TbK.Value.Value;
                UserConfigFile.SetValue(UserConfigFile.ConfigFileZone.Settings, "mprDimBias", "K",
                    TbK.Value.Value.ToString(CultureInfo.InvariantCulture), true);
                Close();
            }
        }

        private void DimBiasSettings_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Statistic.SendCommandStarting(new ModPlusConnector());
                TbK.Value = MprDimBiasApp.K;
                ChkOnOffDimBias.IsChecked = bool.TryParse(UserConfigFile.GetValue(UserConfigFile.ConfigFileZone.Settings, "mprDimBias", "DimBiasOnOff"), out var b) && b; 
                ChkOnOffDimModifyBias.IsChecked = bool.TryParse(UserConfigFile.GetValue(UserConfigFile.ConfigFileZone.Settings,"mprDimBias", "ModifiedDimBiasOnOff"), out b) && b;
                ChkOnOffDimBias.Checked += ChkOnOffDimBias_OnChecked;
                ChkOnOffDimBias.Unchecked += ChkOnOffDimBias_OnUnchecked;
                ChkOnOffDimModifyBias.Checked += ChkOnOffDimModifyBias_Checked;
                ChkOnOffDimModifyBias.Unchecked += ChkOnOffDimModifyBias_Unchecked;
            }
            catch (Exception exception)
            {
                ExceptionBox.Show(exception);
            }
        }

        private void BtResetTextPositionForSelected_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Hide();
                var doc = _uiApplication.ActiveUIDocument.Document;
                var selection = _uiApplication.ActiveUIDocument.Selection;
                var dimensions = PickDimensions(selection, doc);

                var transactionName = ModPlusAPI.Language.GetItem(LangItem, "h8");
                if (string.IsNullOrEmpty(transactionName))
                    transactionName = "Restore the position of the dimension text for the selected dimensions";
                using (Transaction transaction = new Transaction(doc))
                {
                    transaction.Start(transactionName);

                    foreach (Dimension dimension in dimensions)
                    {
                        if (dimension.NumberOfSegments > 0)
                        {
                            foreach (DimensionSegment dimensionSegment in dimension.Segments)
                            {
                                dimensionSegment.ResetTextPosition();
                            }
                        }
                        else dimension.ResetTextPosition();
                    }

                    transaction.Commit();
                }
            }
            catch (Exception exception)
            {
                ExceptionBox.Show(exception);
            }
            finally
            {
                Close();
            }
        }

        private void BtProcessSelected_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Hide();
                var doc = _uiApplication.ActiveUIDocument.Document;
                var selection = _uiApplication.ActiveUIDocument.Selection;
                var dimensions = PickDimensions(selection, doc);

                if (dimensions.Any())
                {
                    var transactionName = ModPlusAPI.Language.GetItem(LangItem, "h7");
                    if (string.IsNullOrEmpty(transactionName))
                        transactionName = "Perform dimension text offset for selected dimensions";
                    using (Transaction transaction = new Transaction(doc))
                    {
                        transaction.Start(transactionName);

                        foreach (Dimension dimension in dimensions)
                        {
                            DimensionsDilution.DoDilution(dimension, doc, out _);
                        }

                        transaction.Commit();
                    }
                }
            }
            catch (Exception exception)
            {
                ExceptionBox.Show(exception);
            }
            finally
            {
                Close();
            }
        }

        private static List<Dimension> PickDimensions(Selection selection, Document doc)
        {
            List<Dimension> dimensions = new List<Dimension>();
            try
            {
                var picked = selection.PickObjects(
                    ObjectType.Element,
                    new DimensionsFilter(),
                    ModPlusAPI.Language.GetItem(LangItem, "h9"));

                foreach (var reference in picked)
                {
                    if (doc.GetElement(reference) is Dimension dimension)
                        dimensions.Add(dimension);
                }
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                // ignore
            }

            return dimensions;
        }

        internal class DimensionsFilter : ISelectionFilter
        {
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

            public bool AllowReference(Reference reference, XYZ position)
            {
                throw new NotImplementedException();
            }
        }
    }
}
