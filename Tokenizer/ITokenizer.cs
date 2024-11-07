using System.Diagnostics.CodeAnalysis;

namespace Tokenizer
{
    public interface ITokenizer<TTokenType> where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
    {
        bool TryParseToken(ReadOnlyMemory<char> buffer, bool moreDataAvailable, out Token<TTokenType> token);
        bool TryParseToken(ReadOnlySpan<char> buffer, bool moreDataAvailable, [MaybeNullWhen(false)] out TTokenType token, out int tokenLength);
    }
}