"use client";

import {
  FlaskConical,
  Activity,
  Shield,
  Cpu,
  MessageSquare,
} from "lucide-react";

export function AgentWorkspace() {
  return (
    <div className="h-full flex flex-col">
      {/* Top nav bar */}
      <header className="flex items-center justify-between px-6 py-3 bg-white border-b border-slate-200">
        <div className="flex items-center gap-3">
          <div className="flex items-center justify-center w-9 h-9 rounded-lg bg-blue-600 text-white">
            <FlaskConical size={20} />
          </div>
          <div>
            <h1 className="text-lg font-semibold text-slate-800">
              Lab Results Assistant
            </h1>
            <p className="text-xs text-slate-500">
              AI-powered lab results workspace
            </p>
          </div>
        </div>
        <div className="flex items-center gap-2">
          <StatusBadge icon={<Cpu size={14} />} label="Agent" status="ready" />
          <StatusBadge
            icon={<Activity size={14} />}
            label="MCP"
            status="connected"
          />
          <StatusBadge
            icon={<Shield size={14} />}
            label="Auth"
            status="secured"
          />
        </div>
      </header>

      {/* Main content area */}
      <div className="flex-1 overflow-auto p-8">
        <div className="max-w-3xl mx-auto">
          {/* Welcome card */}
          <div className="bg-white rounded-2xl border border-slate-200 p-8 mb-6">
            <div className="flex items-start gap-4">
              <div className="flex-shrink-0 w-12 h-12 rounded-xl bg-blue-50 flex items-center justify-center">
                <MessageSquare size={24} className="text-blue-600" />
              </div>
              <div>
                <h2 className="text-xl font-semibold text-slate-800 mb-2">
                  Welcome to the Lab Results Workspace
                </h2>
                <p className="text-slate-600 leading-relaxed">
                  Use the chat sidebar on the right to interact with the AI
                  assistant. You can ask it to fetch and analyze patient lab
                  results. The assistant will request approval before accessing
                  any patient data.
                </p>
              </div>
            </div>
          </div>

          {/* Quick start cards */}
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <QuickStartCard
              title="Fetch Results"
              description='Try: "Show lab results for patient 12345"'
              icon={<FlaskConical size={20} />}
            />
            <QuickStartCard
              title="Filter by Date"
              description='Try: "Get results from January 2026"'
              icon={<Activity size={20} />}
            />
            <QuickStartCard
              title="Analyze Abnormals"
              description='Try: "What are the abnormal values?"'
              icon={<Shield size={20} />}
            />
          </div>

          {/* Architecture info */}
          <div className="mt-8 bg-slate-50 rounded-xl border border-slate-200 p-6">
            <h3 className="text-sm font-semibold text-slate-700 mb-3">
              System Architecture
            </h3>
            <div className="flex items-center justify-center gap-3 text-sm text-slate-600">
              <span className="px-3 py-1.5 bg-white rounded-lg border border-slate-200 font-medium">
                Frontend (Next.js)
              </span>
              <span className="text-slate-400">→ AG-UI →</span>
              <span className="px-3 py-1.5 bg-blue-50 rounded-lg border border-blue-200 font-medium text-blue-700">
                AI Agent (.NET)
              </span>
              <span className="text-slate-400">→ MCP →</span>
              <span className="px-3 py-1.5 bg-green-50 rounded-lg border border-green-200 font-medium text-green-700">
                Lab Results DB
              </span>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

function StatusBadge({
  icon,
  label,
  status,
}: {
  icon: React.ReactNode;
  label: string;
  status: string;
}) {
  return (
    <div className="flex items-center gap-1.5 px-3 py-1.5 rounded-full bg-green-50 border border-green-200">
      <span className="text-green-600">{icon}</span>
      <span className="text-xs font-medium text-green-700">
        {label}: {status}
      </span>
    </div>
  );
}

function QuickStartCard({
  title,
  description,
  icon,
}: {
  title: string;
  description: string;
  icon: React.ReactNode;
}) {
  return (
    <div className="bg-white rounded-xl border border-slate-200 p-5 hover:border-blue-300 hover:shadow-sm transition-all cursor-default">
      <div className="w-10 h-10 rounded-lg bg-blue-50 flex items-center justify-center text-blue-600 mb-3">
        {icon}
      </div>
      <h3 className="text-sm font-semibold text-slate-800 mb-1">{title}</h3>
      <p className="text-xs text-slate-500">{description}</p>
    </div>
  );
}
