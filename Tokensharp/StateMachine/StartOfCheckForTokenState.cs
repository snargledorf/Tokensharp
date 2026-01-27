using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal class StartOfCheckForTokenState<TTokenType>(
    ITokenTreeNode<TTokenType> node,
    IState<TTokenType> fallback,
    IEndOfTokenStateAccessor<TTokenType> fallbackStateEndOfTokenStateAccessor,
    IStateCharacterCheck fallbackStateCharacterCheck)
    : CheckForTokenState<TTokenType>(node, fallback, fallbackStateEndOfTokenStateAccessor, fallbackStateCharacterCheck)
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public override void UpdateCounts(ref StateMachineContext context)
    {
        context.FallbackLexemeLength = context.PotentialLexemeLength;
        context.PotentialLexemeLength++;
    }
}