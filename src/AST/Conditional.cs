
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
        bool check = true;
        //Se revisa que la condicional sea semánticamente correcta
        check = Condition.CheckSemantic(context, scope, errors);

        foreach (var item in IfBody)
        {
            if (item is Expression expression)
            {
                check = expression.CheckSemantic(context, scope, errors);
            }
            else if (item is Variable variable)
            {
                check = variable.CheckSemantic(context, scope, errors);
            }
            else if (item is FunctionCall call)
            {
                check = call.CheckSemantic(context, scope, errors);
            }
            else if (item is Conditional conditional)
            {
                check = conditional.CheckSemantic(context, scope, errors);
            }
            else if (item is Print print)
            {
                check = print.CheckSemantic(context, scope, errors);
            }
            else if (item is ElementalFunction elem)
            {
                check = elem.CheckSemantic(context, scope, errors);
            }
        }
        
        if (ElseBody != null)
        {
            foreach (var item in ElseBody)
            {
                if (item is Expression expression)
                {
                    check = expression.CheckSemantic(context, scope, errors);
                }
                else if (item is Variable variable)
                {
                    check = variable.CheckSemantic(context, scope, errors);
                }
                else if (item is FunctionCall call)
                {
                    check = call.CheckSemantic(context, scope, errors);
                }
                else if (item is Conditional conditional)
                {
                    check = conditional.CheckSemantic(context, scope, errors);
                }
                else if (item is Print print)
                {
                    check = print.CheckSemantic(context, scope, errors);
                }
                else if (item is ElementalFunction elem)
                {
                    check = elem.CheckSemantic(context, scope, errors);
                }
            }
        }

        return check;
    }

    public override void Evaluate()
    {
        //Se evalúa la condicional y en caso de que la condicional sea verdadera se evalúa cada elemento dentro del if, en caso de que sea false se evalúa en cuerpo del else en caso de que exista.
        if (bool.Parse(Condition.Value!.ToString()!))
        {
            foreach (var item in IfBody)
            {
                if (item is Expression expression)
                {
                    expression.Evaluate();
                    Console.WriteLine(expression.Value!.ToString());
                }
                else if (item is ElementalFunction elem)
                {
                    elem.Evaluate();
                    Console.WriteLine(elem.Value!.ToString());
                }
                else if (item is Variable variable)
                {
                    variable.Evaluate();
                }
                else if (item is FunctionCall call)
                {
                    call.Evaluate();
                }
                else if (item is Conditional conditional)
                {
                    conditional.Evaluate();
                }
                else if (item is Print print)
                {
                    print.Evaluate();
                }
            }
        }
        else if (ElseBody != null)
        {
            foreach (var item in ElseBody)
            {
                if (item is Expression expression)
                {
                    expression.Evaluate();
                    Console.WriteLine(expression.Value!.ToString());
                }
                else if (item is ElementalFunction elem)
                {
                    elem.Evaluate();
                    Console.WriteLine(elem.Value!.ToString());
                }
                else if (item is Variable variable)
                {
                    variable.Evaluate();
                }
                else if (item is FunctionCall call)
                {
                    call.Evaluate();
                }
                else if (item is Conditional conditional)
                {
                    conditional.Evaluate();
                }
                else if (item is Print print)
                {
                    print.Evaluate();
                }
            }
        }
    }
}