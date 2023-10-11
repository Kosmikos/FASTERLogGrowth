using FASTER.core;

namespace SimpleChecks.Data
{
    public struct Key
    {
        public long key;

        public Key(long first) => key = first;

        public override string ToString() => key.ToString();

        internal class Comparer : IFasterEqualityComparer<Key>
        {
            public long GetHashCode64(ref Key key) => Utility.GetHashCode(key.key);

            public bool Equals(ref Key k1, ref Key k2) => k1.key == k2.key;
        }
    }
}