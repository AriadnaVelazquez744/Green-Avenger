public class Parser
{
    public Parser(TokenStream stream)
    {
        Stream = stream;
    }

    public TokenStream Stream { get; set; }

    private Expression? ParseExpression()
    {
        return ParseExpressionLv1();
    }
    private Expression? ParseExpressionLv1()
    {
        Expression? newLeft = ParseExpressionLv2();
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
        Expression? newLeft = ParseExpressionLv3();
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
        Expression? newLeft = ParseExpressionLv4();
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

        return null;
    }


    private Expression? ParseAdd(Expression? left)
    {
        Add sum = new(Stream.LookAhead().Location);

        if (left == null || !Stream.Next(TokenValues.Add)) return null;

        sum.Left = left;

        Expression? right = ParseExpressionLv2();
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

        Expression? right = ParseExpressionLv2();
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

        Expression? right = ParseExpressionLv3();
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

        Expression? right = ParseExpressionLv3();
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

        Expression? right = ParseExpressionLv4();
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

        return new Text(Stream.LookAhead().Value, Stream.LookAhead(),Location);
    }

}