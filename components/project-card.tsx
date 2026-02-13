"use client";

import Link from "next/link";
import { FolderKanban, ArrowRight } from "lucide-react";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { ProjectStatus, PROJECT_STATUS_LABELS } from "@/lib/types";
import type { Project } from "@/lib/types";

interface ProjectCardProps {
  project: Project;
}

const statusColors: Record<ProjectStatus, string> = {
  [ProjectStatus.Active]:
    "bg-primary/10 text-primary border-primary/20",
  [ProjectStatus.Inactive]:
    "bg-muted text-muted-foreground border-muted",
  [ProjectStatus.Completed]:
    "bg-blue-400/10 text-blue-400 border-blue-400/20",
  [ProjectStatus.PendingApproval]:
    "bg-amber-400/10 text-amber-400 border-amber-400/20",
};

export function ProjectCard({ project }: ProjectCardProps) {
  const statusLabel =
    PROJECT_STATUS_LABELS[project.status] || "Desconhecido";
  const statusColor =
    statusColors[project.status] || statusColors[ProjectStatus.Active];

  return (
    <Link href={`/dashboard/projects/${project.id}`}>
      <Card className="group cursor-pointer transition-colors hover:border-primary/40">
        <CardHeader className="flex flex-row items-start justify-between">
          <div className="flex items-center gap-3">
            <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-primary/10">
              <FolderKanban className="h-5 w-5 text-primary" />
            </div>
            <div>
              <CardTitle className="text-base">{project.name}</CardTitle>
              <Badge className={`mt-1 ${statusColor}`} variant="outline">
                {statusLabel}
              </Badge>
            </div>
          </div>
          <ArrowRight className="h-4 w-4 text-muted-foreground opacity-0 transition-opacity group-hover:opacity-100" />
        </CardHeader>
        <CardContent>
          <p className="text-sm text-muted-foreground line-clamp-2">
            {project.description}
          </p>
        </CardContent>
      </Card>
    </Link>
  );
}
