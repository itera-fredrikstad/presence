import { format } from "date-fns";

export function getDayId(date: Date) {
  return format(date, "ddMMyy");
}

export function map<TIn, TOut>(target: TIn, mapFn: (i: TIn) => TOut) {
    return mapFn(target);
}