using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using mprDimBias.Body;

namespace mprDimBias
{
    [Transaction(TransactionMode.Manual)]
    public class Test : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var selection = commandData.Application.ActiveUIDocument.Selection;
            var doc = commandData.Application.ActiveUIDocument.Document;

            var refEl = selection.PickObject(ObjectType.Element);
            var dim = doc.GetElement(refEl) as Dimension;
            using (Transaction tr = new Transaction(doc, "test"))
            {
                tr.Start();
                var advDim = new AdvancedDimension(dim, doc);
                    advDim.SetMoveForCorrect(out bool _);
                tr.Commit();
            }
            return Result.Succeeded;
        }
    }
}
