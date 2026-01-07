using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Tokensharp.StateMachine;

namespace Tokensharp;

public ref struct TokenReader<TTokenType>(ReadOnlySpan<char> buffer, bool moreDataAvailable = false, TokenReaderOptions options = default)
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private static readonly TokenizerState<TTokenType> StartState;
    
    private readonly ReadOnlySpan<char> _buffer = buffer;

    static TokenReader() => StartState = TokenizerStateMachine<TTokenType>.StartState;

    public int Consumed { get; private set; }

    public bool Read([NotNullWhen(true)] out TTokenType? tokenType, out ReadOnlySpan<char> lexeme)
    {
        while (ReadInternal(out tokenType, out lexeme))
        {
            if (options.IgnoreWhiteSpace && tokenType == TokenType<TTokenType>.WhiteSpace)
                continue;
            
            return true;
        }
        
        return false;
    }

    private bool ReadInternal([NotNullWhen(true)] out TTokenType? tokenType, out ReadOnlySpan<char> lexeme)
    {
        tokenType = null;
        int lexemeLength = 0;

        TokenizerState<TTokenType> currentState = StartState;
        
        try
        {
            foreach (char c in _buffer[Consumed..])
            {
                if (currentState.TryTransition(c, out TokenizerState<TTokenType>? newState))
                {
                    currentState = newState;

                    if (currentState.TokenType == TokenType<TTokenType>.EndOfText)
                    {
                        tokenType = TokenType<TTokenType>.Text;
                        return true;
                    }

                    if (currentState.TokenType == TokenType<TTokenType>.EndOfNumber)
                    {
                        tokenType = TokenType<TTokenType>.Number;
                        return true;
                    }

                    if (currentState.TokenType == TokenType<TTokenType>.EndOfWhiteSpace)
                    {
                        tokenType = TokenType<TTokenType>.WhiteSpace;
                        return true;
                    }

                    if (currentState.TokenType != TokenType<TTokenType>.Text &&
                        currentState.TokenType != TokenType<TTokenType>.Number &&
                        currentState.TokenType != TokenType<TTokenType>.WhiteSpace &&
                        currentState.TokenType is { IsGenerated: false })
                    {
                        tokenType = currentState.TokenType;
                        return true;
                    }
                }

                lexemeLength++;
            }

            if (currentState.TokenType == TokenType<TTokenType>.Start)
            {
                Debug.Assert(tokenType == null);
                return false;
            }
            
            // If we have more data, check if this current token type is the start of another token type
            // If it is, then we should wait for more data to be available
            if (moreDataAvailable && StartState.HasInputTransitions)
            {
                Debug.Assert(tokenType == null);
                return false;
            }

            TTokenType? currentTokenType = currentState.TokenType;
            if (currentState.TryGetDefault(out TTokenType? defaultType))
                currentTokenType = defaultType;

            if (currentTokenType == TokenType<TTokenType>.EndOfText)
            {
                tokenType = TokenType<TTokenType>.Text;
                return true;
            }

            if (currentTokenType == TokenType<TTokenType>.EndOfNumber)
            {
                tokenType = TokenType<TTokenType>.Number;
                return true;
            }

            if (currentTokenType == TokenType<TTokenType>.EndOfWhiteSpace)
            {
                tokenType = TokenType<TTokenType>.WhiteSpace;
                return true;
            }

            if (currentTokenType is { IsGenerated: false })
            {
                tokenType = currentTokenType;
                return true;
            }

            Debug.Assert(tokenType == null);
            return false;
        }
        finally
        {
            lexeme = _buffer.Slice(Consumed, lexemeLength);
            Consumed += lexemeLength;
        }
    }
}