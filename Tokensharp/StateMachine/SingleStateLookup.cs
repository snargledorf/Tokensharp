using System.Diagnostics.CodeAnalysis;

namespace Tokensharp.StateMachine;

internal class SingleStateLookup<TTokenType>(char character, IState<TTokenType> state) : IStateLookup<TTokenType> where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public bool TryGetState(in char c, [NotNullWhen(true)] out IState<TTokenType>? result)
    {
        if (c == character)
        {
            result = state;
            return true;
        }
        
        result = null;
        return false;
    }
}