using System.Diagnostics.CodeAnalysis;

namespace Tokensharp.StateMachine;

internal class EndOfSingleCharacterTokenState<TTokenType>(TTokenType tokenType) 
    : EndOfTokenState<TTokenType>(tokenType) where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public override bool IsEndOfToken(ref StateMachineContext context, out int length, [NotNullWhen(true)] out TokenType<TTokenType>? tokenType)
    {
        length = 1;
        tokenType = TokenType;
        return true;
    }
}