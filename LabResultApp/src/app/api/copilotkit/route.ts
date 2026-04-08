import { 
  CopilotRuntime, 
  copilotRuntimeNextJSAppRouterEndpoint,
  EmptyAdapter 
} from "@copilotkit/runtime";
import { HttpAgent } from "@ag-ui/client";

export const runtime = "nodejs";

const agentUrl = process.env.AGENT_URL || "http://localhost:8000";
const agentUsername = process.env.AGENT_AUTH_USERNAME || "admin";
const agentPassword = process.env.AGENT_AUTH_PASSWORD || "changeme";

// Create Basic Auth header
const basicAuth = Buffer.from(`${agentUsername}:${agentPassword}`).toString("base64");

// Create HttpAgent for AG-UI endpoint
const labResultAgent = new HttpAgent({
  url: agentUrl,
  agentId: "labResultAgent",
  description: "Agent that retrieves and analyzes patient lab results from the MCP server",
  headers: {
    Authorization: `Basic ${basicAuth}`,
  },
});

const copilotRuntime = new CopilotRuntime({
  agents: {
    labResultAgent: labResultAgent as any,
  },
});

const handler = copilotRuntimeNextJSAppRouterEndpoint({
  runtime: copilotRuntime,
  serviceAdapter: new EmptyAdapter(),
  endpoint: "/api/copilotkit",
});

export const { POST, GET, OPTIONS } = handler;

