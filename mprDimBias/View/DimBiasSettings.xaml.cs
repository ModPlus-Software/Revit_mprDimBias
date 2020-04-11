namespace mprDimBias.View
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Windows;
    using Application;
    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;
    using Autodesk.Revit.UI.Selection;
    using Body;
    using ModPlusAPI;
    using ModPlusAPI.Windows;
    using Work;

    public partial class DimBiasSettings
    {
        private const string LangItem = "mprDimBias";
        private readonly UIApplication _uiApplication;

        public DimBiasSettings(UIApplication uiApplication)
        {
            _uiApplication = uiApplication;
            InitializeComponent();
            Title = ModPlusAPI.Language.GetItem(LangItem, "h1");
        }

        private static void ChkOnOffDimBias_OnChecked(object sender, RoutedEventArgs e)
        {
            UserConfigFile.SetValue(LangItem, "DimBiasOnOff", true.ToString(), true);
            DimensionsDilution.DimDilutionOn(MprDimBiasApp.DimensionsDilutionUpdater);
        }

        private static void ChkOnOffDimBias_OnUnchecked(object sender, RoutedEventArgs e)
        {
            UserConfigFile.SetValue(LangItem, "DimBiasOnOff", false.ToString(), true);
            DimensionsDilution.DimDilutionOff(MprDimBiasApp.DimensionsDilutionUpdater);
        }

        private static void ChkOnOffDimModifyBias_Checked(object sender, RoutedEventArgs e)
        {
            var showNotification = !bool.TryParse(UserConfigFile.GetValue(LangItem, "ShowNotification"), out var b) || b;
            if (showNotification)
            {
                var hideNotification = ModPlusAPI.Windows.MessageBox.Show(
                    ModPlusAPI.Language.GetItem(LangItem, "h6"),
                    ModPlusAPI.Language.GetFunctionLocalName(new ModPlusConnector()),
                    ModPlusAPI.Language.GetItem(LangItem, "h10"),
                    MessageBoxIcon.Alert);
                if (hideNotification)
                    UserConfigFile.SetValue(LangItem, "ShowNotification", false.ToString(), true);
            }

            UserConfigFile.SetValue(LangItem, "ModifiedDimBiasOnOff", true.ToString(), true);
            DimensionsDilution.DimModifiedDilutionOn(MprDimBiasApp.DimensionsModifyDilutionUpdater);
        }

        private static void ChkOnOffDimModifyBias_Unchecked(object sender, RoutedEventArgs e)
        {
            UserConfigFile.SetValue(LangItem, "ModifiedDimBiasOnOff", false.ToString(), true);
            DimensionsDilution.DimModifiedDilutionOff(MprDimBiasApp.DimensionsModifyDilutionUpdater);
        }

        private void BtOk_OnClick(object sender, RoutedEventArgs e)
        {
            if (TbK.Value != null)
            {
                if (TbK.Value.Value < 0.1 || TbK.Value.Value > 2.0)
                {
                    ModPlusAPI.Windows.MessageBox.Show(ModPlusAPI.Language.GetItem(LangItem, "h5"), MessageBoxIcon.Alert);
                    return;
                }

                MprDimBiasApp.OffsetFactor = TbK.Value.Value;
                UserConfigFile.SetValue(LangItem, "K", TbK.Value.Value.ToString(CultureInfo.InvariantCulture), true);
                Close();
            }
        }

        private void DimBiasSettings_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Statistic.SendCommandStarting(new ModPlusConnector());
                TbK.Value = MprDimBiasApp.OffsetFactor;
                ChkOnOffDimBias.IsChecked = bool.TryParse(UserConfigFile.GetValue(LangItem, "DimBiasOnOff"), out var b) && b; 
                ChkOnOffDimModifyBias.IsChecked = bool.TryParse(UserConfigFile.GetValue(LangItem, "ModifiedDimBiasOnOff"), out b) && b;
                ChkMoveDownInsteadSide.IsChecked = bool.TryParse(UserConfigFile.GetValue(LangItem, "MoveDownInsteadSide"), out b) && b;

                ChkOnOffDimBias.Checked += ChkOnOffDimBias_OnChecked;
                ChkOnOffDimBias.Unchecked += ChkOnOffDimBias_OnUnchecked;
                ChkOnOffDimModifyBias.Checked += ChkOnOffDimModifyBias_Checked;
                ChkOnOffDimModifyBias.Unchecked += ChkOnOffDimModifyBias_Unchecked;
                ChkMoveDownInsteadSide.Checked += ChkMoveDownInsteadSideOnChecked;
                ChkMoveDownInsteadSide.Unchecked += ChkMoveDownInsteadSideOnUnchecked;
            }
            catch (Exception exception)
            {
                ExceptionBox.Show(exception);
            }
        }

        private void ChkMoveDownInsteadSideOnChecked(object sender, RoutedEventArgs e)
        {
            UserConfigFile.SetValue(LangItem, "MoveDownInsteadSide", true.ToString(), true);
        }

        private void ChkMoveDownInsteadSideOnUnchecked(object sender, RoutedEventArgs e)
        {
            UserConfigFile.SetValue(LangItem, "MoveDownInsteadSide", false.ToString(), true);
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
                using (var transaction = new Transaction(doc))
                {
                    transaction.Start(transactionName);

                    foreach (var dimension in dimensions)
                    {
                        if (dimension.NumberOfSegments > 0)
                        {
                            foreach (DimensionSegment dimensionSegment in dimension.Segments)
                            {
                                dimensionSegment.ResetTextPosition();
                            }
                        }
                        else
                        {
                            dimension.ResetTextPosition();
                        }
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
                    using (var transaction = new Transaction(doc))
                    {
                        transaction.Start(transactionName);

                        foreach (var dimension in dimensions)
                        {
                            new AdvancedDimension(dimension).SetMoveForCorrect();
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
            var dimensions = new List<Dimension>();
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
    }
}
