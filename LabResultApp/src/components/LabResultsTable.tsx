"use client";

import { LabResult, LabResultResponse } from "@/lib/types";
import { AlertTriangle, CheckCircle, TrendingDown, TrendingUp } from "lucide-react";

interface LabResultsTableProps {
  data: LabResultResponse;
}

export function LabResultsTable({ data }: LabResultsTableProps) {
  const { patient, results } = data;

  // Group results by category
  const grouped = results.reduce(
    (acc, result) => {
      const key = result.category || "Uncategorized";
      if (!acc[key]) acc[key] = [];
      acc[key].push(result);
      return acc;
    },
    {} as Record<string, LabResult[]>
  );

  const abnormalCount = results.filter(
    (r) => r.status === "H" || r.status === "L"
  ).length;

  return (
    <div className="w-full space-y-4">
      {/* Patient header */}
      <div className="bg-blue-50 rounded-lg p-4 border border-blue-200">
        <div className="flex items-center justify-between">
          <div>
            <h3 className="font-semibold text-blue-900">{patient.name}</h3>
            <p className="text-sm text-blue-700">
              ID: {patient.id} | NDA: {patient.nda}
            </p>
          </div>
          <div className="text-right">
            <p className="text-xs text-blue-600">
              Sample: {new Date(patient.sampleDate).toLocaleDateString()}
            </p>
            <p className="text-xs text-blue-600">
              {results.length} results | {abnormalCount} abnormal
            </p>
          </div>
        </div>
      </div>

      {/* Summary bar */}
      {abnormalCount > 0 && (
        <div className="flex items-center gap-2 p-3 bg-amber-50 rounded-lg border border-amber-200">
          <AlertTriangle size={16} className="text-amber-600" />
          <span className="text-sm font-medium text-amber-800">
            {abnormalCount} abnormal value{abnormalCount > 1 ? "s" : ""} detected
          </span>
        </div>
      )}

      {/* Grouped results */}
      {Object.entries(grouped).map(([category, categoryResults]) => (
        <div
          key={category}
          className="bg-white rounded-lg border border-slate-200 overflow-hidden"
        >
          <div className="px-4 py-2 bg-slate-50 border-b border-slate-200">
            <h4 className="text-sm font-semibold text-slate-700">{category}</h4>
          </div>
          <table className="w-full text-sm">
            <thead>
              <tr className="text-left text-xs text-slate-500 border-b border-slate-100">
                <th className="px-4 py-2 font-medium">Test</th>
                <th className="px-4 py-2 font-medium">Value</th>
                <th className="px-4 py-2 font-medium">Unit</th>
                <th className="px-4 py-2 font-medium">Normal Range</th>
                <th className="px-4 py-2 font-medium">Status</th>
              </tr>
            </thead>
            <tbody>
              {categoryResults.map((result, idx) => (
                <tr
                  key={idx}
                  className={`border-b border-slate-50 ${
                    result.status === "H" || result.status === "L"
                      ? "bg-red-50/50"
                      : ""
                  }`}
                >
                  <td className="px-4 py-2 font-medium text-slate-800">
                    {result.testName}
                  </td>
                  <td className="px-4 py-2">
                    <span
                      className={
                        result.status === "H" || result.status === "L"
                          ? "font-bold text-red-700"
                          : "text-slate-700"
                      }
                    >
                      {result.value}
                    </span>
                  </td>
                  <td className="px-4 py-2 text-slate-500">
                    {result.unit || "—"}
                  </td>
                  <td className="px-4 py-2 text-slate-500">
                    {result.normalRange || "—"}
                  </td>
                  <td className="px-4 py-2">
                    <StatusBadge status={result.status} />
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      ))}
    </div>
  );
}

function StatusBadge({ status }: { status: string }) {
  if (status === "H") {
    return (
      <span className="inline-flex items-center gap-1 badge-high">
        <TrendingUp size={12} />
        HIGH
      </span>
    );
  }
  if (status === "L") {
    return (
      <span className="inline-flex items-center gap-1 badge-low">
        <TrendingDown size={12} />
        LOW
      </span>
    );
  }
  return (
    <span className="inline-flex items-center gap-1 badge-normal">
      <CheckCircle size={12} />
      Normal
    </span>
  );
}
