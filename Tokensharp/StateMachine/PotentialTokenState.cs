using System.Diagnostics.CodeAnalysis;
using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal class PotentialTokenState<TTokenType>(
    ITokenTreeNode<TTokenType> node,
    IState<TTokenType> fallbackState,
    WhiteSpaceState<TTokenType> whiteSpaceState,
    NumberState<TTokenType> numberState,
    TextState<TTokenType> textState)
    : NodeStateBase<TTokenType>(node)
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private readonly Dictionary<char, IState<TTokenType>> _transitions = new();

    internal WhiteSpaceState<TTokenType> WhiteSpaceState { get; } = whiteSpaceState;
    internal NumberState<TTokenType> NumberState { get; } = numberState;
    internal TextState<TTokenType> TextState { get; } = textState;
    
    protected override bool TryGetStateNextState(char c, [NotNullWhen(true)] out IState<TTokenType>? nextState)
    {
        if (_transitions.TryGetValue(c, out nextState)) 
            return true;

        if (Node.TryGetChild(c, out ITokenTreeNode<TTokenType>? childNode))
        {
            IState<TTokenType> childFallbackState = GetChildFallbackState();
            nextState = new PotentialTokenState<TTokenType>(childNode, childFallbackState, WhiteSpaceState,
                NumberState, TextState);

            _transitions.Add(c, nextState);
            return true;
        }
        
        return TryGetDefaultState(out nextState);
    }

    private IState<TTokenType> GetChildFallbackState()
    {
        return Node.IsEndOfToken ? EndOfTokenState<TTokenType>.For(Node.TokenType) : fallbackState;
    }

    protected override bool TryGetDefaultState([NotNullWhen(true)] out IState<TTokenType>? defaultState)
    {
        if (Node.IsEndOfToken)
        {
            defaultState = EndOfTokenState<TTokenType>.For(Node.TokenType);
            return true;
        }
        
        defaultState = fallbackState;
        return true;
    }

    public override void OnEnter(StateMachineContext<TTokenType> context)
    {
        if (Node.IsEndOfToken)
        {
            context.TokenType = Node.TokenType;
            context.PotentialLexemeLength++;
            context.ConfirmedLexemeLength = context.PotentialLexemeLength;
        }
        else
        {
            context.PotentialLexemeLength++;
        }
    }
}