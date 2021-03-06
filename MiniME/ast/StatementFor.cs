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
	// Represents a for(;;) loop
	class StatementFor : Statement
	{
		// Constructor
		public StatementFor(Bookmark bookmark) : base(bookmark)
		{
		}

		// Attributes
		public Statement Initialize;
		public Expression Condition;
		public Expression Increment;
		public CodeBlock Code;

		public override void Dump(int indent)
		{
			writeLine(indent, "for loop init:");
			if (Initialize != null)
				Initialize.Dump(indent + 1);
			else
				writeLine(indent+1, "n/a");

			writeLine(indent, "condition:");
			if (Condition!=null)
				Condition.Dump(indent+1);
			else
				writeLine(indent+1, "n/a");

			writeLine(indent, "increment:");
			if (Increment!=null)
				Increment.Dump(indent + 1);
			else
				writeLine(indent+1, "n/a");

			writeLine(indent, "do:");
			Code.Dump(indent + 1);
		}

		public override bool Render(RenderContext dest)
		{
			dest.Append("for(");
			if (Initialize != null)
				Initialize.Render(dest);
			dest.Append(";");
			if (Condition != null)
				Condition.Render(dest);
			dest.Append(";");
			if (Increment != null)
				Increment.Render(dest);
			dest.Append(")");
			return Code.RenderIndented(dest);
		}

		public override void OnVisitChildNodes(IVisitor visitor)
		{
			if (Initialize!=null)
				Initialize.Visit(visitor);

			if (Condition!=null)
				Condition.Visit(visitor);

			if (Increment!=null)
				Increment.Visit(visitor);

			Code.Visit(visitor);
		}


	}
}
