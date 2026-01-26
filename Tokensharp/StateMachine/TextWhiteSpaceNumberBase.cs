using System.Diagnostics.CodeAnalysis;
using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal abstract class TextWhiteSpaceNumberBase<TTokenType>(ITokenTreeNode<TTokenType> rootNode, TTokenType tokenType)
    : NodeStateBase<TTokenType>(rootNode.RootNode), IEndOfTokenStateAccessor<TTokenType>, IStateCharacterCheck
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private readonly EndOfTokenState<TTokenType> _endOfTokenStateInstance = new(tokenType);
    public EndOfTokenState<TTokenType> EndOfTokenStateInstance => _endOfTokenStateInstance;

    protected override bool TryGetNextState(in char c, out IState<TTokenType> nextState)
    {
        if (!CharacterIsValidForState(c))
        {
            nextState = EndOfTokenStateInstance;
        }
        else if (!base.TryGetNextState(in c, out nextState))
        {
            nextState = this;
        }

        return true;
    }

    protected override bool TryGetDefaultState(out IState<TTokenType> defaultState)
    {
        defaultState = _endOfTokenStateInstance;
        return true;
    }

    public abstract bool CharacterIsValidForState(char c);
}