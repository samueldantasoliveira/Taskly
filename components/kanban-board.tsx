"use client";

import { useState, useCallback } from "react";
import { Circle, Clock, CheckCircle2 } from "lucide-react";
import { cn } from "@/lib/utils";
import { TaskCard } from "@/components/task-card";
import { EditTaskDialog } from "@/components/edit-task-dialog";
import { updateTodoTask } from "@/lib/api";
import { TodoStatus, TODO_STATUS_LABELS } from "@/lib/types";
import type { TodoTask } from "@/lib/types";

interface KanbanBoardProps {
  tasks: TodoTask[];
  onTasksChange: (tasks: TodoTask[]) => void;
}

const columns = [
  {
    status: TodoStatus.Pending,
    label: TODO_STATUS_LABELS[TodoStatus.Pending],
    icon: Circle,
    color: "text-muted-foreground",
    borderColor: "border-muted-foreground/30",
    dotColor: "bg-muted-foreground",
  },
  {
    status: TodoStatus.InProgress,
    label: TODO_STATUS_LABELS[TodoStatus.InProgress],
    icon: Clock,
    color: "text-amber-400",
    borderColor: "border-amber-400/30",
    dotColor: "bg-amber-400",
  },
  {
    status: TodoStatus.Done,
    label: TODO_STATUS_LABELS[TodoStatus.Done],
    icon: CheckCircle2,
    color: "text-primary",
    borderColor: "border-primary/30",
    dotColor: "bg-primary",
  },
];

export function KanbanBoard({ tasks, onTasksChange }: KanbanBoardProps) {
  const [editTask, setEditTask] = useState<TodoTask | null>(null);
  const [editOpen, setEditOpen] = useState(false);
  const [dragOverColumn, setDragOverColumn] = useState<TodoStatus | null>(null);

  const handleDragStart = useCallback(
    (e: React.DragEvent, taskId: string) => {
      e.dataTransfer.setData("text/plain", taskId);
      e.dataTransfer.effectAllowed = "move";
    },
    []
  );

  const handleDragOver = useCallback(
    (e: React.DragEvent, status: TodoStatus) => {
      e.preventDefault();
      e.dataTransfer.dropEffect = "move";
      setDragOverColumn(status);
    },
    []
  );

  const handleDragLeave = useCallback(() => {
    setDragOverColumn(null);
  }, []);

  const handleDrop = useCallback(
    async (e: React.DragEvent, newStatus: TodoStatus) => {
      e.preventDefault();
      setDragOverColumn(null);
      const taskId = e.dataTransfer.getData("text/plain");
      const task = tasks.find((t) => t.id === taskId);
      if (!task || task.status === newStatus) return;

      // Optimistic update
      const updated = tasks.map((t) =>
        t.id === taskId ? { ...t, status: newStatus } : t
      );
      onTasksChange(updated);

      try {
        await updateTodoTask(taskId, {
          title: task.title,
          description: task.description,
          status: newStatus,
          projectId: task.projectId,
          assignedUserId: task.assignedUserId,
        });
      } catch {
        // Revert on error
        onTasksChange(tasks);
      }
    },
    [tasks, onTasksChange]
  );

  const handleTaskUpdated = useCallback(
    (updated: TodoTask) => {
      onTasksChange(tasks.map((t) => (t.id === updated.id ? updated : t)));
    },
    [tasks, onTasksChange]
  );

  return (
    <>
      <div className="grid gap-6 lg:grid-cols-3">
        {columns.map((col) => {
          const columnTasks = tasks.filter((t) => t.status === col.status);
          const isDragOver = dragOverColumn === col.status;

          return (
            <div
              key={col.status}
              onDragOver={(e) => handleDragOver(e, col.status)}
              onDragLeave={handleDragLeave}
              onDrop={(e) => handleDrop(e, col.status)}
              className={cn(
                "flex flex-col rounded-xl border bg-card/50 p-4 transition-colors",
                isDragOver && "border-primary/50 bg-primary/5"
              )}
            >
              <div className="mb-4 flex items-center gap-2">
                <div className={`h-2.5 w-2.5 rounded-full ${col.dotColor}`} />
                <h3 className={`text-sm font-semibold ${col.color}`}>
                  {col.label}
                </h3>
                <span className="ml-auto rounded-full bg-secondary px-2 py-0.5 text-xs font-medium text-muted-foreground">
                  {columnTasks.length}
                </span>
              </div>

              <div className="flex flex-col gap-2">
                {columnTasks.length === 0 ? (
                  <div className="flex items-center justify-center rounded-lg border border-dashed py-8">
                    <p className="text-xs text-muted-foreground">
                      Arraste tarefas aqui
                    </p>
                  </div>
                ) : (
                  columnTasks.map((task) => (
                    <TaskCard
                      key={task.id}
                      task={task}
                      onClick={() => {
                        setEditTask(task);
                        setEditOpen(true);
                      }}
                      onDragStart={handleDragStart}
                    />
                  ))
                )}
              </div>
            </div>
          );
        })}
      </div>

      <EditTaskDialog
        task={editTask}
        open={editOpen}
        onOpenChange={setEditOpen}
        onUpdated={handleTaskUpdated}
      />
    </>
  );
}
