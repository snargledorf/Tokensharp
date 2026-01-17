using System.Diagnostics.CodeAnalysis;
using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal class NumberState<TTokenType>(ITokenTreeNode<TTokenType> rootNode) : RootState<TTokenType>(rootNode)
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public static readonly EndOfTokenState<TTokenType> EndOfTokenState = EndOfTokenState<TTokenType>.For(TokenType<TTokenType>.Number);
    
    private readonly Dictionary<char, IState<TTokenType>> _states = new();
    
    private WhiteSpaceState<TTokenType>? _whiteSpaceState;
    private TextState<TTokenType>? _textState;

    private static NumberState<TTokenType>? _instance;

    internal override WhiteSpaceState<TTokenType> WhiteSpaceStateInstance => _whiteSpaceState ?? throw new InvalidOperationException($"{nameof(WhiteSpaceStateInstance)} not initialized");
    internal override NumberState<TTokenType> NumberStateInstance => this;
    internal override TextState<TTokenType> TextStateInstance => _textState ?? throw new InvalidOperationException($"{nameof(TextStateInstance)} not initialized");

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
        _whiteSpaceState = WhiteSpaceState<TTokenType>.For(RootNode);
        _textState = TextState<TTokenType>.For(RootNode);
    }

    protected override bool TryGetStateNextState(char c, [NotNullWhen(true)] out IState<TTokenType>? nextState)
    {
        if (!char.IsDigit(c))
        {
            nextState = EndOfTokenState;
            return true;
        }
        
        if (base.TryGetStateNextState(c, out nextState))
            return true;
        
        nextState = this;
        return true;
    }

    public override bool TryDefaultTransition(StateMachineContext<TTokenType> context, [NotNullWhen(true)] out IState<TTokenType>? defaultState)
    {
        defaultState = EndOfTokenState;
        return true;
    }

    protected override IState<TTokenType> GetFallbackEndOfTokenState(ITokenTreeNode<TTokenType> node)
    {
        return EndOfTokenState;
    }

    public override void OnEnter(StateMachineContext<TTokenType> context)
    {
        context.TokenType = TokenType<TTokenType>.Number;
        base.OnEnter(context);
    }
}