"use client";

import { LabResult } from "@/lib/types";
import { TrendingUp, TrendingDown, AlertTriangle } from "lucide-react";

interface LabResultCardProps {
  result: LabResult;
}

export function LabResultCard({ result }: LabResultCardProps) {
  const isAbnormal = result.status === "H" || result.status === "L";
  const isHigh = result.status === "H";

  return (
    <div
      className={`rounded-lg border p-4 ${
        isAbnormal
          ? "border-red-200 bg-red-50"
          : "border-slate-200 bg-white"
      }`}
    >
      <div className="flex items-start justify-between">
        <div>
          <h4 className="font-semibold text-slate-800">{result.testName}</h4>
          <p className="text-xs text-slate-500 mt-0.5">
            {result.category}
            {result.subcategory ? ` / ${result.subcategory}` : ""}
          </p>
        </div>
        {isAbnormal && (
          <div
            className={`flex items-center gap-1 px-2 py-1 rounded-full text-xs font-bold ${
              isHigh
                ? "bg-red-100 text-red-700"
                : "bg-amber-100 text-amber-700"
            }`}
          >
            {isHigh ? <TrendingUp size={12} /> : <TrendingDown size={12} />}
            {isHigh ? "HIGH" : "LOW"}
          </div>
        )}
      </div>

      <div className="mt-3 flex items-baseline gap-2">
        <span
          className={`text-2xl font-bold ${
            isAbnormal ? "text-red-700" : "text-slate-800"
          }`}
        >
          {result.value}
        </span>
        <span className="text-sm text-slate-500">{result.unit || ""}</span>
      </div>

      {result.normalRange && (
        <div className="mt-2 flex items-center gap-1.5 text-xs text-slate-500">
          <AlertTriangle size={12} />
          Normal range: {result.normalRange}
        </div>
      )}

      <div className="mt-2 text-xs text-slate-400">
        {new Date(result.datePerformed).toLocaleDateString()} — Validated by{" "}
        {result.validatedBy}
      </div>
    </div>
  );
}
