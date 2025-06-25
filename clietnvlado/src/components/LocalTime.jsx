// components/LocalTime.jsx
import React from "react";

/** дефолтный вариант форматирования (24 июня 2025 г., 17:12) */
const DEFAULT_OPTS = {
  dateStyle: "medium",
  timeStyle: "short"
};

/** если строка не содержит ни Z, ни смещения ±hh:mm — дописываем Z */
function toUtcIso(s) {
  return /Z$|[+-]\d{2}:\d{2}$/.test(s) ? s : `${s}Z`;
}

/**
 * Показать UTC-дату в локальном формате.
 * @param utc  ISO-строка, epoch-число или Date
 * @param opts Intl.DateTimeFormatOptions (необязательно)
 */
export default function LocalTime({ utc, opts = DEFAULT_OPTS, className = "" }) {
  const iso = typeof utc === "string" ? toUtcIso(utc) : utc;
  const str = new Date(iso).toLocaleString(undefined, opts);
  return <span className={className}>{str}</span>;
}