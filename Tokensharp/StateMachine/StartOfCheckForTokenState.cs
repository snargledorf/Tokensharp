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

    public new static StartOfCheckForTokenState<TTokenType> For(
        ITokenTreeNode<TTokenType> node,
        IState<TTokenType> fallbackState,
        IEndOfTokenStateAccessor<TTokenType> fallbackStateEndOfTokenStateAccessor,
        IStateCharacterCheck fallbackStateCharacterCheck)
    {
        var childStates = new StateLookupBuilder<TTokenType>();
        foreach (ITokenTreeNode<TTokenType> childNode in node)
        {
            if (childNode.IsEndOfToken)
            {
                childStates.Add(childNode.Character, fallbackStateEndOfTokenStateAccessor.EndOfTokenStateInstance);
            }
            else
            {
                childStates.Add(childNode.Character, CheckForTokenState<TTokenType>.For(childNode, fallbackState, fallbackStateEndOfTokenStateAccessor, fallbackStateCharacterCheck));
            }
        }

        return new StartOfCheckForTokenState<TTokenType>(node, fallbackState, fallbackStateEndOfTokenStateAccessor,
            fallbackStateCharacterCheck)
        {
            StateLookup = childStates.Build()
        };
    }
}