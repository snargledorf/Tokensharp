using System.Diagnostics;
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
        IState<TTokenType>? currentState = _startState;
        var context = new StateMachineContext();

        foreach (char c in buffer)
        {
            if (currentState.TryTransition(c, ref context, out currentState))
                continue;
            
            Debug.Assert(currentState is not null, $"State transition failed for character: {c}");
            
            bool tokenFinalized = currentState.FinalizeToken(ref context, out length, out tokenType);
            
            Debug.Assert(tokenFinalized, $"Finalize token failed: '{c}', Current state: {currentState}");

            return tokenFinalized;
        }
        
        while (!moreDataAvailable && currentState.TryDefaultTransition(ref context, out currentState)) ;
            
        Debug.Assert(currentState is not null, "Default state transition resulted in null state");
            
        return currentState.FinalizeToken(ref context, out length, out tokenType);
    }
}