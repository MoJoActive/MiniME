﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MiniMETestCases
{
	class TestFile
	{
		public string Input;
		public string Output;

		private static Regex FileParser = new Regex(
			@"
			(.*)\r\n				# input
			\-{5,}\r\n				# -----
			(.*)\r\n				# output
			\-{5,}\r\n				# -----
			",
			RegexOptions.Multiline | 
			RegexOptions.Singleline |
			RegexOptions.IgnorePatternWhitespace | 
			RegexOptions.Compiled);

		public void LoadFromString(string str)
		{
			var m=FileParser.Match(str);
			if (!m.Success)
				throw new Exception("Failed to parse test script");

			Input = m.Groups[1].ToString().Replace("\r\n", "\n").Trim();
			Output = m.Groups[2].ToString().Replace("\r\n", "\n").Trim();
		}
	}
}
