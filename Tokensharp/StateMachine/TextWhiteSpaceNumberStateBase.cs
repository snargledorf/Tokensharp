using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal abstract class TextWhiteSpaceNumberStateBase<TTokenType>(ITokenTreeNode<TTokenType> rootNode)
    : NodeStateBase<TTokenType>(rootNode.RootNode), IEndOfTokenAccessorState<TTokenType>
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private IStateLookup<TTokenType>? _states;
    
    private IStateLookup<TTokenType> States => _states ?? throw new InvalidOperationException("States not initialized");
    
    public abstract EndOfTokenState<TTokenType> EndOfTokenStateInstance { get; }

    public void InitializeStates(IStateLookup<TTokenType> states)
    {
        _states = states;
    }

    protected override bool TryGetStateNextState(char c, [NotNullWhen(true)] out IState<TTokenType>? nextState)
    {
        if (!CharacterIsValidForState(c))
        {
            nextState = EndOfTokenStateInstance;
        }
        else if (!States.TryGetState(c, out nextState))
        {
            nextState = this;
        }

        return true;
    }

    protected override IState<TTokenType> CreateStateForChildNode(ITokenTreeNode<TTokenType> childNode)
    {
        if (childNode.IsEndOfToken)
            return EndOfTokenStateInstance;

        return new StartOfCheckForTokenState<TTokenType>(childNode, this);
    }

    protected override bool TryGetDefaultState([NotNullWhen(true)] out IState<TTokenType>? defaultState)
    {
        defaultState = EndOfTokenStateInstance;
        return true;
    }
}