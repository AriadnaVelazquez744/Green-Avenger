public class Print : Expression
{
    //Deriva de la clase Exxpression ya que puede ser utilizado en la funcionalidad de cualquier otro tipo de estructura
    //Recibe unicamente una expresion cuyo valor ha de imprimir en consola y su tipo se determina en dependencia del tipo
    //de la expresion que contiene.
    public Expression? Expression { get; set; }
    public override ExpressionType Type { get; set; }
    public override object? Value { get; set; }
    public Print(Expression expression, CodeLocation location) : base(location)
    {
        Expression = expression;
    }

    public override bool CheckSemantic(Context context, Scope scope, List<CompilingError> errors)
    {
        if (Expression is null)
        {
            errors.Add(new CompilingError(Location, ErrorCode.Invalid, "The expression inside Print can't be null"));
            return false;
        }

        bool check = Expression!.CheckSemantic(context, scope, errors);
        Type = Expression.Type;
        return check;
    }

    public override void Evaluate(Context context, Scope scope)
    {
        Expression!.Evaluate(context, scope);
        Value = Expression.Value; 
    }
}