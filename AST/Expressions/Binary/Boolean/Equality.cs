public class Equal : BooleanExpression
{
    public Equal(CodeLocation location) : base(location) { }

    public override ExpressionType Type { get; set; }
    public override object? Value { get; set; }

    public override void Evaluate()
    {
        Right!.Evaluate();
        Left!.Evaluate();

        Value = Left.Equals(Right);
    }

    public override string ToString()
    {
        if (Value == null)
            return String.Format("({0} != {1})", Left, Right);
        return Value.ToString()!;
    }
}

public class Different : BooleanExpression
{
    public Different(CodeLocation location) : base(location) { }

    public override ExpressionType Type { get; set; }
    public override object? Value { get; set; }

    public override void Evaluate()
    {
        Right!.Evaluate();
        Left!.Evaluate();

        Value = !Left.Equals(Right);
    }

    public override string ToString()
    {
        if (Value == null)
            return String.Format("({0} == {1})", Left, Right);
        return Value.ToString()!;
    }
}