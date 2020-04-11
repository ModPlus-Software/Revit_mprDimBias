namespace mprDimBias.Work
{
    using Autodesk.Revit.DB;
    using Body;

    public class DimensionsDilution
    {
        public static void DimDilutionOn(DimensionsDilutionUpdater updater)
        {
            if (!UpdaterRegistry.IsUpdaterRegistered(updater.GetUpdaterId()))
            {
                UpdaterRegistry.RegisterUpdater(updater, true);
                var f = new ElementCategoryFilter(BuiltInCategory.OST_Dimensions);
                UpdaterRegistry.AddTrigger(updater.GetUpdaterId(), f, Element.GetChangeTypeElementAddition());
            }
        }

        public static void DimModifiedDilutionOn(DimensionsModifyDilutionUpdater modifyUpdater)
        {
            if (!UpdaterRegistry.IsUpdaterRegistered(modifyUpdater.GetUpdaterId()))
            {
                UpdaterRegistry.RegisterUpdater(modifyUpdater, true);
                var f = new ElementCategoryFilter(BuiltInCategory.OST_Dimensions);
                UpdaterRegistry.AddTrigger(modifyUpdater.GetUpdaterId(), f, Element.GetChangeTypeAny());
            }
        }

        public static void DimDilutionOff(DimensionsDilutionUpdater updater)
        {
            if (UpdaterRegistry.IsUpdaterRegistered(updater.GetUpdaterId()))
            {
                UpdaterRegistry.UnregisterUpdater(updater.GetUpdaterId());
            }
        }

        public static void DimModifiedDilutionOff(DimensionsModifyDilutionUpdater modifyUpdater)
        {
            if (UpdaterRegistry.IsUpdaterRegistered(modifyUpdater.GetUpdaterId()))
            {
                UpdaterRegistry.UnregisterUpdater(modifyUpdater.GetUpdaterId());
            }
        }

        public static void DimDilutionStatus(DimensionsDilutionUpdater updater)
        {
            if (updater != null)
            {
                UpdaterRegistry.UnregisterUpdater(updater.GetUpdaterId());
            }
            else
            {
                updater = new DimensionsDilutionUpdater();
                UpdaterRegistry.RegisterUpdater(updater, false);
                var f = new ElementCategoryFilter(BuiltInCategory.OST_Dimensions);
                UpdaterRegistry.AddTrigger(updater.GetUpdaterId(), f, Element.GetChangeTypeElementAddition());
            }
        }

        public static void DimModifiedDilutionStatus(DimensionsModifyDilutionUpdater modifyUpdater)
        {
            if (modifyUpdater != null)
            {
                UpdaterRegistry.UnregisterUpdater(modifyUpdater.GetUpdaterId());
            }
            else
            {
                modifyUpdater = new DimensionsModifyDilutionUpdater();
                UpdaterRegistry.RegisterUpdater(modifyUpdater, false);
                var f = new ElementCategoryFilter(BuiltInCategory.OST_Dimensions);
                UpdaterRegistry.AddTrigger(modifyUpdater.GetUpdaterId(), f, Element.GetChangeTypeAny());
            }
        }
    }
}