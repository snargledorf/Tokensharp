namespace Tokensharp;

public partial class TokenConfiguration<TTokenType> : ITokenConfiguration<TTokenType>
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private readonly Dictionary<string, TTokenType> _tokenDefinitions = new();
}