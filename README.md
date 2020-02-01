# PSValueWildcard

A span based wildcard pattern evaluator compatible with PowerShell's [WildcardPattern] implementation.

- **Testers needed**: Ideally this project will be as reliable as PowerShell's implementation, but a
  whole lot of testing is needed before this project could be considered production ready. If this project
  interests you, try to break it! Especially folks who use culture settings other than `en-US`!

## Features

- Accepts [ReadOnlySpan&lt;char&gt;] for both input and pattern
- Often heap allocation free (see [About heap allocations](#about-heap-allocations))
- Focus on performance
- Completely standalone, PowerShell is not required

## Installation

```powershell
dotnet add package PSValueWildcard --version 1.0.0-alpha1
```

## Usage

### On demand

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

**NOTE**: Unlike the regex based [WildcardPattern], the parsed result will not be cached when utilizing
`ValueWildcardPattern`. For the majority of pattern lengths and complexities it should still be performant.

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

### Cmdlet example (requires PowerShell)

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

## Why

I made this because I really like using the [Span&lt;T&gt;] API these days. If I'm making something, then
it's often for PowerShell.  And if it's for PowerShell, I often want to support wildcard matching. In
order to do that, I end up allocating *a whole bunch* of strings that I wouldn't have to otherwise
just trying to find an item.

That's why I built it, but here's a few scenarios where you might consider this library:

1. You're already using [Span&lt;T&gt;]
2. You're building an application where you want to support PowerShell style wildcard patterns,
   but do not want to reference all of [System.Management.Automation]
3. You feel that [WildcardPattern] is too slow or allocates too much

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
  to the heap along with a [GCHandle] for the pinned string

[Span&lt;T&gt;]: https://docs.microsoft.com/en-us/dotnet/api/system.span-1
[ReadOnlySpan&lt;char&gt;]:https://docs.microsoft.com/en-us/dotnet/api/system.readonlyspan-1
[WildcardPattern]: https://docs.microsoft.com/en-us/dotnet/api/system.management.automation.wildcardpattern
[System.Management.Automation]: https://www.nuget.org/packages/System.Management.Automation
[GCHandle]: https://docs.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.gchandle
