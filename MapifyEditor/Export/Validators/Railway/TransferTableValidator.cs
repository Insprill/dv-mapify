#if UNITY_EDITOR
using System.Collections.Generic;
using Mapify.Editor;
using Mapify.Editor.Utils;
using Mapify.Editor.Validators;

namespace MapifyEditor.Export.Validators
{
    public class TransferTableValidator: Validator
    {
        protected override IEnumerator<Result> Validate(Scenes scenes)
        {
            foreach (var transferTable in scenes.railwayScene.GetAllComponents<TransferTable>())
            {
                if (transferTable.Pit == null)
                {
                    yield return Result.Error("Transfer table must have a pit set", transferTable);
                }
            }
        }
    }
}
#endif
