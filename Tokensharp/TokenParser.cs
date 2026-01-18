using System.Diagnostics.CodeAnalysis;
using Tokensharp.StateMachine;

namespace Tokensharp;

public readonly ref struct TokenParser<TTokenType>(TokenConfiguration<TTokenType> tokenConfiguration)
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private readonly StartState<TTokenType> _startState = new(tokenConfiguration.TokenTree);

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
        var context = new StateMachineContext();

        foreach (char c in buffer)
        {
            if (currentState.TryTransition(c, context, out IState<TTokenType>? nextState))
            {
                currentState = nextState;

                if (currentState is EndOfTokenState<TTokenType> endOfTokenState)
                {
                    tokenType = endOfTokenState.TokenType;
                    length = context.ConfirmedLexemeLength;
                    break;
                }
            }
            else
            {
                throw new InvalidDataException($"Unexpected character: {c}, Current state: {currentState}");
            }
        }

        if (!moreDataAvailable && tokenType is null)
        {
            IState<TTokenType>? defaultState;
            while (currentState.TryDefaultTransition(context, out defaultState) &&
                   defaultState is not EndOfTokenState<TTokenType>)
            {
                currentState = defaultState;
            }

            if (defaultState is EndOfTokenState<TTokenType> endOfTokenState)
            {
                tokenType = endOfTokenState.TokenType;
                length = context.ConfirmedLexemeLength;
            }
        }

        return tokenType is not null;
    }
}