public class Parser
{
    public Parser(TokenStream stream)
    {
        Stream = stream;
        this.Context = new();
        this.Scope = new();
    }

    public TokenStream Stream { get; set; }
    public Context Context { get; set; }
    public Scope Scope { get; set; }

    public void Parse(List<CompilingError> errors)
    {
        Token currenToken = Stream.LookAhead(0);

        List<object> exp = new();

        if (Context.ElementalFunctions.Contains(currenToken.Value))
        {
            object value = ParseElementalFunction(errors, currenToken);
            if (value == null)
                errors.Add(new CompilingError(currenToken.Location, ErrorCode.Invalid, String.Format("The function {0} is wrong declared", currenToken.Value)));

            exp.Add(value!);
        }
    }

    public void ParseFunction(List<CompilingError> errors)
    {
        throw new NotImplementedException();
    }

    public void ParseVariable(List<CompilingError> errors) 
    {
        throw new NotImplementedException();
    }

    private ASTNode ParsePrint(List<CompilingError> errors)
    {
        if (!Stream.Next(TokenValues.OpenBracket))
        {
            errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Expected, "( expected"));
            return null!;
        }

        Expression exp = ParseExpression()!;
        if (exp == null)
        {
            errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Expected, "expression expected"));
            return null!;
        }

        if (!Stream.Next(TokenValues.ClosedBracket))
        {
            errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Expected, ") expected"));
            return null!;
        }

        return new Print(exp, Stream.LookAhead().Location);
    }

    public object ParseElementalFunction(List<CompilingError> errors, Token Id)
    {
        ElementalFunction func = new();

        string id = Id.Value.ToString();

        foreach (var item in func.Constants)
        {
            if (item.Key == id) return func.Constants[id]();
        }

        if (!Stream.CanLookAhead(0)) return null!;

        if (!Stream.Next(TokenValues.OpenBracket))
        {
            errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Expected, "( expected"));
            return null!;
        }

        if (!Stream.Next(TokenType.Number))
        {
            errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Expected, "argument expected"));
            return null!;
        }
        if (!Stream.Next(TokenValues.ClosedBracket) || !Stream.Next(TokenValues.ValueSeparator))
        {
            errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Expected, ") expected"));
        }
        string argument = Stream.LookAhead().Value;
        double arg1 = double.Parse(argument);

        foreach (var item in func.MathFunction)
        {
            if (item.Key == id)
                return func.MathFunction[id](arg1);
        }

        if (Stream.Next(TokenValues.ClosedBracket)) 
        {
            if (Id.Value.Equals("log"))
                errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Expected, "second argument value was expected for this function"));

            return null!;
        }
        if (!Stream.Next(TokenValues.ValueSeparator))
        {
            errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Expected, ", expected"));
        }
        if (!Stream.Next(TokenType.Number))
        {
            errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Expected, "second argument expected"));
            return null!;
        }
        if (!Stream.Next(TokenValues.ClosedBracket))
        {
            errors.Add( new CompilingError(Stream.LookAhead().Location, ErrorCode.Expected, ") expected"));
        }
        argument = Stream.LookAhead().Value;
        double arg2 = double.Parse(argument);
        
        return func.Log[id](arg1, arg2);
    }

    private Expression? ParseConcat(List<CompilingError> errors, Token text)
    {
        string left = text.Value;

        Expression? Right = ParseText()!;
        if (Right == null)
        {
            errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Expected, "Expected text for the concat operation"));
            Stream.MoveBack(2);
            return null;
        }
        string right = Right.Value!.ToString()!;

        return new Text(left + right, Stream.LookAhead().Location);
    }

    private Expression? ParseExpression()
    {
        return ParseExpressionLv1();
    }
    private Expression? ParseExpressionLv0()
    {
        if (Stream.Next(TokenValues.OpenBracket))
        {
            Expression? exp = ParseExpression();
            
            if (exp == null)
                return null;
            if (!Stream.Next(TokenValues.ClosedBracket))
                return null;
            
            return exp;
        }
        return ParseExpressionLv4();
    }
    private Expression? ParseExpressionLv1()
    {
        Expression? newLeft = ParseExpressionLv0();
        Expression? exp = ParseExpressionLv1_(newLeft);
        return exp;
    }
    private Expression? ParseExpressionLv1_(Expression? left)
    {
        Expression? exp = ParseAdd(left);
        if (exp != null) return exp;

        exp = ParseSub(left);
        if (exp != null) return exp;

        return left;
    }
    private Expression? ParseExpressionLv2()
    {
        Expression? newLeft = ParseExpressionLv1();
        return ParseExpressionLv2_(newLeft);
    }
    private Expression? ParseExpressionLv2_(Expression? left)
    {
        Expression? exp = ParseMul(left);
        if (exp != null) return exp;

        exp = ParseDiv(left);
        if (exp != null) return exp;

        return left;
    }
    private Expression? ParseExpressionLv3()
    {
        Expression? newLeft = ParseExpressionLv2();
        return ParseExpressionLv3_(newLeft);
    }
    private Expression? ParseExpressionLv3_(Expression? left)
    {
        Expression? exp = ParsePow(left);
        if (exp != null) return exp;

        return left;
    }
    private Expression? ParseExpressionLv4()
    {
        Expression? exp = ParseNumber();
        if (exp != null) return exp;

        exp = ParseText();
        if (exp != null) return exp;

        exp = ParseBooleanExpression();
        if (exp != null) return exp;

        return null;
    }


    private Expression? ParseAdd(Expression? left)
    {
        Add sum = new(Stream.LookAhead().Location);

        if (left == null || !Stream.Next(TokenValues.Add)) return null;

        sum.Left = left;

        Expression? right = ParseExpressionLv1();
        if (right == null)
        {
            Stream.MoveBack(2);
            return null;
        }
        sum.Right = right;

        return ParseExpressionLv1_(sum);
    }
    private Expression? ParseSub(Expression? left)
    {
        Sub sub = new(Stream.LookAhead().Location);

        if (left == null || !Stream.Next(TokenValues.Sub)) return null;

        sub.Left = left;

        Expression? right = ParseExpressionLv1();
        if (right == null)
        {
            Stream.MoveBack(2);
            return null;
        }
        sub.Right = right;

        return ParseExpressionLv1_(sub);
    }
    private Expression? ParseMul(Expression? left)
    {
        Mul mul = new(Stream.LookAhead().Location);

        if (left is null || !Stream.Next(TokenValues.Mul)) return null;

        mul.Left = left;

        Expression? right = ParseExpressionLv2();
        if (right == null)
        {
            Stream.MoveBack(2);
            return null;
        }
        mul.Right = right;

        return ParseExpressionLv2_(mul);
    }

    private Expression? ParseDiv(Expression? left)
    {
        Div div = new(Stream.LookAhead().Location);

        if (left is null || !Stream.Next(TokenValues.Div)) return null;

        div.Left = left;

        Expression? right = ParseExpressionLv2();
        if (right == null)
        {
            Stream.MoveBack(2);
            return null;   
        }
        div.Right = right;

        return ParseExpressionLv2_(div);
    }
    private Expression? ParsePow(Expression? left)
    {
        Pow pow = new(Stream.LookAhead().Location);

        if (left is null || !Stream.Next(TokenValues.Pow)) return null;

        pow.Left = left;

        Expression? right = ParseExpressionLv3();
        if (right == null)
        {
            Stream.MoveBack(2);
            return null;
        }
        pow.Right = right;

        return ParseExpressionLv3_(pow);
    }

    private Expression? ParseNumber()
    {
        if (!Stream.Next(TokenType.Number)) return null;
    
        return new Number(double.Parse(Stream.LookAhead().Value), Stream.LookAhead().Location);
    }
    private Expression? ParseText()
    {
        if (!Stream.Next(TokenType.Text)) return null;

        return new Text(Stream.LookAhead().Value, Stream.LookAhead().Location);
    }

    private BooleanExpression? ParseBooleanExpression()
    {
        throw new NotImplementedException();
    }
}