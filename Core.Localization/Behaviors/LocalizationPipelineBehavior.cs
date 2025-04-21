using Core.Localization.Abstract;
using MediatR;
using System.Globalization;

namespace Core.Localization.Behaviors;

/// <summary>
/// Pipeline behavior for handling localization of responses implementing ILocalizedResponse.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public class LocalizationPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : class
{
    private readonly ICultureProvider _cultureProvider;
    private readonly ILocalizationServiceAsync _localizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizationPipelineBehavior{TRequest, TResponse}"/> class.
    /// </summary>
    /// <param name="localizationServiceAsync">The localization service.</param>
    public LocalizationPipelineBehavior(ILocalizationServiceAsync localizationServiceAsync, ICultureProvider cultureProvider)
    {
        _localizationService = localizationServiceAsync;
        _cultureProvider = cultureProvider;
    }

    /// <summary>
    /// Handles the localization of the response if it implements ILocalizedResponse.
    /// </summary>
    /// <param name="request">The request object.</param>
    /// <param name="next">The next delegate in the pipeline.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The localized response.</returns>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // 1) Gelen isteğin kültür kodunu al
        // 1) Mevcut kültürü al ve Thread/AsyncLocal’a uygula
        var cultureCode = _cultureProvider.GetRequestCulture();
        _localizationService.SetCurrentCulture(new CultureInfo(cultureCode));

        var culture = _localizationService.GetCurrentCulture();
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;

        // 2) İşleyici çağrısından gelen response.
        var response = await next();

        // Eğer response, ILocalizedResponse arayüzünü implemente ediyorsa
        if (response is ILocalizedResponse localizedResponse && !string.IsNullOrEmpty(localizedResponse.MessageKey))
        {
            // Lokalize mesajı asenkron olarak al ve set et.
            localizedResponse.Message = await _localizationService.GetStringAsync(localizedResponse.MessageKey);
        }

        return response;
    }
}
