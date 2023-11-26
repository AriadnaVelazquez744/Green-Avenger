public abstract class AtomExpression : Expression
{
    public AtomExpression(CodeLocation location) : base(location) { }

    public override bool CheckSemantic(Context context, Scope scope, List<CompilingError> errors) => true;

    public override void Evaluate(Context context, Scope scope) { }

    public override string ToString() => String.Format("{0}", Value);

}