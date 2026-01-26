using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal class StartState<TTokenType> : NodeStateBase<TTokenType>
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private readonly ITextWhiteSpaceNumberLookup<TTokenType> _textWhiteSpaceNumberLookup;

    public StartState(ITokenTreeNode<TTokenType> tokenTreeNode, bool numbersAreText) : base(tokenTreeNode.RootNode)
    {
        _textWhiteSpaceNumberLookup = numbersAreText
            ? new TextAndNumberAsTextLookup<TTokenType>(tokenTreeNode)
            : new TextWhiteSpaceNumberLookup<TTokenType>(tokenTreeNode);

        var startStates = new StateLookupBuilder<TTokenType>();

        foreach (ITokenTreeNode<TTokenType> startNode in tokenTreeNode.RootNode)
        {
            TextWhiteSpaceNumberBase<TTokenType> fallback = _textWhiteSpaceNumberLookup.GetState(startNode.Character);
            
            if (startNode.HasChildren)
            {
                startStates.Add(startNode.Character, PotentialTokenState<TTokenType>.For(startNode, fallback, fallback, fallback));
            }
            else
            {
                startStates.Add(startNode.Character, new EndOfSingleCharacterTokenState<TTokenType>(startNode.TokenType));
            }
        }
        
        StateLookup = startStates.Build();
    }

    protected override bool TryGetNextState(in char c, out IState<TTokenType> nextState)
    {
        if (StateLookup.TryGetState(c, out nextState!))
            return true;

        nextState = _textWhiteSpaceNumberLookup.GetState(in c);
        return true;
    }

    protected override bool TryGetDefaultState(out IState<TTokenType> defaultState)
    {
        defaultState = this;
        return false;
    }
}