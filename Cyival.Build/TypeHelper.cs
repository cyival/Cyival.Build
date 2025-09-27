using System.Reflection;

namespace Cyival.Build;

public static class TypeHelper
{
    public static T MergeStructs<T>(T baseInstance, T partialInstance) where T : struct
    {
        // Get the type of the struct
        var type = typeof(T);
    
        // Create a new instance to hold the final merged values.
        // This defaults to a copy of the baseInstance.
        var merged = baseInstance;

        // Get all public fields and properties of the struct
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanWrite);

        // Iterate through fields
        foreach (var field in fields)
        {
            var valueFromPartial = field.GetValue(partialInstance);
            // Check if the value from the partial instance is not the default
            if (!IsDefaultValue(valueFromPartial))
            {
                // Apply the value from the partial instance to the merged copy
                field.SetValueDirect(__makeref(merged), valueFromPartial);
            }
        }

        // Iterate through properties
        foreach (var prop in properties)
        {
            var valueFromPartial = prop.GetValue(partialInstance);
            if (!IsDefaultValue(valueFromPartial))
            {
                prop.SetValue(merged, valueFromPartial);
            }
        }

        // Return the fully merged struct
        return merged;
    }

    public static T MergeStructsConsiderate<T>(T baseInstance, T partialInstance) where T : struct
    {
        // Get the type of the struct
        var type = typeof(T);
    
        // Create a new instance to hold the final merged values.
        // This defaults to a copy of the baseInstance.
        var merged = baseInstance;

        // Get all public fields and properties of the struct
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanWrite);

        // Iterate through fields
        foreach (var field in fields)
        {
            var valueFromPartial = field.GetValue(partialInstance);
            // Check if the value from the partial instance is not the default
            if (!IsConsideredDefault(valueFromPartial, field.FieldType))
            {
                // Apply the value from the partial instance to the merged copy
                field.SetValueDirect(__makeref(merged), valueFromPartial!);
            }
        }

        // Iterate through properties
        foreach (var prop in properties)
        {
            var valueFromPartial = prop.GetValue(partialInstance);
            if (!IsConsideredDefault(valueFromPartial, prop.PropertyType))
            {
                prop.SetValue(merged, valueFromPartial);
            }
        }

        // Return the fully merged struct
        return merged;
    }

    
    // Helper method to determine if a value is the default for its type
    private static bool IsDefaultValue(object? value)
    {
        if (value is null) return true;
        Type valueType = value.GetType();
        return valueType.IsValueType && value.Equals(Activator.CreateInstance(valueType));
    }
    
    // A more nuanced helper method to determine if a value should be considered "default" for merging purposes.
    private static bool IsConsideredDefault(object? value, Type fieldType)
    {
        if (value is null) return true;

        // Special handling for bool: false is a meaningful value and should NOT be treated as default for merging.
        if (fieldType == typeof(bool))
        {
            // For a bool, the only "default" state is if we cannot determine if it was set.
            // Since we are working with a struct, a bool will always be true or false.
            // We return false, meaning "do not consider it default", so that any value (true or false) from the partial instance overrides the base.
            return false;
        }
    
        // Special handling for bool? (Nullable<bool>)
        if (fieldType == typeof(bool?))
        {
            var nullableBool = (bool?)value;
            return !nullableBool.HasValue; // Only consider it default if it's null (unset).
        }

        // For other value types, compare against the default value.
        if (fieldType.IsValueType)
        {
            return value.Equals(Activator.CreateInstance(fieldType));
        }

        // For reference types (like string), null is default.
        return false;
    }
}