import { format } from "date-fns";
import emojiRegex from "emoji-regex";

export function getDayId(date: Date) {
  return format(date, "ddMMyy");
}

export function map<TIn, TOut>(target: TIn, mapFn: (i: TIn) => TOut) {
    return mapFn(target);
}

export function getInitials(name: string) {
  const isEmail = name.indexOf("@") != -1;

  if(isEmail) {
    return getInitialsFromEmail(name)
  }

  const parts = name.split(" ")
  return `${parts[0][0].toUpperCase()}${parts[parts.length-1][0].toUpperCase()}`
}

function getInitialsFromEmail(email: string) {
  const [username] = email.split("@")
  const parts = username.split(".")
  return `${parts[0][0].toUpperCase()}${parts[parts.length-1][0].toUpperCase()}`
}

export function emphasizeEmojis(text: string) {
  return text.replaceAll(emojiRegex(), "<strong class=\"emoji\">$&</strong>")
}