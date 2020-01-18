using System;

namespace PSValueWildcard
{
    internal unsafe ref struct WildcardParser
    {
        internal StackLoadedValueList<WildcardInstruction> Steps;

        private readonly StringPart _pattern;

        private int _index;

        private char* _chars;

        private int? _exactStart;

        internal WildcardParser(Span<WildcardInstruction> initialBuffer, StringPart pattern)
        {
            _pattern = pattern;
            Steps = new StackLoadedValueList<WildcardInstruction>(initialBuffer);
            _index = -1;
            _chars = _pattern.Pointer;
            _exactStart = null;
        }

        public void Dispose() => Steps.Dispose();

        public void Parse()
        {
            bool ignoreNext = false;
            while (MoveNext())
            {
                if (ignoreNext)
                {
                    _exactStart ??= _index;
                    ignoreNext = false;
                    continue;
                }

                switch (*_chars)
                {
                    case '`': MaybeAddPendingExact(); ignoreNext = true; break;
                    case '*': MaybeAddPendingExact(); Steps.Add(WildcardInstruction.AnyAny); break;
                    case '?': MaybeAddPendingExact(); Steps.Add(WildcardInstruction.AnyOne); break;
                    case '[':
                    {
                        MaybeAddPendingExact();
                        int start = _index;
                        int length = 0;
                        bool found = false;
                        while (MoveNext())
                        {
                            length++;
                            if (*_chars != ']')
                            {
                                continue;
                            }

                            found = true;
                            Steps.Add(
                                WildcardInstruction.AnyOf(
                                    _pattern.Slice(
                                        start + 1,
                                        length - 1)));

                            break;
                        }

                        if (!found)
                        {
                            Steps.Add(WildcardInstruction.Exact(_pattern.Slice(start)));
                        }

                        break;
                    }
                    default: _exactStart ??= _index; break;
                }
            }

            if (_exactStart == null)
            {
                return;
            }

            Steps.Add(WildcardInstruction.Exact(_pattern.Slice(_exactStart.Value)));
            _exactStart = null;
        }

        private void MaybeAddPendingExact()
        {
            if (_exactStart == null)
            {
                return;
            }

            Steps.Add(
                WildcardInstruction.Exact(
                    _pattern.Slice(
                        _exactStart.Value,
                        _index - _exactStart.Value)));
            _exactStart = null;
        }

        private bool MoveNext()
        {
            if (_index == -1)
            {
                if (_pattern.Length == 0)
                {
                    return false;
                }

                _index++;
                return true;
            }

            if (_index >= _pattern.Length - 1)
            {
                return false;
            }

            _index++;
            _chars++;
            return true;
        }
    }
}
