import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import {FaHistory,FaChevronLeft,FaChevronRight,FaTimes
} from "react-icons/fa";

import api from "./api";
import { useAlert } from "./contexts/AlertContext";
import LoadingSpinner from "./LoadingSpinner";
import LocalTime from "./components/LocalTime";

/**
 * Форматирование активности (точно такое же, как в Notifications.jsx)
 */
function formatActivity(a) {
    const { type, meta } = a;

    switch (type) {
        // ─── Документы ──────────────────────────────────────────────
        case "CreatedDocument":
            return `📄 Добавлен документ «${meta?.Name ?? "(без названия)"}».`;
        case "UpdatedDocument":
            return `✏️ Обновлён документ «${meta?.Name ?? "(без названия)"}» v${meta?.Version}-${meta?.ForkPath || '0'}.`;
        case "DeletedDocument":
            return `🗑️ Удалён документ «${meta?.Name ?? "(без названия)"}».`;
        case "ArchivedDocument":
            return `📦 Архивирован документ ${meta?.Name} v${meta?.Version}-${meta?.ForkPath || "0"}.`;
        case "RenamedDocument":
            return `🔖 Переименован документ: «${meta?.OldName}» → «${meta?.NewName}».`;

        // ─── Токены ────────────────────────────────────────────────
        case "IssuedToken":
            return `🔑 Выдан доступ к документу «${meta?.Name}».`;
        case "UpdatedToken":
            return `🔐 Обновлён доступ к документу «${meta?.Name}».`;
        case "RevokedToken":
            return `🚫 Отозван доступ к документу «${meta?.Name}».`;

        // ─── Комнаты ───────────────────────────────────────────────
        case "CreatedRoom":
            return `🆕 Создана комната «${meta?.RoomTitle ?? "(без названия)"}».`;
        case "InvitedToRoom":
            return `✉️ Приглашение в комнату «${meta?.RoomTitle ?? "(без названия)"}» от ${meta?.UserName}.`;
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

        // ─── Контакты ───────────────────────────────────────────────
        case "InvitedToContacts":
            return `👤 Запрос на добавление в контакты от ${meta?.UserName}.`;
        case "AcceptedContact":
            return `✅ Ваш запрос принял ${meta?.UserName}.`;
        case "DeclinedContact":
            return `🚫 Ваш запрос отклонил ${meta?.UserName}.`;

        default:
            return "ℹ️ Неизвестная активность";
    }
}

export default function ActivitiesPage() {
    const navigate = useNavigate();
    const { push } = useAlert();

    const [isLoading, setIsLoading] = useState(true);
    const [fadeOut, setFadeOut] = useState(false);
    const toggleTheme = () => {
        setTheme(t => {
            const next = t === "dark" ? "light" : "dark";
            localStorage.setItem("theme", next);   // ← обязательно
            return next;
        });
    };
    
    setTimeout(() => {
        setFadeOut(true);
        setTimeout(() => setIsLoading(false), 400);
    }, 100);

    // ─────────────────────── тема ──────────────────────────────
    
    const [theme, setTheme] = useState(() =>(
        document.body.classList.contains("dark") ? "dark" : "light"
    ));
    
    useEffect(() => {
        document.body.classList.toggle("dark", theme === "dark");
        document.body.classList.toggle("light", theme !== "dark");
    }, [theme]);

    // ─────────────────────── данные ────────────────────────────
    const [page, setPage] = useState(1);
    const [totalPages, setTotalPages] = useState(1);
    const [items, setItems] = useState(null);

    // ─────────────────────── запрос ────────────────────────────
    useEffect(() => {
        let cancelled = false;
        (async () => {
            try {
                const { data } = await api.get(`/activity/my?page=${page}&pageSize=10`);
                if (cancelled) return;
                setItems(data.items);
                setTotalPages(data.totalPages);
            } catch {
                if (!cancelled) push("Не удалось получить активности", "danger");
            }
        })();
        return () => {
            cancelled = true;
        };
    }, [page]);

    // ─────────────────────── действия ──────────────────────────
    const respondContact = async (activity, accepted) => {
        const contactId = activity.meta?.ContactId;
        if (!contactId) return;

        try {
            if (accepted) await api.post(`/contacts/${contactId}/accept`);
            else await api.post(`/contacts/${contactId}/block`);
            push("Действие выполнено", "success");
        } catch {
            push("Ошибка при обновлении контакта", "danger");
        }
    };

    const respondRoom = async (activity, accepted) => {
        const roomId = activity.roomId;
        if (!roomId) return;

        const url = accepted ? `/rooms/${roomId}/accept` : `/rooms/${roomId}/decline`;
        try {
            await api.post(url);
            push("Действие выполнено", "success");
        } catch {
            push("Ошибка при обработке приглашения", "danger");
        }
    };

    // ─────────────────── рендер элемента ───────────────────────
    const renderActivity = (a) => {
        const dt = <LocalTime utc={a.createdAt} className="small" />;

        const Wrap = ({ children, extra }) => (
            <>
                {dt}
                <div>{children}</div>
                {extra && <div className="mt-1">{extra}</div>}
            </>
        );

        switch (a.type) {
            case "InvitedToRoom":
                return (
                    <Wrap
                        extra={
                            !a.isRead && (
                                <div className="d-flex gap-2">
                                    <button className="btn btn-sm btn-success" onClick={() => respondRoom(a, true)}>
                                        Принять
                                    </button>
                                    <button className="btn btn-sm btn-outline-danger" onClick={() => respondRoom(a, false)}>
                                        Отклонить
                                    </button>
                                </div>
                            )
                        }
                    >
                        ✉️ Приглашение в комнату «<strong>{a.meta?.RoomTitle}</strong>» от {a.meta?.UserName}.
                    </Wrap>
                );

            case "InvitedToContacts":
                return (
                    <Wrap
                        extra={
                            !a.isRead && (
                                <div className="d-flex gap-2">
                                    <button className="btn btn-sm btn-success" onClick={() => respondContact(a, true)}>
                                        Принять
                                    </button>
                                    <button className="btn btn-sm btn-outline-danger" onClick={() => respondContact(a, false)}>
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
                return <Wrap>✅ Ваш запрос принял <strong>{a.meta?.UserName}</strong>.</Wrap>;
            case "DeclinedContact":
                return <Wrap>🚫 Ваш запрос отклонил <strong>{a.meta?.UserName}</strong>.</Wrap>;

            default:
                return <Wrap>{formatActivity(a)}</Wrap>;
        }
    };

    // ─────────────────────── загрузка ──────────────────────────
        if (isLoading) {
            return (
                <div className={`fade-screen ${fadeOut ? "fade-out" : ""} ${theme === "dark" ? "bg-dark" : "bg-light"}`}>
                    <LoadingSpinner size={200} />
                </div>
            );
        }

    const bgItem = theme === "dark" ? "bg-dark text-white border-secondary" : "bg-white border-light";

    return (
        <div className="container py-4 min-vh-100 d-flex flex-column">
            {/* header */}
            <div className={`card p-3 mb-4 shadow-sm ${theme === "dark" ? "bg-dark text-white border-secondary" : "bg-white border-light"}`} style={{ borderRadius: "1rem", maxWidth: "800px", margin: "0 auto" }}>
                <div className="d-flex align-items-center">
                        <FaHistory size={10} className="me-2" />
                    <h3 className="mb-0">Мои активности</h3>
                    <button className="btn btn-link ms-auto" title="Закрыть" onClick={() => navigate(-1)}>
                        <FaTimes size={18} />
                    </button>
                </div>
            </div>

            {/* список */}
            <div className={`card shadow-sm mb-4 flex-fill overflow-auto ${theme === "dark" ? "bg-dark border-secondary" : "bg-white border-light"}`} style={{ borderRadius: "1rem", maxWidth: "800px", margin: "0 auto", width: "100%" }}>
            <ul className="list-group list-group-flush">
                {(items || []).map((act) => (
                <li
                    key={act.activity.id}
                    className={`list-group-item d-flex flex-column gap-1 ${bgItem} ${act.isRead ? "" : "fw-bold border-start border-4 border-primary-subtle"}`}
                    style={{ transition: "background 0.3s", borderRadius: "0.5rem" }}
                >
                    {renderActivity({ ...act.activity, isRead: act.isRead })}
                </li>
                ))}
            </ul>
            </div>

            {/* пагинация */}
            {totalPages > 1 && (
                <nav className="d-flex justify-content-center mb-5 mt-3">
                    <ul className="pagination mb-0">
                        <li className={`page-item ${page === 1 ? "disabled" : ""}`}>
                            <button className="page-link" onClick={() => setPage((p) => p - 1)}>
                                <FaChevronLeft />
                            </button>
                        </li>
                        {Array.from({ length: totalPages }, (_, i) => i + 1).map((n) => (
                            <li key={n} className={`page-item ${n === page ? "active" : ""}`}>
                                <button className="page-link" onClick={() => setPage(n)}>
                                    {n}
                                </button>
                            </li>
                        ))}
                        <li className={`page-item ${page === totalPages ? "disabled" : ""}`}>
                            <button className="page-link" onClick={() => setPage((p) => p + 1)}>
                                <FaChevronRight />
                            </button>
                        </li>
                    </ul>
                </nav>
            )}

            {/* нижняя кнопка закрытия */}
            <div className="text-center">
                <button className="btn btn-outline-secondary" onClick={() => navigate(-1)}>
                    Закрыть
                </button>
            </div>
        </div>
    );
}