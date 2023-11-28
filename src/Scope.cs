public class Scope
{
    //Son los elementos que pertenecen a una vecindad del código y no al código completo.
    //En esta clase se guardarán las variables generadas a partir del let y los argumentos 
    //de las funciones que presentan una estructura similar a dichas variables .
    //Los métodos presentan nombres muy especificativos por lo que no los comentaré.
    
    public Dictionary<string, Expression> Variables { get; set; }
    public Dictionary<string, Expression> FuncVars { get; set; }
    public Scope()
    {
        Variables = new();
        FuncVars = new();
    }

    public bool ContainsVariable(string name)
    {
        return Variables.ContainsKey(name);
    }
    public void AddVariable(string name, Expression value)
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

    public bool ContainFuncVar(string id)
    {
        return FuncVars.ContainsKey(id);
    }
    public void AddFuncVar(string name, Expression value)
    {
        FuncVars.Add(name, value);
    }

    public Expression TakeVar(string id)
    {
        if (FuncVars.ContainsKey(id))
        {
            return FuncVars[id];
        }
        else
        {
            return Variables[id];
        }
    }
}