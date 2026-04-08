"use client";

import { PatientInfo } from "@/lib/types";
import { User, Hash, FileText, Calendar } from "lucide-react";

interface PatientInfoHeaderProps {
  patient: PatientInfo;
  resultCount?: number;
}

export function PatientInfoHeader({
  patient,
  resultCount,
}: PatientInfoHeaderProps) {
  return (
    <div className="bg-gradient-to-r from-blue-600 to-blue-700 rounded-xl p-5 text-white">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <div className="w-12 h-12 rounded-full bg-white/20 flex items-center justify-center">
            <User size={24} />
          </div>
          <div>
            <h2 className="text-lg font-bold">{patient.name}</h2>
            <div className="flex items-center gap-4 mt-1 text-sm text-blue-100">
              <span className="flex items-center gap-1">
                <Hash size={14} />
                {patient.id}
              </span>
              <span className="flex items-center gap-1">
                <FileText size={14} />
                NDA: {patient.nda}
              </span>
              <span className="flex items-center gap-1">
                <Calendar size={14} />
                {new Date(patient.sampleDate).toLocaleDateString()}
              </span>
            </div>
          </div>
        </div>
        {resultCount !== undefined && (
          <div className="text-right">
            <div className="text-2xl font-bold">{resultCount}</div>
            <div className="text-xs text-blue-200">Results</div>
          </div>
        )}
      </div>
    </div>
  );
}
