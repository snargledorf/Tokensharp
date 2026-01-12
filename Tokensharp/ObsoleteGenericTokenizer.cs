using System.Diagnostics.CodeAnalysis;

namespace Tokensharp;

[Obsolete("Replaced by non-generic Tokenizer class.")]
public static class Tokenizer<TTokenType>
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public static bool TryParseToken(ReadOnlyMemory<char> buffer, out Token<TTokenType> token) =>
        TryParseToken(buffer, false, out token);

    public static bool TryParseToken(ReadOnlyMemory<char> buffer, bool moreDataAvailable, out Token<TTokenType> token)
    {
        return Tokenizer.TryParseToken(buffer, moreDataAvailable, out token);
    }

    public static bool TryParseToken(ReadOnlySpan<char> buffer, [MaybeNullWhen(false)] out TokenType<TTokenType> tokenType,
        out ReadOnlySpan<char> lexeme) => TryParseToken(buffer, false, out tokenType, out lexeme);

    public static bool TryParseToken(ReadOnlySpan<char> buffer,
        bool moreDataAvailable, [MaybeNullWhen(false)] out TokenType<TTokenType> tokenType,
        out ReadOnlySpan<char> lexeme)
    {
        return Tokenizer.TryParseToken(buffer, moreDataAvailable, out tokenType, out lexeme);
    }

    public static IEnumerable<Token<TTokenType>> EnumerateTokens(string str) => EnumerateTokens(str.AsMemory());

    public static IEnumerable<Token<TTokenType>> EnumerateTokens(ReadOnlyMemory<char> buffer, CancellationToken cancellationToken = default)
    {
        foreach (Token<TTokenType> token in Tokenizer.EnumerateTokens<TTokenType>(buffer))
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return token;
        }
    }

    public static IAsyncEnumerable<Token<TTokenType>> EnumerateTokensAsync(Stream tokenStream, TokenizerOptions? options = null, CancellationToken cancellationToken = default)
    {
        return Tokenizer.EnumerateTokensAsync<TTokenType>(tokenStream, options, cancellationToken);
    }
}