using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using System.Text.Json;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Tokenizer.TokenTypeGenerator
{
    [Generator(LanguageNames.CSharp)]
    public class TokenTypeGenerator : IIncrementalGenerator
    {
        private static readonly string AttributeFullName = typeof(TokenTypeAttribute).FullName;

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            IncrementalValuesProvider<TokenTypeClass> tokenTypeClasses = context.SyntaxProvider.ForAttributeWithMetadataName(AttributeFullName,
                    predicate: (node, _) => node is RecordDeclarationSyntax,
                    transform: GetTypeToGenerate)
                .WithTrackingName("InitialExtraction")
                .Where(static m => m is not null)
                .Select(static (m, _) => m!.Value)
                .WithTrackingName("RemovingNulls");
            
            IncrementalValuesProvider<TokenDefinition> tokenDefinitions = context.AdditionalTextsProvider
                .Where(static file => file.Path.EndsWith(".json"))
                .Select(GetTokenDefinition)
                .WithTrackingName("InitialExtraction")
                .Where(static m => m is not null)
                .Select(static (m, _) => m!.Value)
                .WithTrackingName("RemovingNulls");

            IncrementalValuesProvider<TokenClassAndDefinition> tokenClassesAndDefinitions = tokenTypeClasses.Combine(tokenDefinitions.Collect())
                .Select(LinkTokenClassToDefinition)
                .WithTrackingName("InitialExtraction")
                .Where(static m => m is not null)
                .Select(static (m, _) => m!.Value)
                .WithTrackingName("RemovingNulls");

            context.RegisterSourceOutput(tokenClassesAndDefinitions, 
                static (spc, tokenClassAndDefinition) => GenerateClass(in tokenClassAndDefinition, spc));
        }

        private static TokenTypeClass? GetTypeToGenerate(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
        {
            if (context.TargetSymbol is not ITypeSymbol symbol)
                return null;

            return new TokenTypeClass(symbol);
        }

        private static TokenDefinition? GetTokenDefinition(AdditionalText file, CancellationToken cancellationToken)
        {
            string className = Path.GetFileNameWithoutExtension(file.Path);
            
            if (file.GetText(cancellationToken) is not { } sourceText)
                return null;

            var tokens = new Dictionary<string, string>();
            JsonDocument jsonDocument = JsonDocument.Parse(sourceText.ToString());
            foreach (JsonProperty property in jsonDocument.RootElement.EnumerateObject())
                tokens.Add(property.Name, property.Value.GetString());
            
            return new TokenDefinition(className, tokens);
        }

        private static TokenClassAndDefinition? LinkTokenClassToDefinition((TokenTypeClass Left, ImmutableArray<TokenDefinition> Right) classAndDefinitions, CancellationToken _)
        {
            TokenDefinition tokenDefinition = classAndDefinitions.Right.FirstOrDefault(td => td.ClassName == classAndDefinitions.Left.TypeSymbol.Name);
            if (tokenDefinition.ClassName != classAndDefinitions.Left.TypeSymbol.Name)
                return null;

            return new TokenClassAndDefinition(classAndDefinitions.Left, tokenDefinition);
        }

        private static void GenerateClass(in TokenClassAndDefinition definition, SourceProductionContext spc)
        {
            string className = definition.TokenClass.TypeSymbol.Name;
            
            var namespaceAndClass = $$"""
                                      namespace {{definition.TokenClass.TypeSymbol.ContainingNamespace}}
                                      {
                                          partial record {{className}}(string Lexeme, int Id) 
                                              : TokenType<{{className}}>(Lexeme, Id), ITokenType<{{className}}>
                                          {
                                              {{
                                                  string.Join("\r\n        ", definition.TokenDefinition.Tokens.Select((def, index) => GenerateFieldDefinition(className, def, index, "        ")))
                                              }}
                                          
                                              public static {{className}} Create(string lexeme, int id) => new(lexeme, id);
                                              
                                              public static IEnumerable<{{className}}> TokenTypes { get; } =
                                              [
                                                  {{
                                                      string.Join(",\r\n            ", definition.TokenDefinition.Tokens.Select(def => def.Key))
                                                  }}
                                              ];
                                          }
                                      }
                                      """;
            
            var fileName = new StringBuilder($"{definition.TokenClass.TypeSymbol}.g.cs")
                .Replace('<', '_')
                .Replace('>', '_')
                .Replace(',', '.')
                .Replace(' ', '_')
                .ToString();
            
            spc.AddSource(fileName, namespaceAndClass);
        }

        private static string GenerateFieldDefinition(string className, KeyValuePair<string, string> arg, int index,
            string indentPadding)
        {
            return $""""
                    public static readonly {className} {arg.Key} = new(
                    {indentPadding}"""
                    {indentPadding}{arg.Value}
                    {indentPadding}""", {index});
                    """";
        }
    }
}