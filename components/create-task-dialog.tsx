"use client";

import { useState } from "react";
import { Loader2, Plus } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import { createTodoTask } from "@/lib/api";
import type { TodoTask } from "@/lib/types";

interface CreateTaskDialogProps {
  projectId: string;
  onCreated: (task: TodoTask) => void;
}

export function CreateTaskDialog({
  projectId,
  onCreated,
}: CreateTaskDialogProps) {
  const [open, setOpen] = useState(false);
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [assignedUserId, setAssignedUserId] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError("");
    setLoading(true);

    try {
      const task = await createTodoTask({
        title,
        description,
        projectId,
        assignedUserId: assignedUserId || undefined,
      });
      onCreated(task);
      setTitle("");
      setDescription("");
      setAssignedUserId("");
      setOpen(false);
    } catch {
      setError("Erro ao criar tarefa. Tente novamente.");
    } finally {
      setLoading(false);
    }
  }

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger asChild>
        <Button>
          <Plus className="mr-2 h-4 w-4" />
          Nova tarefa
        </Button>
      </DialogTrigger>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Criar tarefa</DialogTitle>
          <DialogDescription>
            Crie uma nova tarefa para este projeto.
          </DialogDescription>
        </DialogHeader>
        <form onSubmit={handleSubmit} className="flex flex-col gap-4">
          <div className="flex flex-col gap-2">
            <Label htmlFor="task-title">Titulo</Label>
            <Input
              id="task-title"
              placeholder="Ex: Implementar pagina de login"
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              required
            />
          </div>

          <div className="flex flex-col gap-2">
            <Label htmlFor="task-desc">Descricao</Label>
            <Textarea
              id="task-desc"
              placeholder="Descreva a tarefa..."
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              required
            />
          </div>

          <div className="flex flex-col gap-2">
            <Label htmlFor="task-user">
              Atribuir a usuario (opcional)
            </Label>
            <Input
              id="task-user"
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
              Criar
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
