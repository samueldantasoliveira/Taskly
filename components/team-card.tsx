"use client";

import { Users } from "lucide-react";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { AddMemberDialog } from "@/components/add-member-dialog";
import type { Team } from "@/lib/types";

interface TeamCardProps {
  team: Team;
  onMemberAdded: (teamId: string, userId: string) => void;
}

export function TeamCard({ team, onMemberAdded }: TeamCardProps) {
  return (
    <Card>
      <CardHeader className="flex flex-row items-start justify-between">
        <div className="flex items-center gap-3">
          <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-blue-400/10">
            <Users className="h-5 w-5 text-blue-400" />
          </div>
          <div>
            <CardTitle className="text-base">{team.name}</CardTitle>
            <p className="text-xs text-muted-foreground">
              {team.memberIds?.length || 0} membros
            </p>
          </div>
        </div>
        <AddMemberDialog
          teamId={team.id}
          teamName={team.name}
          onMemberAdded={onMemberAdded}
        />
      </CardHeader>
      <CardContent>
        {team.memberIds && team.memberIds.length > 0 ? (
          <div className="flex flex-wrap gap-2">
            {team.memberIds.map((memberId) => (
              <Badge key={memberId} variant="secondary" className="font-mono text-xs">
                {memberId.slice(0, 8)}...
              </Badge>
            ))}
          </div>
        ) : (
          <p className="text-sm text-muted-foreground">
            Nenhum membro adicionado.
          </p>
        )}
      </CardContent>
    </Card>
  );
}
