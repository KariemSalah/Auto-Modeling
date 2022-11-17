#region Namespaces
using Autodesk.Revit.DB;
using System.Collections.Generic;
#endregion

namespace CadToBim.Util
{
    public class FailureProcess : IFailuresPreprocessor
    {

        private List<FailureDefinitionId> _failureIdList = new List<FailureDefinitionId>();
        private bool deleteErrors = false;
        private bool deleteWarnings = false;

        public FailureProcess(bool deleteWarings, bool deleteErrors)
        {
            this.deleteWarnings = deleteWarings;
            this.deleteErrors = deleteErrors;
        }
        public FailureProcess(FailureDefinitionId id)
        {
            _failureIdList.Add(id);
        }
        public FailureProcess(List<FailureDefinitionId> idList)
        {
            this._failureIdList = idList;
        }

        FailureProcessingResult IFailuresPreprocessor.PreprocessFailures(FailuresAccessor failuresAccessor)
        {
            IList<FailureMessageAccessor> failList = failuresAccessor.GetFailureMessages();

            if (_failureIdList.Count == 0)
            {
                failuresAccessor.DeleteAllWarnings();
                if (deleteErrors)
                {
                    foreach (FailureMessageAccessor accessor in failList)
                    {
                        if (accessor.GetSeverity() == FailureSeverity.Error)
                        {
                            var ids = accessor.GetFailingElementIds();
                            failuresAccessor.DeleteElements((IList<ElementId>)ids.GetEnumerator());
                        }
                    }
                }
            }
            else
            {
                foreach (FailureMessageAccessor failure in failList)
                {
                    FailureDefinitionId failId = failure.GetFailureDefinitionId();
                    if (_failureIdList.Exists(p => p == failId))
                    {
                        failuresAccessor.DeleteWarning(failure);
                    }
                }
            }
            return FailureProcessingResult.Continue;
        }

    }
}

