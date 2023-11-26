public class Var : AtomExpression
{
    public Var(string id, CodeLocation location) : base(location)
    {
        Id = id;
    }

    public override object? Value { get; set; }
    public string Id { get; }
    public override ExpressionType Type { get; set; }

    public override bool CheckSemantic(Context context, Scope scope, List<CompilingError> errors)
    {
        if (!scope.ContainsVariable(Id))
        {
            errors.Add(new CompilingError(Location, ErrorCode.Invalid, String.Format("The variable {0} has not been declared in this space", Id)));
            return false;
        }

        Expression exp = scope.Variables[Id];
        Type = exp.Type;
        return true;
    }

    public override void Evaluate(Context context, Scope scope)
    {
        Expression exp = scope.Variables[Id];
        exp.Evaluate(context, scope);
        Value = exp.Value!;
    }
}