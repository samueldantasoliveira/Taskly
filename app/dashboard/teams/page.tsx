"use client";

import { useEffect, useState, useCallback } from "react";
import { Users } from "lucide-react";
import { CreateTeamDialog } from "@/components/create-team-dialog";
import { TeamCard } from "@/components/team-card";
import type { Team } from "@/lib/types";

const STORAGE_KEY = "taskly_teams";

function loadTeams(): Team[] {
  if (typeof window === "undefined") return [];
  const raw = localStorage.getItem(STORAGE_KEY);
  return raw ? JSON.parse(raw) : [];
}

function saveTeams(teams: Team[]) {
  localStorage.setItem(STORAGE_KEY, JSON.stringify(teams));
}

export default function TeamsPage() {
  const [teams, setTeams] = useState<Team[]>([]);

  useEffect(() => {
    setTeams(loadTeams());
  }, []);

  const handleCreated = useCallback((team: Team) => {
    setTeams((prev) => {
      const next = [...prev, team];
      saveTeams(next);
      return next;
    });
  }, []);

  const handleMemberAdded = useCallback(
    (teamId: string, userId: string) => {
      setTeams((prev) => {
        const next = prev.map((t) =>
          t.id === teamId
            ? { ...t, memberIds: [...(t.memberIds || []), userId] }
            : t
        );
        saveTeams(next);
        return next;
      });
    },
    []
  );

  return (
    <div className="flex flex-col gap-8">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight text-foreground">
            Equipes
          </h1>
          <p className="mt-1 text-muted-foreground">
            Gerencie suas equipes e seus membros.
          </p>
        </div>
        <CreateTeamDialog onCreated={handleCreated} />
      </div>

      {teams.length === 0 ? (
        <div className="flex flex-col items-center justify-center rounded-lg border border-dashed py-16">
          <Users className="mb-4 h-12 w-12 text-muted-foreground/50" />
          <h3 className="text-lg font-medium text-foreground">
            Nenhuma equipe
          </h3>
          <p className="mt-1 text-sm text-muted-foreground">
            Crie sua primeira equipe para comecar.
          </p>
        </div>
      ) : (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {teams.map((team) => (
            <TeamCard
              key={team.id}
              team={team}
              onMemberAdded={handleMemberAdded}
            />
          ))}
        </div>
      )}
    </div>
  );
}
