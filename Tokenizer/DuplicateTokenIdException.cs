namespace Tokenizer;

public class DuplicateTokenIdException<TTokenType>(ILookup<int, TTokenType> tokenTypes) : Exception
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public ILookup<int, TTokenType> TokenTypes { get; } = tokenTypes;

    public static void ThrowIfDuplicateIds()
    {
        ILookup<int, TTokenType> tokenIdLookup = TTokenType.TokenTypes
            .ToLookup(tt => tt.Id, tt => tt);

        if (tokenIdLookup.Any(g => g.Count() > 1))
            throw new DuplicateTokenIdException<TTokenType>(tokenIdLookup);
    }
}