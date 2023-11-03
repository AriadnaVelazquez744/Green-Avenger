public static class Lexer   //Vocabulario.
{
    private static LexicalAnalyzer? _LexicalProcess;    //Declara una instancia de la clase LexicalAnalyzer que me permite acceder a sus propiedades (en este caso lo que me interesa es modificar sus diccionarios y establecer el vocabulario)
    public static LexicalAnalyzer LexicalAnalyzer 
    {
        //Hace las modificaciones en la class LA antes de devolver el valor definido
        //Se añaden como keys a los dict los string que representan en el vocabulario y como value los string del nombre relacionado al TokenValue que representan
        get
        {
            if (_LexicalProcess == null)
            {
                _LexicalProcess = new LexicalAnalyzer();

                _LexicalProcess.Operators["+"] = TokenValues.Add;
                _LexicalProcess.Operators["-"] = TokenValues.Sub;
                _LexicalProcess.Operators["*"] = TokenValues.Mul;
                _LexicalProcess.Operators["/"] = TokenValues.Div;
                _LexicalProcess.Operators["^"] = TokenValues.Pow;

                _LexicalProcess.Operators["="] = TokenValues.Assign;
                _LexicalProcess.Operators[":="] = TokenValues.Reassign;
                _LexicalProcess.Operators[","] = TokenValues.ValueSeparator;
                _LexicalProcess.Operators[";"] = TokenValues.StatementSeparator;
                _LexicalProcess.Operators["("] = TokenValues.OpenBracket;
                _LexicalProcess.Operators[")"] = TokenValues.ClosedBracket;
                _LexicalProcess.Operators["=>"] = TokenValues.LambdaExpression;
                _LexicalProcess.Operators["@"] = TokenValues.Concat;

                _LexicalProcess.Operators["&"] = TokenValues.And;
                _LexicalProcess.Operators["|"] = TokenValues.Or;
                _LexicalProcess.Operators["!"] = TokenValues.Not;

                _LexicalProcess.Operators["=="] = TokenValues.Equal;
                _LexicalProcess.Operators["!="] = TokenValues.Different;
                _LexicalProcess.Operators["<"] = TokenValues.Less;
                _LexicalProcess.Operators["<="] = TokenValues.LessOrEqual;
                _LexicalProcess.Operators[">"] = TokenValues.More;
                _LexicalProcess.Operators[">="] = TokenValues.MoreOrEqual;


                _LexicalProcess.KeyWords["sqrt"] = TokenValues.sqrt;
                _LexicalProcess.KeyWords["sin"] = TokenValues.sin;
                _LexicalProcess.KeyWords["cos"] = TokenValues.cos;
                _LexicalProcess.KeyWords["exp"] = TokenValues.exp;
                _LexicalProcess.KeyWords["log"] = TokenValues.log;
                _LexicalProcess.KeyWords["rand"] = TokenValues.rand;

                _LexicalProcess.KeyWords["PI"] = TokenValues.PI;
                _LexicalProcess.KeyWords["E"] = TokenValues.E;

                _LexicalProcess.KeyWords["true"] = TokenValues.True;
                _LexicalProcess.KeyWords["false"] = TokenValues.False;

                _LexicalProcess.KeyWords["if"] = TokenValues.If;
                _LexicalProcess.KeyWords["else"] = TokenValues.Else;

                _LexicalProcess.KeyWords["let"] = TokenValues.let;
                _LexicalProcess.KeyWords["in"] = TokenValues.In;
                _LexicalProcess.KeyWords["function"] = TokenValues.function;
                _LexicalProcess.KeyWords["print"] = TokenValues.print;


                _LexicalProcess.Texts["\""] = "\"";
            }

            return _LexicalProcess;
        }
    }
}