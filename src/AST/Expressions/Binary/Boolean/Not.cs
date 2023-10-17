public class Not : BooleanExpression
{
    public Not(CodeLocation location) : base(location) { }

    public override ExpressionType Type { get; set; }
    public override object? Value { get; set; }

    public override void Evaluate()
    {
        Right!.Evaluate();

        bool right = Right.Equals(true);

        Value = !right;
    }

    public override string ToString()
    {
        if (Value == null)
            return String.Format("!{0}", Right);
        return Value.ToString()!;
    }
}