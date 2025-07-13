namespace Mapify.Utils
{
    public static class Misc
    {
        // modulo operator but it works for negative numbers
        // https://stackoverflow.com/a/1082938
        public static int BetterModulo(int x, int m) {
            return (x%m + m)%m;
        }
    }
}
