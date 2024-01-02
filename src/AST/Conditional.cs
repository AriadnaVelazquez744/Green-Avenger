
public class Conditional : Expression
{
    //Contiene una expression que es la condicional, y cada parte (en caso de que contenga el else) tiene su propio cuerpo de expresiones a ejecutar que no puede ser null.
    public Expression Condition { get; set; }
    public Expression? IfBody { get; set; }
    public Expression? ElseBody { get; set; }
    public override ExpressionType Type { get; set; }
    public override object? Value { get; set; }
    public Conditional(Expression condition, Expression ifBody, Expression elseBody, CodeLocation location) : base(location)
    {
        Condition = condition;
        IfBody = ifBody;
        ElseBody = elseBody;
    }

    public override bool CheckSemantic(Context context, Scope scope, List<CompilingError> errors)
    {
        //Se revisa que la condicional sea semánticamente correcta
        bool check = Condition.CheckSemantic(context, scope, errors);
        bool x = check;

        check = IfBody!.CheckSemantic(context, scope, errors);
        if (check is false) x = false;

        if (ElseBody is not null)
        {
            check = ElseBody.CheckSemantic(context, scope, errors);
            if (check is false) x = false;
        }

        ExpressionType IfType = IfBody.Type;
        if(ElseBody is not null)
        {
            ExpressionType ElseType = ElseBody!.Type;
            if (IfType.Equals(ElseType))
            {
                Type = IfType;
            }
            else
            {
                Type = ExpressionType.Undeclared;
            }
        }
        else
            Type = IfType;
        

        return check && x;
    }

    public override void Evaluate(Context context, Scope scope)
    {
        Condition.Evaluate(context, scope);
        //Se evalúa la condicional y en caso de que la condicional sea verdadera se evalúa cada elemento dentro del if, en caso de que sea false se evalúa en cuerpo del else en caso de que exista.
        if (bool.Parse(Condition.Value!.ToString()!))
        {
            if (IfBody is not null)
            {
                IfBody.Evaluate(context, scope);
                Value = IfBody.Value;
            }
        }
        else if (ElseBody != null)
        {
            ElseBody.Evaluate(context, scope);
            Value = ElseBody.Value;
        }
    }
}