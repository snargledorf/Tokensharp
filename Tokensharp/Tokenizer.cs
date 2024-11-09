using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Tokensharp;

public static class Tokenizer
{
    public static bool TryParseToken<TTokenType>(ReadOnlyMemory<char> buffer, out Token<TTokenType> token)
        where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType> =>
        TryParseToken(buffer, false, out token);

    public static bool TryParseToken<TTokenType>(ReadOnlyMemory<char> buffer, bool moreDataAvailable, out Token<TTokenType> token)
        where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
    {
        if (TryParseToken<TTokenType>(buffer.Span, moreDataAvailable, out TTokenType? tokenType, out ReadOnlySpan<char> lexeme))
        {
            token = new Token<TTokenType>(tokenType, lexeme.ToString());
            return true;
        }
            
        token = default;
        return false;
    }

    public static bool TryParseToken<TTokenType>(
        ReadOnlySpan<char> buffer, 
        [MaybeNullWhen(false)] out TTokenType tokenType,
        out ReadOnlySpan<char> lexeme)
        where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType> => 
        TryParseToken(buffer, false, out tokenType, out lexeme);

    public static bool TryParseToken<TTokenType>(ReadOnlySpan<char> buffer, bool moreDataAvailable,
        [MaybeNullWhen(false)] out TTokenType tokenType, out ReadOnlySpan<char> lexeme)
        where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
    {
        var tokenReader = new TokenReader<TTokenType>(buffer, moreDataAvailable);
        return tokenReader.Read(out tokenType, out lexeme);
    }

    public static IEnumerable<Token<TTokenType>> EnumerateTokens<TTokenType>(string str)
        where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType> => EnumerateTokens<TTokenType>(str.AsMemory());

    public static IEnumerable<Token<TTokenType>> EnumerateTokens<TTokenType>(
        ReadOnlyMemory<char> buffer, 
        CancellationToken cancellationToken = default)
        where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
    {
        while (TryParseToken(buffer, out Token<TTokenType> token))
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return token;
            buffer = buffer[token.Length..];
        }
    }

    public static async IAsyncEnumerable<Token<TTokenType>> EnumerateTokensAsync<TTokenType>(
        Stream tokenStream,
        TokenizerOptions? options = default, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
        where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
    {
        options ??= new TokenizerOptions();

        using var sr = new StreamReader(tokenStream, options.DefaultEncoding);

        var readBuffer = new ReadBuffer<TTokenType>(options.DefaultBufferSize);
        var tokenQueue = new Queue<Token<TTokenType>>();
        try
        {
            do
            {
                readBuffer = await readBuffer.ReadAsync(sr, cancellationToken).ConfigureAwait(false);

                ParseTokens(ref readBuffer, ref tokenQueue);

                while (tokenQueue.TryDequeue(out Token<TTokenType> token))
                    yield return token;
            } while (!readBuffer.EndOfReader);
        }
        finally
        {
            readBuffer.Dispose();
        }
    }

    private static void ParseTokens<TTokenType>(ref ReadBuffer<TTokenType> readBuffer, ref Queue<Token<TTokenType>> tokens)
        where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
    {
        var tokenReader = new TokenReader<TTokenType>(readBuffer.Chars, !readBuffer.EndOfReader);
        while (tokenReader.Read(out TTokenType? tokenType, out ReadOnlySpan<char> lexeme))
            tokens.Enqueue(new Token<TTokenType>(tokenType, lexeme.ToString()));

        readBuffer.AdvanceBuffer(tokenReader.Consumed);
    }
}

[Obsolete("Replaced by non-generic Tokenizer class.")]
public static class Tokenizer<TTokenType>
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public static bool TryParseToken(ReadOnlyMemory<char> buffer, out Token<TTokenType> token) =>
        TryParseToken(buffer, false, out token);

    public static bool TryParseToken(ReadOnlyMemory<char> buffer, bool moreDataAvailable, out Token<TTokenType> token)
    {
        if (TryParseToken(buffer.Span, moreDataAvailable, out TTokenType? tokenType, out ReadOnlySpan<char> lexeme))
        {
            token = new Token<TTokenType>(tokenType, lexeme.ToString());
            return true;
        }
            
        token = default;
        return false;
    }

    public static bool TryParseToken(ReadOnlySpan<char> buffer, [MaybeNullWhen(false)] out TTokenType tokenType,
        out ReadOnlySpan<char> lexeme) => TryParseToken(buffer, false, out tokenType, out lexeme);

    public static bool TryParseToken(ReadOnlySpan<char> buffer, bool moreDataAvailable,
        [MaybeNullWhen(false)] out TTokenType tokenType, out ReadOnlySpan<char> lexeme)
    {
        var tokenReader = new TokenReader<TTokenType>(buffer, moreDataAvailable);
        return tokenReader.Read(out tokenType, out lexeme);
    }

    public static IEnumerable<Token<TTokenType>> EnumerateTokens(string str) => EnumerateTokens(str.AsMemory());

    public static IEnumerable<Token<TTokenType>> EnumerateTokens(ReadOnlyMemory<char> buffer, CancellationToken cancellationToken = default)
    {
        while (TryParseToken(buffer, out Token<TTokenType> token))
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return token;
            buffer = buffer[token.Length..];
        }
    }

    public static async IAsyncEnumerable<Token<TTokenType>> EnumerateTokensAsync(Stream tokenStream, TokenizerOptions? options = default, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        options ??= new TokenizerOptions();
        
        using var sr = new StreamReader(tokenStream, options.DefaultEncoding);

        var readBuffer = new ReadBuffer<TTokenType>(options.DefaultBufferSize);
        var tokenQueue = new Queue<Token<TTokenType>>();
        try
        {
            do
            {
                readBuffer = await readBuffer.ReadAsync(sr, cancellationToken).ConfigureAwait(false);
                    
                ParseTokens(ref readBuffer, ref tokenQueue);
                    
                while (tokenQueue.TryDequeue(out Token<TTokenType> token))
                    yield return token;
            } while (!readBuffer.EndOfReader);
        }
        finally
        {
            readBuffer.Dispose();
        }
    }

    private static void ParseTokens(ref ReadBuffer<TTokenType> readBuffer, ref Queue<Token<TTokenType>> tokens)
    {
        var tokenReader = new TokenReader<TTokenType>(readBuffer.Chars, !readBuffer.EndOfReader);
        while (tokenReader.Read(out TTokenType? tokenType, out ReadOnlySpan<char> lexeme))
            tokens.Enqueue(new Token<TTokenType>(tokenType, lexeme.ToString()));

        readBuffer.AdvanceBuffer(tokenReader.Consumed);
    }
}