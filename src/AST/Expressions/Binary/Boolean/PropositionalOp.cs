public class PropOp : BinaryExpression
{
    // Determina un valor boleano en referencia a la veracidad de ambos miembros dependiendo del operador que se utilice
    public PropOp(CodeLocation location) : base(location) { }

    public override ExpressionType Type { get; set; }
    public override object? Value { get; set; }
    public string? Op { get; set; }

    public override bool CheckSemantic(Context context, Scope scope, List<CompilingError> errors)
    {
        bool right = Right!.CheckSemantic(context, scope, errors);
        bool left = Left!.CheckSemantic(context, scope, errors);

        if ((Right.Type != ExpressionType.Boolean && Right.Type != ExpressionType.Undeclared) || (Left.Type != ExpressionType.Boolean && Left.Type != ExpressionType.Undeclared))
        {
            errors.Add(new CompilingError(Location, ErrorCode.Invalid, "We don't do that here... "));
            Type = ExpressionType.ErrorType;
            return false;
        }

        if (Right is Var varR && Right.Type == ExpressionType.Undeclared)
        {
            scope.FuncVars[varR.Id].Type = ExpressionType.Boolean;
        }
        if (Left is Var varL && Left.Type == ExpressionType.Undeclared)
        {
            scope.FuncVars[varL.Id].Type = ExpressionType.Boolean;
        }
        Right.Type = Left.Type = ExpressionType.Boolean;


        Type = ExpressionType.Boolean;
        return right && left;
    }

    public override void Evaluate(Context context, Scope scope)
    {
        Right!.Evaluate(context, scope);
        Left!.Evaluate(context, scope);

        bool left = Left.Value!.Equals(true);
        bool right = Right.Value!.Equals(true);

        if (Op == "&")
            Value = left && right;
        else
            Value = left || right;
    }

    public override string ToString()
    {
        if (Value == null)
            return String.Format("({0} {1} {2})", Left, Op, Right);
        return Value.ToString()!;
    }
}