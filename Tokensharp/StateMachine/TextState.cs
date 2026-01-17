using System.Diagnostics.CodeAnalysis;
using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal class TextState<TTokenType>(ITokenTreeNode<TTokenType> rootNode) : RootState<TTokenType>(rootNode)
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private WhiteSpaceState<TTokenType>? _whiteSpaceState;
    private NumberState<TTokenType>? _numberState;

    private static TextState<TTokenType>? _instance;

    public static EndOfTokenState<TTokenType> EndOfTokenState { get; } = EndOfTokenState<TTokenType>.For(TokenType<TTokenType>.Text);
    
    internal override WhiteSpaceState<TTokenType> WhiteSpaceStateInstance => _whiteSpaceState ?? throw new InvalidOperationException($"{nameof(WhiteSpaceStateInstance)} not initialized");
    internal override NumberState<TTokenType> NumberStateInstance => _numberState ?? throw new InvalidOperationException($"{nameof(NumberStateInstance)} not initialized");
    internal override TextState<TTokenType> TextStateInstance => this;


    internal static TextState<TTokenType> For(ITokenTreeNode<TTokenType> treeNode)
    {
        if (_instance is { } instance)
            return instance;
        
        _instance = new TextState<TTokenType>(treeNode.RootNode);

        _instance.Initialize();

        return _instance;
    }

    private void Initialize()
    {
        _whiteSpaceState = WhiteSpaceState<TTokenType>.For(RootNode);
        _numberState = NumberState<TTokenType>.For(RootNode);
    }

    protected override bool TryGetStateNextState(char c, [NotNullWhen(true)] out IState<TTokenType>? nextState)
    {
        if (char.IsDigit(c) || char.IsWhiteSpace(c))
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

    public override void OnEnter(StateMachineContext<TTokenType> context)
    {
        context.TokenType = TokenType<TTokenType>.Text;
        base.OnEnter(context);
    }
}