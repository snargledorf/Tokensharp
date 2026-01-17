using System.Diagnostics.CodeAnalysis;

namespace Tokensharp.TokenTree;

internal interface ITokenTreeNodeBuilder<TTokenType> where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    TTokenType? TokenType { get; set; }
    ITokenTreeNodeBuilder<TTokenType> AddChild(char childCharacter);
    bool TryGetChild(char key, [NotNullWhen(true)] out ITokenTreeNodeBuilder<TTokenType>? node);
    ITokenTreeNode<TTokenType> Build();
}