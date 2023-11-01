public class Scope
{
    //Son los elementos que pertenecen a una vecindad del código y no al código completo, en este caso solo existen en mi lenguaje variables locales
    
    //public Scope? Parent { get; set; }
    
    public Dictionary<string, object> Variables { get; set; }
    public Scope()
    {
        Variables = new();
    }

    public bool ContainsVariable(string name)
    {
        return Variables.ContainsKey(name);
    }

    public void AddVariable(string name, object value)
    {
        Variables.Add(name, value);
    }
    public void DeleteVariables(Dictionary<string, Expression> vars)
    {
        foreach (var item in vars)
        {
            Variables.Remove(item.Key);
        }
    }

    // public Scope CreateChild()
    // {
    //     Scope child = new()
    //     {
    //         Parent = this
    //     };

    //     return child;
    // }
}