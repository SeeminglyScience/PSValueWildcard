using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace PSValueWildcard
{
    internal ref partial struct WildcardInterpreter
    {
        private const int FrameStackAllocThreshold = 0x200;

        // Estimated characters per instruction, e.g. "*test*" would be 6 characters and
        // 3 instructions. This is currently a guess and needs to be adjusted with research.
        private const int AverageCharactersPerInstruction = 5;

        // Fallback to array pool buffers if we expect a large amount of instructions.
        private const int ParserStackAllocThreshold = 0x50;

        private readonly StringPart _source;

        private readonly bool _isCaseSensitive;

        private readonly ReadOnlySpan<WildcardInstruction> _instructions;

        // Do not make this field readonly.
#pragma warning disable IDE0044
        private Span<Frame> _frames;
#pragma warning restore IDE0044

        private int _index;

        private int _textPosition;

        private readonly CultureInfo _culture;

        private WildcardInterpreter(
            ReadOnlySpan<WildcardInstruction> instructions,
            Span<Frame> frames,
            StringPart source,
            ValueWildcardOptions options)
        {
            _instructions = instructions;
            _frames = frames;
            _source = source;
            _culture = options.Culture;
            _isCaseSensitive = options.IsCaseSensitive;
            _index = 0;
            _textPosition = 0;
        }

        internal static unsafe bool IsMatch(
            char* input,
            int inputLength,
            char* pattern,
            int patternLength,
            ValueWildcardOptions options = default)
        {
            return IsMatch(
                new StringPart(input, inputLength),
                new StringPart(pattern, patternLength),
                options);
        }

        internal static unsafe bool IsMatch(
            StringPart input,
            ReadOnlySpan<WildcardInstruction> instructions,
            ValueWildcardOptions options = default)
        {
            Span<Frame> state = instructions.Length > FrameStackAllocThreshold
                ? new Frame[instructions.Length]
                : stackalloc Frame[instructions.Length];

            var interpreter = new WildcardInterpreter(instructions, state, input, options);
            return interpreter.IsMatch();
        }

        private static unsafe bool IsMatch(
            StringPart input,
            StringPart pattern,
            ValueWildcardOptions options = default)
        {
            int estimatedStackSize = pattern.Length / AverageCharactersPerInstruction;
            Span<WildcardInstruction> buffer = estimatedStackSize > ParserStackAllocThreshold
                ? default
                : stackalloc WildcardInstruction[estimatedStackSize];

            var parser = new WildcardParser(buffer, pattern);
            try
            {
                parser.Parse();
                int stepCount = parser.Steps.Length;
                if (stepCount <= estimatedStackSize)
                {
                    return IsMatch(input, buffer.Slice(0, stepCount), options);
                }

                return IsMatch(input, parser.Steps.ToArray(), options);
            }
            finally
            {
                parser.Dispose();
            }
        }

        public unsafe bool IsMatch()
        {
            while (true)
            {
                if (_index >= _frames.Length)
                {
                    return _textPosition == _source.Length;
                }

                ref readonly WildcardInstruction opcode = ref _instructions[_index];
                ref Frame frame = ref _frames[_index];
                if (opcode.Kind == WildcardStepKind.AnyAny)
                {
                    if (_index == _frames.Length - 1)
                    {
                         return true;
                    }

                    _frames[++_index].PreceededByWildcard = true;
                    continue;
                }

                if (opcode.Kind == WildcardStepKind.AnyOne)
                {
                    if (_source.Length - _textPosition - 1 > 0)
                    {
                        _textPosition++;
                        _index++;
                        continue;
                    }

                    return false;
                }

                bool shouldBacktrack;
                if (opcode.Kind == WildcardStepKind.PartialAnyOf)
                {
                    int stepLength = 0;
                    int argumentLength = 0;
                    for (int i = _index; i < _instructions.Length; i++)
                    {
                        ref readonly var currentOpCode = ref _instructions[i];
                        if (currentOpCode.Kind != WildcardStepKind.PartialAnyOf)
                        {
                            break;
                        }

                        stepLength++;
                        argumentLength += currentOpCode.Args.Length;
                    }

                    const int StackAllocThreshold = 0x100;
                    Span<char> combinedArgs = argumentLength < StackAllocThreshold
                        ? stackalloc char[argumentLength]
                        : new char[argumentLength];

                    int currentPosition = 0;
                    for (int i = 0; i < stepLength; i++)
                    {
                        var argSpan = _instructions[i + _index].Args.AsSpan();
                        argSpan.CopyTo(combinedArgs.Slice(currentPosition));
                        currentPosition += argSpan.Length;
                    }

                    var combinedOpCode = WildcardInstruction.AnyOf(
                        new StringPart(
                            (char*)Unsafe.AsPointer(ref MemoryMarshalPoly.GetReference(combinedArgs)),
                            combinedArgs.Length));

                    if (ProcessOpCode(ref frame, in combinedOpCode, out shouldBacktrack, stepLength))
                    {
                        continue;
                    }
                }
                else if (ProcessOpCode(ref frame, in opcode, out shouldBacktrack))
                {
                    continue;
                }


                if (shouldBacktrack && TryBacktrack())
                {
                    continue;
                }

                return false;
            }
        }

        private bool ProcessOpCode(
            ref Frame frame,
            in WildcardInstruction opcode,
            out bool shouldBacktrack,
            int framesToJump = 1)
        {
            if (frame.PreceededByWildcard)
            {
                frame.CanBacktrackTo = true;
                Range range = FindNext(in opcode);
                if (range.IsInvalid)
                {
                    shouldBacktrack = true;
                    return false;
                }

                frame.Position = range;
                _textPosition = range.End;
                _index += framesToJump;
                shouldBacktrack = false;
                return true;
            }

            if (IsAtMatch(in opcode))
            {
                _index += framesToJump;
                opcode.TryGetLength(out int length);
                frame.Position = new Range(
                    _textPosition,
                    _textPosition + length);
                _textPosition = frame.Position.End;
                shouldBacktrack = false;
                return true;
            }

            shouldBacktrack = true;
            return false;
        }

        private bool TryBacktrack()
        {
            for (int i = _index - 1; i >= 0; i--)
            {
                ref Frame frame = ref _frames[i];
                if (frame.CanBacktrackTo)
                {
                    _index = i;
                    _textPosition = frame.Position.Start + 1;
                    return true;
                }
            }

            return false;
        }

        private Range FindNext(in WildcardInstruction opcode)
        {
            return opcode.Kind switch
            {
                WildcardStepKind.AnyOf => FindNextAnyOf(opcode.Args),
                WildcardStepKind.Exact => FindNextExplicit(opcode.Args),
                _ => throw new ArgumentOutOfRangeException(nameof(opcode.Kind)),
            };
        }

        private bool IsAtMatch(in WildcardInstruction opcode)
        {
            return opcode.Kind switch
            {
                WildcardStepKind.AnyOf => IsAtMatchAnyOf(opcode.Args),
                WildcardStepKind.Exact => IsAtMatchExplicit(opcode.Args),
                _ => throw new ArgumentOutOfRangeException(nameof(opcode.Kind)),
            };
        }

        private bool IsAtMatchAnyOf(StringPart arguments)
        {
            var remaining = GetRemaining();
            if (remaining.Length < 1)
            {
                return false;
            }

            char next = remaining[0];
            for (int i = 0; i < arguments.Length; i++)
            {
                if (CharEquals(next, arguments[i]))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsAtMatchExplicit(StringPart arguments)
        {
            var remaining = GetRemaining();
            if (remaining.Length < arguments.Length)
            {
                return false;
            }

            for (int i = arguments.Length - 1; i >= 0; i--)
            {
                if (!CharEquals(remaining[i], arguments[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private (int index, int length) FindNextAnyOf(StringPart arguments)
        {
            var index = IndexOfAny(GetRemaining(), arguments);
            if (index == -1)
            {
                return (-1, 0);
            }

            return (index, 1);
        }

        private (int index, int length) FindNextExplicit(StringPart arguments)
        {
            var index = IndexOf(GetRemaining(), arguments);
            if (index == -1)
            {
                return (-1, 0);
            }

            return (index + _textPosition, index + _textPosition + arguments.Length);
        }

        private unsafe int IndexOf(StringPart source, char value)
        {
            char normalizedValue = Normalize(value);
            char* current = source.Pointer;
            for (int i = 0; i < source.Length; i++)
            {
                if (Normalize(*current++) == normalizedValue)
                {
                    return i;
                }
            }

            return -1;
        }

        private char Normalize(char value)
        {
            return _isCaseSensitive ? value : char.ToLower(value, _culture);
        }

        private unsafe int IndexOf(StringPart source, StringPart value)
        {
            if (value.IsEmpty)
            {
                return -1;
            }

            if (source.Length < value.Length)
            {
                return -1;
            }

            char firstChar = value[0];
            int firstCharIndex;
            StringPart remainingChars = value.Slice(1);
            StringPart remainingSource = source;
            int rollingIndex = 0;
            while (true)
            {
                FindFirstCharLoop:
                firstCharIndex = IndexOf(remainingSource, firstChar);
                if (firstCharIndex == -1)
                {
                    return -1;
                }

                remainingSource = remainingSource.Slice(firstCharIndex + 1);
                rollingIndex += firstCharIndex + 1;

                if (remainingSource.Length < remainingChars.Length)
                {
                    return -1;
                }

                for (int i = 0; i < remainingChars.Length; i++)
                {
                    if (!CharEquals(remainingSource[i], remainingChars[i]))
                    {
                        goto FindFirstCharLoop;
                    }
                }

                return rollingIndex - 1;
            }
        }

        private unsafe int IndexOfAny(StringPart source, StringPart value)
        {
            char* currentSource = source.Pointer;
            for (int i = 0; i < source.Length; i++)
            {
                char* currentValue = value.Pointer;
                char normalizedCurrentSource = Normalize(*currentSource++);
                for (int j = value.Length; j > 0; j--)
                {
                    if (normalizedCurrentSource == Normalize(*currentValue++))
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        private StringPart GetRemaining()
        {
            return _source.Slice(_textPosition);
        }

        private bool CharEquals(char right, char left)
        {
            if (_isCaseSensitive)
            {
                return right == left;
            }

            return char.ToLower(right, _culture) == char.ToLower(left, _culture);
        }
    }
}
