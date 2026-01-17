using System.Diagnostics.CodeAnalysis;
using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal abstract class BaseState<TTokenType>(ITokenTreeNode<TTokenType> node, WhiteSpaceState<TTokenType> whiteSpaceState, NumberState<TTokenType> numberState, TextState<TTokenType> textState)
    : State<TTokenType> where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private readonly Dictionary<char, IState<TTokenType>> _transitions = new();

    protected ITokenTreeNode<TTokenType> Node { get; } = node;
    internal WhiteSpaceState<TTokenType> WhiteSpaceState { get; } = whiteSpaceState;
    internal NumberState<TTokenType> NumberState { get; } = numberState;
    internal TextState<TTokenType> TextState { get; } = textState;

    protected override bool TryGetStateNextState(char c, [NotNullWhen(true)] out IState<TTokenType>? nextState)
    {
        if (_transitions.TryGetValue(c, out nextState)) 
            return true;

        if (Node.TryGetChild(c, out ITokenTreeNode<TTokenType>? childNode))
        {
            IState<TTokenType>? defaultState;
            if (childNode.IsEndOfToken)
                defaultState = EndOfTokenState<TTokenType>.For(childNode.TokenType);
            else if (Node.IsEndOfToken)
                defaultState = EndOfTokenState<TTokenType>.For(Node.TokenType);
            else
                defaultState = null;

            // TODO Handle mixed character type tokens
            if (char.IsWhiteSpace(c))
            {
                nextState = new PotentialWhiteSpaceTokenState<TTokenType>(childNode, defaultState ?? WhiteSpaceState, WhiteSpaceState,
                    NumberState, TextState);
            }
            else if (char.IsDigit(c))
            {
                nextState = new PotentialNumberTokenState<TTokenType>(childNode, defaultState ?? NumberState, NumberState,
                    WhiteSpaceState, TextState);
            }
            else
            {
                nextState = new PotentialTextTokenState<TTokenType>(childNode, defaultState ?? TextState, TextState,
                    WhiteSpaceState, NumberState);
            }

            _transitions.Add(c, nextState);
            return true;
        }

        if (Node.IsEndOfToken)
        {
            nextState = EndOfTokenState<TTokenType>.For(Node.TokenType);
            return true;
        }

        nextState = null;
        return false;
    }

    public override void OnEnter(StateMachineContext<TTokenType> context)
    {
        if (!Node.IsEndOfToken)
            return;
        
        context.TokenType = Node.TokenType;
        context.ConfirmedLexemeLength++;
        context.PotentialLexemeLength = context.ConfirmedLexemeLength;
    }
}