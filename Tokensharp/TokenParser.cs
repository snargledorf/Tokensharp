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
        IState<TTokenType> currentState = _startState;
        var context = new StateMachineContext();

        foreach (char c in buffer)
        {
            if (currentState.TryTransition(c, ref context, out IState<TTokenType>? nextState))
            {
                if (nextState.IsEndOfToken(ref context, out length, out tokenType))
                    return true;
                
                currentState = nextState;
            }
            else
            {
                throw new InvalidDataException($"Unexpected character: {c}, Current state: {currentState}");
            }
        }

        if (!moreDataAvailable)
        {
            while (currentState.TryDefaultTransition(ref context, out IState<TTokenType>? defaultState))
            {
                if (defaultState.IsEndOfToken(ref context, out length, out tokenType))
                    return true;
                
                currentState = defaultState;
            }
        }
        
        tokenType = null;
        length = 0;
        return false;
    }
}