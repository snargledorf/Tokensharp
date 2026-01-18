using System.Diagnostics.CodeAnalysis;
using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal abstract class TextWhiteSpaceNumberStateBase<TTokenType>(ITokenTreeNode<TTokenType> rootNode)
    : RootState<TTokenType>(rootNode), IEndOfTokenAccessorState<TTokenType> where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public abstract EndOfTokenState<TTokenType> EndOfTokenStateInstance { get; }
    
    protected override bool TryGetStateNextState(char c, [NotNullWhen(true)] out IState<TTokenType>? nextState)
    {
        if (!CharacterIsValidForState(c))
        {
            nextState = EndOfTokenStateInstance;
            return true;
        }
        
        if (base.TryGetStateNextState(c, out nextState))
            return true;
        
        nextState = this;
        return true;
    }

    protected override bool TryGetDefaultState([NotNullWhen(true)] out IState<TTokenType>? defaultState)
    {
        defaultState = EndOfTokenStateInstance;
        return true;
    }

    protected override IEndOfTokenAccessorState<TTokenType> GetNextStateForChildNode(ITokenTreeNode<TTokenType> childNode, IEndOfTokenAccessorState<TTokenType> fallbackState)
    {
        return new StartOfCheckForTokenState<TTokenType>(childNode, this);
    }
}