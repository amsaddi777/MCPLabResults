namespace LabResultMcpServer.Models;

public class LabResultResponse
{
    public PatientInfo Patient { get; set; } = new();
    public List<LabResult> Results { get; set; } = new();
}

public class PatientInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Nda { get; set; } = string.Empty;
    public DateTime SampleDate { get; set; }
}

public class LabResult
{
    public string Category { get; set; } = string.Empty;
    public string Subcategory { get; set; } = string.Empty;
    public string TestName { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Unit { get; set; } = string.Empty;
    public string? NormalRange { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // L, H, or empty
    public DateTime DatePerformed { get; set; }
    public string ValidatedBy { get; set; } = string.Empty;
}