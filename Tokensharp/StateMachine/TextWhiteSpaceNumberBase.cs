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

    protected override bool TryGetNextState(in char c, out IState<TTokenType> nextState)
    {
        if (StateLookup.TryGetState(in c, out nextState!))
            return true;

        nextState = CharacterIsValidForState(c) ? this : _endOfTokenStateInstance;

        return true;
    }

    protected override bool TryGetDefaultState(out IState<TTokenType> defaultState)
    {
        defaultState = _endOfTokenStateInstance;
        return true;
    }

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
                    StartOfCheckForTokenState<TTokenType>.For(startNode, this, this, this));
            }
        }

        return textWhiteSpaceNumberStates.Build();
    }

    public abstract bool CharacterIsValidForState(in char c);
}