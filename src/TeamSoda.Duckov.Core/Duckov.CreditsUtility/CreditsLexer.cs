using System.Collections;
using System.Collections.Generic;

namespace Duckov.CreditsUtility;

public class CreditsLexer : IEnumerable<Token>, IEnumerable
{
	private readonly string content;

	private ushort cursor;

	private ushort lineBegin;

	public CreditsLexer(string content)
	{
		this.content = content;
		cursor = 0;
		lineBegin = 0;
	}

	public void Reset()
	{
		cursor = 0;
		lineBegin = 0;
	}

	private void TrimLeft()
	{
		while (cursor < content.Length)
		{
			char c = content[cursor];
			if (!char.IsWhiteSpace(c) || c == '\n')
			{
				break;
			}
			cursor++;
		}
	}

	public Token Next()
	{
		TrimLeft();
		if (cursor >= content.Length)
		{
			cursor++;
			return new Token(TokenType.End);
		}
		switch (content[cursor])
		{
		case '\n':
			cursor++;
			return new Token(TokenType.EmptyLine);
		case '#':
		{
			cursor++;
			int startIndex = cursor;
			while (cursor < content.Length && content[cursor] != '\n')
			{
				cursor++;
			}
			cursor++;
			return new Token(TokenType.Comment, content.Substring(startIndex, cursor));
		}
		case '[':
		{
			cursor++;
			int num2 = cursor;
			while (cursor < content.Length)
			{
				if (content[cursor] == ']')
				{
					string text = content.Substring(num2, cursor - num2);
					while (cursor < content.Length)
					{
						cursor++;
						if (cursor >= content.Length)
						{
							break;
						}
						char c = content[cursor];
						if (c == '\n')
						{
							cursor++;
							break;
						}
						if (!char.IsWhiteSpace(c))
						{
							break;
						}
					}
					return new Token(TokenType.Instructor, text);
				}
				if (content[cursor] == '\n')
				{
					cursor++;
					return new Token(TokenType.Invalid, content.Substring(num2, cursor - num2));
				}
				cursor++;
			}
			return new Token(TokenType.Invalid, content.Substring(num2 - 1));
		}
		default:
		{
			int num = cursor;
			string raw;
			while (cursor < content.Length)
			{
				switch (content[cursor])
				{
				case '\n':
					raw = content.Substring(num, cursor - num);
					cursor++;
					return new Token(TokenType.String, ConvertEscapes(raw));
				case '#':
					raw = content.Substring(num, cursor - num);
					return new Token(TokenType.String, ConvertEscapes(raw));
				}
				cursor++;
			}
			raw = content.Substring(num, cursor - num);
			return new Token(TokenType.String, ConvertEscapes(raw));
		}
		}
	}

	private string ConvertEscapes(string raw)
	{
		return raw.Replace("\\n", "\n");
	}

	public IEnumerator<Token> GetEnumerator()
	{
		while (cursor < content.Length)
		{
			yield return Next();
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
