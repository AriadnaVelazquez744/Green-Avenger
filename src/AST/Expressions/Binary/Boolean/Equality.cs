public class Equality : BinaryExpression
{
    // Se encarga de establecer la relacion de igualdad entre dos elementos del mismo tipo
    public Equality(CodeLocation location) : base(location) { }

    public override ExpressionType Type { get; set; }
    public override object? Value { get; set; }
    public string? Op { get; set; }

    public override bool CheckSemantic(Context context, Scope scope, List<CompilingError> errors)
    {
        bool right = Right!.CheckSemantic(context, scope, errors);
        bool left = Left!.CheckSemantic(context, scope, errors);

        if (!Right.Type.Equals(Left.Type) && (Right.Type is not ExpressionType.Undeclared || Left.Type is not ExpressionType.Undeclared))
        {
            errors.Add(new CompilingError(Location, ErrorCode.Invalid, "It's impossible to equal different types of elements"));
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

        bool eq = Left.Value!.Equals(Right.Value!);
        
        if (Op == "==")
            Value = eq;
        else
            Value = !eq;
    }

    public override string ToString()
    {
        if (Value == null)
            return String.Format("({0} {1} {2})", Left, Op, Right);
        return Value.ToString()!;
    }
}