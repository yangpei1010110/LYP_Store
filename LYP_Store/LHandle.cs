namespace LYP_Utils.LYP_Store
{
    public readonly struct LHandle<TKey>
    {
        public readonly int  Handle;
        public readonly int  HandleVersion;
        public readonly TKey Key;

        public LHandle(int handle, int handleVersion, TKey key)
        {
            Handle = handle;
            HandleVersion = handleVersion;
            Key = key;
        }
    }
}