using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Linq.Dynamic.Core;

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
                return collection.OrderBy(x => 1);
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
                    // skip this criterion if the property does not exist on the type T.
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

        public static IQueryable<T> ApplyDatabaseSorting<T>(this IQueryable<T> query, string? orderBy, string defaultSortProperty) where T : class
        {
            if (string.IsNullOrWhiteSpace(defaultSortProperty))
            {
                throw new ArgumentException("Default sort property cannot be null or empty for ApplySorting.", nameof(defaultSortProperty));
            }

            IOrderedQueryable<T>? orderedQuery = null;

            if (!string.IsNullOrWhiteSpace(orderBy))
            {
                var sortCriteria = orderBy.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                foreach (var criterion in sortCriteria)
                {
                    string[] parts = criterion.Split('_', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                    if (parts.Length == 2)
                    {
                        string propertyName = parts[0];
                        string direction = parts[1].ToLowerInvariant();


                        if (typeof(T) == typeof(InventoryManagementAPI.Models.Product))
                        {
                            propertyName = propertyName.ToLowerInvariant() switch
                            {
                                "categoryname" => "Category.CategoryName",
                                "productname" => "ProductName",
                                "unitprice" => "UnitPrice",
                                "sku" => "SKU",
                                "description" => "Description",
                                "isdeleted" => "IsDeleted",
                                "categoryid" => "CategoryId",
                                "productid" => "ProductId",
                                _ => throw new ArgumentException($"Invalid property name: {propertyName}")
                            };
                        }

                        if (typeof(T) == typeof(InventoryManagementAPI.Models.User))
                        {
                            propertyName = propertyName.ToLowerInvariant() switch
                            {
                                "userid" => "UserId",
                                "username" => "Username",
                                "email" => "Email",
                                "phone" => "Phone",
                                "roleid" => "RoleId",
                                "rolename" => "Role.RoleName",
                                "isdeleted" => "IsDeleted",
                                _ => throw new ArgumentException($"Invalid property name: {propertyName}")
                            };
                        }


                        if (typeof(T) == typeof(InventoryManagementAPI.Models.InventoryProduct))
                        {
                            propertyName = propertyName.ToLowerInvariant() switch
                            {
                                "id" => "Id",
                                "inventoryid" => "InventoryId",
                                "inventoryname" => "Inventory.Name",
                                "inventorylocation" => "Inventory.Location",
                                "productid" => "ProductId",
                                "productname" => "Product.ProductName",
                                "categoryid" => "Product.CategoryId",
                                "categoryname" => "Product.Category.CategoryName",
                                "productsku" => "Product.SKU",
                                "quantity" => "Quantity",
                                "minstockquantity" => "MinStockQuantity",
                                _ => throw new ArgumentException($"Invalid property name: {propertyName}")
                            };
                        }




                        // // Build dynamic sort expression
                        // var propertyInfo = typeof(T).GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                        // if (propertyInfo == null)
                        // {
                        //     throw new ArgumentException($"Invalid property name: {propertyName}");
                        // }

                        // // Check if the property type is string
                        // bool isStringProperty = propertyInfo.PropertyType == typeof(string);

                        // string sortExpression = isStringProperty
                        //     ? $"{propertyName}.ToLower() {(direction == "desc" ? "descending" : "ascending")}"
                        //     : $"{propertyName} {(direction == "desc" ? "descending" : "ascending")}";

                        // var propertyType = typeof(T).GetProperty(propertyName)?.PropertyType;
                        // Console.WriteLine($"Property Type: {propertyType}, Property Name: {propertyName}, Direction: {direction}");
                        // Console.WriteLine($"Type of T: {typeof(T)}");

                        string sortExpression;

                        string[] propertyParts = propertyName.Split('.');
                        Type currentType = typeof(T);
                        PropertyInfo propertyInfo = null;

                        foreach (var part in propertyParts)
                        {
                            propertyInfo = currentType.GetProperty(part);
                            if (propertyInfo == null)
                            {
                                break;
                            }
                            currentType = propertyInfo.PropertyType;
                        }

                        Type propertyType = propertyInfo?.PropertyType;

                        if (propertyType == typeof(string))
                        {
                            Console.WriteLine($"Sorting by string property: {propertyName}");
                            sortExpression = $"{propertyName}.ToLower() {(direction == "desc" ? "descending" : "ascending")}";
                        }
                        else
                        {
                            sortExpression = $"{propertyName} {(direction == "desc" ? "descending" : "ascending")}";
                        }

                        //string sortExpression = $"{propertyName}.ToLower() {(direction == "desc" ? "descending" : "ascending")}";

                        // Apply ordering
                        if (orderedQuery == null)
                        {
                            orderedQuery = query.OrderBy(sortExpression); // First sort
                        }
                        else
                        {
                            orderedQuery = orderedQuery.ThenBy(sortExpression); // Subsequent sorts
                        }
                    }
                }
            }

            return orderedQuery ?? query.OrderBy(defaultSortProperty);
        }
    }
}
