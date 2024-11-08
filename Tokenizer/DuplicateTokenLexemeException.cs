namespace Tokenizer;

public class DuplicateTokenLexemeException<TTokenType>(ILookup<string, TTokenType> tokenTypes) : Exception
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public ILookup<string, TTokenType> TokenTypes { get; } = tokenTypes;

    public static void ThrowIfDuplicateLexemes()
    {
        ILookup<string, TTokenType> tokenLexemeLookup = TTokenType.TokenTypes
            .ToLookup(tt => tt.Lexeme, tt => tt);

        if (tokenLexemeLookup.Any(g => g.Count() > 1))
            throw new DuplicateTokenLexemeException<TTokenType>(tokenLexemeLookup);
    }
}