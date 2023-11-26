﻿using System.Linq.Expressions;

public class MainProgram : ASTNode
{
    //Sus propiedades son la lista de nodos a analizar y la ubicación que proviene de su padre.
    public List<ASTNode> Statements { get; set; }
    public Context Context { get; set; }
    public MainProgram(List<ASTNode> statements, Context context, CodeLocation location) : base(location)
    {
        Statements = statements;
        Context = context;
    }

    public override bool CheckSemantic(Context context, Scope scope, List<CompilingError> errors)
    {
        //Para revisar la semántica se comprueba que todos los nodos sean semánticamente correctos.

        bool check = true;
        bool x = true;

        foreach (ASTNode item in Statements)
        {
            if (item is FunctionDeclare declare)
            {
                check = declare.CheckSemantic(context, scope, errors);
                if (check is false)  x = false;
            }
        }
        
        foreach (var item in Statements)
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
            else if (item is FunctionDeclare declare)
            {
                continue;
            }
            else if (item is ElementalFunction elem)
            {
                check = elem.CheckSemantic(context, scope, errors);
            }

            if (check is false)  x = false;
        }

        if (x is false)
            errors.Add(new CompilingError(Location, ErrorCode.Invalid, "At least one of the statement has a semantic problem"));

        return check && x;
    }

    public override void Evaluate(Context context, Scope scope)
    {
        //Se evalúan primero las declaraciones de funciones y se eliminan de la lista de nodos para no volver a tomarlas.
        foreach (ASTNode item in Statements)
        {
            if (item is FunctionDeclare declare)
            {
                context.AddFuncExpression(declare);                
            }
        }

        //Luego se evalúan ordenadamente los nodos restantes
        foreach (ASTNode item in Statements)
        {
            if (item is Expression expression)
            {
                expression.Evaluate(context, scope);
                Console.WriteLine(expression.Value!.ToString());
            }
            else if (item is ElementalFunction elem)
            {
                elem.Evaluate(context, scope);
                Console.WriteLine(elem.Value!.ToString());
            }
            else if (item is Variable variable)
            {
                variable.Evaluate(context, scope);
            }
            else if (item is FunctionCall call)
            {
                call.Evaluate(context, scope);
            }
            else if (item is Conditional conditional)
            {
                conditional.Evaluate(context, scope);
            }
            if (item is FunctionDeclare)
            {
                continue;
            }
            else if (item is Print print)
            {
                print.Evaluate(context, scope);
            }
        }
    }
}