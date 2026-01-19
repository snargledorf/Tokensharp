using System.Diagnostics.CodeAnalysis;

namespace Tokensharp.StateMachine;

internal abstract class State<TTokenType> : IState<TTokenType> where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public virtual bool TryTransition(char c, [NotNullWhen(true)] out IState<TTokenType>? nextState)
    {
        if (!TryGetNextState(c, out nextState) &&
            !TryGetDefaultState(out nextState))
            return false;
        
        return true;
    }

    protected abstract bool TryGetNextState(char c, [NotNullWhen(true)] out IState<TTokenType>? nextState);

    public virtual bool TryDefaultTransition([NotNullWhen(true)] out IState<TTokenType>? defaultState)
    {
        if (!TryGetDefaultState(out defaultState))
            return false;
        
        return true;
    }

    protected abstract bool TryGetDefaultState([NotNullWhen(true)] out IState<TTokenType>? defaultState);

    public virtual TTokenType? TokenType => null;

    public abstract bool CharacterIsValidForState(char c);

    public virtual bool UpdateCounts(ref int potentialLexemeLength, ref int fallbackLexemeLength,
        ref int confirmedLexemeLength, [NotNullWhen(true)] out TokenType<TTokenType>? tokenType)
    {
        tokenType = null;
        potentialLexemeLength++;
        return false;
    }
}