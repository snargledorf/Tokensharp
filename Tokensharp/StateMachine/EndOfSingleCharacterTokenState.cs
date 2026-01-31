using System.Diagnostics.CodeAnalysis;

namespace Tokensharp.StateMachine;

internal class EndOfSingleCharacterTokenState<TTokenType>(TTokenType tokenType) 
    : EndOfTokenState<TTokenType>(tokenType) where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public override bool FinalizeToken(ref StateMachineContext context, [NotNullWhen(true)] ref TokenType<TTokenType>? tokenType,
        ref int length)
    {
        length = 1;
        tokenType = TokenType;
        return true;
    }
}