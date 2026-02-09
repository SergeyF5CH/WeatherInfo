using Microsoft.AspNetCore.Mvc.ModelBinding;
using WeatherInfo.API.Exceptions;

namespace WeatherInfo.API.ModelBinders
{
    public class DateOnlyModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null) throw new ArgumentNullException(nameof(bindingContext));

            var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.FieldName);
            if (valueProviderResult == ValueProviderResult.None)
            {
                return Task.CompletedTask;
            }

            var value = valueProviderResult.FirstValue;
            if (string.IsNullOrWhiteSpace(value))
            {
                return Task.CompletedTask;
            }

            if (!DateOnly.TryParse(value, out var parsedDate))
            {
                throw new InvalidDateFormatException(value);
            }

            bindingContext.Result = ModelBindingResult.Success(parsedDate);
            return Task.CompletedTask;
        }
    }
}
