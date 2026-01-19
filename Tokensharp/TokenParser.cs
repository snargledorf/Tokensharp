using System.Diagnostics.CodeAnalysis;
using Tokensharp.StateMachine;

namespace Tokensharp;

public readonly ref struct TokenParser<TTokenType>(TokenConfiguration<TTokenType> tokenConfiguration)
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private readonly StartState<TTokenType> _startState = tokenConfiguration.StartState;

    public TokenParser() : this(TTokenType.Configuration)
    {
    }

    public bool TryParseToken(ReadOnlySpan<char> buffer, bool moreDataAvailable,
        [NotNullWhen(true)] out TokenType<TTokenType>? tokenType, out ReadOnlySpan<char> lexeme)
    {
        if (TryParseToken(buffer, moreDataAvailable, out tokenType, out int length))
        {
            lexeme = buffer[..length];
            return true;
        }

        tokenType = null;
        lexeme = default;
        return false;
    }
    
    public bool TryParseToken(ReadOnlySpan<char> buffer, bool moreDataAvailable, [NotNullWhen(true)] out TokenType<TTokenType>? tokenType, out int length)
    {
        tokenType = null;
        length = 0;

        IState<TTokenType> currentState = _startState;
        int potentialLexemeLength = 0;
        int fallbackLexemeLength = 0;

        foreach (char c in buffer)
        {
            if (currentState.TryTransition(c, out IState<TTokenType>? nextState))
            {
                currentState = nextState;
                currentState.UpdateCounts(ref potentialLexemeLength, ref fallbackLexemeLength, ref length);

                if (currentState is EndOfTokenState<TTokenType> endOfTokenState)
                {
                    tokenType = endOfTokenState.TokenType;
                    return true;
                }
            }
            else
            {
                throw new InvalidDataException($"Unexpected character: {c}, Current state: {currentState}");
            }
        }

        if (!moreDataAvailable && tokenType is null)
        {
            while (currentState.TryDefaultTransition(out IState<TTokenType>? defaultState))
            {
                currentState = defaultState;
                currentState.UpdateCounts(ref potentialLexemeLength, ref fallbackLexemeLength, ref length);
                
                if (currentState is EndOfTokenState<TTokenType> endOfTokenState)
                {
                    tokenType = endOfTokenState.TokenType;
                    return true;
                }
            }
        }

        return false;
    }
}