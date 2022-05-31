export type DayAtWorkType = "FULL" | "FIRST-HALF" | "LAST-HALF" | "EMPTY";
export type DayAtWork = {
    userId: string;
    date: Date;
    type: DayAtWorkType;
    comment?: string;
};
export type Identifiable<T> = T & { id: string; };