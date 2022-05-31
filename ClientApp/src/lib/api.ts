import axios from "axios";
import type { DayAtWork, DayAtWorkType, Identifiable } from "./models";
import { getDayId, map } from "./utils";

export type DayAtWorkItemsMap = { [id: string]: Identifiable<DayAtWork> };
export type DayAtWorkDto = {
  userId: string;
  date: string;
  type: DayAtWorkType;
  comment?: string;
};

export function getDayAtWorkItemsForUser(userId: string): Promise<DayAtWorkItemsMap> {
  return axios
    .get<DayAtWorkDto[]>("https://localhost:7080/dayAtWork/" + userId)
    .then(res => res.data.reduce(
      (p, n) => ({
        ...p, 
        ...(map(new Date(n.date), date => map(getDayId(date), id => ({
          [id]: ({ 
            id,
            userId: n.userId,
            date,
            type: n.type, 
            comment: n.comment })
        }))))
      }), 
      {} as DayAtWorkItemsMap));
}
