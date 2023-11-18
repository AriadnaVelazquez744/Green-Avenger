public class Not1 : Expression
{
    public override ExpressionType Type { get; set; }
    public override object? Value { get; set; }
    public Expression? Expression { get; set; }
    public Not1(Expression expression, CodeLocation location) : base(location) 
    { 
        Expression = expression;
    }

    public override bool CheckSemantic(Context context, Scope scope, List<CompilingError> errors)
    {
        bool exp = Expression!.CheckSemantic(context, scope, errors);

        if (!(Expression.Type == ExpressionType.Text || Expression.Type == ExpressionType.Number || Expression.Type == ExpressionType.Boolean))
        {
            errors.Add(new CompilingError(Location, ErrorCode.Invalid, "That's not a correct boolean declaration"));
            Type = ExpressionType.ErrorType;
            return false;
        }

        Type = ExpressionType.Boolean;
        return exp;
    }

    public override void Evaluate()
    {
        Expression!.Evaluate();
        
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