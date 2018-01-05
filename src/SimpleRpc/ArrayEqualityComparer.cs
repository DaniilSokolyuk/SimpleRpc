using System.Collections.Generic;

namespace SimpleRpc
{
    public sealed class ArrayEqualityComparer<T> : IEqualityComparer<T[]>
    {
        public static readonly ArrayEqualityComparer<T> Instance = new ArrayEqualityComparer<T>();

        private static readonly EqualityComparer<T> _elementComparer = EqualityComparer<T>.Default;

        public bool Equals(T[] first, T[] second)
        {
            if (first == second)
            {
                return true;
            }
            if (first == null || second == null)
            {
                return false;
            }
            if (first.Length != second.Length)
            {
                return false;
            }
            for (int i = 0; i < first.Length; i++)
            {
                if (!_elementComparer.Equals(first[i], second[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public int GetHashCode(T[] array)
        {
            unchecked
            {
                if (array == null)
                {
                    return 0;
                }

                int hash = 17;
                foreach (T element in array)
                {
                    hash = hash * 31 + _elementComparer.GetHashCode(element);
                }
                return hash;
            }
        }
    }

}
