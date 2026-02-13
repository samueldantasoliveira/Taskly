"use client";

import { useState, useEffect } from "react";
import { Loader2 } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { updateTodoTask } from "@/lib/api";
import { TodoStatus, TODO_STATUS_LABELS } from "@/lib/types";
import type { TodoTask } from "@/lib/types";

interface EditTaskDialogProps {
  task: TodoTask | null;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onUpdated: (task: TodoTask) => void;
}

export function EditTaskDialog({
  task,
  open,
  onOpenChange,
  onUpdated,
}: EditTaskDialogProps) {
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [status, setStatus] = useState<string>("0");
  const [assignedUserId, setAssignedUserId] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => {
    if (task) {
      setTitle(task.title);
      setDescription(task.description);
      setStatus(String(task.status));
      setAssignedUserId(task.assignedUserId || "");
    }
  }, [task]);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!task) return;
    setError("");
    setLoading(true);

    try {
      const updated = await updateTodoTask(task.id, {
        title,
        description,
        status: Number(status) as TodoStatus,
        projectId: task.projectId,
        assignedUserId: assignedUserId || undefined,
      });
      onUpdated(updated);
      onOpenChange(false);
    } catch {
      setError("Erro ao atualizar tarefa.");
    } finally {
      setLoading(false);
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Editar tarefa</DialogTitle>
          <DialogDescription>
            Atualize os detalhes da tarefa.
          </DialogDescription>
        </DialogHeader>
        <form onSubmit={handleSubmit} className="flex flex-col gap-4">
          <div className="flex flex-col gap-2">
            <Label htmlFor="edit-title">Titulo</Label>
            <Input
              id="edit-title"
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              required
            />
          </div>

          <div className="flex flex-col gap-2">
            <Label htmlFor="edit-desc">Descricao</Label>
            <Textarea
              id="edit-desc"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              required
            />
          </div>

          <div className="flex flex-col gap-2">
            <Label>Status</Label>
            <Select value={status} onValueChange={setStatus}>
              <SelectTrigger>
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="0">
                  {TODO_STATUS_LABELS[TodoStatus.Pending]}
                </SelectItem>
                <SelectItem value="1">
                  {TODO_STATUS_LABELS[TodoStatus.InProgress]}
                </SelectItem>
                <SelectItem value="2">
                  {TODO_STATUS_LABELS[TodoStatus.Done]}
                </SelectItem>
              </SelectContent>
            </Select>
          </div>

          <div className="flex flex-col gap-2">
            <Label htmlFor="edit-user">
              Atribuir a usuario (opcional)
            </Label>
            <Input
              id="edit-user"
              placeholder="ID do usuario"
              value={assignedUserId}
              onChange={(e) => setAssignedUserId(e.target.value)}
            />
          </div>

          {error && (
            <p className="text-sm text-destructive" role="alert">
              {error}
            </p>
          )}

          <DialogFooter>
            <Button type="submit" disabled={loading}>
              {loading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
              Salvar
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
