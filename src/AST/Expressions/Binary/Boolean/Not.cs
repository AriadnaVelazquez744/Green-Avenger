public class Not : Expression
{
    // Evalua la expresion que va a modificar y una vez obtenido el valor de esta inviente su resultado y es lo que devuelve.
    public override ExpressionType Type { get; set; }
    public override object? Value { get; set; }
    public Expression? Expression { get; set; }
    public Not(Expression expression, CodeLocation location) : base(location) 
    { 
        Expression = expression;
    }

    public override bool CheckSemantic(Context context, Scope scope, List<CompilingError> errors)
    {
        if (Expression is null)
        {
            errors.Add(new CompilingError(Location, ErrorCode.Invalid, "The expression that fallows \"!\" can't be null"));
            return false;
        }
        
        if (!(Expression.Type == ExpressionType.Text || Expression.Type == ExpressionType.Number || Expression.Type == ExpressionType.Boolean || Expression.Type == ExpressionType.Undeclared))
        {
            errors.Add(new CompilingError(Location, ErrorCode.Invalid, "That's not a correct boolean declaration"));
            Type = ExpressionType.ErrorType;
            return false;
        }

        if (Expression is Var varL && Expression.Type == ExpressionType.Undeclared)
        {
            scope.FuncVars[varL.Id].Type = ExpressionType.Boolean;
        }
        Expression.Type = ExpressionType.Boolean;

        bool exp = Expression!.CheckSemantic(context, scope, errors);
        Type = ExpressionType.Boolean;
        return exp;
    }

    public override void Evaluate(Context context, Scope scope)
    {
        Expression!.Evaluate(context, scope);
        
        bool exp = Convert.ToBoolean(Expression.Value);

        Value = !exp;
    }

    public override string ToString()
    {
        if (Value == null)
            return String.Format("(!{0})", Expression);
        return Value.ToString()!;
    }
}