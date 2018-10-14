using Antlr4.Runtime.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCL
{
  class HelloPrinter : HelloBaseVisitor<bool>
  {
    ArrayList<Dictionary<string, string>> vars;
    HelloCalc calc;
    Dictionary<string, FunctionDef> funcs;
    public HelloPrinter() {
      vars = new ArrayList<Dictionary<string, string>>();
      vars.Add(new Dictionary<string, string>());
      funcs = new Dictionary<string, FunctionDef>();
      calc = new HelloCalc(vars, funcs, this);
    }
    public override bool VisitOclExpression([NotNull] HelloParser.OclExpressionContext context)
    {
      for(int i = 0; i < context.letExpression().Length; i++)
      {
        VisitLetExpression(context.letExpression()[i]);
      }
      if (!VisitExpression(context.expression()))
      {
        Console.WriteLine("err");
      }
      else
      {
        Console.WriteLine("ok");
      }
      return true;
    }
    public override bool VisitLetExpression([NotNull] HelloParser.LetExpressionContext context)
    {
      if(context.formalParameterList() != null)
        funcs[context.name().GetText()] = new FunctionDef { param = context.formalParameterList().name().Select(x => x.GetText()).ToList(), context = context.expression() };
      else
        vars[vars.Count - 1][context.name().GetText()] = context.expression().GetText();
      
      return true;
    }

    public override bool VisitRelationalExpression([NotNull] HelloParser.RelationalExpressionContext context)
    {
      switch (context.relationalOperator().Start.Text)
      {
        case "=":
          return calc.VisitAdditiveExpression(context.additiveExpression()[0]) == calc.VisitAdditiveExpression(context.additiveExpression()[1]);
      }
      return true;
    }

    class HelloCalc : HelloBaseVisitor<double>
    {
      ArrayList<Dictionary<string, string>> vars = null;
      Dictionary<string, FunctionDef> funcs = null;
      HelloPrinter hp = null;
      public HelloCalc(ArrayList<Dictionary<string, string>> vars, Dictionary<string, FunctionDef> funcs, HelloPrinter hp)
      {
        this.vars = vars;
        this.funcs = funcs;
        this.hp = hp;
      }
      public override double VisitAdditiveExpression([NotNull] HelloParser.AdditiveExpressionContext context)
      {
        double startAdd = VisitMultiplicativeExpression(context.multiplicativeExpression()[0]);
        for (int i = 1; i < context.multiplicativeExpression().Length; i++)
        {
          switch(context.addOperator()[i-1].Start.Text)
          {
            case "+":
              startAdd += VisitMultiplicativeExpression(context.multiplicativeExpression()[i]);
              break;
            case "-":
              startAdd -= VisitMultiplicativeExpression(context.multiplicativeExpression()[i]);
              break;
          }
        }
        return startAdd;
      }

      public override double VisitMultiplicativeExpression([NotNull] HelloParser.MultiplicativeExpressionContext context)
      {
        double startMul = VisitUnaryExpression(context.unaryExpression()[0]);
        for (int i = 1; i < context.unaryExpression().Length; i++)
        {
          switch (context.multiplyOperator()[i - 1].Start.Text)
          {
            case "*":
              startMul *= VisitUnaryExpression(context.unaryExpression()[i]);
              break;
            case "/":
              startMul /= VisitUnaryExpression(context.unaryExpression()[i]);
              break;
          }
        }
        return startMul;
      }

      public override double VisitUnaryExpression([NotNull] HelloParser.UnaryExpressionContext context)
      {
        double postfix = VisitPostfixExpression(context.postfixExpression());
        if(context.unaryOperator() != null)
        {
          switch (context.unaryOperator().Start.Text)
          {
            case "-":
              postfix = -postfix;
              break;
          }
        }
        return postfix;
      }

      public override double VisitPostfixExpression([NotNull] HelloParser.PostfixExpressionContext context)
      {
        double primary = VisitPrimaryExpression(context.primaryExpression());

        return primary;
      }

      public override double VisitPrimaryExpression([NotNull] HelloParser.PrimaryExpressionContext context)
      {
        double res = 0;
        if(context.literal() != null)
        {
          res = VisitLiteral(context.literal());
        }
        else if(context.propertyCall() != null)
        {
          res = VisitPropertyCall(context.propertyCall());
        }
        else if(context.ifExpression() != null)
        {
          res = VisitIfExpression(context.ifExpression());
        }

        return res;
      }

      public override double VisitLiteral([NotNull] HelloParser.LiteralContext context)
      {
        double num = 0;
        if (context.number() != null)
        {
          num = Double.Parse(context.number().GetText());
        }

        return num;
      }

      public override double VisitPropertyCall([NotNull] HelloParser.PropertyCallContext context)
      {
        if(context.propertyCallParameters() != null)
        {
          Dictionary<string, string> stack = new Dictionary<string, string>();
          List<string> names = funcs[context.pathName().GetText()].param;
          HelloParser.ExpressionContext contextFunc = funcs[context.pathName().GetText()].context;
          for (int i = 0; i < names.Count; i++)
          {
            stack[names[i]] = VisitExpression(context.propertyCallParameters().actualParameterList().expression()[i]).ToString();
          }
          vars.Add(stack);
          double res = VisitExpression(contextFunc);
          vars.RemoveAt(vars.Count - 1);
          return res;
        }
        return VisitPathName(context.pathName());
      }
      public override double VisitPathName([NotNull] HelloParser.PathNameContext context)
      {
        for(int i = vars.Count - 1; i >= 0; i--)
        {
          if(vars[i].ContainsKey(context.name()[0].GetText()))
          {
            return Double.Parse(vars[i][context.name()[0].GetText()]);
          }
        }
        return 0;
      }
      public override double VisitIfExpression([NotNull] HelloParser.IfExpressionContext context)
      {
        if(hp.VisitExpression(context.expression()[0]))
        {
          return VisitExpression(context.expression()[1]);
        }
        else
        {
          return VisitExpression(context.expression()[2]);
        }
      }
    }
  }
}
