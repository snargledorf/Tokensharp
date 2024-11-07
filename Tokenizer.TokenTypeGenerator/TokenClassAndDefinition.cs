namespace Tokenizer.TokenTypeGenerator;

internal record struct TokenClassAndDefinition
{
    public readonly TokenTypeClass TokenClass;
    public readonly TokenDefinition TokenDefinition;

    public TokenClassAndDefinition(TokenTypeClass tokenClass, TokenDefinition tokenDefinition)
    {
        TokenClass = tokenClass;
        TokenDefinition = tokenDefinition;
    }
}