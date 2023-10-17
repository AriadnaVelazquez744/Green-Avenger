public class Sub :BinaryExpression
{
    public Sub(CodeLocation location) : base(location) { }

    public override ExpressionType Type { get; set; }
    public override object? Value { get; set; }

    public override void Evaluate()
    {
        Left!.Evaluate();
        Right!.Evaluate();

        Value = (double)Left.Value! - (double)Right.Value!;
    }

    public override string? ToString()
    {
        if (Value == null)
            return String.Format("({0} - {1})", Left, Right);

        return Value!.ToString();
    }
}