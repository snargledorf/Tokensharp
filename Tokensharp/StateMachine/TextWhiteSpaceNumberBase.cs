using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal abstract class TextWhiteSpaceNumberBase<TTokenType> : NodeStateBase<TTokenType>, IEndOfTokenStateAccessor<TTokenType>, IStateCharacterCheck
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private readonly EndOfTokenState<TTokenType> _endOfTokenStateInstance;

    protected TextWhiteSpaceNumberBase(ITokenTreeNode<TTokenType> rootNode, TTokenType tokenType) : base(rootNode.RootNode)
    {
        _endOfTokenStateInstance = new EndOfTokenState<TTokenType>(tokenType);
        StateLookup = BuildStateLookup(rootNode);
    }

    public EndOfTokenState<TTokenType> EndOfTokenStateInstance => _endOfTokenStateInstance;

    protected override IState<TTokenType> GetNextState(in char c)
    {
        if (StateLookup.TryGetState(in c, out IState<TTokenType>? nextState))
            return nextState;

        if (CharacterIsValidForState(c))
            return this;
        
        return _endOfTokenStateInstance;
    }

    protected override IState<TTokenType> DefaultState => _endOfTokenStateInstance;

    private IStateLookup<TTokenType> BuildStateLookup(ITokenTreeNode<TTokenType> tokenTreeNode)
    {
        var textWhiteSpaceNumberStates = new StateLookupBuilder<TTokenType>();

        foreach (ITokenTreeNode<TTokenType> startNode in tokenTreeNode.RootNode)
        {
            if (!CharacterIsValidForState(startNode.Character))
                continue;

            if (startNode.IsEndOfToken)
            {
                textWhiteSpaceNumberStates.Add(startNode.Character,
                    EndOfTokenStateInstance);
            }
            else
            {
                textWhiteSpaceNumberStates.Add(startNode.Character,
                    new StartOfCheckForTokenState<TTokenType>(startNode, this, this, this));
            }
        }

        return textWhiteSpaceNumberStates.Build();
    }

    public abstract bool CharacterIsValidForState(in char c);
}