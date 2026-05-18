namespace Tokensharp.TokenTypeGenerator;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class TokenTypeAttribute(bool numbersAreText = false) : Attribute
{
    public bool NumbersAreText { get; } = numbersAreText;
    public static readonly string FullName = typeof(TokenTypeAttribute).FullName!;
}