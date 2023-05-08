namespace Mapify.Utils
{
    // I'm feeling a little... *rusty* 🦀
    public struct Option<T>
    {
        public static Option<T> None = new Option<T>();

        private static Option<T> Some(T value)
        {
            return new Option<T>(value);
        }

        private T value;
        private bool hasValue;

        private Option(T value)
        {
            this.value = value;
            hasValue = true;
        }

        public bool IsSome(out T some)
        {
            some = value;
            return hasValue;
        }

        private T Take()
        {
            T val = value;
            value = default;
            hasValue = false;
            return val;
        }

        public bool TakeIfSome(out T val)
        {
            if (!hasValue)
            {
                val = default;
                return false;
            }

            val = Take();
            return true;
        }

        public static implicit operator Option<T>(T value)
        {
            return value != null ? Some(value) : None;
        }
    }
}
