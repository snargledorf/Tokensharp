# Tokensharp

[![NuGet](https://img.shields.io/nuget/v/Tokensharp.svg)](https://www.nuget.org/packages/Tokensharp/)

Tokensharp is a high-performance, lightweight tokenization library for .NET. It provides a flexible and efficient way to break text into tokens based on a defined set of token types.

## Features

- **High Performance**: Optimized for speed and low memory allocation using `ReadOnlySpan<char>` and `ReadOnlyMemory<char>`.
- **Streaming Support**: Easily tokenize large files or network streams using `IAsyncEnumerable`.
- **Extensible**: Define your own token types and lexemes.
- **Source Generation**: Works seamlessly with [Tokensharp.TokenTypeGenerator](https://www.nuget.org/packages/Tokensharp.TokenTypeGenerator/) to automatically generate token type classes from JSON definitions.

## Installation

Install the package via NuGet:

```bash
dotnet add package Tokensharp
```

## Quick Start

1. Define your token types (manually or using the generator):

```csharp
public record MyTokenType(string Lexeme) : TokenType<MyTokenType>(Lexeme), ITokenType<MyTokenType>
{
    public static readonly MyTokenType Var = new("var");
    public static readonly MyTokenType Assign = new("=");
    public static readonly MyTokenType EndStatement = new(";");
    
    public static MyTokenType Create(string lexeme) => new(lexeme);
    public static IEnumerable<MyTokenType> TokenTypes => [Var, Assign, EndStatement];
}
```

2. Use the `Tokenizer` to parse tokens:

```csharp
string input = "var x = 42;";
foreach (var token in Tokenizer.EnumerateTokens<MyTokenType>(input))
{
    Console.WriteLine($"Token: {token.Type}, Lexeme: {token.Lexeme}");
}
```

## Related Packages

- [Tokensharp.TokenTypeGenerator](https://www.nuget.org/packages/Tokensharp.TokenTypeGenerator/): A source generator that automates the creation of token type classes from JSON files.
