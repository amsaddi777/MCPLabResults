"use client";

import { Loader2, CheckCircle, AlertCircle, Cpu } from "lucide-react";

interface AgentStep {
  id: string;
  label: string;
  status: "pending" | "running" | "complete" | "error";
  detail?: string;
}

interface AgentStepIndicatorProps {
  steps: AgentStep[];
}

export function AgentStepIndicator({ steps }: AgentStepIndicatorProps) {
  if (steps.length === 0) return null;

  return (
    <div className="bg-slate-50 rounded-lg border border-slate-200 p-4 my-3">
      <div className="flex items-center gap-2 mb-3 text-xs font-semibold text-slate-500 uppercase tracking-wider">
        <Cpu size={14} />
        Agent Activity
      </div>
      <div className="space-y-2">
        {steps.map((step) => (
          <div key={step.id} className="flex items-center gap-3">
            <StepIcon status={step.status} />
            <div className="flex-1">
              <span
                className={`text-sm ${
                  step.status === "running"
                    ? "text-blue-700 font-medium"
                    : step.status === "error"
                      ? "text-red-700"
                      : step.status === "complete"
                        ? "text-green-700"
                        : "text-slate-500"
                }`}
              >
                {step.label}
              </span>
              {step.detail && (
                <p className="text-xs text-slate-400 mt-0.5">{step.detail}</p>
              )}
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}

function StepIcon({ status }: { status: AgentStep["status"] }) {
  switch (status) {
    case "running":
      return <Loader2 size={16} className="text-blue-500 animate-spin" />;
    case "complete":
      return <CheckCircle size={16} className="text-green-500" />;
    case "error":
      return <AlertCircle size={16} className="text-red-500" />;
    default:
      return (
        <div className="w-4 h-4 rounded-full border-2 border-slate-300" />
      );
  }
}
