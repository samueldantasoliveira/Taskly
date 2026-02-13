"use client";

import { useState } from "react";
import { Loader2, UserPlus } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import { addMemberToTeam } from "@/lib/api";

interface AddMemberDialogProps {
  teamId: string;
  teamName: string;
  onMemberAdded: (teamId: string, userId: string) => void;
}

export function AddMemberDialog({
  teamId,
  teamName,
  onMemberAdded,
}: AddMemberDialogProps) {
  const [open, setOpen] = useState(false);
  const [userId, setUserId] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError("");
    setLoading(true);

    try {
      await addMemberToTeam(teamId, userId);
      onMemberAdded(teamId, userId);
      setUserId("");
      setOpen(false);
    } catch {
      setError("Erro ao adicionar membro. Verifique o ID do usuario.");
    } finally {
      setLoading(false);
    }
  }

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger asChild>
        <Button variant="ghost" size="sm">
          <UserPlus className="mr-1 h-3.5 w-3.5" />
          Adicionar
        </Button>
      </DialogTrigger>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Adicionar membro</DialogTitle>
          <DialogDescription>
            Adicione um membro a equipe {teamName}.
          </DialogDescription>
        </DialogHeader>
        <form onSubmit={handleSubmit} className="flex flex-col gap-4">
          <div className="flex flex-col gap-2">
            <Label htmlFor="user-id">ID do usuario</Label>
            <Input
              id="user-id"
              placeholder="Cole o ID do usuario aqui"
              value={userId}
              onChange={(e) => setUserId(e.target.value)}
              required
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
              Adicionar
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
