# PSValueWildcard

A span based PowerShell wildcard matcher.

## Features

- Accepts `ReadOnlySpan<char>` for both input and pattern
- Often heap allocation free (see [About heap allocations](#about-heap-allocations))
- Focus on performance

## Installation

```powershell
dotnet add package PSValueWildcard
```

## Usage

### Ad-hoc

```csharp
using PSValueWildcard;

public void Match()
{
    ValueWildcardPattern.IsMatch(input: "test", pattern: "T*t");
    // true

    ValueWildcardPattern.IsMatch(input: "test", pattern: "T*t", options: ValueWildcardOptions.Ordinal);
    // false
}
```

**NOTE**: Because `ValueWildcardPattern` is not regex based, the parsed result will not be cached.
For the majority of pattern lengths and complexities it should still be performant.

### Heap cached

```csharp
using PSValueWildcard;

public void Match(string pattern)
{
    using var wildcard = ValueWildcardPattern.Parse(pattern);

    foreach (string input in GetNames())
    {
        // Options are optional. Default is InvariantCultureIgnoreCase.
        if (!wildcard.IsMatch(input, ValueWildcardOptions.CurrentCultureIgnoreCase))
        {
            throw new Exception();
        }
    }
}
```

### Cmdlet example

```csharp
using System.Management.Automation;

using PSValueWildcard;

[Cmdlet(VerbsDiagnostic.Test, "Pattern")]
public sealed class TestPatternCommand : PSCmdlet, IDisposable
{
    private ParsedWildcardPattern _pattern;

    [Parameter(Mandatory = true, ValueFromPipeline = true)]
    public string InputObject { get; set; }

    [Parameter(Mandatory = true, Position = 0)]
    public string Pattern { get; set; }

    protected override void BeginProcessing()
    {
        _pattern = ValueWildcardPattern.Parse(Pattern);
    }

    protected override void ProcessRecord()
    {
        WriteObject(_pattern.IsMatch(InputObject));
    }

    public void Dispose() => _pattern.Dispose();
}
```

## About heap allocations

This library attempts to achieve zero heap allocations, however there are a few circumstances
where they may be required. These circumstances include (but may not be limited to):

- Heuristics are used to guess how many "instructions" (e.g. `[ad]`, `?` and `whole string` are
  one instruction each) are in a pattern. If the estimation is too low and the stack allocated
  buffer is too small, some heap allocations will be made

- When an "any of" pattern (e.g. `[qwerty]`) contains an escape character (<kbd>`</kbd>) they
  are parsed as separate "partial" instructions. During evaluation they are combined into a
  single instruction.  When this happens, if the combined text is too long to be allocated
  on the stack, a heap allocation will be made

- When saving the parsed pattern as a `ParsedWildcardPattern`, the entire object is allocated
  to the heap along with a `GCHandle` for the pinned string
