using Sincro.Application.DTOs;
using Sincro.Application.Exceptions;

namespace Sincro.Presentation.Middleware
{
    // Converte as exceções dos services em respostas HTTP: { "erro": "mensagem" }
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex) when (!context.Response.HasStarted)
            {
                var (status, mensagem) = ex switch
                {
                    NaoEncontradoException => (StatusCodes.Status404NotFound, ex.Message),
                    RegraDeNegocioException => (StatusCodes.Status400BadRequest, ex.Message),
                    NaoAutorizadoException => (StatusCodes.Status401Unauthorized, ex.Message),
                    AcessoNegadoException => (StatusCodes.Status403Forbidden, ex.Message),
                    _ => (StatusCodes.Status500InternalServerError, "Erro interno do servidor")
                };

                if (status == StatusCodes.Status500InternalServerError)
                    _logger.LogError(ex, "Erro não tratado em {Metodo} {Caminho}", context.Request.Method, context.Request.Path);

                context.Response.StatusCode = status;
                await context.Response.WriteAsJsonAsync(new ErroDto(mensagem));
            }
        }
    }
}