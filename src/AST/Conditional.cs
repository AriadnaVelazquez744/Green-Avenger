
public class Conditional : ASTNode
{
    //Contiene una expression que es la condicional, y cada parte (en caso de que contenga el else) tiene su propio cuerpo de expresiones a ejecutar.
    public Expression Condition { get; set; }
    public List<ASTNode> IfBody { get; set; }
    public List<ASTNode> ElseBody { get; set; }
    public Conditional(Expression condition, List<ASTNode> ifBody, List<ASTNode> elseBody, CodeLocation location) : base(location)
    {
        Condition = condition;
        IfBody = ifBody;
        ElseBody = elseBody;
    }

    public override bool CheckSemantic(Context context, Scope scope, List<CompilingError> errors)
    {
        //Se revisa que la condicional sea semánticamente correctas
        if (!Condition.CheckSemantic(context, scope, errors))
            return false;

        foreach (var item in IfBody)
        {
            if (!item.CheckSemantic(context, scope, errors))
                return false;
        }
        
        if (ElseBody != null)
        {
            foreach (var item in ElseBody)
            {
                if (!item.CheckSemantic(context, scope, errors))
                    return false;
            }
        }

        return true;
    }

    public override void Evaluate()
    {
        //Se evalúa la condicional y en caso de que la condicional sea verdadera se evalúa cada elemento dentro del if, en caso de que sea false se evalúa en cuerpo del else en caso de que exista.
        if (Condition.Value!.ToString() == "true")
        {
            foreach (var item in IfBody)
            {
                item.Evaluate();
            }
        }
        else if (ElseBody != null)
        {
            foreach (var item in ElseBody)
            {
                item.Evaluate();
            }
        }
    }
}