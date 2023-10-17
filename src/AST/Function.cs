// public class Function : ASTNode
// {
//     public string Id { get; set; }
//     public List<Expression> Argument { get; set; }
//     public Expression Declaration { get; set; }
//     public Function(string id, List<Expression> arguments, Expression declaration, CodeLocation location) : base(location)
//     {
//         this.Id = id;
//         this.Argument = arguments;
//         this.Declaration = declaration;
//     }

//     public bool CollectFunctions(Context context, Scope scope, List<CompilingError> errors)
//     {
//         if (context.Functions.Contains(Id))
//         {
//             errors.Add(new CompilingError(Location, ErrorCode.Invalid, "Function already exist"));
//             return false;           
//         }
        
//         context.Functions.Add(Id);
//         return true;
//     }

//     public override bool CheckSemantic(Context context, Scope scope, List<CompilingError> errors)
//     {
//         bool checkArgument;
//         if (Argument == null)
//             errors.Add(new CompilingError(Location, ErrorCode.Expected, "arguments has not been found and are necessary for function declaration"));
        
//         foreach (var arg in Argument!)
//         {
            
//         }
        
        
//         //no se q más necesito

//         return true;
//     }

//     public void Add(string name, object arg1, object arg2) { }
// }

public class FunctionDeclare : ASTNode
{
    public string Identifier { get; set; }
    public List<string> Arguments { get; set; }
    public Statement Statement { get; set; }
    public FunctionDeclare(string identifier, List<string> arguments, Statement statement, CodeLocation location) : base(location)
    {
        Identifier = identifier;
        Arguments = arguments;
        Statement = statement;
    }

    public override bool CheckSemantic(Context context, Scope scope, List<CompilingError> errors)
    {
        bool result = true;
        if (context.Functions.Contains(Identifier))
            errors.Add(new CompilingError(Location, ErrorCode.Invalid, "The variables name already exist in the context"));
            result = false;
        if (!Statement.CheckSemantic(context, scope, errors))
            result = false;

        return result;
    }
}

public class FunctionCall : ASTNode
{
    public string Identifier { get; set; }
    public List<Expression> Arguments { get; set; }
    public FunctionCall(string identifier, List<Expression> arguments, CodeLocation location) : base(location)
    {
        Identifier = identifier;
        Arguments = arguments;
    }

    public override bool CheckSemantic(Context context, Scope scope, List<CompilingError> errors)
    {
        if (!context.Functions.Contains(Identifier))
            errors.Add(new CompilingError(Location, ErrorCode.Invalid, "This function has not been declare so it can't be call"));
            return false;
    }
}