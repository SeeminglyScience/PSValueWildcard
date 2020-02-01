using System;

namespace PSValueWildcard
{
    internal ref struct StackLoadedValueList<T>
        where T : unmanaged
    {
        private readonly Span<T> _buffer;

        private ValueList<T> _list;

        private int _length;

        public StackLoadedValueList(Span<T> initialBuffer)
        {
            _buffer = initialBuffer;
            _length = 0;
            _list = new ValueList<T>(capacity: 0);
        }

        public int Length
        {
            readonly get
            {
                return _list.Length + _length;
            }
            set
            {
                int currentLength = Length;
                int difference;
                if (value > currentLength)
                {
                    difference = value - currentLength;
                    if (!IsInitialBufferFilled)
                    {
                        int remainingBuffer = _buffer.Length - _length;
                        if (remainingBuffer >= difference)
                        {
                            _length = value;
                            return;
                        }

                        _length = _buffer.Length;
                        _list.Length = difference - remainingBuffer;
                        return;
                    }

                    _list.Length = difference - _buffer.Length;
                    return;
                }

                difference = currentLength - value;
                if (!IsInitialBufferFilled)
                {
                    _buffer.Slice(difference).Clear();
                    _length = value;
                    return;
                }

                if (difference <= _list.Length)
                {
                    _list.Length -= difference;
                    return;
                }

                int remainingDifference = difference - _list.Length;
                _list.Length = 0;
                _buffer.Slice(remainingDifference).Clear();
                _length = value;
            }
        }

        private readonly bool IsInitialBufferFilled => _length >= _buffer.Length;

        public ref T Create()
        {
            if (IsInitialBufferFilled)
            {
                return ref _list.Create();
            }

            return ref _buffer[_length++];
        }

        public void Add(T item)
        {
            Create() = item;
        }

        public readonly T[] ToArray()
        {
            T[] result;
            if (!IsInitialBufferFilled)
            {
                result = new T[_length];
                _buffer.Slice(0, _length).CopyTo(result.AsSpan());
                return result;
            }

            result = new T[_length + _list.Length];
            _buffer.CopyTo(result);
            _list.AsSpan().CopyTo(result.AsSpan().Slice(_length));
            return result;
        }

        public readonly bool TryGetSpan(out ReadOnlySpan<T> result)
        {
            if (IsInitialBufferFilled && _list.Length > 0)
            {
                result = default;
                return false;
            }

            result = _buffer.Slice(0, _length);
            return true;
        }

        public void Dispose()
        {
            _list.Dispose();
        }
    }
}
