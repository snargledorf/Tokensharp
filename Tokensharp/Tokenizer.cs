using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Tokensharp;

public static class Tokenizer
{
    public static bool TryParseToken(ReadOnlyMemory<char> buffer,
        TokenConfiguration<RuntimeConfigToken> tokenConfiguration, out Token<RuntimeConfigToken> token)
    {
        return TryParseToken(buffer, tokenConfiguration, false, out token);
    }
    
    public static bool TryParseToken<TTokenType>(ReadOnlyMemory<char> buffer, out Token<TTokenType> token)
        where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
    {
        return TryParseToken(buffer, false, out token);
    }

    public static bool TryParseToken<TTokenType>(ReadOnlyMemory<char> buffer, bool moreDataAvailable, out Token<TTokenType> token)
        where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
    {
        return TryParseToken(buffer, ITokenType<TTokenType>.DefaultConfiguration, moreDataAvailable, out token);
    }

    private static bool TryParseToken<TTokenType>(ReadOnlyMemory<char> buffer, TokenConfiguration<TTokenType> tokenConfiguration, bool moreDataAvailable, out Token<TTokenType> token)
        where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
    {
        if (TryParseToken(buffer.Span, tokenConfiguration, moreDataAvailable, out TokenType<TTokenType>? tokenType, out ReadOnlySpan<char> lexeme))
        {
            token = new Token<TTokenType>(tokenType, lexeme.ToString());
            return true;
        }
        
        token = default;
        return false;
    }

    public static bool TryParseToken<TTokenType>(
        ReadOnlySpan<char> buffer, 
        [MaybeNullWhen(false)] out TokenType<TTokenType> tokenType,
        out ReadOnlySpan<char> lexeme)
        where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
    {
        return TryParseToken(buffer, false, out tokenType, out lexeme);
    }

    public static bool TryParseToken<TTokenType>(ReadOnlySpan<char> buffer,
        bool moreDataAvailable,
        [NotNullWhen(true)] out TokenType<TTokenType>? tokenType,
        out ReadOnlySpan<char> lexeme)
        where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
    {
        return TryParseToken(buffer, ITokenType<TTokenType>.DefaultConfiguration, moreDataAvailable, out tokenType, out lexeme);
    }

    private static bool TryParseToken<TTokenType>(ReadOnlySpan<char> buffer,
        TokenConfiguration<TTokenType> tokenConfiguration,
        bool moreDataAvailable,
        [NotNullWhen(true)] out TokenType<TTokenType>? tokenType, 
        out ReadOnlySpan<char> lexeme)
        where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
    {
        var tokenReader = new TokenReader<TTokenType>(buffer, tokenConfiguration, moreDataAvailable);
        return tokenReader.Read(out tokenType, out lexeme);
    }

    public static IEnumerable<Token<TTokenType>> EnumerateTokens<TTokenType>(string str, TokenizerOptions? options = null)
        where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType> => EnumerateTokens(str, ITokenType<TTokenType>.DefaultConfiguration, options);

    public static IEnumerable<Token<TTokenType>> EnumerateTokens<TTokenType>(string str,
        TokenConfiguration<TTokenType> tokenConfiguration, TokenizerOptions? options = null)
        where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType> => EnumerateTokens(str.AsMemory(), tokenConfiguration, options);

    public static IEnumerable<Token<TTokenType>> EnumerateTokens<TTokenType>(ReadOnlyMemory<char> buffer,
        TokenizerOptions? options = null)
        where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
    {
        return EnumerateTokens(buffer, ITokenType<TTokenType>.DefaultConfiguration, options);
    }

    public static IEnumerable<Token<RuntimeConfigToken>> EnumerateTokens(ReadOnlyMemory<char> buffer,
        TokenConfiguration<RuntimeConfigToken> tokenConfiguration,
        TokenizerOptions? options = null)
    {
        return EnumerateTokens<RuntimeConfigToken>(buffer, tokenConfiguration, options);
    }

    private static IEnumerable<Token<TTokenType>> EnumerateTokens<TTokenType>(ReadOnlyMemory<char> buffer,
        TokenConfiguration<TTokenType> tokenConfiguration,
        TokenizerOptions? options = null)
        where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
    {
        options ??= new TokenizerOptions();

        using var sr = new StringReader(buffer.ToString());

        var readBuffer = new ReadBuffer<TTokenType>(options.DefaultBufferSize);
        var readerOptions = new TokenReaderOptions(IgnoreWhiteSpace: options.IgnoreWhiteSpace);
        var tokenQueue = new Queue<Token<TTokenType>>();
        try
        {
            do
            {
                readBuffer.Read(sr);

                ParseTokens(tokenConfiguration, ref readBuffer, ref readerOptions, ref tokenQueue);

                while (tokenQueue.TryDequeue(out Token<TTokenType> token))
                    yield return token;
            } while (!readBuffer.EndOfReader);
        }
        finally
        {
            readBuffer.Dispose();
        }
    }

    public static IAsyncEnumerable<Token<TTokenType>> EnumerateTokensAsync<TTokenType>(Stream tokenStream,
        TokenizerOptions? options = null,
        CancellationToken cancellationToken = default)
        where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
    {
        return EnumerateTokensAsync(tokenStream, ITokenType<TTokenType>.DefaultConfiguration, options, cancellationToken);
    }

    public static IAsyncEnumerable<Token<RuntimeConfigToken>> EnumerateTokensAsync(Stream tokenStream,
        TokenConfiguration<RuntimeConfigToken> tokenConfiguration, TokenizerOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return EnumerateTokensAsync<RuntimeConfigToken>(tokenStream, tokenConfiguration, options, cancellationToken);
    }

    private static async IAsyncEnumerable<Token<TTokenType>> EnumerateTokensAsync<TTokenType>(Stream tokenStream,
        TokenConfiguration<TTokenType> tokenConfiguration,
        TokenizerOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
        where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
    {
        options ??= new TokenizerOptions();

        using var sr = new StreamReader(tokenStream, options.DefaultEncoding);

        var readBuffer = new ReadBuffer<TTokenType>(options.DefaultBufferSize);
        var readerOptions = new TokenReaderOptions(IgnoreWhiteSpace: options.IgnoreWhiteSpace);
        var tokenQueue = new Queue<Token<TTokenType>>();
        try
        {
            do
            {
                readBuffer = await readBuffer.ReadAsync(sr, cancellationToken).ConfigureAwait(false);

                ParseTokens(tokenConfiguration, ref readBuffer, ref readerOptions, ref tokenQueue);

                while (tokenQueue.TryDequeue(out Token<TTokenType> token))
                    yield return token;
            } while (!readBuffer.EndOfReader);
        }
        finally
        {
            readBuffer.Dispose();
        }
    }

    private static void ParseTokens<TTokenType>(TokenConfiguration<TTokenType> tokenConfiguration, ref ReadBuffer<TTokenType> readBuffer, ref TokenReaderOptions options, ref Queue<Token<TTokenType>> tokens)
        where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
    {
        var tokenReader = new TokenReader<TTokenType>(readBuffer.Chars, tokenConfiguration, !readBuffer.EndOfReader, options);
        while (tokenReader.Read(out TokenType<TTokenType>? tokenType, out ReadOnlySpan<char> lexeme))
            tokens.Enqueue(new Token<TTokenType>(tokenType, lexeme.ToString()));

        readBuffer.AdvanceBuffer(tokenReader.Consumed);
    }
}