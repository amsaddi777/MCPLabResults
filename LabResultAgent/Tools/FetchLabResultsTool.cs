using System.ComponentModel;
using System.Text.Json;
using LabResultAgent.Services;
using Microsoft.Extensions.AI;

namespace LabResultAgent.Tools;

/// <summary>
/// Provides the fetch_patient_lab_results tool as an AIFunction
/// for the Microsoft Agent Framework. This bridges MCP tool calls
/// into the Agent Framework's tool-calling system.
/// </summary>
public static class FetchLabResultsTool
{
    /// <summary>
    /// Creates an AIFunction that wraps the MCP fetch_patient_lab_results tool call.
    /// </summary>
    public static AIFunction Create(McpClientService mcpClient, ILogger logger)
    {
        return AIFunctionFactory.Create(
            async (
                [Description("The patient ID to fetch lab results for")] string patientId,
                [Description("Optional NDA (admission number) for filtering results")] string nda = null,
                [Description("Optional start date for filtering results (format: yyyy-MM-dd)")] string startDate = null,
                [Description("Optional end date for filtering results (format: yyyy-MM-dd)")] string endDate = null,
                CancellationToken cancellationToken = default) =>
            {
                logger.LogInformation(
                    "Tool invoked: fetch_patient_lab_results(patientId={PatientId}, nda={Nda}, startDate={Start}, endDate={End})",
                    patientId, nda ?? "null", startDate ?? "null", endDate ?? "null");

                try
                {
                    var result = await mcpClient.FetchPatientLabResultsAsync(
                        patientId, nda, startDate, endDate, cancellationToken);

                    // Some MCP servers return a wrapper JSON like { CallId, Result }
                    // where Result contains either a human-readable string or an
                    // escaped JSON string. Unwrap that to get the inner payload.
                    try
                    {
                        using var wrapperDoc = JsonDocument.Parse(result);
                        var root = wrapperDoc.RootElement;
                        if (root.TryGetProperty("Result", out var inner))
                        {
                            // If Result is a JSON string containing the actual payload,
                            // extract its unescaped string value.
                            if (inner.ValueKind == JsonValueKind.String)
                                result = inner.GetString() ?? result;
                        }
                    }
                    catch
                    {
                        // Not a wrapper JSON, proceed with original result string
                    }

                    // If the inner result is structured JSON, format it for the LLM.
                    try
                    {
                        using var doc = JsonDocument.Parse(result);
                        // If it looks like the structured lab-result JSON, format it.
                        var formatted = LabResultFormatter.FormatForLlm(result);
                        return formatted;
                    }
                    catch
                    {
                        logger.LogWarning("MCP returned non-JSON result or unstructured text; returning raw text");
                        return result;
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error fetching lab results for patient {PatientId}", patientId);
                    return JsonSerializer.Serialize(new
                    {
                        error = true,
                        message = $"Failed to fetch lab results: {ex.Message}"
                    });
                }
            },
            name: "fetch_patient_lab_results",
            description: "Fetches laboratory results for a patient from the hospital database. " +
                         "Always requires a patientId. Optionally filter by NDA (admission number) " +
                         "and/or date range. Returns structured JSON with patient info and lab results " +
                         "including test names, values, units, normal ranges, and abnormal flags (H/L)."
        );
    }
}
