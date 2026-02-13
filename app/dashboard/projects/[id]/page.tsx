"use client";

import { useEffect, useState, useCallback, use } from "react";
import Link from "next/link";
import { ArrowLeft, Loader2, FolderKanban } from "lucide-react";
import { Button } from "@/components/ui/button";
import { KanbanBoard } from "@/components/kanban-board";
import { CreateTaskDialog } from "@/components/create-task-dialog";
import { getTasksByProject } from "@/lib/api";
import type { TodoTask, Project } from "@/lib/types";

export default function ProjectDetailPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id: projectId } = use(params);

  const [tasks, setTasks] = useState<TodoTask[]>([]);
  const [project, setProject] = useState<Project | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    // Load project info from localStorage
    const raw = localStorage.getItem("taskly_projects");
    if (raw) {
      const projects: Project[] = JSON.parse(raw);
      const found = projects.find((p) => p.id === projectId);
      if (found) setProject(found);
    }

    // Load tasks from API
    async function loadTasks() {
      try {
        const data = await getTasksByProject(projectId);
        setTasks(data);
      } catch {
        setError("Erro ao carregar tarefas.");
      } finally {
        setLoading(false);
      }
    }

    loadTasks();
  }, [projectId]);

  const handleTaskCreated = useCallback((task: TodoTask) => {
    setTasks((prev) => [...prev, task]);
  }, []);

  const handleTasksChange = useCallback((updated: TodoTask[]) => {
    setTasks(updated);
  }, []);

  return (
    <div className="flex flex-col gap-6">
      <div className="flex items-center gap-4">
        <Link href="/dashboard/projects">
          <Button variant="ghost" size="icon">
            <ArrowLeft className="h-4 w-4" />
          </Button>
        </Link>
        <div className="flex items-center gap-3">
          <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-primary/10">
            <FolderKanban className="h-5 w-5 text-primary" />
          </div>
          <div>
            <h1 className="text-2xl font-bold tracking-tight text-foreground">
              {project?.name || "Projeto"}
            </h1>
            {project?.description && (
              <p className="text-sm text-muted-foreground">
                {project.description}
              </p>
            )}
          </div>
        </div>
        <div className="ml-auto">
          <CreateTaskDialog
            projectId={projectId}
            onCreated={handleTaskCreated}
          />
        </div>
      </div>

      {loading ? (
        <div className="flex items-center justify-center py-16">
          <Loader2 className="h-8 w-8 animate-spin text-primary" />
        </div>
      ) : error ? (
        <div className="flex flex-col items-center justify-center rounded-lg border border-dashed py-16">
          <p className="text-sm text-muted-foreground">{error}</p>
          <p className="mt-1 text-xs text-muted-foreground">
            Verifique se a API esta rodando em localhost:5219
          </p>
        </div>
      ) : (
        <KanbanBoard tasks={tasks} onTasksChange={handleTasksChange} />
      )}
    </div>
  );
}
