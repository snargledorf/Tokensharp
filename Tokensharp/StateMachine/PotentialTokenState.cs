using System.Diagnostics.CodeAnalysis;
using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal class PotentialTokenState<TTokenType>(
    ITokenTreeNode<TTokenType> node,
    IState<TTokenType> fallbackState,
    WhiteSpaceState<TTokenType> whiteSpaceState,
    NumberState<TTokenType> numberState,
    TextState<TTokenType> textState)
    : State<TTokenType>()
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private readonly IState<TTokenType> _fallbackState = fallbackState;
    
    private readonly Dictionary<char, IState<TTokenType>> _transitions = new();

    internal ITokenTreeNode<TTokenType> Node { get; } = node;
    internal WhiteSpaceState<TTokenType> WhiteSpaceState { get; } = whiteSpaceState;
    internal NumberState<TTokenType> NumberState { get; } = numberState;
    internal TextState<TTokenType> TextState { get; } = textState;
    
    protected override bool TryGetStateNextState(char c, [NotNullWhen(true)] out IState<TTokenType>? nextState)
    {
        if (Node.IsEndOfToken)
        {
            nextState = EndOfTokenState<TTokenType>.For(Node.TokenType);
            return true;
        }
        
        if (_transitions.TryGetValue(c, out nextState)) 
            return true;

        if (Node.TryGetChild(c, out ITokenTreeNode<TTokenType>? childNode))
        {
            IState<TTokenType>? fallbackState;
            if (node.IsEndOfToken)
                fallbackState = EndOfTokenState<TTokenType>.For(Node.TokenType);
            else
                fallbackState = null;
            
            if (char.IsWhiteSpace(c))
            {
                nextState = new PotentialTokenState<TTokenType>(childNode, fallbackState ?? WhiteSpaceState, WhiteSpaceState,
                    NumberState, TextState);
            }
            else if (char.IsDigit(c))
            {
                nextState = new PotentialTokenState<TTokenType>(childNode, fallbackState ?? NumberState, WhiteSpaceState,
                    NumberState, TextState);
            }
            else
            {
                nextState = new PotentialTokenState<TTokenType>(childNode, fallbackState ?? TextState, WhiteSpaceState,
                    NumberState, TextState);
            }

            _transitions.Add(c, nextState);
            return true;
        }

        nextState = null;
        return false;
    }

    protected override bool TryGetDefaultState([NotNullWhen(true)] out IState<TTokenType>? defaultState)
    {
        if (Node.IsEndOfToken)
        {
            defaultState = EndOfTokenState<TTokenType>.For(Node.TokenType);
            return true;
        }
        
        defaultState = _fallbackState;
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