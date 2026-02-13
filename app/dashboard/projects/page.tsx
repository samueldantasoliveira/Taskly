"use client";

import { useEffect, useState, useCallback } from "react";
import { FolderKanban } from "lucide-react";
import { CreateProjectDialog } from "@/components/create-project-dialog";
import { ProjectCard } from "@/components/project-card";
import type { Project } from "@/lib/types";

const STORAGE_KEY = "taskly_projects";

function loadProjects(): Project[] {
  if (typeof window === "undefined") return [];
  const raw = localStorage.getItem(STORAGE_KEY);
  return raw ? JSON.parse(raw) : [];
}

function saveProjects(projects: Project[]) {
  localStorage.setItem(STORAGE_KEY, JSON.stringify(projects));
}

export default function ProjectsPage() {
  const [projects, setProjects] = useState<Project[]>([]);

  useEffect(() => {
    setProjects(loadProjects());
  }, []);

  const handleCreated = useCallback((project: Project) => {
    setProjects((prev) => {
      const next = [...prev, project];
      saveProjects(next);
      return next;
    });
  }, []);

  return (
    <div className="flex flex-col gap-8">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight text-foreground">
            Projetos
          </h1>
          <p className="mt-1 text-muted-foreground">
            Gerencie seus projetos. Clique em um projeto para ver as tarefas.
          </p>
        </div>
        <CreateProjectDialog onCreated={handleCreated} />
      </div>

      {projects.length === 0 ? (
        <div className="flex flex-col items-center justify-center rounded-lg border border-dashed py-16">
          <FolderKanban className="mb-4 h-12 w-12 text-muted-foreground/50" />
          <h3 className="text-lg font-medium text-foreground">
            Nenhum projeto
          </h3>
          <p className="mt-1 text-sm text-muted-foreground">
            Crie seu primeiro projeto para comecar.
          </p>
        </div>
      ) : (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {projects.map((project) => (
            <ProjectCard key={project.id} project={project} />
          ))}
        </div>
      )}
    </div>
  );
}
