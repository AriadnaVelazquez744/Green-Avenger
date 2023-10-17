public class Scope
{
    //Son los elementos que pertenecen a una vecindad del código y no al código completo, en este caso solo existen en mi lenguaje variables locales
    public Scope? Parent { get; set; }
    public List<string> Variables { get; set; }
    public Scope()
    {
        Variables = new();
    }

    public Scope CreateChild()
    {
        Scope child = new()
        {
            Parent = this
        };

        return child;
    }
}