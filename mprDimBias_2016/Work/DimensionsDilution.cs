using Autodesk.Revit.DB;
using mprDimBias.Body;

namespace mprDimBias.Work
{
    public class DimensionsDilution
    {
        /// <summary>Выполнить "разнесение" размерных значений для указанного размера</summary>
        /// <param name="dimension"></param>
        /// <param name="doc"></param>
        /// <param name="modified">Размер был изменен</param>
        public static void DoDilution(Dimension dimension, Document doc, out bool modified)
        {
            try
            {
                new AdvancedDimension(dimension, doc).SetMoveForCorrect(out modified);
            }
            catch
            {
                modified = false;
            }
        }

        public static void DimDilutionOn(AddInId activeAddInId, ref DimensionsDilutionUpdater updater)
        {
            if (!UpdaterRegistry.IsUpdaterRegistered(updater.GetUpdaterId()))
            {
                UpdaterRegistry.RegisterUpdater(updater, true);
                ElementCategoryFilter f = new ElementCategoryFilter(BuiltInCategory.OST_Dimensions);
                UpdaterRegistry.AddTrigger(updater.GetUpdaterId(), f, Element.GetChangeTypeElementAddition());
            }
        }
        public static void DimModifiedDilutionOn(AddInId activeAddInId,  ref DimensionsModifyDilutionUpdater modifyUpdater)
        {
            if (!UpdaterRegistry.IsUpdaterRegistered(modifyUpdater.GetUpdaterId()))
            {
                UpdaterRegistry.RegisterUpdater(modifyUpdater, true);
                ElementCategoryFilter f = new ElementCategoryFilter(BuiltInCategory.OST_Dimensions);
                UpdaterRegistry.AddTrigger(modifyUpdater.GetUpdaterId(), f, Element.GetChangeTypeAny());
            }
        }

        public static void DimDilutionOff(AddInId activeAddInId, ref DimensionsDilutionUpdater updater)
        {
            if (UpdaterRegistry.IsUpdaterRegistered(updater.GetUpdaterId()))
            {
                UpdaterRegistry.UnregisterUpdater(updater.GetUpdaterId());
            }
        }
        public static void DimModifiedDilutionOff(AddInId activeAddInId, ref DimensionsModifyDilutionUpdater modifyUpdater)
        {
            if (UpdaterRegistry.IsUpdaterRegistered(modifyUpdater.GetUpdaterId()))
            {
                UpdaterRegistry.UnregisterUpdater(modifyUpdater.GetUpdaterId());
            }
        }
        public static void DimDilutionStatus(AddInId activeAddInId, ref DimensionsDilutionUpdater updater)
        {
            if (updater != null)
            {
                UpdaterRegistry.UnregisterUpdater(updater.GetUpdaterId());
            }
            else
            {
                updater = new DimensionsDilutionUpdater();
                UpdaterRegistry.RegisterUpdater(updater, false);
                ElementCategoryFilter f = new ElementCategoryFilter(BuiltInCategory.OST_Dimensions);
                UpdaterRegistry.AddTrigger(updater.GetUpdaterId(), f, Element.GetChangeTypeElementAddition());
            }
        }
        public static void DimModifiedDilutionStatus(AddInId activeAddInId, ref DimensionsModifyDilutionUpdater modifyUpdater)
        {
            if (modifyUpdater != null)
            {
                UpdaterRegistry.UnregisterUpdater(modifyUpdater.GetUpdaterId());
            }
            else
            {
                modifyUpdater = new DimensionsModifyDilutionUpdater();
                UpdaterRegistry.RegisterUpdater(modifyUpdater, false);
                ElementCategoryFilter f = new ElementCategoryFilter(BuiltInCategory.OST_Dimensions);
                UpdaterRegistry.AddTrigger(modifyUpdater.GetUpdaterId(), f, Element.GetChangeTypeAny());
            }
        }
    }
}