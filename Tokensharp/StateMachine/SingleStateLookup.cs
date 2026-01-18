using System.Diagnostics.CodeAnalysis;

namespace Tokensharp.StateMachine;

internal class SingleStateLookup<TTokenType>(char character, IState<TTokenType> associatedState) 
    : IStateLookup<TTokenType> where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public bool TryGetState(char c, [NotNullWhen(true)] out IState<TTokenType>? state)
    {
        if (c == character)
        {
            state = associatedState;
            return true;
        }

        state = null;
        return false;
    }
}