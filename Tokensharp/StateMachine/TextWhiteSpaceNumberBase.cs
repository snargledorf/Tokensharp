using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal abstract class TextWhiteSpaceNumberBase<TTokenType> : NodeStateBase<TTokenType>,
    IEndOfTokenStateAccessor<TTokenType>, IStateCharacterCheck
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private readonly EndOfTokenState<TTokenType> _endOfTokenStateInstance;
    private readonly StateLookup<TTokenType> _stateLookup;

    protected TextWhiteSpaceNumberBase(ITokenTreeNode<TTokenType> rootNode, TTokenType tokenType) : base(
        rootNode.RootNode)
    {
        _endOfTokenStateInstance = new EndOfTokenState<TTokenType>(tokenType);
        _stateLookup = BuildStateLookup(rootNode);
    }

    public EndOfTokenState<TTokenType> EndOfTokenStateInstance => _endOfTokenStateInstance;

    protected override State<TTokenType> GetNextState(char c)
    {
        if (_stateLookup.TryGetState(c, out State<TTokenType>? nextState))
            return nextState;

        if (CharacterIsValidForState(c))
            return this;

        return _endOfTokenStateInstance;
    }

    protected override State<TTokenType> DefaultState => _endOfTokenStateInstance;

    private StateLookup<TTokenType> BuildStateLookup(ITokenTreeNode<TTokenType> tokenTreeNode)
    {
        var textWhiteSpaceNumberStates = new StateLookupBuilder<TTokenType>();

        foreach (ITokenTreeNode<TTokenType> startNode in tokenTreeNode.RootNode)
        {
            if (!CharacterIsValidForState(startNode.Character))
                continue;

            if (startNode.IsEndOfToken)
            {
                textWhiteSpaceNumberStates.Add(startNode.Character, _endOfTokenStateInstance);
            }
            else
            {
                textWhiteSpaceNumberStates.Add(startNode.Character,
                    new StartOfCheckForTokenState<TTokenType>(startNode, this, this, this));
            }
        }

        return textWhiteSpaceNumberStates.Build();
    }

    public abstract bool CharacterIsValidForState(char c);
}