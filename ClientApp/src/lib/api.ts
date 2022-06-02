import axios from "axios";
import { formatISO } from "date-fns";
import type { DayAtWork, DayAtWorkType, DaySummary, Identifiable } from "./models";
import { getDayId, map } from "./utils";

export type DayAtWorkItemsMap = { [id: string]: Identifiable<DayAtWork> };

export type DayAtWorkDto = {
  userId: string;
  date: string;
  type: DayAtWorkType;
  comment?: string;
};

export async function getDayAtWorkItemsForUser(userId: string): Promise<DayAtWorkItemsMap> {
  const res = await axios
    .get<DayAtWorkDto[]>("https://localhost:7080/dayAtWork/" + userId);
  
  return res.data.reduce(
    (p, n) => ({
      ...p,
      ...(map(new Date(n.date), date => map(getDayId(date), id => ({
        [id]: ({
          id,
          userId: n.userId,
          date,
          type: n.type,
          comment: n.comment
        })
      }))))
    }),
    {} as DayAtWorkItemsMap);
}

export async function getDayAtWorkItems(day: Date): Promise<DayAtWork[]> {
  const date = formatISO(day, {representation: "date"})
  const res = await axios
    .get<DaySummary>(`https://localhost:7080/daySummary?date=${date}`);


  return res.data.attendees
}