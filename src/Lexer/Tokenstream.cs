using System.Collections;

public class TokenStream : IEnumerable<Token>
{
    //Itera a través de un lista de Tokens y provee de varios métodos para mover la posición del iterador y chequear el siguiente token de la interface.
    List<Token> tokens;
    int position;

    public TokenStream(IEnumerable<Token> tokens)
    {
        this.tokens = new List<Token>(tokens);
        position = 0;
    }

    public int Position { get { return position; } }

    public bool End => position == tokens.Count - 1;
    //End comprueba si se ha llegado al final de la lista.
    public void MoveNext(int k) => position += k;
    public void MoveBack(int k) => position -= k;


    //Los métodos Next (y todas sus sobrecargas), CanLookAhead y LookAhead permiten comprobar el próximo token sin mover el iterador de lugar
    public bool Next()
    {
        //Comprueba si existe el proximo token
        if (position < tokens.Count - 1) position++;
        return position < tokens.Count;
    }
    public bool Next(TokenType type)
    {
        //Comprueba si el próximo token es de un tipo determinado
        if (position < tokens.Count - 1 && LookAhead(1).Type == type)
        {
            position++;
            return true;
        }
        return false;
    }
    public bool Next(string value)
    {
        //Comprueba si el valor del próximo token coincide con el del argumento
        if (position < tokens.Count - 1 && LookAhead(1).Value == value)
        {
            position++;
            return true;
        }
        return false;
    }

    public bool CanLookAhead(int k = 0)
    {
        //Confirma si se hay tokens en la posición que se indique con el int del argumento
        return tokens.Count - position > k;
    }

    public Token LookAhead(int k = 0)
    {
        //Devuelve el token tantas posiciones adelante como se indique.
        return tokens[position + k];
    }

    public IEnumerator<Token> GetEnumerator()
    {
        //Permite iterar a través de la lista de token usando el foreach, es un método propio de la interface por lo que su implementación es obligatoria.
        for (int i = position; i < tokens.Count; i++)
            yield return tokens[i];
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
        //Forma parte de las propiedades imprescindibles del IEnumerable<>
        return GetEnumerator();
    }
}