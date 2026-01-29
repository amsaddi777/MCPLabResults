using System.ComponentModel.DataAnnotations;

namespace LabResultMcpServer.Models;

public class FetchLabResultsRequest
{
    [Required]
    public string PatientId { get; set; } = string.Empty;

    public DateRange? DateRange { get; set; }
}

public class DateRange
{
    public DateTime? Start { get; set; }
    public DateTime? End { get; set; }
}