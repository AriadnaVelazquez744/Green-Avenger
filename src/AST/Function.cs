public class FunctionDeclare : ASTNode
{
    public string Id { get; set; }
    public List<string> Arguments { get; set; }
    public List<ASTNode> Statement { get; set; }
    public FunctionDeclare(string id, List<string> arguments, List<ASTNode> statement, CodeLocation location) : base(location)
    {
        Id = id;
        Arguments = arguments;
        Statement = statement;
    }

    public override bool CheckSemantic(Context context, Scope scope, List<CompilingError> errors)
    {
        foreach (var item in Statement)
        {
            if (!item.CheckSemantic(context, scope, errors))
            {
                return false;
            }
        }
        
        return true;
    }

    public override void Evaluate()
    {
        throw new NotImplementedException();
    }
}

public class FunctionCall : ASTNode
{
    public string Id { get; set; }
    public List<Expression> Arguments { get; set; }
    public FunctionCall(string id, List<Expression> arguments, CodeLocation location) : base(location)
    {
        Id = id;
        Arguments = arguments;
    }

    public override bool CheckSemantic(Context context, Scope scope, List<CompilingError> errors)
    {
        if (!context.ContainFunc(Id))
        {
            errors.Add(new CompilingError(Location, ErrorCode.Invalid, "This function has not been declare, so it can't be call"));
            return false;
        }

        return true;
    }

    public override void Evaluate()
    {
        throw new NotImplementedException();
    }
}