using LabResultMcpServer.Models;
using LabResultMcpServer.Services;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace LabResultMcpServer;

[McpServerToolType]
public class LabResultTool
{
    private readonly LabResultService _service;

    public LabResultTool(LabResultService service)
    {
        _service = service;
    }

    [McpServerTool(Name = "fetch_patient_lab_results"), Description("Fetches laboratory results for a patient from the database.")]
    public async Task<string> FetchLabResults(
        [Description("The patient ID to fetch results for")] string patientId,
        [Description("Optional NDA for filtering results")] string nda = null,
        [Description("Optional date range for filtering results")] DateRange? dateRange = null)
    {
        var result = await _service.FetchLabResultsAsync(patientId, nda, dateRange);
        return System.Text.Json.JsonSerializer.Serialize(result);
    }
}