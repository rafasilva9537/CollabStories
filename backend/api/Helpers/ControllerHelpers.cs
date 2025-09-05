using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace api.Helpers;

public static class ControllerHelpers
{
    public static void AddErrorsToModelState(Dictionary<string, string[]> errors, ModelStateDictionary modelState)
    {
        foreach (var error in errors)
        {
            foreach (string description in error.Value)
            {
                modelState.AddModelError(error.Key, description);
            }
        }
    }
    
    public static void AddErrorsToModelState(IReadOnlyDictionary<string, string[]> errors, ModelStateDictionary modelState)
    {
        foreach (var error in errors)
        {
            foreach (string description in error.Value)
            {
                modelState.AddModelError(error.Key, description);
            }
        }
    }
}