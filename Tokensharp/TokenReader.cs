using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using SwiftState;
using Tokensharp.StateMachine;

namespace Tokensharp;

public ref struct TokenReader<TTokenType>(ReadOnlySpan<char> buffer, bool moreDataAvailable = false, TokenReaderOptions options = default)
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private static readonly State<char, TTokenType> StartState;
    
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
        lexeme = default;
        int lexemeLength = 0;

        State<char, TTokenType> currentState = StartState;

        foreach (char c in _buffer[Consumed..])
        {
            if (currentState.TryTransition(c, out State<char, TTokenType>? newState))
            {
                currentState = newState;

                if (currentState.Id == TokenType<TTokenType>.EndOfText)
                {
                    tokenType = TokenType<TTokenType>.Text;
                    break;
                }

                if (currentState.Id == TokenType<TTokenType>.EndOfNumber)
                {
                    tokenType = TokenType<TTokenType>.Number;
                    break;
                }

                if (currentState.Id == TokenType<TTokenType>.EndOfWhiteSpace)
                {
                    tokenType = TokenType<TTokenType>.WhiteSpace;
                    break;
                }

                if (currentState.Id != TokenType<TTokenType>.Text &&
                    currentState.Id != TokenType<TTokenType>.Number &&
                    currentState.Id != TokenType<TTokenType>.WhiteSpace &&
                    currentState.Id is { IsGenerated: false })
                {
                    tokenType = currentState.Id;
                    break;
                }
            }

            lexemeLength++;
        }

        // If we are currently parsing a token but there is no more data, then we should check
        // if our current state (or it's default transition) is a terminal state.
        if (tokenType is null && currentState.Id != TokenType<TTokenType>.Start && !moreDataAvailable)
        {
            TTokenType currentTokenType = currentState.Id;
            if (currentState.TryGetDefault(out State<char, TTokenType>? defaultState))
                currentTokenType = defaultState.Id;

            if (currentTokenType == TokenType<TTokenType>.EndOfText)
            {
                tokenType = TokenType<TTokenType>.Text;
            }
            else if (currentTokenType == TokenType<TTokenType>.EndOfNumber)
            {
                tokenType = TokenType<TTokenType>.Number;
            }
            else if (currentTokenType == TokenType<TTokenType>.EndOfWhiteSpace)
            {
                tokenType = TokenType<TTokenType>.WhiteSpace;
            }
            else if (currentTokenType is { IsGenerated: false })
            {
                tokenType = currentTokenType;
            }
        }

        if (tokenType is null)
            return false;
        
        lexeme = _buffer.Slice(Consumed, lexemeLength);
        Consumed += lexemeLength;

        return true;
    }
}