namespace Sincro.Application.Exceptions
{
    public class NaoEncontradoException : Exception
    {
        public NaoEncontradoException(string mensagem) : base(mensagem) { }
    }

    public class RegraDeNegocioException : Exception
    {
        public RegraDeNegocioException(string mensagem) : base(mensagem) { }
    }

    public class NaoAutorizadoException : Exception
    {
        public NaoAutorizadoException(string mensagem = "Não autorizado") : base(mensagem) { }
    }

    public class AcessoNegadoException : Exception
    {
        public AcessoNegadoException(string mensagem = "Acesso negado") : base(mensagem) { }
    }
}