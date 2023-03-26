using System.Collections.Generic;

namespace Mapify.Editor.Validators
{
    public abstract class Validator
    {
        public abstract IEnumerator<Result> Validate();
    }
}
