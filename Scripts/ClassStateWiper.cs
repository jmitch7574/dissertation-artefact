using System;
using System.Reflection;
using Godot;

public static class ClassStateWiper
{
    public static void Unload(Type type, object? instance, bool isRecursive)
    {
        if (type.IsGenericType)
            return;

        foreach (
            var field in type.GetFields(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static
            )
        )
        {
            try
            {
                var value = field.GetValue(instance);
                if (value is not null)
                {
                    if (value is IDisposable disposable)
                        disposable.Dispose();

                    if (isRecursive)
                        Unload(field.FieldType, value, isRecursive);
                }
            }
            catch (Exception ex)
            {
                GD.Print($"Failed to dispose '{type.Name}.{field.Name}', ex: {ex}");
            }

            if (isRecursive)
                Unload(field.FieldType, null, isRecursive);
        }

        foreach (
            var property in type.GetProperties(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static
            )
        )
        {
            try
            {
                var value = property.GetValue(instance);
                if (value is not null)
                {
                    if (value is IDisposable disposable)
                        disposable.Dispose();

                    if (isRecursive)
                        Unload(property.PropertyType, value, isRecursive);
                }
            }
            catch (Exception ex)
            {
                GD.Print($"Failed to dispose '{type.Name}.{property.Name}', ex: {ex}");
            }

            if (isRecursive)
                Unload(property.PropertyType, null, isRecursive);
        }
    }
}
