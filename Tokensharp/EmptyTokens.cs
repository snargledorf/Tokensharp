namespace Tokensharp;

public record EmptyTokens(string Identifier) : TokenType<EmptyTokens>(Identifier), ITokenType<EmptyTokens>
{
    public static EmptyTokens Create(string identifier) => new(identifier);

    public static TokenConfiguration<EmptyTokens> Configuration { get; } = new TokenConfigurationBuilder<EmptyTokens>().Build();
}