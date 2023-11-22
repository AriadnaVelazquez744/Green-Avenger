public class Var : AtomExpression
{
    public Var(string id, Scope scope, CodeLocation location) : base(location)
    {
        Id = id;
        Scope = scope;
    }

    public override object? Value { get; set; }
    public string Id { get; }
    public Scope Scope { get; }

    public override ExpressionType Type
    {
        get => ExpressionType.Identifier;
        set { }
    }

    public override bool CheckSemantic(Context context, Scope scope, List<CompilingError> errors)
    {
        if (!scope.ContainsVariable(Id))
        {
            errors.Add(new CompilingError(Location, ErrorCode.Invalid, String.Format("The variable {0} has not been declared in this space", Id)));
            return false;
        }
        return true;
    }

    public override void Evaluate()
    {
        Expression exp = Scope.Variables[Id];
        exp.Evaluate();
        Value = exp.Value!;
    }
}