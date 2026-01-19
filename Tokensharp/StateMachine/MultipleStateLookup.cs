using System.Diagnostics.CodeAnalysis;
using SwiftState;

namespace Tokensharp.StateMachine;

internal class MultipleStateLookup<TTokenType>(State<char, SwiftStateId<TTokenType>> lookupState) : IStateLookup<TTokenType> where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public bool TryGetState(char c, [NotNullWhen(true)] out IState<TTokenType>? state)
    {
        if (lookupState.TryTransition(c, out State<char, SwiftStateId<TTokenType>>? nextState))
        {
            state = nextState.Id.State;
            return true;
        }

        state = null;
        return false;
    }
}