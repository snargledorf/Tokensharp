using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal class StartOfCheckForTokenState<TTokenType>(
    ITokenTreeNode<TTokenType> node,
    IEndOfTokenAccessorState<TTokenType> fallbackState)
    : CheckForTokenState<TTokenType>(node, fallbackState) 
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public override void UpdateCounts(ref int potentialLexemeLength, ref int fallbackLexemeLength, ref int confirmedLexemeLength)
    {
        fallbackLexemeLength = potentialLexemeLength;
        potentialLexemeLength++;
    }

    public new static StartOfCheckForTokenState<TTokenType> For(ITokenTreeNode<TTokenType> node,
        IEndOfTokenAccessorState<TTokenType> fallbackState)
    {
        var childStates = new StateLookupBuilder<TTokenType>();
        foreach (ITokenTreeNode<TTokenType> childNode in node)
        {
            if (childNode.IsEndOfToken)
            {
                childStates.Add(childNode.Character, fallbackState.EndOfTokenStateInstance);
            }
            else
            {
                childStates.Add(childNode.Character, CheckForTokenState<TTokenType>.For(childNode, fallbackState));
            }
        }
        
        return new StartOfCheckForTokenState<TTokenType>(node, fallbackState)
        {
            StateLookup = childStates.Build()
        };
    }
}