using System.Diagnostics.CodeAnalysis;

namespace Tokensharp.StateMachine;

internal sealed class SingleStateLookup<TTokenType>(char character, State<TTokenType> state)
    : StateLookup<TTokenType> where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public override bool TryGetState(char c, [NotNullWhen(true)] out State<TTokenType>? result)
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