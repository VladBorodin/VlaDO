// constants.ts / constants.js
export const AccessLevelOptions = [
  { value: 0, label: "Только чтение" },
  { value: 1, label: "Редактирование" },
  { value: 2, label: "Полный доступ" }
];

export const LevelLabel = AccessLevelOptions.reduce(
  (acc, { value, label }) => ({ ...acc, [value]: label }), {}
);
