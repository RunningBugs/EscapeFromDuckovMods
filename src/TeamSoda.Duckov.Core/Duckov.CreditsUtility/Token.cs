namespace Duckov.CreditsUtility;

public struct Token
{
	public TokenType type;

	public string text;

	public Token(TokenType type, string text = null)
	{
		this.type = type;
		this.text = text;
	}
}
