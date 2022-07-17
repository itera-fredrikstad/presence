import axios from "axios";
import { formatISO, parse, parseISO, parseJSON } from "date-fns";
import type { DayAtWork, DayAtWorkType, DaySummary, Identifiable } from "./models";
import { getDayId, map } from "./utils";
import debounce from "p-debounce";
import memoize from "p-memoize";

export type DayAtWorkItemsMap = { [id: string]: Identifiable<DayAtWork> };

export type DayAtWorkDto = {
  userId: string;
  date: string;
  type: DayAtWorkType;
  comment?: string;
};

export type UserDto = {
  userId: string;
  name: string;
  photo: string;
};

export async function getUser(): Promise<UserDto> {
  return (await axios.get<UserDto>(`api/user`)).data;
}

export function mapDayAtWorkItem(dayDto: DayAtWorkDto): Identifiable<DayAtWork> {
  const date = parseJSON(dayDto.date);
  const id = getDayId(date);

  return ({
    id,
    date,
    userId: dayDto.userId,
    type: dayDto.type,
    comment: dayDto.comment
  });
}

export async function getDayAtWorkItemsForUser(): Promise<DayAtWorkItemsMap> {
  const res = await axios
    .get<DayAtWorkDto[]>(`api/dayAtWork`);
  
  return res.data.reduce(
    (p, n) => ({
      ...p,
      ...(map(mapDayAtWorkItem(n), d => ({[d.id]: d})))
    }),
    {} as DayAtWorkItemsMap);
}

export async function getDayAtWorkItems(day: Date): Promise<DayAtWork[]> {
  const date = formatISO(day, {representation: "date"})
  const res = await axios
    .get<DaySummary>(`api/daySummary?date=${date}`);


  return res.data.attendees
}

export async function addOrUpdateDayAtWork(dayAtWork: DayAtWork): Promise<DayAtWork> {
  const dto: DayAtWorkDto = {
    userId: dayAtWork.userId,
    date: formatISO(dayAtWork.date, { representation: "date" }),
    type: dayAtWork.type,
    comment: dayAtWork.comment
  };
  
  const res = await axios.put<DayAtWorkDto>("api/dayAtWork", dto);
  
  return mapDayAtWorkItem(res.data);
}

export type PublicHolidayDto = {
  date: string;
  localName: string;
};

export type PublicHoliday = {
  date: Date;
  name: string;
};

export type PublicHolidayMap = {[id: string]: PublicHoliday};

export async function getPublicHolidays(year: string): Promise<PublicHolidayMap> {
  const res = await axios.get<PublicHolidayDto[]>(`https://date.nager.at/api/v3/PublicHolidays/${year}/NO`);
  return res.data.reduce<PublicHolidayMap>((p, n) => ({
    ...p, 
    ...map(parse(n.date, "yyyy-MM-dd", new Date()), d => ({ [getDayId(d)]: { name: n.localName, date: d }}))}), {});
}

export type TeamEventDto = {
  name: string;
  start: string;
  end: string;
};

export type TeamEvent = {
  name: string;
  start: Date;
  end: Date;
};

export type TeamEventMap = {[id: string]: TeamEvent[]};

export async function getTeamEvents(): Promise<TeamEventMap> {
  const res = await axios.get<TeamEventDto[]>(`api/teamEvents`);
  return res.data.reduce<TeamEventMap>((p, n) => ({
    ...p,
    ...map(parseJSON(n.start), start => map(parseJSON(n.end), end => map(getDayId(start), id => ({
      [id]: [
        ...(p[id] ? p[id] : []),
        { name: n.name, start: start, end: end }
      ]
    }))))
  }), {});
}
