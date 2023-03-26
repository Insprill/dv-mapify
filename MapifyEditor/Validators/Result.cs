using UnityEngine;

namespace Mapify.Editor.Validators
{
    public class Result
    {
        public readonly Object context;
        public readonly string message;

        private Result(string message, Object context)
        {
            this.message = message;
            this.context = context;
        }

        public static Result Error(string message, Object context = null)
        {
            return new Result(message, context);
        }
    }
}
