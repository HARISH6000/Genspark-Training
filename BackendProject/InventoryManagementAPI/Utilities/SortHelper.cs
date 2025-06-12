using System.Collections.Generic;
using System.Linq;
using System.Reflection; // Required for PropertyInfo and GetProperty

namespace InventoryManagementAPI.Utilities
{
    public static class SortHelper
    {
        
        /// Example sortBy: "username_asc,id_desc" or "name,location_desc"
        
        ///"FieldName_Direction"
        public static IOrderedEnumerable<T> ApplySorting<T>(IEnumerable<T> collection, string? sortBy)
        {
            // If collection is null, empty, or no sorting criteria, return a trivially sorted collection (or original).
            if (collection == null || !collection.Any() || string.IsNullOrWhiteSpace(sortBy))
            {
                return collection.OrderBy(x => 1); // Apply a default, non-disruptive order
            }

            IOrderedEnumerable<T> sortedCollection = null;
            var sortCriteria = sortBy.Split(',', System.StringSplitOptions.RemoveEmptyEntries | System.StringSplitOptions.TrimEntries);

            bool isFirstSort = true;

            foreach (var criterion in sortCriteria)
            {
                var parts = criterion.Split('_', System.StringSplitOptions.RemoveEmptyEntries | System.StringSplitOptions.TrimEntries);
                var fieldName = parts[0];
                var direction = parts.Length > 1 ? parts[1].ToLowerInvariant() : "asc";

                // Get the PropertyInfo for the field name
                // BindingFlags.IgnoreCase allows for case-insensitive matching of property names
                PropertyInfo? propertyInfo = typeof(T).GetProperty(fieldName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (propertyInfo == null)
                {
                    // Log a warning or throw an exception if the sort field is invalid
                    // For now, we'll skip this criterion if the property does not exist on the type T.
                    continue;
                }

                // Create a lambda expression for sorting based on the property's value
                // x => propertyInfo.GetValue(x) will get the value of the property for each item.
                // This is cast to IComparable to ensure consistent comparison, assuming properties are comparable.
                System.Func<T, object?> keySelector = x => propertyInfo.GetValue(x);

                if (isFirstSort)
                {
                    if (direction == "desc")
                    {
                        sortedCollection = collection.OrderByDescending(keySelector);
                    }
                    else
                    {
                        sortedCollection = collection.OrderBy(keySelector);
                    }
                    isFirstSort = false;
                }
                else
                {
                    if (direction == "desc")
                    {
                        sortedCollection = sortedCollection.ThenByDescending(keySelector);
                    }
                    else
                    {
                        sortedCollection = sortedCollection.ThenBy(keySelector);
                    }
                }
            }

            // If no valid sorting criteria were applied, ensure a default order is returned
            return sortedCollection ?? collection.OrderBy(x => 1);
        }
    }
}
