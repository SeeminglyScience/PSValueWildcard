using System;
using System.Runtime.InteropServices;

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
                    case '[': ProcessAnyOf(); break;
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

        private void ProcessAnyOf()
        {
            MaybeAddPendingExact();
            int start = _index;
            int length = 0;
            bool found = false;
            bool isPartial = false;
            while (MoveNext())
            {
                length++;
                char current = *_chars;
                if (current == '`')
                {
                    isPartial = true;
                    if (length > 1)
                    {
                        Steps.Add(
                            WildcardInstruction.PartialAnyOf(
                                _pattern.Slice(
                                    start + 1,
                                    length - 1)));
                    }

                    // Skip trying to process the next character.
                    start = _index;
                    length = 1;
                    MoveNext();
                    continue;
                }

                if (current != ']')
                {
                    continue;
                }

                found = true;
                Steps.Add(
                    WildcardInstruction.AnyOf(
                        _pattern.Slice(
                            start + 1,
                            length - 1),
                        isPartial));

                break;
            }

            if (!found)
            {
                throw new InvalidOperationException(
                    string.Format(
                        System.Globalization.CultureInfo.CurrentCulture,
                        "The specified wildcard character pattern is not valid: {0}",
                        _pattern.ToString()));
            }
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
