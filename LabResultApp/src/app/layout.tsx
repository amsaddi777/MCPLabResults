import type { Metadata } from "next";
import { ErrorBoundary } from "@/components/ErrorBoundary";
import "@/styles/globals.css";

export const metadata: Metadata = {
  title: "Lab Results Assistant",
  description: "AI-powered lab results workspace with CopilotKit",
};

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="en">
      <body className="min-h-screen bg-[var(--color-bg)]">
        <ErrorBoundary>{children}</ErrorBoundary>
      </body>
    </html>
  );
}
