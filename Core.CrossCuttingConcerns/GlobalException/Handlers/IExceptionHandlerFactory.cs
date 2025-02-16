namespace Core.CrossCuttingConcerns.GlobalException.Handlers;

public interface IExceptionHandlerFactory
{
    IExceptionHandler GetHandler(Exception exception);
}