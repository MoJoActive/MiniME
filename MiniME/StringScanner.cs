﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MiniME
{
	/*
	 * StringScanner is a simple class to help scan through an input string.
	 * 
	 * Maintains a current position with various operations to inspect the current
	 * character, skip forward, check for matches, skip whitespace etc...
	 */
	public class StringScanner
	{
		// Constructor
		public StringScanner()
		{
		}

		// Constructor
		public StringScanner(string str)
		{
			Reset(str);
		}

		// Constructor
		public StringScanner(string str, int pos)
		{
			Reset(str, pos);
		}

		// Constructor
		public StringScanner(string str, int pos, int len)
		{
			Reset(str, pos, len);
		}

		// Reset
		public void Reset(string str)
		{
			Reset(str, 0, str!=null ? str.Length : 0);
		}

		// Reset
		public void Reset(string str, int pos)
		{
			Reset(str, pos, str!=null ? str.Length - pos : 0);
		}

		// Reset
		public void Reset(string str, int pos, int len)
		{
			if (str == null)
				str = "";
			if (len < 0)
				len = 0;
			if (pos < 0)
				pos = 0;
			if (pos > str.Length)
				pos = str.Length;

			this.str = str;
			this.start = pos;
			this.pos = pos;
			this.end = pos + len;
		}

		// Get the entire input string
		public string input
		{
			get
			{
				return str;
			}
		}

		// Get the character at the current position
		public char current
		{
			get
			{
				if (pos < start || pos >= end)
					return '\0';
				else
					return str[pos];
			}
		}

		// Get/set the current position
		public int position
		{
			get
			{
				return pos;
			}
			set
			{
				pos = value;
			}
		}

		// Get the remainder of the input 
		// (use this in a watch window while debugging :)
		public string remainder
		{
			get
			{
				return Substring(position);
			}
		}

		// Skip to the end of file
		public void SkipToEof()
		{
			pos = end;
		}


		// Skip to the end of the current line
		public void SkipToEol()
		{
			while (pos < end)
			{
				char ch=str[pos];
				if (ch=='\r' || ch=='\n')
					break;
				pos++;
			}
		}

		// Skip if currently at a line end
		public bool SkipEol()
		{
			if (pos < end)
			{
				char ch = str[pos];
				if (ch == '\r')
				{
					pos++;
					if (pos < end && str[pos] == '\n')
						pos++;
					return true;
				}

				else if (ch == '\n')
				{
					pos++;
					if (pos < end && str[pos] == '\r')
						pos++;
					return true;
				}
			}

			return false;
		}

		// Skip to the next line
		public void SkipToNextLine()
		{
			SkipToEol();
			SkipEol();
		}

		// Get the character at offset from current position
		// Or, \0 if out of range
		public char CharAtOffset(int offset)
		{
			int index = pos + offset;
			
			if (index < start)
				return '\0';
			if (index >= end)
				return '\0';
			return str[index];
		}

		// Skip a number of characters
		public void SkipForward(int characters)
		{
			pos += characters;
		}

		// Skip a character if present
		public bool SkipChar(char ch)
		{
			if (current == ch)
			{
				SkipForward(1);
				return true;
			}

			return false;	
		}

		// Skip a matching string
		public bool SkipString(string str)
		{
			if (DoesMatch(str))
			{
				SkipForward(str.Length);
				return true;
			}

			return false;
		}

		// Skip a matching string
		public bool SkipStringI(string str)
		{
			if (DoesMatchI(str))
			{
				SkipForward(str.Length);
				return true;
			}

			return false;
		}

		// Skip any whitespace
		public bool SkipWhitespace()
		{
			if (!char.IsWhiteSpace(current))
				return false;
			SkipForward(1);

			while (char.IsWhiteSpace(current))
				SkipForward(1);

			return true;
		}

		// Check if a character is space or tab
		public static bool IsLineSpace(char ch)
		{
			return ch == ' ' || ch == '\t';
		}

		// Skip spaces and tabs
		public bool SkipLinespace()
		{
			if (!IsLineSpace(current))
				return false;
			SkipForward(1);

			while (IsLineSpace(current))
				SkipForward(1);

			return true;
		}

		// Does current character match something
		public bool DoesMatch(char ch)
		{
			return current == ch;
		}

		// Does character at offset match a character
		public bool DoesMatch(int offset, char ch)
		{
			return CharAtOffset(offset) == ch;
		}

		// Does current character match any of a range of characters
		public bool DoesMatchAny(char[] chars)
		{
			for (int i = 0; i < chars.Length; i++)
			{
				if (DoesMatch(chars[i]))
					return true;
			}
			return false;
		}

		// Does current character match any of a range of characters
		public bool DoesMatchAny(int offset, char[] chars)
		{
			for (int i = 0; i < chars.Length; i++)
			{
				if (DoesMatch(offset, chars[i]))
					return true;
			}
			return false;
		}

		// Does current string position match a string
		public bool DoesMatch(string str)
		{
			for (int i = 0; i < str.Length; i++)
			{
				if (str[i] != CharAtOffset(i))
					return false;
			}
			return true;
		}

		// Does current string position match a string
		public bool DoesMatchI(string str)
		{
			return string.Compare(str, Substring(position, str.Length), true) == 0;
		}

		// Extract a substring
		public string Substring(int start)
		{
			return str.Substring(start, end-start);
		}

		// Extract a substring
		public string Substring(int start, int len)
		{
			if (start + len > end)
				len = end - start;

			return str.Substring(start, len);
		}

		// Scan forward for a character
		public bool Find(char ch)
		{
			if (pos >= end)
				return false;

			// Find it
			int index = str.IndexOf(ch, pos);
			if (index < 0 || index>=end)
				return false;

			// Store new position
			pos = index;
			return true;
		}

		// Find any of a range of characters
		public bool FindAny(char[] chars)
		{
			if (pos >= end)
				return false;

			// Find it
			int index = str.IndexOfAny(chars, pos);
			if (index < 0 || index>=end)
				return false;

			// Store new position
			pos = index;
			return true;
		}

		// Forward scan for a string
		public bool Find(string find)
		{
			if (pos >= end)
				return false;

			int index = str.IndexOf(find, pos);
			if (index < 0 || index > end-find.Length)
				return false;

			pos = index;
			return true;
		}

		// Forward scan for a string (case insensitive)
		public bool FindI(string find)
		{
			if (pos >= end)
				return false;

			int index = str.IndexOf(find, pos, StringComparison.InvariantCultureIgnoreCase);
			if (index < 0 || index >= end - find.Length)
				return false;

			pos = index;
			return true;
		}

		// Are we at eof?
		public bool eof
		{
			get
			{
				return pos >= end;
			}
		}

		// Are we at eol?
		public bool eol
		{
			get
			{
				return IsLineEnd(current);
			}
		}

		// Are we at bof?
		public bool bof
		{
			get
			{
				return pos == start;
			}
		}

		// Mark current position
		public void Mark()
		{
			mark = pos;
		}

		// Extract string from mark to current position
		public string Extract()
		{
			if (mark >= pos)
				return "";

			return str.Substring(mark, pos - mark);
		}

		// Scan a string for a valid identifier.  Identifier must start with alpha or underscore
		// and can be followed by alpha, digit or underscore
		// Updates `pos` to character after the identifier if matched
		public static bool ParseIdentifier(string str, ref int pos, ref string identifer)
		{
			if (pos >= str.Length)
				return false;

			// Must start with a letter or underscore
			if (!char.IsLetter(str[pos]) && str[pos] != '_')
			{
				return false;
			}

			// Find the end
			int startpos = pos;
			pos++;
			while (pos < str.Length && (char.IsDigit(str[pos]) || char.IsLetter(str[pos]) || str[pos] == '_'))
				pos++;

			// Return it
			identifer = str.Substring(startpos, pos - startpos);
			return true;
		}

		// Skip an identifier
		public bool SkipIdentifier(ref string identifier)
		{
			int savepos = position;
			if (!ParseIdentifier(this.str, ref pos, ref identifier))
				return false;
			if (pos >= end)
			{
				pos = savepos;
				return false;
			}
			return true;
		}

		// Check if a character marks end of line
		public static bool IsLineEnd(char ch)
		{
			return ch == '\r' || ch == '\n' || ch=='\0';
		}

		public int LineNumberFromOffset(int offset, out int lineoffset)
		{
			PrepareLineNumbers();

			for (int i = 1; i < m_lineNumberOffsets.Count; i++)
			{
				if (offset < m_lineNumberOffsets[i])
				{
					lineoffset = offset - m_lineNumberOffsets[i - 1];
					return i-1;
				}
			}

			lineoffset = offset - m_lineNumberOffsets[m_lineNumberOffsets.Count - 1] ;
			return m_lineNumberOffsets.Count;
		}

		public void PrepareLineNumbers()
		{
			if (m_lineNumberOffsets != null)
				return;


			// Create the list and add offset of first line
			m_lineNumberOffsets = new List<int>();
			m_lineNumberOffsets.Add(0);

			int pos = 0;
			while (true)
			{
				// Find the end of this line
				int offset = str.IndexOfAny(new char[] { '\r', '\n' }, pos);
				if (offset < 0)
					break;

				// Find the start of the next line
				pos = offset;
				if (str[pos] == '\r')
					pos++;
				if (str[pos] == '\n')
					pos++;

				// Store the start position of this line
				m_lineNumberOffsets.Add(pos);
			}
		}

		// Attributes
		string str;
		int start;
		int pos;
		int end;
		int mark;
		List<int> m_lineNumberOffsets;
	}
}
