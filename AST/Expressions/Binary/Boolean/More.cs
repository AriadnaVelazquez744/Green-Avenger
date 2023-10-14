public class More : BooleanExpression
{
    public More(CodeLocation location) : base(location) { }

    public override ExpressionType Type { get; set; }
    public override object? Value { get; set; }

    public override void Evaluate()
    {
        Right!.Evaluate();
        Left!.Evaluate();

        if (double.TryParse(Left.Value?.ToString(), out double leftValue) && double.TryParse(Right.Value?.ToString(), out double rightValue))
        {
            Value = leftValue > rightValue;
        }
    }

    public override string ToString()
    {
        if (Value == null)
        {
            return String.Format("({0} > {1})", Left, Right);
        }
        return Value.ToString()!;
    }
}


public class MoreOrEqual : BooleanExpression
{
    public MoreOrEqual(CodeLocation location) : base(location) { }

    public override ExpressionType Type { get; set; }
    public override object? Value { get; set; }

    public override void Evaluate()
    {
        Right!.Evaluate();
        Left!.Evaluate();

        if (double.TryParse(Left.Value?.ToString(), out double leftValue) && double.TryParse(Right.Value?.ToString(), out double rightValue))
        {
            Value = leftValue >= rightValue;
        }
    }

    public override string ToString()
    {
        if (Value == null)
        {
            return String.Format("({0} >= {1})", Left, Right);
        }
        return Value.ToString()!;
    }
}
