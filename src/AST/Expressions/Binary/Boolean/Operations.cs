public class BooleanOp : BinaryExpression
{
    public BooleanOp(CodeLocation location) : base(location) { }

    public override  ExpressionType Type { get; set; }
    public override object? Value { get; set; }
    public string? Op { get; set; }

    public override bool CheckSemantic(Context context, Scope scope, List<CompilingError> errors)
    {
        bool right = Right!.CheckSemantic(context, scope, errors);
        bool left = Left!.CheckSemantic(context, scope, errors);

        if (Right.Type != ExpressionType.Number || Left.Type != ExpressionType.Number)
        {
            errors.Add(new CompilingError(Location, ErrorCode.Invalid, "We don't do that here... "));
            Type = ExpressionType.ErrorType;
            return false;
        }

        Type = ExpressionType.Boolean;
        return right && left;
    }
    public override void Evaluate(Context context, Scope scope)
    {
        Right!.Evaluate(context, scope);
        Left!.Evaluate(context, scope);

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