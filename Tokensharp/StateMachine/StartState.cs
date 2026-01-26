using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal class StartState<TTokenType>(
    ITokenTreeNode<TTokenType> rootNode,
    ITextWhiteSpaceNumberLookup<TTokenType> textWhiteSpaceNumberLookup)
    : NodeStateBase<TTokenType>(rootNode.RootNode)
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    protected override bool TryGetNextState(in char c, out IState<TTokenType> nextState)
    {
        if (StateLookup.TryGetState(c, out nextState!))
            return true;

        nextState = textWhiteSpaceNumberLookup.GetState(in c);
        return true;
    }

    protected override bool TryGetDefaultState(out IState<TTokenType> defaultState)
    {
        defaultState = this;
        return false;
    }

    public static StartState<TTokenType> For(ITokenTreeNode<TTokenType> tokenTreeNode, bool numbersAreText)
    {
        ITextWhiteSpaceNumberLookup<TTokenType> textWhiteSpaceNumberLookup;
        if (numbersAreText)
        {
            textWhiteSpaceNumberLookup = new TextAndNumberAsTextLookup<TTokenType>(tokenTreeNode);
        }
        else
        {
            textWhiteSpaceNumberLookup = new TextWhiteSpaceNumberLookup<TTokenType>(tokenTreeNode);
        }

        var startStates = new StateLookupBuilder<TTokenType>();

        foreach (ITokenTreeNode<TTokenType> startNode in tokenTreeNode.RootNode)
        {
            TextWhiteSpaceNumberBase<TTokenType> fallback = textWhiteSpaceNumberLookup.GetState(startNode.Character);
            
            if (startNode.HasChildren)
            {
                startStates.Add(startNode.Character, PotentialTokenState<TTokenType>.For(startNode, fallback, fallback, fallback));
            }
            else
            {
                startStates.Add(startNode.Character, new EndOfSingleCharacterTokenState<TTokenType>(startNode.TokenType));
            }
        }

        var startState = new StartState<TTokenType>(tokenTreeNode.RootNode, textWhiteSpaceNumberLookup)
        {
            StateLookup = startStates.Build()
        };
        
        return startState;
    }
}