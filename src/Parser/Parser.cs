public class Parser
{
    public TokenStream Stream { get; set; }
    public Context Context { get; set; }
    public Scope Scope { get; set; }
    public Parser(TokenStream stream)
    {
        Stream = stream;
        this.Context = new();
        this.Scope = new();
    }

    public List<ASTNode> Parse(List<CompilingError> errors)
    {
        List<ASTNode> nodes = new();

        while (Stream.CanLookAhead(0))
        {
            Token currentToken = Stream.LookAhead();

            if (Context.ContainsElemFunc(currentToken.Value.ToString()))
            {
                ASTNode value = ParseElementalFunction(errors, currentToken);
                if (value is null)
                    errors.Add(new CompilingError(currentToken.Location, ErrorCode.Invalid, String.Format("The function {0} is wrong use", currentToken.Value)));
                else
                    nodes.Add(value!);
            }

            else if (currentToken.Value.ToString() == "function")
            {
                FunctionDeclare element = ParseFunctionDeclaration(errors);
                if (element is null)
                    errors.Add(new CompilingError(currentToken.Location, ErrorCode.Invalid, String.Format("Wrong declaration of the function {0}", Stream.LookAhead().Value.ToString())));
                else
                    nodes.Add(element!);
            }

            else if (currentToken.Value.ToString() == "let")
            {
                ParseVariable(errors);
            }

            else if (currentToken.Value.ToString() == "if")
            {
                Conditional decision = ParseConditional(errors);
                if (decision is null)
                    errors.Add(new CompilingError(currentToken.Location, ErrorCode.Invalid, "Wrong conditional declaration"));
                else
                    nodes.Add(decision!);
            }

            else if (currentToken.Type == TokenType.Identifier)
            {
                FunctionCall call = ParseFunctionInvocation(errors, currentToken.Value.ToString());
                if (call is null)
                    errors.Add(new CompilingError(currentToken.Location, ErrorCode.Invalid, String.Format("Wrong invocation of the function {0}", currentToken.Value.ToString())));
                else
                    nodes.Add(call!);
            }

            else if (currentToken.Value.ToString() == "print")
            {
                Print impression = ParsePrint(errors);
                if (impression is null)
                    errors.Add(new CompilingError(currentToken.Location, ErrorCode.Invalid, "Wrong use of print"));
                else
                    nodes.Add(impression!);
            }

            else if (currentToken.Type == TokenType.Text)
            {
                Expression literal;
                if (Stream.Next(TokenValues.Concat))
                {
                    literal = ParseConcat(errors, currentToken)!;
                    if (literal is null)
                        errors.Add(new CompilingError(currentToken.Location, ErrorCode.Invalid, "Wrong concat declaration"));
                    else
                        nodes.Add(literal!);
                }
                else 
                {
                    literal = ParseBooleanExpression()!;
                    if (literal is null)
                        errors.Add(new CompilingError(currentToken.Location, ErrorCode.Invalid, "Wrong implementation of a text"));
                    else
                        nodes.Add(literal!);
                }
            }

            else if (currentToken.Value.ToString() == "!")
            {
                BooleanExpression boolean = ParseBooleanExpression()!;
                if (boolean is null)
                    errors.Add(new CompilingError(currentToken.Location, ErrorCode.Invalid, "Wrong use of \"!\""));
                else
                    nodes.Add(boolean!);
            }

            else
            {
                Expression exp = ParseExpression()!;
                if (exp is null)
                    errors.Add(new CompilingError(currentToken.Location, ErrorCode.Unknown, String.Format("The element {0} doesn't belong to the syntax", currentToken.Value.ToString())));
                else
                    nodes.Add(exp!);
            }
        }  

        Stream.MoveBack(1);
        if (!Stream.Next(TokenValues.StatementSeparator))
            errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Expected, "; expected")); 

        return nodes;
    }

    private List<ASTNode> ParseBody(List<CompilingError> errors)
    {
        List<ASTNode> body = new();

        while(!Stream.Next(TokenValues.ClosedBracket) || !Stream.Next(TokenValues.StatementSeparator))
        {
            Token currentToken = Stream.LookAhead(1);

            if (Context.ContainsElemFunc(currentToken.Value.ToString()))
            {
                ASTNode value = ParseElementalFunction(errors, currentToken);
                if (value is null)
                    errors.Add(new CompilingError(currentToken.Location, ErrorCode.Invalid, String.Format("The function {0} is wrong use", currentToken.Value)));
                else
                    body.Add(value!);
            }

            else if (currentToken.Value.ToString() == "function")
            {
                FunctionDeclare element = ParseFunctionDeclaration(errors);
                if (element is null)
                    errors.Add(new CompilingError(currentToken.Location, ErrorCode.Invalid, String.Format("Wrong declaration of the function {0}", Stream.LookAhead().Value.ToString())));
                else
                    body.Add(element!);
            }

            else if (currentToken.Value.ToString() == "let")
            {
                ParseVariable(errors);
            }

            else if (currentToken.Value.ToString() == "if")
            {
                Conditional decision = ParseConditional(errors);
                if (decision is null)
                    errors.Add(new CompilingError(currentToken.Location, ErrorCode.Invalid, "Wrong conditional declaration"));
                else
                    body.Add(decision!);
            }

            else if (currentToken.Type == TokenType.Identifier)
            {
                FunctionCall call = ParseFunctionInvocation(errors, currentToken.Value.ToString());
                if (call is null)
                    errors.Add(new CompilingError(currentToken.Location, ErrorCode.Invalid, String.Format("Wrong invocation of the function {0}", currentToken.Value.ToString())));
                else
                    body.Add(call!);
            }

            else if (currentToken.Value.ToString() == "print")
            {
                Print impression = ParsePrint(errors);
                if (impression is null)
                    errors.Add(new CompilingError(currentToken.Location, ErrorCode.Invalid, "Wrong use of print"));
                else
                    body.Add(impression!);
            }

            else if (currentToken.Type == TokenType.Text)
            {
                Expression literal;
                if (Stream.Next(TokenValues.Concat))
                {
                    literal = ParseConcat(errors, currentToken)!;
                    if (literal is null)
                        errors.Add(new CompilingError(currentToken.Location, ErrorCode.Invalid, "Wrong concat declaration"));
                    else
                        body.Add(literal!);
                }
                else 
                {
                    literal = ParseBooleanExpression()!;
                    if (literal is null)
                        errors.Add(new CompilingError(currentToken.Location, ErrorCode.Invalid, "Wrong implementation of a text"));
                    else
                        body.Add(literal!);
                }
            }

            else if (currentToken.Value.ToString() == "!")
            {
                BooleanExpression boolean = ParseBooleanExpression()!;
                if (boolean is null)
                    errors.Add(new CompilingError(currentToken.Location, ErrorCode.Invalid, "Wrong use of \"!\""));
                else
                    body.Add(boolean!);
            }

            else
            {
                Expression exp = ParseExpression()!;
                if (exp is null)
                    errors.Add(new CompilingError(currentToken.Location, ErrorCode.Unknown, String.Format("The element {0} doesn't belong to the syntax", currentToken.Value.ToString())));
                else
                    body.Add(exp!);
            }
        }   

        return body;

    }
    
    private Number ParseElementalFunction(List<CompilingError> errors, Token Id)
    {
        ElementalFunction func = new();

        string id = Id.Value.ToString();

        foreach (var item in func.Constants)
        {
            if (item.Key == id)
                return new Number(func.Constants[id](), Id.Location);
        }

        if (!Stream.CanLookAhead(0)) return null!;

        if (!Stream.Next(TokenValues.OpenBracket))
            errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Expected, "( expected"));

        List<double> arguments = new();
        do
        {
            if (!Stream.Next(TokenType.Number))
            {
                errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Expected, "argument expected after the ,"));
            }
            else
            {
                string argument = Stream.LookAhead().Value.ToString();
                double arg = double.Parse(argument);   
                arguments.Add(arg);
            }

        } while (Stream.Next(TokenValues.ValueSeparator));
        
        if (!Stream.Next(TokenValues.ClosedBracket))
        {
            errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Expected, ") expected"));
        }

        if (arguments.Count == 1)
        {
            return new Number(func.MathFunction[id](arguments[0]), Id.Location);
        }
        else if (arguments.Count == 2)
        {
            return new Number(func.Log[id](arguments[0], arguments[1]), Stream.LookAhead().Location);
        }

        return null!;
    }

    private FunctionDeclare ParseFunctionDeclaration(List<CompilingError> errors)
    {
        if (!Stream.Next(TokenType.Identifier))
        {
            errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Expected, "function name expected"));
        }
        Token id = Stream.LookAhead();
        string functionName = id.Value.ToString();
        

        if (!Stream.Next(TokenValues.OpenBracket))
        {
            errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Expected, "( expected"));
            return null!;
        }

        List<string> arguments = new();
        int n = 0;
        if (!Stream.Next(TokenValues.ClosedBracket))
        {
            do
            {
                n++;
                if (!Stream.Next(TokenType.Identifier))
                {
                    errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Expected, "argument expected"));
                    return null!;
                }
                arguments.Add(Stream.LookAhead().Value.ToString());

            } while (Stream.Next(TokenValues.ValueSeparator));
            
        } 

        if (!Stream.Next(TokenValues.ClosedBracket))
        {
            errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Expected, ") expected"));
        }

        if (!Stream.Next(TokenValues.LambdaExpression))
        {
            errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Expected, "=> expected"));
            return null!;
        }

        if (!Stream.Next(TokenValues.OpenBracket))
        {
            errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Expected, "( expected"));
            return null!;
        }

        List<ASTNode> body = ParseBody(errors);
        if (body == null)
        {
            errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Expected, "Function statement expected"));
            return null!;
        }

        if (id is null)
            return null!;

        if (Context.ContainFunc(functionName))
        {
            errors.Add(new CompilingError(id.Location, ErrorCode.Invalid, String.Format("The function {0} already exist", functionName)));
            return null!;
        }
        if (Scope.ContainsVariable(functionName))
        {
            errors.Add(new CompilingError(id.Location, ErrorCode.Invalid, "Already exist a variable with the same name"));
            return null!;
        }
        
        Context.AddFunc(functionName, n);

        return new FunctionDeclare(functionName, arguments, body, id.Location);
    }

    private FunctionCall ParseFunctionInvocation(List<CompilingError> errors, string functionName)
    {
        List<Expression> argumentValues = new();
        int count = Context.GetArgNumber(functionName);

        if (!Stream.Next(TokenValues.OpenBracket))
        {
            errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Expected, "( expected"));
            return null!;
        }

        if (count != 0)
        {
            while (true)
            {
                count--;
                Expression argValue = ParseExpression()!;
                if (argValue == null) 
                    return null!;

                argumentValues.Add(argValue);

                if (!Stream.Next(TokenValues.ValueSeparator))
                    break;    
            }
            if (count != 0 || count < 0)
            {
                errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Invalid, "Wrong number of arguments"));
                return null!;
            }
        }

        return new FunctionCall(functionName, argumentValues, Stream.LookAhead().Location);
    }

    private void ParseVariable(List<CompilingError> errors) 
    {
        do
        {
            if (!Stream.Next(TokenType.Identifier))
            {
                errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Expected, "identifier expected"));
                return;
            }
            string variableName = Stream.LookAhead().Value.ToString();

            if (Scope.ContainsVariable(variableName))
            {
                errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Invalid, "variable already declare"));
                return;
            }

            if (!Stream.Next(TokenValues.Assign))
            {
                errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Expected, "= expected"));
                return;
            }

            object value = ParseExpression()!;
            if (value == null)
            {
                errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Expected, "expression value expected"));
                return;
            }

            Scope.AddVariable(variableName, value);

        } while (Stream.Next(TokenValues.StatementSeparator));
        

        if (!Stream.Next(TokenValues.In))
        {
            errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.None, "The variables will not be use in anything"));
        }
        
        List<ASTNode> body = ParseBody(errors);

    }

    private Conditional ParseConditional(List<CompilingError> errors)
    {
        if (!Stream.Next(TokenValues.OpenBracket))
        {
            errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Expected, ") expected"));
            return null!;
        }

        BooleanExpression condition = ParseBooleanExpression()!;
        if (condition is null)
        {
            errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Expected, "condition value expected"));
        }

        if (!Stream.Next(TokenValues.ClosedBracket) || !Stream.Next(TokenValues.LambdaExpression))
        {
            errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Expected, ") => expected"));
            return null!;
        }

        if (!Stream.Next(TokenValues.OpenBracket))
        {
            errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Expected, ") expected"));
        }

        List<ASTNode> ifBody = ParseBody(errors);
        List<ASTNode> elseBody = null!;

        if (Stream.Next(TokenValues.Else))
        {
            if (!Stream.Next(TokenValues.OpenBracket))
            {
                errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Expected, "() expected"));
            }

            elseBody = ParseBody(errors);
        }

        if (condition is null)
            return null!;

        return new Conditional(condition, ifBody, elseBody, Stream.LookAhead().Location);
    }

    private Print ParsePrint(List<CompilingError> errors)
    {
        if (!Stream.Next(TokenValues.OpenBracket))
        {
            errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Expected, "( expected"));
        }

        Expression exp = ParseExpression()!;
        if (exp == null)
        {
            errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Expected, "expression expected"));
            return null!;
        }

        if (!Stream.Next(TokenValues.ClosedBracket))
        {
            errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Expected, ") expected"));
        }

        return new Print(exp, Stream.LookAhead().Location);
    }

    private Expression? ParseConcat(List<CompilingError> errors, Token text)
    {
        string left = text.Value.ToString();

        Expression? Right = ParseText()!;
        if (Right == null)
        {
            errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Expected, "Expected text for the concat operation"));
            return null;
        }
        string right = Right.Value!.ToString()!;

        return new Text(left + right, text.Location);
    }

    private Expression? ParseNumber()
    {
        if (!Stream.Next(TokenType.Number)) return null;
    
        return new Number(double.Parse(Stream.LookAhead().Value.ToString()), Stream.LookAhead().Location);
    }
    private Expression? ParseText()
    {
        if (!Stream.Next(TokenType.Text)) return null;

        return new Text(Stream.LookAhead().Value, Stream.LookAhead().Location);
    }
    
    private Expression? ParseExpression()
    {
        return ParseExpressionLv1();
    }
    private Expression? ParseExpressionLv0()
    {
        if (Stream.Next(TokenValues.OpenBracket))
        {
            Expression? exp = ParseExpression();
            
            if (exp == null)
                return null;
            if (!Stream.Next(TokenValues.ClosedBracket))
                return null;
            
            return exp;
        }
        return ParseExpressionLv4();
    }
    private Expression? ParseExpressionLv1()
    {
        Expression? newLeft = ParseExpressionLv0();
        Expression? exp = ParseExpressionLv1_(newLeft);
        return exp;
    }
    private Expression? ParseExpressionLv1_(Expression? left)
    {
        Expression? exp = ParseAdd(left);
        if (exp != null) return exp;

        exp = ParseSub(left);
        if (exp != null) return exp;

        return left;
    }
    private Expression? ParseExpressionLv2()
    {
        Expression? newLeft = ParseExpressionLv1();
        return ParseExpressionLv2_(newLeft);
    }
    private Expression? ParseExpressionLv2_(Expression? left)
    {
        Expression? exp = ParseMul(left);
        if (exp != null) return exp;

        exp = ParseDiv(left);
        if (exp != null) return exp;

        return left;
    }
    private Expression? ParseExpressionLv3()
    {
        Expression? newLeft = ParseExpressionLv2();
        return ParseExpressionLv3_(newLeft);
    }
    private Expression? ParseExpressionLv3_(Expression? left)
    {
        Expression? exp = ParsePow(left);
        if (exp != null) return exp;

        return left;
    }
    private Expression? ParseExpressionLv4()
    {
        Expression? exp = ParseNumber();
        if (exp != null) return exp;

        exp = ParseText();
        if (exp != null) return exp;

        exp = ParseBooleanExpression();
        if (exp != null) return exp;

        return null;
    }

    private Expression? ParseAdd(Expression? left)
    {
        Add sum = new(Stream.LookAhead().Location);

        if (left == null || !Stream.Next(TokenValues.Add)) return null;

        sum.Left = left;

        Expression? right = ParseExpressionLv1();
        if (right == null)
        {
            Stream.MoveBack(2);
            return null;
        }
        sum.Right = right;

        return ParseExpressionLv1_(sum);
    }
    private Expression? ParseSub(Expression? left)
    {
        Sub sub = new(Stream.LookAhead().Location);

        if (left == null || !Stream.Next(TokenValues.Sub)) return null;

        sub.Left = left;

        Expression? right = ParseExpressionLv1();
        if (right == null)
        {
            Stream.MoveBack(2);
            return null;
        }
        sub.Right = right;

        return ParseExpressionLv1_(sub);
    }
    private Expression? ParseMul(Expression? left)
    {
        Mul mul = new(Stream.LookAhead().Location);

        if (left is null || !Stream.Next(TokenValues.Mul)) return null;

        mul.Left = left;

        Expression? right = ParseExpressionLv2();
        if (right == null)
        {
            Stream.MoveBack(2);
            return null;
        }
        mul.Right = right;

        return ParseExpressionLv2_(mul);
    }
    private Expression? ParseDiv(Expression? left)
    {
        Div div = new(Stream.LookAhead().Location);

        if (left is null || !Stream.Next(TokenValues.Div)) return null;

        div.Left = left;

        Expression? right = ParseExpressionLv2();
        if (right == null)
        {
            Stream.MoveBack(2);
            return null;   
        }
        div.Right = right;

        return ParseExpressionLv2_(div);
    }
    private Expression? ParsePow(Expression? left)
    {
        Pow pow = new(Stream.LookAhead().Location);

        if (left is null || !Stream.Next(TokenValues.Pow)) return null;

        pow.Left = left;

        Expression? right = ParseExpressionLv3();
        if (right == null)
        {
            Stream.MoveBack(2);
            return null;
        }
        pow.Right = right;

        return ParseExpressionLv3_(pow);
    }


    private BooleanExpression? ParseBooleanExpression()
    {
        BooleanExpression? newLeft = ParseBooleanExpressionLv1();
        return ParseBooleanExpressionLv1_(newLeft!);
    }
    private BooleanExpression? ParseBooleanExpressionLv1()
    {
        BooleanExpression? newLeft = ParseBooleanExpressionLv2();
        return ParseBooleanExpressionLv2_(newLeft!);
    }
    private BooleanExpression? ParseBooleanExpressionLv1_(BooleanExpression left)
    {
        if (Stream.Next(TokenValues.And))
        {
            And and = new(Stream.LookAhead().Location)
            {
                Left = left,
                Right = ParseBooleanExpressionLv2()
            };
            return ParseBooleanExpressionLv1_(and);
        }

        if (Stream.Next(TokenValues.Or))
        {
            Or or = new(Stream.LookAhead().Location)
            {
                Left = left,
                Right = ParseBooleanExpressionLv2()
            };
            return ParseBooleanExpressionLv1_(or);
        }        

        return left;
    }
    private BooleanExpression? ParseBooleanExpressionLv2()
    {
        BooleanExpression? newLeft = ParseBooleanExpressionLv3();
        return ParseBooleanExpressionLv3_(newLeft!); 
    }
    private BooleanExpression? ParseBooleanExpressionLv2_(BooleanExpression left)
    {
        if (Stream.Next(TokenValues.Equal))
        {
            Equal equal = new(Stream.LookAhead().Location)
            {
                Left = left,
                Right = ParseBooleanExpressionLv3()
            };
            return ParseBooleanExpressionLv2_(equal);
        }        

        if (Stream.Next(TokenValues.Different))
        {
            Different different = new(Stream.LookAhead().Location)
            {
                Left = left,
                Right = ParseBooleanExpressionLv3()
            };
            return ParseBooleanExpressionLv2_(different);
        }        

        if (Stream.Next(TokenValues.Less))
        {
            Less less = new(Stream.LookAhead().Location)
            {
                Left = left,
                Right = ParseBooleanExpressionLv3()
            };
            return ParseBooleanExpressionLv2_(less);
        }        

        if (Stream.Next(TokenValues.LessOrEqual))
        {
            LessOrEqual lessOrEqual = new(Stream.LookAhead().Location)
            {
                Left = left,
                Right = ParseBooleanExpressionLv3()
            };
            return ParseBooleanExpressionLv2_(lessOrEqual);
        }        

        if (Stream.Next(TokenValues.More))
        {
            More more = new(Stream.LookAhead().Location)
            {
                Left = left,
                Right = ParseBooleanExpressionLv3()
            };
            return ParseBooleanExpressionLv2_(more);
        }        

        if (Stream.Next(TokenValues.MoreOrEqual))
        {
            MoreOrEqual moreOrEqual = new(Stream.LookAhead().Location)
            {
                Left = left,
                Right = ParseBooleanExpressionLv3()
            };
            return ParseBooleanExpressionLv2_(moreOrEqual);
        }        

        return left;
    }
    private BooleanExpression? ParseBooleanExpressionLv3()
    {
        BooleanExpression? newLeft = ParseBooleanExpressionLv4();
        return newLeft; 
    }
    private BooleanExpression? ParseBooleanExpressionLv3_(BooleanExpression left)
    {
        if (Stream.Next(TokenValues.Not))
        {
            Not not = new(Stream.LookAhead().Location);
            not.Right = ParseBooleanExpressionLv4();
            return ParseBooleanExpressionLv3_(not);
        }

        return left;
    }
    private BooleanExpression? ParseBooleanExpressionLv4()
    {
        if (Stream.Next(TokenType.BooleanExpression))
        {
        
        }
        throw new NotImplementedException();
    }

}