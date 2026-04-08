"use client";

import { Shield, Check, X, Database, AlertTriangle } from "lucide-react";

interface ToolCallApprovalProps {
  toolName: string;
  args: Record<string, unknown>;
  onApprove: () => void;
  onReject: () => void;
}

export function ToolCallApproval({
  toolName,
  args,
  onApprove,
  onReject,
}: ToolCallApprovalProps) {
  return (
    <div className="bg-amber-50 border border-amber-200 rounded-xl p-5 my-3">
      <div className="flex items-start gap-3">
        <div className="flex-shrink-0 w-10 h-10 rounded-lg bg-amber-100 flex items-center justify-center">
          <Shield size={20} className="text-amber-600" />
        </div>
        <div className="flex-1">
          <h3 className="font-semibold text-amber-900 flex items-center gap-2">
            <AlertTriangle size={16} />
            Approval Required
          </h3>
          <p className="text-sm text-amber-800 mt-1">
            The agent wants to access patient data. Please review and approve.
          </p>

          {/* Tool details */}
          <div className="mt-3 bg-white rounded-lg border border-amber-200 p-3">
            <div className="flex items-center gap-2 text-xs font-medium text-amber-700 mb-2">
              <Database size={14} />
              {toolName}
            </div>
            <div className="space-y-1">
              {Object.entries(args).map(([key, value]) => (
                <div key={key} className="flex items-center gap-2 text-sm">
                  <span className="font-medium text-slate-600 min-w-[100px]">
                    {key}:
                  </span>
                  <span className="text-slate-800 font-mono text-xs bg-slate-50 px-2 py-0.5 rounded">
                    {String(value ?? "null")}
                  </span>
                </div>
              ))}
            </div>
          </div>

          {/* Action buttons */}
          <div className="flex items-center gap-3 mt-4">
            <button
              onClick={onApprove}
              className="flex items-center gap-2 px-4 py-2 rounded-lg bg-green-600 hover:bg-green-700 text-white text-sm font-medium transition-colors"
            >
              <Check size={16} />
              Approve
            </button>
            <button
              onClick={onReject}
              className="flex items-center gap-2 px-4 py-2 rounded-lg bg-slate-200 hover:bg-slate-300 text-slate-700 text-sm font-medium transition-colors"
            >
              <X size={16} />
              Deny
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
