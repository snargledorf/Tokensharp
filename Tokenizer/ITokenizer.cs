using System.Diagnostics.CodeAnalysis;

namespace Tokenizer
{
    public interface ITokenizer<TTokenType>
    {
        bool TryParseToken(ReadOnlySpan<char> buffer, bool moreDataAvailable, [MaybeNullWhen(false)] out TTokenType type, out int tokenLength);
    }
}