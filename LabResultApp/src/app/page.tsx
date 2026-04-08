"use client";

import { AgentWorkspace } from "@/components/AgentWorkspace";
import { ChatSidebar } from "@/components/ChatSidebar";
import { ConnectionStatus } from "@/components/ConnectionStatus";

export default function Home() {
  return (
    <div className="flex h-screen">
      <main className="flex-1 overflow-hidden flex flex-col">
        <div className="px-6 py-3 border-b border-slate-200 bg-white flex items-center justify-end">
          <ConnectionStatus />
        </div>
        <div className="flex-1 overflow-auto">
          <AgentWorkspace />
        </div>
      </main>
      <ChatSidebar />
    </div>
  );
}
