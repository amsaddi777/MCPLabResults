"use client";

import { useState, useEffect } from "react";
import { Wifi, WifiOff, AlertCircle } from "lucide-react";

interface ConnectionStatusProps {
  agentUrl?: string;
  mcpUrl?: string;
}

export function ConnectionStatus({
  agentUrl = "http://localhost:8000",
  mcpUrl = "http://localhost:3001",
}: ConnectionStatusProps) {
  const [agentStatus, setAgentStatus] = useState<"online" | "offline" | "checking">("checking");
  const [mcpStatus, setMcpStatus] = useState<"online" | "offline" | "checking">("checking");

  useEffect(() => {
    const checkStatus = async () => {
      // Check Agent
      try {
        const agentResponse = await fetch(`${agentUrl}/health`, {
          method: "GET",
          signal: AbortSignal.timeout(3000),
        });
        setAgentStatus(agentResponse.ok ? "online" : "offline");
      } catch {
        setAgentStatus("offline");
      }

      // Check MCP Server
      try {
        const mcpResponse = await fetch(`${mcpUrl}/health`, {
          method: "GET",
          signal: AbortSignal.timeout(3000),
        });
        setMcpStatus(mcpResponse.ok ? "online" : "offline");
      } catch {
        setMcpStatus("offline");
      }
    };

    checkStatus();
    const interval = setInterval(checkStatus, 30000); // Check every 30 seconds

    return () => clearInterval(interval);
  }, [agentUrl, mcpUrl]);

  const allOnline = agentStatus === "online" && mcpStatus === "online";
  const anyOffline = agentStatus === "offline" || mcpStatus === "offline";

  return (
    <div
      className={`flex items-center gap-2 px-3 py-1.5 rounded-full text-xs font-medium ${
        allOnline
          ? "bg-green-50 text-green-700 border border-green-200"
          : anyOffline
          ? "bg-red-50 text-red-700 border border-red-200"
          : "bg-slate-50 text-slate-700 border border-slate-200"
      }`}
    >
      {allOnline ? (
        <>
          <Wifi size={14} />
          <span>Connected</span>
        </>
      ) : anyOffline ? (
        <>
          <WifiOff size={14} />
          <span>Connection Issue</span>
        </>
      ) : (
        <>
          <AlertCircle size={14} className="animate-pulse" />
          <span>Checking...</span>
        </>
      )}
    </div>
  );
}
