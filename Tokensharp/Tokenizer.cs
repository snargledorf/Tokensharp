using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Tokensharp.StateMachine;

namespace Tokensharp;

public static class Tokenizer
{
    public static bool TryParseToken<TTokenType>(ReadOnlyMemory<char> buffer, out Token<TTokenType> token)
        where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
    {
        TokenReaderStateMachine<TTokenType> tokenReaderStateMachine = TokenReaderStateMachine<TTokenType>.For(TTokenType.Configuration);
        return TryParseToken(buffer, tokenReaderStateMachine, out token);
    }

    public static bool TryParseToken<TTokenType>(ReadOnlyMemory<char> buffer, TokenReaderStateMachine<TTokenType> tokenReaderStateMachine, out Token<TTokenType> token)
        where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
    {
        return TryParseToken(buffer, tokenReaderStateMachine, false, out token);
    }

    public static bool TryParseToken<TTokenType>(ReadOnlyMemory<char> buffer, bool moreDataAvailable, out Token<TTokenType> token)
        where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
    {
        TokenReaderStateMachine<TTokenType> tokenReaderStateMachine = TokenReaderStateMachine<TTokenType>.For(TTokenType.Configuration);
        return TryParseToken(buffer, tokenReaderStateMachine, moreDataAvailable, out token);
    }

    public static bool TryParseToken<TTokenType>(ReadOnlyMemory<char> buffer, TokenReaderStateMachine<TTokenType> tokenReaderStateMachine, bool moreDataAvailable, out Token<TTokenType> token)
        where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
    {
        if (TryParseToken(buffer.Span, tokenReaderStateMachine, moreDataAvailable, out TokenType<TTokenType>? tokenType, out ReadOnlySpan<char> lexeme))
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
        TokenReaderStateMachine<TTokenType> tokenReaderStateMachine = TokenReaderStateMachine<TTokenType>.For(TTokenType.Configuration);
        return TryParseToken(buffer, tokenReaderStateMachine, moreDataAvailable, out tokenType, out lexeme);
    }

    public static bool TryParseToken<TTokenType>(ReadOnlySpan<char> buffer,
        TokenReaderStateMachine<TTokenType> tokenReaderStateMachine,
        bool moreDataAvailable,
        [NotNullWhen(true)] out TokenType<TTokenType>? tokenType, 
        out ReadOnlySpan<char> lexeme)
        where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
    {
        var tokenReader = new TokenReader<TTokenType>(buffer, tokenReaderStateMachine, moreDataAvailable);
        return tokenReader.Read(out tokenType, out lexeme);
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

    private static IEnumerable<Token<TTokenType>> EnumerateTokens<TTokenType>(ReadOnlyMemory<char> buffer,
        TokenConfiguration<TTokenType> tokenConfiguration,
        TokenizerOptions? options = null)
        where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
    {
        options ??= new TokenizerOptions();

        using var sr = new StringReader(buffer.ToString());

        var readBuffer = new ReadBuffer(options.DefaultBufferSize);
        var readerOptions = new TokenReaderOptions(IgnoreWhiteSpace: options.IgnoreWhiteSpace);
        var tokenQueue = new Queue<Token<TTokenType>>();
        try
        {
            TokenReaderStateMachine<TTokenType> tokenReaderStateMachine = TokenReaderStateMachine<TTokenType>.For(tokenConfiguration);
            
            do
            {
                readBuffer.Read(sr);

                ParseTokens(tokenReaderStateMachine, ref readBuffer, ref readerOptions, ref tokenQueue);

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
            TokenReaderStateMachine<TTokenType> tokenReaderStateMachine = TokenReaderStateMachine<TTokenType>.For(tokenConfiguration);
            
            do
            {
                readBuffer = await readBuffer.ReadAsync(sr, cancellationToken).ConfigureAwait(false);

                ParseTokens(tokenReaderStateMachine, ref readBuffer, ref readerOptions, ref tokenQueue, cancellationToken);

                while (tokenQueue.TryDequeue(out Token<TTokenType> token))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    yield return token;
                }
            } while (!readBuffer.EndOfReader);
        }
        finally
        {
            readBuffer.Dispose();
        }
    }

    private static void ParseTokens<TTokenType>(TokenReaderStateMachine<TTokenType> tokenReaderStateMachine,
        ref ReadBuffer readBuffer, ref TokenReaderOptions options, ref Queue<Token<TTokenType>> tokens,
        CancellationToken cancellationToken = default)
        where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
    {
        var tokenReader = new TokenReader<TTokenType>(readBuffer.Chars, tokenReaderStateMachine, !readBuffer.EndOfReader, options);
        while (tokenReader.Read(out TokenType<TTokenType>? tokenType, out ReadOnlySpan<char> lexeme))
        {
            cancellationToken.ThrowIfCancellationRequested();
            tokens.Enqueue(new Token<TTokenType>(tokenType, lexeme.ToString()));
        }

        readBuffer.AdvanceBuffer(tokenReader.Consumed);
    }
}