using LabResultMcpServer.Models;
using Oracle.ManagedDataAccess.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LabResultMcpServer.Services;

public class LabResultService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<LabResultService> _logger;

    public LabResultService(IConfiguration configuration, ILogger<LabResultService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<LabResultResponse> FetchLabResultsAsync(string patientId, string nda, DateRange? dateRange)
    {
        var response = new LabResultResponse();

        using var connection = new OracleConnection(_configuration.GetConnectionString("OracleDb"));
        await connection.OpenAsync();

        // Fetch lab results (contains all patient information)
        var resultsQuery = @"
            SELECT T.NIP, T.NOM||' '||T.PRENOM NAME, J.NDA, B.REF_LABO, C.LIBELLE CATEGORY, SC.LIBELLE SUBCATEGORY, E.LIBELLE_RECU TEST_NAME, S.LIBELLE, E.DATEHEURE DATE_PERFORMED, P.DH_DEBUT SAMPLE_DATE,
TO_CHAR(N.RESULT) RESULT, U.LIBELLE UNIT, N.LIMITES NORMAL_RANGE      
FROM RE_BILAN B, RE_ANALYSIS A, RE_EXA E, C_EXA_STAT S, C_EXA_REF R, C_RUB_EXA SC, C_RUB_EXA C, RE_PRELEV P, PATIENT T, SEJOUR J,RE_NUM N, C_EXA_UNIT U
WHERE B.NISEJOUR = J.NISEJOUR AND J.NIPATIENT = T.NIPATIENT AND B.NIBILAN = A.NIBILAN AND A.NIANALYSIS = E.NIANALYSIS AND E.NIEXA_REF = R.NIEXA_REF AND E.NIPRELEV = P.NIPRELEV
AND R.RUBRIQUE = SC.NIRUBRIQUE(+) AND SC.RUBRIQUE_SUP = C.NIRUBRIQUE(+) AND E.STATUT = S.NI AND N.NIEXA = E.NIEXA AND N.UNITE=U.NI(+) AND E.TYPE_RESULT=1 AND T.NIP=:id 
AND (:start IS NULL OR E.DATEHEURE>=:start) AND (:end IS NULL OR E.DATEHEURE<=:end)
AND (:nda IS NULL OR J.NDA=:nda)
UNION ALL
(SELECT T.NIP, T.NOM||' '||T.PRENOM NAME, J.NDA, B.REF_LABO, C.LIBELLE CATEGORY, SC.LIBELLE SUBCATEGORY, E.LIBELLE_RECU TEST_NAME, S.LIBELLE, E.DATEHEURE DATE_PERFORMED, P.DH_DEBUT SAMPLE_DATE,
PENSOINS.ITB_RE_GET_RESULT (E.NIEXA) RESULT, U.LIBELLE UNIT, N.LIMITES NORMAL_RANGE      
FROM RE_BILAN B, RE_ANALYSIS A, RE_EXA E, C_EXA_STAT S, C_EXA_REF R, C_RUB_EXA SC, C_RUB_EXA C, RE_PRELEV P, PATIENT T, SEJOUR J,RE_TEXT N, C_EXA_UNIT U
WHERE B.NISEJOUR = J.NISEJOUR AND J.NIPATIENT = T.NIPATIENT AND B.NIBILAN = A.NIBILAN AND A.NIANALYSIS = E.NIANALYSIS AND E.NIEXA_REF = R.NIEXA_REF AND E.NIPRELEV = P.NIPRELEV
AND R.RUBRIQUE = SC.NIRUBRIQUE(+) AND SC.RUBRIQUE_SUP = C.NIRUBRIQUE(+) AND E.STATUT = S.NI AND N.NIEXA = E.NIEXA AND N.UNITE=U.NI(+) AND E.TYPE_RESULT=2 AND T.NIP=:id 
AND (:start IS NULL OR E.DATEHEURE>=:start) AND (:end IS NULL OR E.DATEHEURE<=:end)
AND (:nda IS NULL OR J.NDA=:nda))
ORDER BY 2,3,6";

        using var cmd = new OracleCommand(resultsQuery, connection);
        cmd.Parameters.Add(new OracleParameter("id", patientId));
        var startParam = dateRange?.Start != null ? (object)int.Parse(dateRange.Start.Value.ToString("yyyyMMdd")) : DBNull.Value;
        var endParam = dateRange?.End != null ? (object)int.Parse(dateRange.End.Value.ToString("yyyyMMdd")) : DBNull.Value;
        cmd.Parameters.Add(new OracleParameter("start", startParam));
        cmd.Parameters.Add(new OracleParameter("end", endParam));
        cmd.Parameters.Add(new OracleParameter("nda", nda ?? (object)DBNull.Value));

        using var reader = await cmd.ExecuteReaderAsync();
        bool patientInfoSet = false;
        
        while (await reader.ReadAsync())
        {
            // Set patient info from the first row
            if (!patientInfoSet)
            {
                response.Patient = new PatientInfo
                {
                    Id = reader.GetString(0),
                    Name = reader.GetString(1),
                    Nda = reader.GetString(2),
                    SampleDate = reader.GetDateTime(9)
                };
                patientInfoSet = true;
            }

            // Add lab result
            response.Results.Add(new LabResult
            {
                Category = reader.GetString(4),
                Subcategory = reader.GetString(5),
                TestName = reader.GetString(6),
                Value = reader.GetString(10),
                Unit = reader.GetString(11),
                NormalRange = reader.GetString(12),
                Status = reader.GetString(7),
                DatePerformed = reader.GetDateTime(8),
                ValidatedBy = reader.GetString(3)
            });
        }

        if (!patientInfoSet)
        {
            throw new KeyNotFoundException("Patient not found");
        }

        return response;
    }
}