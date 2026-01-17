using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Tokensharp;

public static class Tokenizer
{
    public static bool TryParseToken<TTokenType>(ReadOnlyMemory<char> buffer, out Token<TTokenType> token)
        where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
    {
        return TryParseToken(buffer, TTokenType.Configuration, out token);
    }

    public static bool TryParseToken<TTokenType>(ReadOnlyMemory<char> buffer, TokenConfiguration<TTokenType> tokenConfiguration, out Token<TTokenType> token)
        where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
    {
        return TryParseToken(buffer, tokenConfiguration, false, out token);
    }

    public static bool TryParseToken<TTokenType>(ReadOnlyMemory<char> buffer, bool moreDataAvailable, out Token<TTokenType> token)
        where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
    {
        return TryParseToken(buffer, TTokenType.Configuration, moreDataAvailable, out token);
    }

    public static bool TryParseToken<TTokenType>(ReadOnlyMemory<char> buffer, TokenConfiguration<TTokenType> tokenConfiguration, bool moreDataAvailable, out Token<TTokenType> token)
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
        return TryParseToken(buffer, TTokenType.Configuration, moreDataAvailable, out tokenType, out lexeme);
    }

    public static bool TryParseToken<TTokenType>(ReadOnlySpan<char> buffer,
        TokenConfiguration<TTokenType> tokenConfiguration,
        bool moreDataAvailable,
        [NotNullWhen(true)] out TokenType<TTokenType>? tokenType, 
        out ReadOnlySpan<char> lexeme)
        where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
    {
        var tokenParser = new TokenParser<TTokenType>(tokenConfiguration);
        return tokenParser.TryParseToken(buffer, moreDataAvailable, out tokenType, out lexeme);
    }

    public static IEnumerable<Token<TTokenType>> EnumerateTokens<TTokenType>(string str, TokenizerOptions? options = null)
        where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType> => EnumerateTokens(str, TTokenType.Configuration, options);

    public static IEnumerable<Token<TTokenType>> EnumerateTokens<TTokenType>(string str,
        TokenConfiguration<TTokenType> tokenConfiguration, TokenizerOptions? options = null)
        where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType> => EnumerateTokens(str.AsMemory(), tokenConfiguration, options);

    public static IEnumerable<Token<TTokenType>> EnumerateTokens<TTokenType>(ReadOnlyMemory<char> buffer,
        TokenizerOptions? options = null)
        where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
    {
        return EnumerateTokens(buffer, TTokenType.Configuration, options);
    }

    public static IEnumerable<Token<TTokenType>> EnumerateTokens<TTokenType>(ReadOnlyMemory<char> buffer,
        TokenConfiguration<TTokenType> tokenConfiguration,
        TokenizerOptions? options = null)
        where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
    {
        options ??= new TokenizerOptions();

        var readerOptions = new TokenReaderOptions(IgnoreWhiteSpace: options.IgnoreWhiteSpace);
        var tokenLocationQueue = new Queue<Token<TTokenType>>();

        ReadOnlySpan<char> bufferSpan = buffer.Span;
        var moreDataAvailable = false;
        CancellationToken cancellationToken = CancellationToken.None;
        
        ParseTokens(tokenConfiguration, ref bufferSpan, ref moreDataAvailable, ref readerOptions, tokenLocationQueue, ref cancellationToken);
        while (tokenLocationQueue.TryDequeue(out Token<TTokenType> token))
            yield return token;
    }

    public static IAsyncEnumerable<Token<TTokenType>> EnumerateTokensAsync<TTokenType>(Stream tokenStream,
        TokenizerOptions? options = null,
        CancellationToken cancellationToken = default)
        where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
    {
        return EnumerateTokensAsync(tokenStream, TTokenType.Configuration, options, cancellationToken);
    }

    private static async IAsyncEnumerable<Token<TTokenType>> EnumerateTokensAsync<TTokenType>(Stream tokenStream,
        TokenConfiguration<TTokenType> tokenConfiguration,
        TokenizerOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
        where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
    {
        options ??= new TokenizerOptions();

        using var sr = new StreamReader(tokenStream, options.DefaultEncoding);

        var readBuffer = new ReadBuffer(options.DefaultBufferSize);
        var readerOptions = new TokenReaderOptions(IgnoreWhiteSpace: options.IgnoreWhiteSpace);
        var tokenQueue = new Queue<Token<TTokenType>>();
        
        try
        {
            do
            {
                readBuffer = await readBuffer.ReadAsync(sr, cancellationToken).ConfigureAwait(false);

                ReadOnlySpan<char> buffer = readBuffer.Chars;

                bool moreDataAvailable = !readBuffer.EndOfReader;
                
                int consumed = ParseTokens(tokenConfiguration, ref buffer, ref moreDataAvailable, ref readerOptions, tokenQueue, ref cancellationToken);

                while (tokenQueue.TryDequeue(out Token<TTokenType> token))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    yield return token;
                }
                
                readBuffer.AdvanceBuffer(consumed);
            } while (!readBuffer.EndOfReader);
        }
        finally
        {
            readBuffer.Dispose();
        }
    }

    private static int ParseTokens<TTokenType>(TokenConfiguration<TTokenType> tokenConfiguration,
        ref ReadOnlySpan<char> buffer, 
        ref bool moreDataAvailable, 
        ref TokenReaderOptions options, 
        Queue<Token<TTokenType>> tokens,
        ref CancellationToken cancellationToken)
        where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
    {
        var tokenReader = new TokenReader<TTokenType>(buffer, tokenConfiguration, moreDataAvailable, options);
        
        while (tokenReader.Read(out TokenType<TTokenType>? tokenType, out int index, out int length))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ReadOnlySpan<char> lexeme = buffer.Slice(index, length);
            tokens.Enqueue(new Token<TTokenType>(tokenType, new string(lexeme)));
        }

        return tokenReader.Consumed;
    }
}