import { format } from "date-fns";

export function getDayId(date: Date) {
  return format(date, "ddMMyy");
}

export function map<TIn, TOut>(target: TIn, mapFn: (i: TIn) => TOut) {
    return mapFn(target);
}

export function getInitials(email: string) {
  const [username] = email.split("@")
  const parts = username.split(".")
  return `${parts[0][0].toUpperCase()}${parts[parts.length-1][0].toUpperCase()}`
}