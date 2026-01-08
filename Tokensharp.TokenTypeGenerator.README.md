# Tokensharp.TokenTypeGenerator

[![NuGet](https://img.shields.io/nuget/v/Tokensharp.TokenTypeGenerator.svg)](https://www.nuget.org/packages/Tokensharp.TokenTypeGenerator/)

Tokensharp.TokenTypeGenerator is a C# source generator that simplifies the creation of token types for the [Tokensharp](https://www.nuget.org/packages/Tokensharp/) library. It allows you to define your tokens in a simple JSON file and automatically generates the corresponding C# partial record.

## Features

- **Automated Code Generation**: No more boilerplate code for defining token types.
- **JSON-Based Definitions**: Keep your token definitions clean and organized in JSON files.
- **Compile-Time Safety**: Generated token types are available immediately in your code with full IntelliSense support.

## Installation

Install the package via NuGet:

```bash
dotnet add package Tokensharp.TokenTypeGenerator
```

## Usage

1. Create a JSON file (e.g., `MyTokenTypes.json`) and add it to your project as an `AdditionalFiles` item:

```json
{
  "Plus": "+",
  "Minus": "-",
  "Number": "[0-9]+"
}
```

In your `.csproj`:
```xml
<ItemGroup>
  <AdditionalFiles Include="MyTokenTypes.json" />
</ItemGroup>
```

2. Define a partial record with the `[TokenType]` attribute:

```csharp
using Tokensharp.TokenTypeGenerator;

[TokenType]
public partial record MyTokenTypes;
```

The source generator will automatically create the implementation for `MyTokenTypes`, including static fields for each token defined in the JSON.

## Related Packages

- [Tokensharp](https://www.nuget.org/packages/Tokensharp/): The core tokenization library.
