public class BoolLiteral : BooleanExpression
{
    public BoolLiteral(CodeLocation location) : base(location) { }

    public override ExpressionType Type { get; set; }
    public override object? Value { get; set; }

    public override void Evaluate()
    {
        Left!.Evaluate();

        bool element = Left.Equals("true");

        Value = element;
    }

    public override string ToString()
    {
        if (Value == null)
        {
            return String.Format("({0})", Left);
        }
        return Value.ToString()!;
    }
}