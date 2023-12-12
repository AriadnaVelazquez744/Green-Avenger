public class CompilingError
{
    //Determina las propiedades que van a poseer los errores, en este caso el lugar donde se encuentra el error, el tipo de error, y un comentario más preciso sobre que ocurrió.
    public ErrorCode Code { get; }
    public string Argument { get; }
    public CodeLocation Location { get; }
    public CompilingError(CodeLocation location, ErrorCode code, string argument)
    {
        this.Code = code;
        this.Argument = argument;
        Location = location;
    }
}

public enum ErrorCode   //Tipos de errores que se pueden encontrar durante el análisis del código de entrada
{
    Expected,
    Invalid,
    Unknown
}