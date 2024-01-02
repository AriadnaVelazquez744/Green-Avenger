public class BooleanOp : BinaryExpression
{
    // Hace los calculos booleanos de comparacion no igualitaria entre dos elementos del mismo tipo.
    public BooleanOp(CodeLocation location) : base(location) { }

    public override  ExpressionType Type { get; set; }
    public override object? Value { get; set; }
    public string? Op { get; set; }

    public override bool CheckSemantic(Context context, Scope scope, List<CompilingError> error)
    {
        bool right = Right!.CheckSemantic(context, scope, error);
        bool left = Left!.CheckSemantic(context, scope, error);

        if ((Right.Type != ExpressionType.Number && Right.Type is not ExpressionType.Undeclared) || (Left.Type != ExpressionType.Number && Left.Type is not ExpressionType.Undeclared))
        {
            error.Add(new CompilingError(Location, ErrorCode.Invalid, "We don't do that here... "));
            Type = ExpressionType.ErrorType;
            return false;
        }

        if (Right is Var varR && Right.Type == ExpressionType.Undeclared)
        {
            scope.FuncVars[varR.Id].Type = ExpressionType.Number;
        }
        if (Left is Var varL && Left.Type == ExpressionType.Undeclared)
        {
            scope.FuncVars[varL.Id].Type = ExpressionType.Number;
        }
        Right.Type = Left.Type = ExpressionType.Number;


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