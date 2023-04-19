using UnityEngine;

namespace Mapify.Editor.Validators
{
    public class Result
    {
        public readonly ResultType type;
        public readonly Object context;
        public readonly string message;

        private Result(ResultType type, string message, Object context)
        {
            this.type = type;
            this.message = message;
            this.context = context;
        }

        public static Result Error(string message, Object context = null)
        {
            return new Result(ResultType.ERROR, message, context);
        }

        public static Result Warning(string message, Object context = null)
        {
            return new Result(ResultType.WARNING, message, context);
        }

        public enum ResultType
        {
            ERROR,
            WARNING
        }
    }
}
