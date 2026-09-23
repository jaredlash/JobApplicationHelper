using System.ComponentModel.DataAnnotations;

namespace JobApplicationHelper.Models;

public sealed class RequiredIfAttribute : ValidationAttribute
{
    public RequiredIfAttribute(string propertyName, object expectedValue)
    {
        PropertyName = propertyName;
        ExpectedValue = expectedValue;
    }

    public string PropertyName { get; }

    public object ExpectedValue { get; }

    protected override ValidationResult? IsValid(
        object? value,
        ValidationContext validationContext)
    {
        var property = validationContext.ObjectType.GetProperty(PropertyName);

        if (property is null)
        {
            throw new InvalidOperationException(
                $"Property '{PropertyName}' was not found on " +
                $"{validationContext.ObjectType.Name}.");
        }

        var otherValue = property.GetValue(validationContext.ObjectInstance);

        // Condition isn't met, so this property isn't required.
        if (!Equals(otherValue, ExpectedValue))
            return ValidationResult.Success;

        // Condition is met, so the value must be non-empty.
        if (value is string stringValue)
        {
            return string.IsNullOrWhiteSpace(stringValue)
                ? new ValidationResult(
                    ErrorMessage ?? $"{validationContext.DisplayName} is required.",
                    [validationContext.MemberName!])
                : ValidationResult.Success;
        }

        return value is null
            ? new ValidationResult(
                ErrorMessage ?? $"{validationContext.DisplayName} is required.",
                [validationContext.MemberName!])
            : ValidationResult.Success;
    }
}