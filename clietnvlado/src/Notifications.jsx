import { useEffect, useState, useRef } from "react";
import { FaBell } from "react-icons/fa";
import api from "./api";

export default function Notifications({ theme, onActivitiesRead }) {
  const [items, setItems] = useState([]);
  const [open , setOpen ] = useState(false);
  const ref = useRef();

  const loadFeed = async () => {
    const { data } = await api.get("/activity", { params:{ top:10 }});
    setItems(data);
  };

  useEffect(() => { loadFeed();
    const id = setInterval(loadFeed, 60_000);
    return () => clearInterval(id);
  }, []);

  useEffect(() => {
    if (!open) return;
    const h = e => {
      if (ref.current && !ref.current.contains(e.target)) {
        handleClose();
      }
    };
    document.addEventListener("mousedown", h);
    return () => document.removeEventListener("mousedown", h);
  }, [open, items]);

  const unread = items.filter(i => !i.isRead).length;

  const handleClose = async () => {
    setOpen(false);
    const toMark = items
      .filter(i => !i.isRead && i.type !== "InvitedToContacts")
      .map(i => i.id);

    await Promise.all(toMark.map(id => api.patch(`/activity/${id}/read`)));
    setItems(prev => prev.map(i =>
      toMark.includes(i.id) ? { ...i, isRead: true } : i));
  };

  const respondContact = async (activity, accepted) => {
    const contactId = activity.meta?.ContactId;
    if (!contactId) return;

    if (accepted)
      await api.post(`/contacts/${contactId}/accept`);
    else
      await api.post(`/contacts/${contactId}/block`);

    await api.patch(`/activity/${activity.id}/read`);
    setItems(prev => prev.map(i =>
      i.id === activity.id ? { ...i, isRead: true } : i));
  };

  const respondRoom = async (activity, accepted) => {
    const roomId = activity.roomId;
    if (!roomId) return;

    const url = accepted
      ? `/rooms/${roomId}/accept`
      : `/rooms/${roomId}/decline`;

    await api.post(url);
    await api.patch(`/activity/${activity.id}/read`);

    setItems(prev =>
      prev.map(i => i.id === activity.id ? { ...i, isRead: true } : i));
  };


  const handleClear = async () => {
    const toMark = items
      .filter(i => !i.isRead && i.type !== "InvitedToContacts")
      .map(i => i.id);

    await Promise.all(toMark.map(id => api.patch(`/activity/${id}/read`)));
    setItems(prev => prev.map(i =>
      toMark.includes(i.id) ? { ...i, isRead: true } : i));
  };

  function formatActivity(activity) {
    const { type, meta, createdAt } = activity;
    const dt = new Date(createdAt).toLocaleString();

    switch (type) {
      case "CreatedDocument":
        return `📄 Добавлен документ «${meta?.Name ?? "(без названия)"}».`;

      case "UpdatedDocument":
        return `✏️ Обновлён документ «${meta?.Name ?? "(без названия)"}» v${meta?.Version}-${meta?.ForkPath || '0'}.`;

      case "DeletedDocument":
        return `🗑️ Удалён документ «${meta?.Name ?? "(без названия)"}».`;

      case "ArchivedDocument":
        return `📦 Архивирован документ ${meta?.Name} v${meta?.Version}-${meta?.ForkPath || '0'}.`;

      case "RenamedDocument":
        return `🔖 Переименован документ: «${meta?.OldName}» → «${meta?.NewName}».`;

      case "IssuedToken":
        return `🔑 Выдан доступ к документу «${meta?.Name}».`;

      case "UpdatedToken":
        return `🔐 Обновлён доступ к документу «${meta?.Name}».`;

      case "RevokedToken":
        return `🚫 Отозван доступ к документу «${meta?.Name}».`;

      case "CreatedRoom":
        return `🆕 Создана комната «${meta?.RoomTitle ?? "(без названия)"}».`;

      case "InvitedToRoom":
        return `✉️ Приглашение в комнату «${meta?.RoomTitle ?? "(без названия)"} от ${meta?.UserName}».`;

      case "AcceptedRoom":
        return `✅ Приглашение для ${meta?.UserName} в комнату принято.`;

      case "DeclinedRoom":
        return `🚫 Приглашение для ${meta?.UserName} в комнату отклонено.`;

      case "RevokedRoom":
        return `🚫 Отозван доступ к комнате «${meta?.RoomTitle}».`;

      case "UpdatedRoomAccess":
        return `🔧 Изменён уровень доступа в комнате «${meta?.RoomTitle ?? "(без названия)"}».`;

      case "DeletedRoom":
        return `❌ Удалена комната «${meta?.RoomTitle ?? "(без названия)"}» с ${meta?.Count ?? "0"} документами.`;

      case "InvitedToContacts":
        return `👤 Запрос на добавление в контакты от ${meta?.UserName}.`;

      case "AcceptedContact":
        return `✅ Запрос для ${meta?.UserName} на добавление в контакты принят.`;

      case "DeclinedContact":
        return `🚫 Запрос для ${meta?.UserName} на добавление в контакты отклонён.`;

      default:
        return `ℹ️ Неизвестная активность от ${dt}`;
    }
  }

  function renderActivity(a, respondContact, theme) {
  const dt = new Date(a.createdAt).toLocaleString();

  const Wrap = ({ children, extra }) => (
    <>
      <div className="small">{dt}</div>
      <div>{children}</div>
      {extra && <div className="mt-1">{extra}</div>}
    </>
  );

  switch (a.type) {
    case "InvitedToRoom":
      return (
        <Wrap
          extra={!a.isRead && (
            <div className="d-flex gap-2">
              <button className="btn btn-sm btn-success"
                      onClick={() => respondRoom(a, true)}>
                Принять
              </button>
              <button className="btn btn-sm btn-outline-danger"
                      onClick={() => respondRoom(a, false)}>
                Отклонить
              </button>
            </div>
          )}
        >
          ✉️ Приглашение в комнату «<strong>{a.meta?.RoomTitle}</strong>»
          от {a.meta?.UserName}.
        </Wrap>
      );
      case "InvitedToContacts":
        return (
          <Wrap
            extra={
              !a.isRead && (
                <div className="d-flex gap-2">
                  <button
                    className="btn btn-sm btn-success"
                    onClick={() => respondContact(a, true)}
                  >
                    Принять
                  </button>
                  <button
                    className="btn btn-sm btn-outline-danger"
                    onClick={() => respondContact(a, false)}
                  >
                    Отклонить
                  </button>
                </div>
              )
            }
          >
            👤 Запрос в контакты от <strong>{a.meta?.UserName}</strong>.
          </Wrap>
        );

      case "AcceptedContact":
        return (
          <Wrap>
            ✅ Ваш запрос принял <strong>{a.meta?.UserName}</strong>.
          </Wrap>
        );

      case "DeclinedContact":
        return (
          <Wrap>
            🚫 Ваш запрос отклонил <strong>{a.meta?.UserName}</strong>.
          </Wrap>
        );

      default:
        return (
          <Wrap>{formatActivity(a)}</Wrap>
        );
    }
  }
  
  return (
    <div className="dropdown position-relative" ref={ref}>
      <button
        className="btn btn-link position-relative"
        title="Уведомления"
        onClick={() => setOpen(o => !o)}
        style={{ color: theme === "dark" ? "#ccc" : "#333" }}
      >
        <FaBell size={20} />
        {unread > 0 && (
          <span
            className="position-absolute top-0 start-100 translate-middle badge rounded-pill bg-danger"
            style={{ fontSize: "0.65rem" }}
          >
            {unread > 99 ? "99+" : unread}
          </span>
        )}
      </button>

      {/* …кнопка колокольчика та же… */}

      {open && (
        <div
          className={`dropdown-menu dropdown-menu-start shadow ${theme==="dark"?"bg-dark text-light":""} show`}
            style={{
              minWidth: 320,
              maxHeight: 420,
              overflowY: "auto",
              transform: "translateX(-100%)"  /* ← главное */
            }}
        >
          {/* Заголовок */}
          <div className="dropdown-header d-flex justify-content-between">
            <span>Уведомления</span>
            <button onClick={handleClear} className="btn btn-sm btn-light">
              Очистить
            </button>
          </div>

          {/* Пусто */}
          {items.length === 0 && (
            <div className="px-3 py-2 text-muted">Нет уведомлений</div>
          )}

          {/* Список уведомлений */}
          {items.map(a => (
            <div key={a.id} className={`dropdown-item${a.isRead?"":" fw-bold"}`}>
              {renderActivity(a, respondContact, theme)}
            </div>
          ))}
        </div>
      )}
    </div>
  );
}