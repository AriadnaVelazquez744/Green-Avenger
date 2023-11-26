public abstract class ASTNode
{
    //Nodo del árbol de análisis sintáctico de mi interpretador, contiene la localización del elemento y una propiedad que hereda a sus hijos que es para comprobar la semántica y evaluar cada tipo de expresión
    public CodeLocation Location {get; set;}

    public ASTNode(CodeLocation location)
    {
        Location = location;
    }
    public abstract bool CheckSemantic(Context context, Scope scope, List<CompilingError> errors);

    public abstract void Evaluate(Context context, Scope scope);
}