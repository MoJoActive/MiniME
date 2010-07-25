﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MiniME.ast
{
	interface IVisitor
	{
		void OnEnterNode(Node n);
		void OnLeaveNode(Node n);

	}

	abstract class Node
	{
		public abstract void Dump(int indent);
		public abstract bool Render(StringBuilder dest);

		public void Visit(IVisitor visitor)
		{
			visitor.OnEnterNode(this);
			OnVisitChildNodes(visitor);
			visitor.OnLeaveNode(this);
		}

		public abstract void OnVisitChildNodes(IVisitor visitor);

		public static void write(int indent, string str, params string[] args)
		{
			Console.Write(new String(' ', indent * 4));
			Console.Write(str, args);
		}
		public static void write(string str, params string[] args)
		{
			Console.Write(str, args);
		}
		public static void writeLine(int indent, string str, params string[] args)
		{
			Console.Write(new String(' ', indent*4));
			Console.WriteLine(str, args);
		}
	}

}
