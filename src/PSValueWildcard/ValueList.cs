using System;
using System.Buffers;

namespace PSValueWildcard
{
    internal ref struct ValueList<T>
    {
        private T[]? _items;

        private int _length;

        public ValueList(int capacity)
        {
            _items = null;
            _length = 0;
            if (capacity > 0)
            {
                EnsureCapacity(capacity);
            }
        }

        public int Length
        {
            get => _length;
            set
            {
                if (Capacity > value)
                {
                    EnsureCapacity(_length - value);
                }
                else if (_items != null && _length > value)
                {
                    Array.Clear(_items, value, _length - value);
                }

                _length = value;
            }
        }

        public int Capacity
        {
            get => _items?.Length ?? 0;
            set => EnsureCapacity(Capacity - value);
        }

        public readonly Span<T> AsSpan()
        {
#pragma warning disable RCS1206
            return _items == null ? default : _items.AsSpan(0, _length);
#pragma warning restore RCS1206
        }

        public void Add(T item)
        {
            EnsureCapacity(amount: 1);
            _items![Length++] = item;
        }

        public ref T Create()
        {
            EnsureCapacity(amount: 1);
            return ref _items![Length++];
        }

        public void Dispose() => Free();

        public void Free()
        {
            if (_items == null)
            {
                return;
            }

            T[] items = _items;
            _items = null;
            Length = 0;
            Array.Clear(items, 0, items.Length);
            ArrayPool<T>.Shared.Return(items);
        }

        public T[] ToArray()
        {
            if (_items == null)
            {
                return Array.Empty<T>();
            }

            var result = new T[Length];
            Array.Copy(_items, result, Length);
            return result;
        }

        private void EnsureCapacity(int amount)
        {
            int capacity = _items?.Length ?? 0;
            int requestedCapacity = Length + amount;
            if (capacity >= requestedCapacity)
            {
                return;
            }

            int targetCapacity = Math.Max(capacity * 2, requestedCapacity);
            if (_items == null)
            {
                _items = ArrayPool<T>.Shared.Rent(targetCapacity);
                return;
            }

            T[] oldItems = _items;
            T[] newItems = ArrayPool<T>.Shared.Rent(targetCapacity);
            Array.Copy(oldItems, newItems, Length);
            _items = newItems;
            Array.Clear(oldItems, 0, oldItems.Length);
            ArrayPool<T>.Shared.Return(oldItems);
        }
    }
}
