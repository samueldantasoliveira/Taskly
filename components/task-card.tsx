"use client";

import { GripVertical, User } from "lucide-react";
import { Card } from "@/components/ui/card";
import type { TodoTask } from "@/lib/types";

interface TaskCardProps {
  task: TodoTask;
  onClick: () => void;
  onDragStart: (e: React.DragEvent, taskId: string) => void;
}

export function TaskCard({ task, onClick, onDragStart }: TaskCardProps) {
  return (
    <Card
      draggable
      onDragStart={(e) => onDragStart(e, task.id)}
      onClick={onClick}
      className="cursor-pointer p-3 transition-all hover:border-primary/40 hover:shadow-md active:scale-[0.98]"
    >
      <div className="flex items-start gap-2">
        <GripVertical className="mt-0.5 h-4 w-4 shrink-0 cursor-grab text-muted-foreground/40" />
        <div className="min-w-0 flex-1">
          <p className="text-sm font-medium text-foreground truncate">
            {task.title}
          </p>
          {task.description && (
            <p className="mt-1 text-xs text-muted-foreground line-clamp-2">
              {task.description}
            </p>
          )}
          {task.assignedUserId && (
            <div className="mt-2 flex items-center gap-1">
              <User className="h-3 w-3 text-muted-foreground" />
              <span className="text-xs text-muted-foreground font-mono">
                {task.assignedUserId.slice(0, 8)}...
              </span>
            </div>
          )}
        </div>
      </div>
    </Card>
  );
}
