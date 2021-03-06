﻿// 
//   MiniME - http://www.toptensoftware.com/minime
// 
//   The contents of this file are subject to the license terms as 
//	 specified at the web address above.
//  
//   Software distributed under the License is distributed on an 
//   "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either express or
//   implied. See the License for the specific language governing
//   rights and limitations under the License.
// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MiniME.ast
{
	// Represents a set of variable declaration.
	class StatementVariableDeclaration : Statement
	{
		// Constructor
		public StatementVariableDeclaration(Bookmark bookmark) : base(bookmark)
		{
		}

		// Add another variable declaration
		public void AddDeclaration(Bookmark bookmark, string Name, ast.Expression InitialValue)
		{
			var v = new Variable();
			v.Bookmark = bookmark;
			v.Name = Name;
			v.InitialValue = InitialValue;
			Variables.Add(v);
		}

		// Check if any variables have an initial value
		// (used to check for invalid variable declaration in for-in loop)
		public bool HasInitialValue()
		{
			foreach (var v in Variables)
			{
				if (v.InitialValue != null)
					return true;
			}
			return false;
		}


		public override void Dump(int indent)
		{
			foreach (var v in Variables)
			{
				if (v.InitialValue != null)
				{
					writeLine(indent, "variable `{0}`, initial value:", v.Name);
					v.InitialValue.Dump(indent + 1);
				}
				else
				{
					writeLine(indent, "variable `{0}`", v.Name);
				}
			}
		}


		public override bool Render(RenderContext dest)
		{
			// Quit if all variables have been eliminated
			if (Variables.Count == 0)
				return false;

			// Statement
			dest.Append("var");

			// Comma separated variables
			bool bFirst = true;
			foreach (var v in Variables)
			{
				if (!bFirst)
				{
					dest.Append(',');
				}
				else
				{
					bFirst = false;
				}

				// Variable name, possibly obfuscated
				dest.Append(dest.Symbols.GetObfuscatedSymbol(v.Name));

				// Optional initial value
				if (v.InitialValue != null)
				{
					dest.Append("=");
					v.InitialValue.Render(dest);
				}
			}
			return true;
		}

		public override void OnVisitChildNodes(IVisitor visitor)
		{
			foreach (var v in Variables)
			{
				if (v.InitialValue != null)
					v.InitialValue.Visit(visitor);
			}
		}


		// Represents a single variable declaration
		public class Variable
		{
			public Bookmark Bookmark;
			public string Name;
			public Expression InitialValue;
		}

		public List<Variable> Variables=new List<Variable>();

		public override bool IsDeclarationOnly()
		{
			foreach (var v in Variables)
			{
				if (v.InitialValue != null)
					return false;
			}

			return true;
		}

	}
}
