using System.Diagnostics.CodeAnalysis;
using Tokensharp.StateMachine;

namespace Tokensharp;

public readonly ref struct TokenParser<TTokenType>(TokenConfiguration<TTokenType> tokenConfiguration)
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private readonly StartState<TTokenType> _startState = tokenConfiguration.StartState;
    
    private readonly StateMachineContext _context = new();

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
        
        _context.Reset();

        IState<TTokenType> currentState = _startState;

        foreach (char c in buffer)
        {
            if (currentState.TryTransition(c, _context, out IState<TTokenType>? nextState))
            {
                currentState = nextState;

                if (currentState is EndOfTokenState<TTokenType> endOfTokenState)
                {
                    tokenType = endOfTokenState.TokenType;
                    length = _context.ConfirmedLexemeLength;
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
            IState<TTokenType>? defaultState;
            while (currentState.TryDefaultTransition(_context, out defaultState) &&
                   defaultState is not EndOfTokenState<TTokenType>)
            {
                currentState = defaultState;
            }

            if (defaultState is EndOfTokenState<TTokenType> endOfTokenState)
            {
                tokenType = endOfTokenState.TokenType;
                length = _context.ConfirmedLexemeLength;
                return true;
            }
        }

        return false;
    }
}