using System;
using System.Globalization;
using System.Windows;
using Autodesk.Revit.DB;
using mprDimBias.Application;
using mprDimBias.Work;
using ModPlusAPI;
using ModPlusAPI.Windows;

namespace mprDimBias.View
{
    public partial class DimBiasSettings
    {
        private const string LangItem = "mprDimBias";
        public DimBiasSettings()
        {
            InitializeComponent();
            Title = ModPlusAPI.Language.GetItem(LangItem, "h1");
        }

        private void ChkOnOffDimBias_OnChecked(object sender, RoutedEventArgs e)
        {
            UserConfigFile.SetValue(UserConfigFile.ConfigFileZone.Settings, "mprDimBias", "DimBiasOnOff", true.ToString(), true);
            AddInId addInId = new AddInId(new Interface().AddInId);
            DimensionsDilution.DimDilutionOn(addInId,
                ref MprDimBiasApp.DimensionsDilutionUpdater);
        }

        private void ChkOnOffDimBias_OnUnchecked(object sender, RoutedEventArgs e)
        {
            UserConfigFile.SetValue(UserConfigFile.ConfigFileZone.Settings, "mprDimBias", "DimBiasOnOff", false.ToString(), true);
            AddInId addInId = new AddInId(new Interface().AddInId);
            DimensionsDilution.DimDilutionOff(addInId,
                ref MprDimBiasApp.DimensionsDilutionUpdater);
        }
        private void ChkOnOffDimModifyBias_Checked(object sender, RoutedEventArgs e)
        {
            UserConfigFile.SetValue(UserConfigFile.ConfigFileZone.Settings, "mprDimBias", "ModifiedDimBiasOnOff", true.ToString(), true);
            AddInId addInId = new AddInId(new Interface().AddInId);
            DimensionsDilution.DimModifiedDilutionOn(addInId,
                ref MprDimBiasApp.DimensionsModifyDilutionUpdater);
        }
        private void ChkOnOffDimModifyBias_Unchecked(object sender, RoutedEventArgs e)
        {
            UserConfigFile.SetValue(UserConfigFile.ConfigFileZone.Settings, "mprDimBias", "ModifiedDimBiasOnOff", false.ToString(), true);
            AddInId addInId = new AddInId(new Interface().AddInId);
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
                Statistic.SendCommandStarting(new Interface());
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
    }
}
