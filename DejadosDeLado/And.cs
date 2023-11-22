public class And : BooleanExpression
{
    public And(CodeLocation location) : base(location) { }

    public override ExpressionType Type { get; set; }
    public override object? Value { get; set; }

    public override void Evaluate()
    {
        Right!.Evaluate();
        Left!.Evaluate();

        bool left = Left.Equals(true);
        bool right = Right.Equals(true);

        Value = left && right;
    }

    public override string ToString()
    {
        if (Value == null)
            return String.Format("({0} & {1})", Left, Right);
        return Value.ToString()!;
    }
}