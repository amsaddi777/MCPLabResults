using System.Text;
using System.Text.Json;

namespace LabResultAgent.Tools;

/// <summary>
/// Formats lab result JSON data into human-readable summaries
/// that help the LLM generate better responses.
/// </summary>
public static class LabResultFormatter
{
    /// <summary>
    /// Formats raw lab result JSON into a structured text summary
    /// highlighting abnormal values for the LLM to use in its response.
    /// </summary>
    public static string FormatForLlm(string rawJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(rawJson);
            var root = doc.RootElement;

            var sb = new StringBuilder();

            // Patient info
            if (root.TryGetProperty("Patient", out var patient) ||
                root.TryGetProperty("patient", out patient))
            {
                sb.AppendLine("=== PATIENT INFORMATION ===");
                if (patient.TryGetProperty("Name", out var name) ||
                    patient.TryGetProperty("name", out name))
                    sb.AppendLine($"Name: {name.GetString()}");
                if (patient.TryGetProperty("Id", out var id) ||
                    patient.TryGetProperty("id", out id))
                    sb.AppendLine($"ID: {id.GetString()}");
                if (patient.TryGetProperty("Nda", out var nda) ||
                    patient.TryGetProperty("nda", out nda))
                    sb.AppendLine($"NDA: {nda.GetString()}");
                sb.AppendLine();
            }

            // Lab results
            if (root.TryGetProperty("Results", out var results) ||
                root.TryGetProperty("results", out results))
            {
                var abnormals = new List<string>();
                int totalCount = 0;

                sb.AppendLine("=== LAB RESULTS ===");

                foreach (var result in results.EnumerateArray())
                {
                    totalCount++;
                    var testName = GetString(result, "TestName", "testName");
                    var value = GetString(result, "Value", "value");
                    var unit = GetString(result, "Unit", "unit");
                    var normalRange = GetString(result, "NormalRange", "normalRange");
                    var status = GetString(result, "Status", "status");
                    var category = GetString(result, "Category", "category");

                    var flag = "";
                    if (status == "H") flag = " [HIGH]";
                    else if (status == "L") flag = " [LOW]";

                    sb.AppendLine($"  {testName}: {value} {unit}{flag}");
                    if (!string.IsNullOrWhiteSpace(normalRange))
                        sb.AppendLine($"    Normal range: {normalRange}");

                    if (!string.IsNullOrWhiteSpace(status) && status != "")
                        abnormals.Add($"{testName}: {value} {unit} ({status})");
                }

                sb.AppendLine();
                sb.AppendLine($"Total results: {totalCount}");

                if (abnormals.Count > 0)
                {
                    sb.AppendLine();
                    sb.AppendLine("=== ABNORMAL VALUES (ATTENTION REQUIRED) ===");
                    foreach (var abnormal in abnormals)
                        sb.AppendLine($"  ⚠ {abnormal}");
                }
                else
                {
                    sb.AppendLine();
                    sb.AppendLine("All values are within normal ranges.");
                }
            }

            return sb.ToString();
        }
        catch
        {
            // If parsing fails, return the raw JSON
            return rawJson;
        }
    }

    private static string GetString(JsonElement element, params string[] propertyNames)
    {
        foreach (var name in propertyNames)
        {
            if (element.TryGetProperty(name, out var prop))
                return prop.GetString() ?? "";
        }
        return "";
    }
}
