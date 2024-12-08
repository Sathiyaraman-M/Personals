using System.Reflection;

namespace Expensive.Common.Constants;

public static class Permissions
{
    public static class Users
    {
        public const string View = "Users.View";
        public const string Create = "Users.Create";
        public const string Update = "Users.Update";
        public const string Delete = "Users.Delete";
        public const string ViewPermissions = "Users.ViewPermissions";
        public const string UpdatePermissions = "Users.UpdatePermissions";
    }

    public static class LookupTypes
    {
        public const string View = "LookupTypes.View";
        public const string Create = "LookupTypes.Create";
        public const string Update = "LookupTypes.Update";
        public const string Delete = "LookupTypes.Delete";
    }

    public static IEnumerable<string> GetAllPermissionCategories()
    {
        var fields = typeof(Permissions).GetNestedTypes();
        foreach (var field in fields)
        {
            yield return field.Name;
        }
    }

    public static IEnumerable<string> GetPermissionsForCategory(string category)
    {
        var fields = typeof(Permissions).GetNestedTypes().FirstOrDefault(x => x.Name == category)
            ?.GetFields(BindingFlags.Public | BindingFlags.Static);
        return fields == null
            ? []
            : fields.Select(x => x.GetValue(null)).Where(x => x is string && !string.IsNullOrWhiteSpace(x.ToString()))
                .Cast<string>();
    }

    public static IEnumerable<string> GetAllPermissions()
    {
        var fields = typeof(Permissions).GetNestedTypes().SelectMany(x =>
            x.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy));
        foreach (var field in fields.Where(f => f.FieldType == typeof(string)))
        {
            yield return field.GetValue(null)?.ToString() ?? string.Empty;
        }
    }
}