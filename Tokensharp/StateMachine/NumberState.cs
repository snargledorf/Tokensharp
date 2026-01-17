using System.Diagnostics.CodeAnalysis;
using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal class NumberState<TTokenType>(ITokenTreeNode<TTokenType> node) : State<TTokenType>
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public static readonly EndOfTokenState<TTokenType> EndOfNumberState = EndOfTokenState<TTokenType>.For(TokenType<TTokenType>.Number);
    
    private readonly Dictionary<char, IState<TTokenType>> _states = new();
    
    private WhiteSpaceState<TTokenType>? _whiteSpaceState;
    private TextState<TTokenType>? _textState;

    private static NumberState<TTokenType>? _instance;

    private WhiteSpaceState<TTokenType> WhiteSpaceState => _whiteSpaceState ?? throw new InvalidOperationException($"{nameof(WhiteSpaceState)} not initialized");
    private TextState<TTokenType> TextState => _textState ?? throw new InvalidOperationException($"{nameof(TextState)} not initialized");

    internal static NumberState<TTokenType> For(ITokenTreeNode<TTokenType> treeNode)
    {
        if (_instance is { } instance)
            return instance;
        
        _instance = new NumberState<TTokenType>(treeNode.RootNode);

        _instance.Initialize();

        return _instance;
    }

    private void Initialize()
    {
        _whiteSpaceState = WhiteSpaceState<TTokenType>.For(node);
        _textState = TextState<TTokenType>.For(node);
    }
    
    protected override bool TryGetStateNextState(char c, [NotNullWhen(true)] out IState<TTokenType>? nextState)
    {
        if (!char.IsDigit(c))
        {
            nextState = EndOfNumberState;
            return true;
        }

        if (_states.TryGetValue(c, out nextState))
            return true;
        
        if (node.RootNode.TryGetChild(c, out ITokenTreeNode<TTokenType>? childNode))
        {
            if (childNode.IsEndOfToken)
            {
                nextState = EndOfTokenState<TTokenType>.For(childNode.TokenType);
                return true;
            }
            
            if (char.IsDigit(c))
                nextState = new CheckForNumberTokenState<TTokenType>(childNode, this, WhiteSpaceState, TextState);
            else
                nextState = new CheckForMixedTypeTokenState<TTokenType>(childNode, this, TextState,
                    WhiteSpaceState, this);

            _states.Add(c, nextState);
            return true;
        }
        
        nextState = this;
        return true;
    }

    public override bool TryDefaultTransition(StateMachineContext<TTokenType> context, [NotNullWhen(true)] out IState<TTokenType>? defaultState)
    {
        defaultState = EndOfNumberState;
        return true;
    }

    public override void OnEnter(StateMachineContext<TTokenType> context)
    {
        context.TokenType = TokenType<TTokenType>.Number;
        base.OnEnter(context);
    }
}