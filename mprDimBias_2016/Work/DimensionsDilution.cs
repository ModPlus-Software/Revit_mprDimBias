using Autodesk.Revit.DB;
using mprDimBias.Body;

namespace mprDimBias.Work
{
    public class DimensionsDilution
    {
        /// <summary>Выполнить "разнесение" размерных значений для указанного размера</summary>
        /// <param name="dimension"></param>
        /// <param name="doc"></param>
        public static void DoDilution(Dimension dimension, Document doc)
        {
            (new AdvancedDimension(dimension, doc)).SetMoveForCorrect();
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

        public static void DimDilutionOff(AddInId activeAddInId, ref DimensionsDilutionUpdater updater)
        {
            if (UpdaterRegistry.IsUpdaterRegistered(updater.GetUpdaterId()))
            {
                UpdaterRegistry.UnregisterUpdater(updater.GetUpdaterId());
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
    }
}