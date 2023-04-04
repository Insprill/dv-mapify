using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace Mapify.Editor.Validators
{
    public abstract class Validator
    {
        public abstract IEnumerator<Result> Validate(List<Scene> scenes);
    }
}
