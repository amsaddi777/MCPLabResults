using Oracle.ManagedDataAccess.Client; // or System.Data.OracleClient
using System.Text;

public static class OracleCommandExtensions
{
    public static string GetCommandTextWithParameters(this OracleCommand command)
    {
        var commandText = command.CommandText;
        var builder = new StringBuilder(commandText);

        foreach (OracleParameter parameter in command.Parameters)
        {
            // The parameter name in the CommandText must be preceded by a colon (:)
            string parameterName = parameter.ParameterName;
            if (!parameterName.StartsWith(":"))
            {
                parameterName = ":" + parameterName;
            }

            // Replace the parameter placeholder with its value in the string
            // Note: This simple replacement doesn't handle complex data types or potential formatting issues perfectly
            string parameterValue = parameter.Value == null || parameter.Value == DBNull.Value
                                    ? "NULL"
                                    : FormatParameterValue(parameter.Value, parameter.OracleDbType);

            // This is a naive replacement and may fail if parameter names are substrings of other words
            // For better reliability, use a more robust string replacement or regex
            builder.Replace(parameterName, parameterValue);
        }

        return builder.ToString();
    }

    private static string FormatParameterValue(object value, OracleDbType dbType)
    {
        // Add quotes for string-like types, format dates, etc.
        switch (dbType)
        {
            case OracleDbType.Char:
            case OracleDbType.Varchar2:
            case OracleDbType.NChar:
            case OracleDbType.NVarchar2:
            case OracleDbType.Date: // Oracle Date types might need specific formatting
            case OracleDbType.TimeStamp:
                return $"'{value.ToString().Replace("'", "''")}'"; // Escape single quotes
            case OracleDbType.Int16:
            case OracleDbType.Int32:
            case OracleDbType.Int64:
            case OracleDbType.Decimal:
            case OracleDbType.Double:
            case OracleDbType.Single:
                return value.ToString();
            // Handle other types as needed (e.g., BLOB, CLOB)
            default:
                return $"'{value.ToString().Replace("'", "''")}'";
        }
    }
}
