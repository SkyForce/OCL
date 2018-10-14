using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCL
{
  class Program
  {
    static void Main(string[] args)
    {
      String input = @"package P
context A
inv:
let fact(n: var) = if n = 1 then n else n * fact(n - 1) endif;
let n = 5;
fact(n+1) = 720
inv:
let a = 6;
let b = 6;
a = b
endpackage";
      ICharStream stream = CharStreams.fromstring(input);
      ITokenSource lexer = new HelloLexer(stream);
      ITokenStream tokens = new CommonTokenStream(lexer);
      HelloParser parser = new HelloParser(tokens);
      parser.BuildParseTree = true;
      IParseTree tree = parser.oclFile();
      HelloPrinter printer = new HelloPrinter();
      //ParseTreeWalker.Default.Walk(printer, tree);
      tree.Accept(printer);
      Console.ReadLine();
    }
  }
}
