using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

// We shoulde not use any NuGet for this scope.

// ************************************************************* Explain This Class *************************************************************
// **                                                                                                                                          **
// **     This class create for : Build simple JSON text, Convert complex JSON to simple text and get only optional value in JSON text         **
// **                                                                                                                                          **
// **     Public Methodes :                                                                                                                    **
// **                        => BuildJsonFromText : Get two string input 1. string value from user                                             **
// **                                                                    2. string key from user                                               **
// **                                               Get key and text from user to make JSON text. (you can use (.) in key for multi tag)       **
// **                                                                                                                                          **
// **                        => FlattenJsonToText : Get JSON text and convert all data to simple text.                                         **
// **                                               Use private helper method ExtractStringValuesRecursive for extract all data from JSON      **
// **                                               text.                                                                                      **
// **                        => GetJsonValuesByKey : Get two string input 1. JSON text                                                         **
// **                                                                     2. string key from user.                                             **
// **                                                Get JSON text and key.                                                                    **
// **                                                Search all tag and find that tag user write in key and return value to string.            **
// **                                                Use private helper method FindValuesByKeyRecursive for find that exact tag and return     **
// **                                                value to string.                                                                          **
// **     Private Methodes :                                                                                                                   **
// **                        => ExtractStringValuesRecursive : Get three string input 1. That Parse JSON text from user.                       **
// **                                                                                 2. String builder for make string from JSON value.       **
// **                                                                                 3. Indent. (white space for make tree view)              **
// **                                               Check 3 mod => 1. node is object  2. node is arry  3. node is simple type.                 **
// **                                               in every mod extract JSON value to string.                                                 **
// **                                                                                                                                          **
// **                        => FindValuesByKeyRecursive : Get four string input 1. That Parse JSON text from user.                            **
// **                                                                            2. String key from user.                                      **
// **                                                                            3. String builder for make string from JSON value.            **
// **                                                                            4. Boolian check root. (for tree view)                        **
// **                                                      Check 2 mod => 1. node is object  2. node is arry                                   **
// **                                                      in every mod search tags that similar to key and return their value.                **
// **                                                                                                                                          **
// **********************************************************************************************************************************************  

namespace MyUtilities
{
    public static class SimpleFileManager
    {
        /// <summary>
        /// Builds a JSON structure by assigning a provided value to a specified key path.
        /// Supports nested key paths using dot-notation (e.g., "user.address.city").
        /// </summary>
        /// <param name="value">
        /// The text value to insert into the resulting JSON structure.
        /// </param>
        /// <param name="keyPath">
        /// A key or nested key path indicating where the value should be placed.
        /// </param>
        /// <returns>
        /// A formatted JSON string containing the generated structure;  
        /// or an error message if the JSON creation process fails.
        /// </returns>
        /// <remarks>
        /// This method dynamically constructs intermediate JSON objects when  
        /// the provided key path represents nested properties.
        /// </remarks>
        public static string BuildJsonFromText(string value, string keyPath)
        {
            var jsonObject = new JsonObject();
            JsonNode? currentNode = jsonObject;

            // if user use string with (.) in keyPath we create new object in our JSON file
            if (keyPath.Contains('.'))
            {
                var keys = keyPath.Split('.');

                for (int i = 0; i < keyPath.Length; i++)
                {
                    var currentKey = keys[i];

                    if (currentNode is null) 
                        return "Empty";

                    // if user use (.) is keyPath string we should check it for craete new object ( example => keyPath = "user.address. city" ) 

                    // this condition is for last part.  in this example this condition is for city and add string value (user text) in this scope 
                    if (i == keys.Length - 1) 
                        currentNode[currentKey] = value;

                    // in this scope build tree view. in this example ==> { "user": { "address": { ... }}}
                    else
                    {
                        if (currentNode[currentKey] is null || currentNode[currentKey]!.GetValueKind() is not JsonValueKind.Object)
                            currentNode[currentKey] = new JsonObject();

                        currentNode = currentNode[currentKey];
                    }
                }
            }
            // if we dont have (.) in keyPath 
            else
            {
                jsonObject[keyPath] = value;
            }
            return jsonObject.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
        }

        /// <summary>
        /// Scans a complex JSON structure and aggregates all string values  
        /// into a single flattened text output.
        /// </summary>
        /// <param name="json">
        /// A valid JSON string containing any combination of objects or arrays.
        /// </param>
        /// <returns>
        /// A concatenated text representation of all extracted string values;  
        /// or an error message if JSON parsing fails.
        /// </returns>
        /// <remarks>
        /// The method performs a full recursive traversal and extracts  
        /// only string-type values, preserving their order of appearance.
        /// </remarks>
        public static string FlattenJsonToText(string json)
        {
            try
            {
                // Parse input json into rootNode.
                var rootNode = JsonNode.Parse (json);

                // Check if user input was null return error message.
                if (rootNode is null) 
                    return "Error: Invalid JSON input.";

                // Create a string builder.
                // Use helper method and get all data in JSON and return them with string type to this method.
                // Return string.
                var builder = new StringBuilder();
                ExtractStringValuesRecursive(rootNode, builder);
                return builder.ToString();
            }

            catch (JsonException ex)
            {
                return $"Error parsing JSON : {ex.Message}";
            }
            catch (Exception ex)
            {
                return $"An error occurred: {ex.Message}";
            }            
        }

        /// <summary>
        /// Searches a JSON string recursively and retrieves all string values  
        /// associated with the specified key.
        /// </summary>
        /// <param name="json">
        /// The JSON string to be inspected.
        /// </param>
        /// <param name="key">
        /// The target key whose associated values should be retrieved.
        /// </param>
        /// <returns>
        /// A text block containing all matching values separated by newlines,  
        /// or an error message if the key does not exist or the JSON is invalid.
        /// </returns>
        /// <remarks>
        /// Key comparison is case-sensitive.  
        /// The search traverses objects and arrays at all nesting levels.
        /// </remarks>
        public static string GetJsonValuesByKey(string json, string key)
        {
            try
            {
                // Parse input json into rootNode.
                var rootNode = JsonNode.Parse(json);

                // Check if user input was null return error message.
                if (rootNode is null) 
                    return "Error: Could not parse JSON or JSON is empty.";

                // Create a string builder.
                // Use helper method and get all data in JSON and return them with string type to this method.
                var stringBuilder = new StringBuilder();
                var found = FindValuesByKeyRecursive(rootNode, key, stringBuilder);

                // If helper method return false return error message.
                if (found is false) 
                    return $"Key '{key}' not found";

                // If length of string is bigger than 0 (check string was not "") return string.
                // Else return error message.
                return stringBuilder.Length > 0 ? stringBuilder.ToString().Trim() : $"Key '{key}' found, but it has no simple string value.";
            }

            catch (JsonException ex)
            {
                return $"Invalid JSON format: {ex.Message}";
            }
            catch (Exception ex)
            {
                return $"An error occurred: {ex.Message}";
            }
        }


        //-------------------------------------------------------------------------- Private Methodes --------------------------------------------------------------------------//


        /// <summary>
        /// Recursively traverses a JSON node to extract all string values  
        /// and appends them to a <see cref="StringBuilder"/> instance.
        /// </summary>
        /// <param name="node">
        /// The <see cref="JsonNode"/> currently being processed.
        /// </param>
        /// <param name="builder">
        /// The target <see cref="StringBuilder"/> that receives all extracted values.
        /// </param>
        /// <param name="indent">
        /// Optional indentation applied to improve readability in formatted scenarios.
        /// </param>
        /// <remarks>
        /// This method is invoked internally by <c>FlattenJsonToText</c>  
        /// and is not intended for public use.
        /// </remarks>
        private static void ExtractStringValuesRecursive(JsonNode node, StringBuilder builder, string indent = "")
        {
            // Check if user input parse was null break.
            if (node is null) 
                return;

            // Mod 1 : node is object.
            if (node is JsonObject obj)
            {
                // Check every object.
                foreach (var kvp in obj)
                {
                    // If object node value have object and arry create line with white space and key. (white space for create tree view)
                    // After that return object value to this method agian.
                    if (kvp.Value is JsonObject || kvp.Value is JsonArray)
                    {
                        builder.AppendLine($"{indent}\"{kvp.Key}\":");
                        ExtractStringValuesRecursive(kvp.Value, builder, indent + "  ");
                    }

                    // If object node value was simple get key and value and write them to string.
                    else
                    {
                        builder.AppendLine($"{indent}\"{kvp.Key}\": {kvp.Value?.ToString() ?? ""}");
                    }
                }
            }
            // Mod 2 : node is arry.
            else if (node is JsonArray arr)
            {
                // Check all arry.
                for (int i = 0; i < arr.Count; i++)
                {
                    // Until all arry check store value of them in element.
                    var element = arr[i];

                    // If object node value have object and arry create line with white space and index. (white space for create tree view)
                    // After that return object value to this method agian.
                    if (element is JsonObject || element is JsonArray)
                    {
                        builder.AppendLine($"{indent} [{i}] :");
                        ExtractStringValuesRecursive(element, builder, indent + "  ");
                    }

                    // If object node value was simple get indext and value and write them to string.
                    else
                    {
                        builder.AppendLine($"{indent} [{i}] : {element?.ToString() ?? "Empty"}");
                    }
                }
            }
            // Mod 3 : node is not object or arry.
            else
            {
                builder.AppendLine($"{indent} {node?.ToString() ?? "Empty"}");
            }
        }

        /// <summary>
        /// Recursively searches a JSON node for occurrences of a key  
        /// and appends its associated string values to a <see cref="StringBuilder"/>.
        /// </summary>
        /// <param name="node">
        /// The <see cref="JsonNode"/> currently being scanned.
        /// </param>
        /// <param name="key">
        /// The key to search for in the JSON hierarchy.
        /// </param>
        /// <param name="builder">
        /// The <see cref="StringBuilder"/> that accumulates matching values.
        /// </param>
        /// <param name="isRoot">
        /// Indicates whether the current call originates from the first recursion level.
        /// </param>
        /// <returns>
        /// <c>true</c> if the key is found in any node;  
        /// otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method is intended solely for internal use  
        /// by the <c>GetJsonValuesByKey</c> method.
        /// </remarks>
        private static bool FindValuesByKeyRecursive(JsonNode? node, string key, StringBuilder builder)
        {
            // Create boolian type and default is false.
            bool found = false;

            // Check if user input parse was null return false to main method.
            if (node is null) 
                return false;

            // Mod 1 : node is object.
            if (node is JsonObject obj)
            {
                // Check every object.
                foreach (var kvp in obj)
                {
                    // Check if input key is equal to JSON key.
                    if (kvp.Key.Equals(key, StringComparison.OrdinalIgnoreCase))
                    {
                        // If object is not null, and it's value is not object and arry write key and value.
                        // Make found true.
                        if (kvp.Value is not null || kvp.Value?.GetValueKind() is not JsonValueKind.Object || kvp.Value?.GetValueKind() is not JsonValueKind.Array)
                        {
                            builder.AppendLine($"{kvp.Key} : {kvp.Value}");
                            found = true;
                        }
                    }

                    // If object value is object or arry check all condition if was right make found true.
                    if (kvp.Value is JsonObject || kvp.Value is JsonArray)
                    {
                        if (FindValuesByKeyRecursive(kvp.Value, key, builder)) 
                            found = true;
                    }
                }
            }
            // Mod 2 : node is arry.
            else if (node is JsonArray arr)
            {
                // Check all arry.
                for (int i = 0; i < arr.Count; i++)
                {
                    // Check all condition if was right make found true.
                    if (FindValuesByKeyRecursive(arr[i], key, builder)) 
                        found = true;
                }
            }

            // At the end return found to main method.
            return found;
        }
    }
}