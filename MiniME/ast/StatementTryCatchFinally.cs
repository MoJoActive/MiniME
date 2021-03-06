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
	// Represents a try-catch-finally statement
	class StatementTryCatchFinally : Statement
	{
		// Constructor
		public StatementTryCatchFinally(Bookmark bookmark) : base(bookmark)
		{
		}

		// Attributes
		public CodeBlock Code;
		public List<CatchClause> CatchClauses = new List<CatchClause>();
		public CodeBlock FinallyClause;

		public override void Dump(int indent)
		{
			writeLine(indent, "try:");
			Code.Dump(indent + 1);

			foreach (var cc in CatchClauses)
			{
				cc.Dump(indent);
			}

		}

		public override bool Render(RenderContext dest)
		{
			// Statement
			dest.Append("try");
			Code.Render(dest);

			// Catch clauses
			foreach (var cc in CatchClauses)
			{
				cc.Render(dest);
			}

			// Finally clause
			if (FinallyClause != null)
			{
				dest.Append("finally");
				FinallyClause.RenderIndented(dest);
			}
			return false;

		}

		public override void OnVisitChildNodes(IVisitor visitor)
		{
			Code.Visit(visitor);
			foreach (var cc in CatchClauses)
			{
				cc.Visit(visitor);
			}
			if (FinallyClause != null)
				FinallyClause.Visit(visitor);
		}
	}

	// Represents a single catch clause in a try-catch statement
	class CatchClause : Statement
	{
		// Constructor
		public CatchClause(Bookmark bookmark) : base(bookmark)
		{
		}

		// Attributes
		public string ExceptionVariable;
		public Expression Condition;
		public CodeBlock Code;

		public override void Dump(int indent)
		{
			if (Condition != null)
			{
				writeLine(indent, "catch `{0}` if:", ExceptionVariable);
				Condition.Dump(indent + 1);
				writeLine(indent, "do:");
			}
			else
			{
				writeLine(indent, "catch `{0}` do:", ExceptionVariable);
			}

			Code.Dump(indent + 1);
		}

		public override bool Render(RenderContext dest)
		{
			// Enter a new symbol scope since the exception variable
			// can be obfuscated
			dest.EnterScope(Scope);

			// Catch clause
			dest.StartLine();
			dest.Append("catch(");
			dest.Append(dest.Symbols.GetObfuscatedSymbol(ExceptionVariable));
			if (Condition != null)
			{
				dest.Append("if");
				Condition.Render(dest);
			}
			dest.Append(')');

			// Associated code
			Code.Render(dest);

			// Done
			dest.LeaveScope();
			return false;
		}

		public override void OnVisitChildNodes(IVisitor visitor)
		{
			if (Condition != null)
				Condition.Visit(visitor);
			if (Code != null)
				Code.Visit(visitor);
		}
	}


}
