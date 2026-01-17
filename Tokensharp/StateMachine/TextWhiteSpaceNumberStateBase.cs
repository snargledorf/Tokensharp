using System.Diagnostics.CodeAnalysis;
using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal abstract class TextWhiteSpaceNumberStateBase<TTokenType>(ITokenTreeNode<TTokenType> rootNode)
    : RootState<TTokenType>(rootNode) where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    protected abstract TTokenType TokenType { get; }
    public abstract EndOfTokenState<TTokenType> EndOfTokenState { get; }


    protected override bool TryGetStateNextState(char c, [NotNullWhen(true)] out IState<TTokenType>? nextState)
    {
        if (!CharacterIsValidForToken(c))
        {
            nextState = EndOfTokenState;
            return true;
        }
        
        if (base.TryGetStateNextState(c, out nextState))
            return true;
        
        nextState = this;
        return true;
    }

    protected override bool TryGetDefaultState([NotNullWhen(true)] out IState<TTokenType>? defaultState)
    {
        defaultState = EndOfTokenState;
        return true;
    }

    protected override IState<TTokenType> GetFallbackEndOfTokenState(ITokenTreeNode<TTokenType> node)
    {
        return EndOfTokenState;
    }

    protected override IState<TTokenType> GetStateForChildNode(ITokenTreeNode<TTokenType> childNode)
    {
        return new CheckForTokenState<TTokenType>(childNode, this);
    }

    public override void OnEnter(StateMachineContext<TTokenType> context)
    {
        context.TokenType = TokenType;
        base.OnEnter(context);
    }

    public abstract bool CharacterIsValidForToken(char c);
}