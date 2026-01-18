using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal class StartOfCheckForTokenState<TTokenType>(
    ITokenTreeNode<TTokenType> node,
    IEndOfTokenAccessorState<TTokenType> fallbackState)
    : CheckForTokenState<TTokenType>(node, fallbackState) 
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public override void OnEnter(StateMachineContext context)
    {
        context.FallbackLexemeLength = context.PotentialLexemeLength;
        context.PotentialLexemeLength++;
    }

    public new static StartOfCheckForTokenState<TTokenType> For(ITokenTreeNode<TTokenType> node,
        Func<ITokenTreeNode<TTokenType>, IEndOfTokenAccessorState<TTokenType>> getFallbackState)
    {
        IEndOfTokenAccessorState<TTokenType> fallbackState = getFallbackState(node);
        
        var childStates = new StateLookupBuilder<TTokenType>();
        foreach (ITokenTreeNode<TTokenType> childNode in node)
            childStates.Add(childNode.Character, CheckForTokenState<TTokenType>.For(childNode, getFallbackState));
        
        return new StartOfCheckForTokenState<TTokenType>(node, fallbackState)
        {
            StateLookup = childStates.Build()
        };
    }
}