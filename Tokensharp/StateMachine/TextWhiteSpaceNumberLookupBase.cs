using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal abstract class TextWhiteSpaceNumberLookupBase<TTokenType> : ITextWhiteSpaceNumberLookup<TTokenType>
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{

    protected IStateLookup<TTokenType> BuildStateLookup(ITokenTreeNode<TTokenType> tokenTreeNode)
    {
        var textWhiteSpaceNumberStates = new StateLookupBuilder<TTokenType>();

        foreach (ITokenTreeNode<TTokenType> startNode in tokenTreeNode.RootNode)
        {
            TextWhiteSpaceNumberBase<TTokenType> fallback = GetState(startNode.Character);

            if (startNode.IsEndOfToken)
            {
                textWhiteSpaceNumberStates.Add(startNode.Character,
                    fallback.EndOfTokenStateInstance);
            }
            else
            {
                textWhiteSpaceNumberStates.Add(startNode.Character,
                    StartOfCheckForTokenState<TTokenType>.For(startNode, fallback, fallback, fallback));
            }
        }

        return textWhiteSpaceNumberStates.Build();
    }

    public abstract TextWhiteSpaceNumberBase<TTokenType> GetState(in char c);
}