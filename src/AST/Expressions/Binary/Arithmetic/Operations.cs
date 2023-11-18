public class BooleanOp : BinaryExpression
{
    public BooleanOp(CodeLocation location) : base(location) { }

    public override  ExpressionType Type { get; set; }
    public override object? Value { get; set; }
    public string Op { get; set; }

    public override void Evaluate()
    {
        Right!.Evaluate();
        Left!.Evaluate();

        if (double.TryParse(Left.Value?.ToString(), out double leftValue) && double.TryParse(Right.Value?.ToString(), out double rightValue))
        {
            switch (Op)
            {
                case "<":
                    Value = leftValue < rightValue;
                    break;
                case "<=":
                    Value = leftValue <= rightValue;
                    break;
                case ">":
                    Value = leftValue > rightValue;
                    break;
                case ">=":
                    Value = leftValue >= rightValue;
                    break;
            }
        }
    }

    public override string ToString()
    {
        if (Value == null)
        {
            return String.Format("({0} {1} {2})", Left, Op, Right);
        }
        return Value.ToString()!;
    }

}