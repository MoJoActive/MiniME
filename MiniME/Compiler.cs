﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MiniME
{
	// Main api into the MiniME minifier/obfuscator
	public class Compiler
	{
		// Constructor
		public Compiler()
		{
			Reset();
			DetectConsts = true;
			UseOptionsFile = true;
			MaxLineLength = 120;
		}

		// Attributes
		List<FileInfo> m_files = new List<FileInfo>();
		List<string> m_ResponseFiles = new List<string>();

		// Maximum line length before wrap
		//  - set to zero for no line breaks
		//  - no guarantees, long strings won't be broken
		//    to enforce this, some operators may overhang
		//    by a character or two.
		public int MaxLineLength
		{
			get;
			set;
		}

		// Enable/disable obfuscation of local symbols inside
		// function closures
		public bool NoObfuscate
		{
			get;
			set;
		}

		// Enable/disable replacement of consts variables
		public bool DetectConsts
		{
			get;
			set;
		}

		// Enable/disable formatted output
		//  - very rough formatting, just enough to be vaguely readable
		//    for diagnostic purposes
		public bool Formatted
		{
			get;
			set;
		}

		// Set to include diagnostic information about symbol obfuscation
		public bool SymbolInfo
		{
			get;
			set;
		}

		// Set to dump the abstract syntax tree to stdout
		public bool DumpAST
		{
			get;
			set;
		}

		// Set to dump scope information about all function scopes to stdout
		public bool DumpScopes
		{
			get;
			set;
		}

		// Set to an encoding for the output file
		//  - defaults to the same encoding as the first input file
		public Encoding OutputEncoding
		{
			get;
			set;
		}

		// Set the output file name
		//  - defaults to the name of the input file with `.js` removed
		//    and `.js.min` appended
		public string OutputFileName
		{
			get;
			set;
		}

		// Write to stdout instead of output file
		public bool StdOut
		{
			get;
			set;
		}

		// When true, don't include the "Minified by MiniME" credit comment
		public bool NoCredit
		{
			get;
			set;
		}

		// When true checks the timestamp of all input files
		// and only regenerates output if something changed
		public bool CheckFileTimes
		{
			get;
			set;
		}

		public bool UseOptionsFile
		{
			get;
			set;
		}

		public string CaptureOptions()
		{
			var buf = new StringBuilder();

			// Options
			buf.AppendFormat("linelen:{0}\n", MaxLineLength);
			buf.AppendFormat("no-obfuscate:{0}\n", NoObfuscate);
			buf.AppendFormat("detect-consts:{0}\n", DetectConsts);
			buf.AppendFormat("formatted:{0}\n", Formatted);
			buf.AppendFormat("diag-symbols:{0}\n", SymbolInfo);
			buf.AppendFormat("output-encoding:{0}\n", OutputEncoding==null ? "null" : OutputEncoding.ToString());

			// File list
			buf.Append("files:\n");
			foreach (var f in m_files)
			{
				buf.Append(f.filename);
				buf.Append(System.IO.Path.PathSeparator);
				buf.Append(f.encoding==null ? "null" : f.encoding.ToString());
			}

			return buf.ToString();
		}


		// Reset this compiler
		public void Reset()
		{
			m_files.Clear();
		}

		public int FileCount
		{
			get
			{
				return m_files.Count;
			}
		}

		public void RegisterResponseFile(string strFileName)
		{
			m_ResponseFiles.Add(strFileName);
		}

		// Add a file to be processed
		public void AddFiles(string strFileName, bool Warnings)
		{
			AddFile(strFileName, null, Warnings);
		}

		// Add a file to be processed (with explicit character encoding specified)
		public void AddFiles(string strFileName, System.Text.Encoding Encoding, bool Warnings)
		{
			// Work out directory
			string strDirectory=System.IO.Path.GetDirectoryName(strFileName);
			string strFile=System.IO.Path.GetFileName(strFileName);
			if (String.IsNullOrEmpty(strDirectory))
			{
				strDirectory = System.IO.Directory.GetCurrentDirectory();
			}
			else
			{
				strDirectory = System.IO.Path.GetFullPath(strDirectory);
			}

			// Wildcard?
			if (strFile.Contains('*') || strFile.Contains('?'))
			{
				var files=System.IO.Directory.GetFiles(strDirectory, strFile, SearchOption.TopDirectoryOnly);
				foreach (var f in files)
				{
					string strThisFile=System.IO.Path.Combine(strDirectory, f);

					if ((from fx in m_files where string.Compare(fx.filename, strThisFile, true) == 0 select fx).Count() > 0)
						continue;

					AddFile(strThisFile, Encoding, Warnings);
				}
			}
			else
			{
				AddFile(System.IO.Path.Combine(strDirectory, strFile), Encoding, Warnings);
			}
		}

		// Add a file to be processed
		public void AddFile(string strFileName, bool Warnings)
		{
			AddFile(strFileName, null, Warnings);
		}

		public void AddFile(string strFileName, System.Text.Encoding Encoding, bool Warnings)
		{
			// Work out auto file encoding
			if (Encoding == null)
			{
				EncodingInfo e = TextFileUtils.DetectFileEncoding(strFileName);
				if (e != null)
					Encoding=e.GetEncoding();
			}

			// Use same encoding for output
			if (OutputEncoding != null)
				OutputEncoding = Encoding;

			else
			{
				Encoding = Encoding.UTF8;
			}


			// Automatic output filename
			if (String.IsNullOrEmpty(OutputFileName))
			{
				int dotpos = strFileName.LastIndexOf('.');
				if (dotpos >= 0)
					OutputFileName = strFileName.Substring(0, dotpos);
				OutputFileName += ".min.js";
			}

			// Add file info
			var i = new FileInfo();
			i.filename = strFileName;
			i.content = File.ReadAllText(strFileName, Encoding);
			i.encoding = Encoding;
			i.warnings = Warnings;
			m_files.Add(i);
		}

		// Add Javascript code to be processed, direct from string
		public void AddScript(string strName, string strScript, bool Warnings)
		{
			var i = new FileInfo();
			i.filename = strName;
			i.content = strScript;
			i.encoding = Encoding.UTF8;
			i.warnings = Warnings;
			m_files.Add(i);
		}

		// Compile all loaded script to a string
		public string CompileToString()
		{
			// Create a symbol allocator
			SymbolAllocator SymbolAllocator = new SymbolAllocator(this);

			// Don't let the symbol allocator use any reserved words or common Javascript bits
			// We only go up to three letters - symbol allocation of more than 3 letters is 
			// highly unlikely.
			// (based on list here: http://www.quackit.com/javascript/javascript_reserved_words.cfm)
			string[] words = new string[] { "if", "in", "do", "for", "new", "var", "int", "try", "NaN", "ref", "sun", "top" };
			foreach (var s in words)
			{
				SymbolAllocator.ClaimSymbol(s);
			}

			// Create a member allocator
			SymbolAllocator MemberAllocator = new SymbolAllocator(this);

			// Render
			RenderContext r = new RenderContext(this, SymbolAllocator, MemberAllocator);
	
			// Process all files
			bool bNeedSemicolon = false;
			foreach (var file in m_files)
			{
				Console.WriteLine("Processing {0}...", System.IO.Path.GetFileName(file.filename));

				// Create a tokenizer and parser
				Tokenizer t = new Tokenizer(file.content, file.filename, file.warnings);
				Parser p = new Parser(t);

				// Create the global statement block
				var code = new ast.CodeBlock(null, TriState.No);

				// Parse the file into a namespace
				p.ParseStatements(code);

				// Ensure everything processed
				if (t.more)
				{
					throw new CompileError("Unexpected end of file", t);
				}


				// Dump the abstract syntax tree
				if (DumpAST)
					code.Dump(0);

				// Create the root symbol scope and build scopes for all 
				// constained function scopes
				SymbolScope rootScope = new SymbolScope(null, Accessibility.Public);
				code.Visit(new VisitorScopeBuilder(rootScope));

				// Combine consecutive var declarations into a single one
				code.Visit(new VisitorCombineVarDecl(rootScope));

				// Find all variable declarations
				code.Visit(new VisitorSymbolDeclaration(rootScope));

				// Do lint stuff
				code.Visit(new VisitorLint(rootScope));

				// Try to eliminate const declarations
				if (DetectConsts && !NoObfuscate)
				{
					code.Visit(new VisitorConstDetectorPass1(rootScope));
					code.Visit(new VisitorConstDetectorPass2(rootScope));
					code.Visit(new VisitorConstDetectorPass3(rootScope));
				}

				// Simplify expressions
				code.Visit(new VisitorSimplifyExpressions());

				// If obfuscation is allowed, find all in-scope symbols and then
				// count the frequency of their use.
				if (!NoObfuscate)
				{
					code.Visit(new VisitorSymbolUsage(rootScope));
				}

				// Process all symbol scopes, applying default accessibility levels
				// and determining the "rank" of each symbol
				rootScope.Prepare();

				// Dump scopes to stdout
				if (DumpScopes)
					rootScope.Dump(0);

				// Tell the global scope to claim all locally defined symbols
				// so they're not re-used (and therefore hidden) by the 
				// symbol allocation
				rootScope.ClaimSymbols(SymbolAllocator);

				// Create a credit comment on the first file
				if (!NoCredit && file==m_files[0])
				{
					int iInsertPos = 0;
					while (iInsertPos < code.Content.Count && code.Content[iInsertPos].GetType() == typeof(ast.StatementComment))
						iInsertPos++;
					code.Content.Insert(iInsertPos, new ast.StatementComment(null, "// Minified by MiniME from toptensoftware.com"));
				}

				if (bNeedSemicolon)
				{
					r.Append(";");
				}

				// Render it
				r.EnterScope(rootScope);
				bNeedSemicolon=code.Render(r);
				r.LeaveScope();
			}

			// return the final script
			string strResult = r.GetGeneratedOutput();
			return strResult;
		}

		// Compile all loaded files and write to the output file
		public void Compile()
		{
			string OptionsFile = OutputFileName + ".minime-options";

			if (!StdOut && CheckFileTimes && File.Exists(OutputFileName))
			{
				// Get the timestamp of the output file
				var dtOutput=System.IO.File.GetLastWriteTimeUtc(OutputFileName);

				// Compare with the timestamp of all the input files
				bool bNeedCompile=false;
				foreach (var f in m_files)
				{
					if (System.IO.File.GetLastWriteTimeUtc(f.filename) > dtOutput)
					{
						bNeedCompile = true;
						break;
					}
				}

				// Also check timestamp of any response files used
				if (!bNeedCompile)
				{
					foreach (var f in m_ResponseFiles)
					{
						if (System.IO.File.GetLastWriteTimeUtc(f)> dtOutput)
						{
							bNeedCompile = true;
							break;
						}
					}
				}

				// Also check if any options have changed
				if (!bNeedCompile && UseOptionsFile)
				{
					if (File.Exists(OptionsFile))
					{
						string oldOptions = File.ReadAllText(OptionsFile, Encoding.UTF8);
						bNeedCompile = oldOptions != CaptureOptions();
					}
					else
					{
						bNeedCompile = true;
					}
				}

				if (!bNeedCompile)
				{
					Console.WriteLine("Nothing Changed");
					return;
				}
			}

			// Compile
			string str = CompileToString();

			// StdOut?
			if (StdOut)
			{
				Console.Write(str);
				Console.WriteLine("");
				return;
			}

			// Write
			if (OutputEncoding!=null)
			{
				System.IO.File.WriteAllText(OutputFileName, str, OutputEncoding);
			}
			else
			{
				System.IO.File.WriteAllText(OutputFileName, str);
			}

			// Save options
			if (UseOptionsFile)
			{
				if (CheckFileTimes)
				{
					// Save options
					File.WriteAllText(OptionsFile, CaptureOptions(), Encoding.UTF8);
				}
				else
				{
					// Delete an old options file
					if (File.Exists(OptionsFile))
						File.Delete(OptionsFile);
				}
			}
		}

		// Stores information about a file to be processed
		class FileInfo
		{
			public string filename;
			public string content;
			public Encoding encoding;
			public bool warnings;
		}

	}
}
