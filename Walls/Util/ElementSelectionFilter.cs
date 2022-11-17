
#region Namespaces
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
#endregion

namespace CadToBim.Util
{
    public class ElementSelectionFilter<T> : ISelectionFilter where T : Element
    {
        // Allow selection of elements of type T only.
        public bool AllowElement(Element e)
        {
            return e is T;
        }

        public bool AllowReference(Reference r, XYZ p)
        {
            return true;
        }
    }
}

