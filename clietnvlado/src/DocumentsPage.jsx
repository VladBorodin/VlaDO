import { useState, useEffect, useRef } from "react";
import { FaSun, FaMoon, FaFolderOpen, FaArrowLeft, FaTimes, FaPlus } from "react-icons/fa";
import { Link, useNavigate } from "react-router-dom";
import api from "./api";
import { useAlert } from "./contexts/AlertContext"
import DocumentPreviewModal from "./DocumentPreviewModal";
import LoadingSpinner from "./LoadingSpinner";
import Notifications from "./Notifications";
import LocalTime from "./components/LocalTime";

export default function DocumentsPage() {
  const navigate = useNavigate();
  const { push } = useAlert();

  const [previewId, setPreviewId] = useState(null);
  const [copyModalVisible, setCopyModalVisible] = useState(false);
  const [rooms, setRooms] = useState([]);
  const [selectedRoomId, setSelectedRoomId] = useState(null);
  const storedUserId = sessionStorage.getItem("userId");
  const currentUser = storedUserId ? storedUserId : null;
  const [menuPosition, setMenuPosition] = useState({ x: 0, y: 0 });
  const [renameModalVisible, setRenameModalVisible] = useState(false);
  const [newName, setNewName] = useState("");
  const [sortField, setSortField] = useState(null);
  const [sortDirection, setSortDirection] = useState("asc");
  const [addRoomModal, setAddRoomModal] = useState(false);
  const [docForRoom,   setDocForRoom] = useState(null);
  const [targetRoomId, setTargetRoomId] = useState(null);

  const [accessModal, setAccessModal] = useState(false);
  const [accessDoc , setAccessDoc ] = useState(null);

  const [users, setUsers] = useState([]);
  const [selUserId, setSelUser] = useState("");
  const [selLevel, setSelLvl ] = useState("Read");

  const [selectedRowId, setSelectedRowId] = useState(null);

  const [versionTreeData, setVersionTreeData] = useState(null);
  const [showTreeModal, setShowTreeModal] = useState(false);

  const [isLoading, setIsLoading] = useState(true);
  const [fadeOut, setFadeOut] = useState(false);

  const loadUsers = async () => {
    if (users.length) return;
    const { data } = await api.get("/contacts");
    setUsers(Array.isArray(data) ? data : []);
  };

  const loadShares = async docId => {
    const { data } = await api.get(`/documents/${docId}/tokens`);
    return Array.isArray(data) ? data : [];
  };
  const [shares, setShares] = useState([]);
  
  const loadRooms = async () => {
    if (rooms.length) return;
    const { data } = await api.get("/rooms/my");
    setRooms(data);
  };
  const roomWritable = r => {
    if (!r) return false;

    const al = r.accessLevel;

    if (typeof al === "number") return al >= 1;

    const num = Number(al);
    if (!Number.isNaN(num)) return num >= 1;

    const s = String(al).toLowerCase();
    return s === "edit" || s === "full";
  };
  const openRenameModal = () => {
    if (!selectedDoc) return;
    setNewName(selectedDoc.name || "");
    setRenameModalVisible(true);
    setModalVisible(false); // закрываем контекстное меню
  };

  const confirmRename = async () => {
    if (!selectedDoc) return;

    try {
      await api.patch(`/documents/${selectedDoc.id}/rename`, {
        name: newName
      });

      setDocuments(prev =>prev.map(d =>d.id === selectedDoc.id ? { ...d, name: newName } : d));
      await loadDocs();
    } catch (err) {
      push("Ошибка при переименовании", "danger");
    } finally {
      push("Название изменено", "success");
      setRenameModalVisible(false);
    }
  };

  const getInitialTheme = () => {
    const saved = localStorage.getItem("theme");
    if (saved) return saved;
    return window.matchMedia("(prefers-color-scheme: dark)").matches
          ? "dark" : "light";
  };

  const [theme, setTheme] = useState(getInitialTheme);

  useEffect(() => {
    document.body.classList.toggle("dark", theme === "dark");
    document.body.classList.toggle("light", theme !== "dark");
    localStorage.setItem("theme", theme);
  }, [theme]);
  
  useEffect(() => {
    document.body.classList.toggle("dark", theme === "dark");
    document.body.classList.toggle("light", theme !== "dark");
  }, [theme]);

  const cardBg = thm => thm === "dark" ? "bg-dark bg-gradient text-light" : "bg-white text-dark";

  const modalContentStyle = thm => thm === "dark" ? { background: "#1e1e1e", color: "#f8f9fa" } : {};

    const modalBodyClass = thm =>thm === "dark" ? "bg-dark text-light" : "bg-light text-dark";

    const tabs = [
      { key: "lastupdate", label: "Все актуальные" },
      { key: "userDoc", label: "Свои документы" },
      { key: "otherDoc", label: "Чужие документы" },
      { key: "all", label: "Все" }
    ];
    const [activeTab, setActiveTab] = useState("lastupdate");
    const LEVEL_VALUE = {
      Read: 0,
      Edit: 1,
      Full: 2
    };
    const loadDocs = async (tab = activeTab) => {
      let endpoint = "/documents";
      if (tab === "userDoc")   endpoint += "?type=userDoc";
      if (tab === "otherDoc")  endpoint += "?type=otherDoc";
      if (tab === "lastupdate")endpoint += "?type=lastupdate";
      if (tab === "all")       endpoint += "?type=all";

      const { data } = await api.get(endpoint);
      setDocuments(Array.isArray(data) ? data : []);
    };

    const [documents, setDocuments] = useState([]);
    useEffect(() => {
      let endpoint = "/documents";

      if (activeTab === "userDoc") {
        endpoint = "/documents?type=userDoc";
      } else if (activeTab === "otherDoc") {
        endpoint = "/documents?type=otherDoc";
      } else if (activeTab === "lastupdate") {
        endpoint = "/documents?type=lastupdate";
      } else if (activeTab === "all") {
        endpoint = "/documents?type=all";
      }

    api.get(endpoint)
      .then((res) => {
        if (Array.isArray(res.data)) {
          setDocuments(res.data);
        }
        setFadeOut(true);
        setTimeout(() => setIsLoading(false), 400);
      })
      .catch((err) => {
        console.error("Ошибка при загрузке документов:", err);
      });
  }, [activeTab]);

  const tabClass = (key) =>
    `nav-link ${activeTab === key ? "active" : ""}`;

  const [modalVisible, setModalVisible] = useState(false);
  const [selectedDoc, setSelectedDoc] = useState(null);
  const modalRef = useRef(null);

  const onRowContextMenu = (e, doc) => {
    e.preventDefault();
    setSelectedDoc(doc);
    setAccessDoc(doc);
    setMenuPosition({ x: e.pageX, y: e.pageY });
    setModalVisible(true);
  };
  
  const accessMatrix = {
    Full: ["edit", "delete", "archive", "copy", "rename", "changeAccess", "toggleRoom", "download", "paste", "tree"],
    Edit: ["edit", "copy", "rename", "changeAccess", "toggleRoom", "download", "paste", "tree"],
    Read: ["copy", "download", "tree"]
  };

  const LEVEL_LABEL = {
    Read : "Чтение",
    Edit : "Изменение",
    Full : "Полный",
    Close: "Закрыть доступ"
  };

  const usedUserIds = new Set(shares.map(s => s.userId));
  const availableUsers = users.filter(u => !usedUserIds.has(u.id));

  useEffect(()=>{ loadDocs(); }, [activeTab]);

  useEffect(() => {
    const handleClickOutside = (e) => {
      if (modalVisible && modalRef.current && !modalRef.current.contains(e.target)) {
        setModalVisible(false);
      }
    };
    
    if (modalVisible) {
      document.addEventListener("mousedown", handleClickOutside);
    } else {
      document.removeEventListener("mousedown", handleClickOutside);
    }
    return () => {
      document.removeEventListener("mousedown", handleClickOutside);
    };
  }, [modalVisible]);

  useEffect(() => {
    const docToHighlight = sessionStorage.getItem("highlightDocId");
    const savedTab = sessionStorage.getItem("activeTab");

    if (savedTab) {
      setActiveTab(savedTab);

      setTimeout(() => {
        if (docToHighlight) {
          setSelectedRowId(docToHighlight);
          const rowEl = document.getElementById(`doc-${docToHighlight}`);
          if (rowEl) rowEl.scrollIntoView({ behavior: "smooth", block: "center" });
        }

        sessionStorage.removeItem("highlightDocId");
        sessionStorage.removeItem("activeTab");
      }, 500);
    }
  }, []);

  useEffect(() => {
    if (!selectedRowId) return;

    const timeout = setTimeout(() => {
      const rowEl = document.getElementById(`doc-${selectedRowId}`);
      if (rowEl) {
        rowEl.scrollIntoView({ behavior: "smooth", block: "center" });
      }
    }, 100);

    return () => clearTimeout(timeout);
  }, [selectedRowId]);

  const handleEdit = () => {
    if (!selectedDoc) return;
    setModalVisible(false);
    navigate(`/documents/${selectedDoc.id}/update`, {
      state: { parentDoc: selectedDoc }
    });
  };

  const handleRowDoubleClick = async (id) => {
    const doc = documents.find(d => d.id === id);
    if (!doc) return;

    const sharesForDoc = await loadShares(doc.id);
    const level = getEffectiveAccessLevel(doc, sharesForDoc, rooms);

    setSelectedDoc({ ...doc, effectiveAccessLevel: level });
    setPreviewId(id);
  };

  function getEffectiveAccessLevel(doc, shares, rooms) {
    const currentUser = sessionStorage.getItem("userId");

    if (doc.createdBy.id === currentUser) return "Full";

    const token = shares.find(s => s.documentId === doc.id);
    if (token) return token.accessLevel;

    const room = doc.room ? rooms.find(r => r.id === doc.room.id) : null;
    return room?.accessLevel || "Read";
  }

  const handleDelete = async () => {
    if (!selectedDoc) return;
    try {
      await api.delete(`/documents/${selectedDoc.id}`);
      setDocuments(prev => prev.filter(d => d.id !== selectedDoc.id));
    } catch (err) {
      push("Не удалось удалить документ", "danger");
    } finally {
      push("Документ удалён", "success");
      setModalVisible(false);
    }
  };

  const handleArchive = async () => {
    if (!selectedDoc) return;
    try {
      await api.post(`/documents/${selectedDoc.id}/archive`);
      push("Документ заархивирован", "success");
      await loadDocs();
    } catch (e) {
      push("Ошибка при архивировании", "danger");
    } finally {
      setModalVisible(false);
    }
  };

  const handleCopy = async () => {
    if (!selectedDoc) return;

    try {
      const { data } = await api.get("/rooms/my");
      setRooms(data);
      setSelectedRoomId(null);
      setCopyModalVisible(true);
      setModalVisible(false);
    } catch (err) {
      push("Не удалось получить список комнат", "danger");
    }
  };

  const confirmCopy = async () => {
    if (!selectedDoc) return;

    try {
      await api.post(`/documents/${selectedDoc.id}/copy`, {
        targetRoomId: selectedRoomId
      });
      setActiveTab(activeTab);
      setCopyModalVisible(false);
      await loadDocs();
      push("Документ скопирован", "success");
    } catch (err) {
      push("Ошибка при копировании", "danger");
    }
  };

  const forkSegments = fp =>
    fp === "0" ? [0] : fp.split(".").map(Number);

  const parseMajor = v => Number(String(v).split("-")[0]);

  const treeCompare = (a, b) => {
    const as = forkSegments(a.forkPath);
    const bs = forkSegments(b.forkPath);

    if (as.length !== bs.length) return as.length - bs.length;

    for (let i = 0; i < as.length; i++) {
      if (as[i] !== bs[i]) return as[i] - bs[i];
    }

    return parseMajor(a.version) - parseMajor(b.version);
  };

  const handleChangeAccess = async () => {
    if (!selectedDoc) return;

    const lst = await loadShares(selectedDoc.id);
    setShares(lst);
    await loadUsers();

    setSelUser(lst[0]?.userId  ?? "");
    setSelLvl (lst[0]?.accessLevel ?? "Read");
    setAccessModal(true);
    setModalVisible(false);
  };

  const handleRemoveFromRoom = async (doc) => {
    if (!doc.room) return;
    try {
      await api.post(`/documents/${doc.id}/remove-room`);
      await loadDocs();
      push("Документ удалён из комнаты", "success");
    } catch (e) {
      push("Не удалось удалить из комнаты", "danger");
    }
  };

  const handleAddToRoom = async (doc) => {
    await loadRooms();
    setDocForRoom(doc);
    setTargetRoomId(null);
    setAddRoomModal(true);
  };

  const handleToggleRoom = () => {
    if (!selectedDoc) return;
     setModalVisible(false);
     selectedDoc.room
       ? handleRemoveFromRoom(selectedDoc)
       : handleAddToRoom(selectedDoc);
  };

  const handleDownload = async () => {
    if (!selectedDoc) return;

    try {
      const token = sessionStorage.getItem("token");
      const response = await fetch(`/api/documents/${selectedDoc.id}/download`, {
        method: "GET",
        headers: {
          "Authorization": `Bearer ${token}`
        }
      });

      if (!response.ok) {
        push("Ошибка при скачивании файла", "danger");
        return;
      }

      const blob = await response.blob();
      const url = window.URL.createObjectURL(blob);

      const link = document.createElement("a");
      link.href = url;
      link.download = selectedDoc.name || `document_${selectedDoc.id}`;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);

      window.URL.revokeObjectURL(url);
      setModalVisible(false);
    } catch (err) {
      push("Ошибка при скачивании файла", "danger");
    }
  };

  const handleShowVersionTree = async () => {
    if (!selectedDoc) return;

    try {
      const { data } = await api.get(`/documents/${selectedDoc.id}/versions`);
      setVersionTreeData(data);
      setShowTreeModal(true);
    } catch (e) {
      push("Не удалось загрузить дерево версий", "danger");
    } finally {
      setModalVisible(false);
    }
  };
  
  const handleSort = (field) => {
    if (sortField === field) {
      setSortDirection((prev) => (prev === "asc" ? "desc" : "asc"));
    } else {
      setSortField(field);
      setSortDirection("asc");
    }
  };

  const handleRowClick = (docId) => {
    setSelectedRowId(docId);
  };

  const seen = new Set();

  const getSortedDocuments = () => {
    if (!sortField) return documents;

    const sorted = [...documents].sort((a, b) => {
      let valA = a[sortField];
      let valB = b[sortField];

      if (sortField === "createdAt") {
        valA = new Date(valA);
        valB = new Date(valB);
      }
      if (sortField === "prev") {
        valA = a.previousVersionId ? 1 : 0;
        valB = b.previousVersionId ? 1 : 0;
        return sortDirection === "asc" ? valA - valB : valB - valA;
      }
      if (sortField === "room") {
        valA = a.room?.title || "";
        valB = b.room?.title || "";
        return sortDirection === "asc"
          ? valA.localeCompare(valB)
          : valB.localeCompare(valA);
      }

      if (valA == null) return 1;
      if (valB == null) return -1;

      if (typeof valA === "string") {
        return sortDirection === "asc"
          ? valA.localeCompare(valB)
          : valB.localeCompare(valA);
      }

      if (sortField === "createdBy") {
        valA = a.createdBy?.name || "";
        valB = b.createdBy?.name || "";
        return sortDirection === "asc"
          ? valA.localeCompare(valB)
          : valB.localeCompare(valA);
      }
      return sortDirection === "asc" ? valA - valB : valB - valA;
    });

    return sorted;
  };

  const menuItems = [
    { key: "edit", label: "Изменить", action: handleEdit },
    { key: "delete", label: "Удалить", action: handleDelete },
    { key: "archive", label: "Архивировать", action: handleArchive },
    { key: "copy", label: "Копировать", action: handleCopy },
    { key: "rename", label: "Переименовать", action: openRenameModal },
    { key: "changeAccess", label: "Изменить доступ", action: handleChangeAccess },
    { key: "toggleRoom", label: selectedDoc?.room ? "Удалить из комнаты" : "Добавить в комнату", action: handleToggleRoom },
    { key: "download", label: "Скачать", action: handleDownload },
    { key: "tree", label: "Дерево версий", action: handleShowVersionTree }
  ];

  const allowedKeys = accessMatrix[selectedDoc?.accessLevel] || [];

  if (isLoading) {
    return (
      <div className={`fade-screen ${fadeOut ? "fade-out" : ""} ${theme === "dark" ? "bg-dark" : "bg-light"}`}>
        <LoadingSpinner size={200} />
      </div>
    );
  }

  return (
    <div className="min-vh-100 d-flex flex-column">
      {/* Header */}
      <nav
        className="navbar navbar-expand-lg shadow-sm"
        style={{
          background:
            theme === "dark"
              ? "linear-gradient(90deg, #2c2c2c, #1a1a1a)"
              : "#fff",
        }}
      >
        <div className="container">
          <button
            className="btn btn-link d-flex align-items-center p-0"
            onClick={() => navigate("/")}
          >
            <FaFolderOpen
              size={28}
              className={theme === "dark" ? "text-warning" : "text-primary"}
            />
            <span
              className={`ms-2 fw-bold ${
                theme === "dark" ? "text-light" : "text-dark"
              }`}
            >
              Менеджер документов
            </span>
          </button>

          <div className="ms-auto d-flex align-items-center gap-3">
            <Link
              to="/documents/create"
              className="btn btn-success btn-sm"
              title="Создать документ"
            >
              <FaPlus className="me-1" /> Документ
            </Link>
            <Notifications theme={theme} />
            <button
              className="btn btn-link"
              title="Переключить тему"
              onClick={() =>
                setTheme(theme === "dark" ? "light" : "dark")
              }
              style={{ color: theme === "dark" ? "#ccc" : "#333" }}
            >
              {theme === "dark" ? <FaSun size={20} /> : <FaMoon size={20} />}
            </button>
            <button
              className="btn btn-outline-secondary btn-sm"
              title="Назад"
              onClick={() => navigate("/")}
            >
              <FaArrowLeft className="me-1" />
              Выйти
            </button>
          </div>
        </div>
      </nav>

      {/* Контент */}
      <div className="container my-4 flex-fill">
        {/* Вкладки */}
        <ul className="nav nav-tabs mb-3">
          {tabs.map((t) => (
            <li className="nav-item" key={t.key}>
              <button
                className={tabClass(t.key)}
                onClick={() => setActiveTab(t.key)}
                style={{ cursor: "pointer" }}
                type="button"
              >
                {t.label}
              </button>
            </li>
          ))}
        </ul>
{/* Рабочая область */}
<div className="doc-row d-flex flex-wrap gap-4">
  {/* ЛЕВАЯ панель — таблица документов */}
  <div className="left-pane flex-grow-1">
    <div className={`card shadow h-100 ${cardBg(theme)}`}>
      <div className="table-responsive"></div>
        {/* Таблица документов */}
        <div className="table-responsive">
          <table className={`table table-hover align-middle ${cardBg(theme)} ${theme==="dark" ? "table-dark" : ""}`}>
            <thead className={theme === "dark" ? "table-dark" : "table-light"}>
              <tr>
                <th style={{cursor:"pointer"}} onClick={() => handleSort("name")}>
                  Название {sortField==="name" ? (sortDirection==="asc"?"↑":"↓") : ""}
                </th>
                <th style={{cursor:"pointer"}} onClick={() => handleSort("version")}>
                  Версия {sortField==="version" ? (sortDirection==="asc"?"↑":"↓") : ""}
                </th>
                <th style={{cursor:"pointer"}} onClick={() => handleSort("createdAt")}>
                  Дата&nbsp;создания {sortField==="createdAt" ? (sortDirection==="asc"?"↑":"↓") : ""}
                </th>
                <th style={{cursor:"pointer"}} onClick={() => handleSort("createdBy")}>
                  Создал {sortField==="createdBy" ? (sortDirection==="asc"?"↑":"↓") : ""}
                </th>
                <th style={{cursor:"pointer"}} onClick={() => handleSort("prev")}>
                   Пред.&nbsp;версия {sortField==="prev" ? (sortDirection==="asc"?"↑":"↓") : ""}
                 </th>
                 <th style={{cursor:"pointer"}} onClick={() => handleSort("room")}>
                   Комната {sortField==="room" ? (sortDirection==="asc"?"↑":"↓") : ""}
                 </th>
              </tr>
            </thead>
            <tbody>
              {documents.length === 0 ? (
                <tr>
                  <td colSpan="6" className="text-center text-muted py-4">
                    Документы не найдены
                  </td>
                </tr>
              ) : (
                getSortedDocuments().map((doc) => (
                  <tr
                    key={doc.id}
                    id={`doc-${doc.id}`}
                    className={[
                      theme === "dark" ? "border-secondary" : "",
                      selectedRowId === doc.id ? "row-selected" : ""
                    ].join(" ").trim()}
                    onClick={() => handleRowClick(doc.id)}
                    onDoubleClick={(e) => {
                      e.stopPropagation();
                      handleRowDoubleClick(doc.id);
                    }}
                    onContextMenu={(e) => onRowContextMenu(e, doc)}
                    style={{ cursor: "pointer" }}
                  >
                    <td>{doc.name}</td>
                    <td>
                      {`v${doc.version}${doc.forkPath && doc.forkPath !== "0" ? `-${doc.forkPath}` : ""}`}
                    </td>
                    <td><LocalTime utc={doc.createdAt} /></td>
                    <td>{doc.createdBy?.name || "-"}</td>
                    <td>
                      {doc.previousVersionId ? (
                        <button
                          className="btn btn-link p-0"
                          onClick={(e) => {
                            e.preventDefault();
                            setActiveTab("all");
                            setTimeout(() => {
                              setSelectedRowId(doc.previousVersionId);
                              const rowEl = document.getElementById(`doc-${doc.previousVersionId}`);
                              if (rowEl) rowEl.scrollIntoView({ behavior: "smooth", block: "center" });
                            }, 300);
                          }}
                        >
                          Просмотр
                        </button>
                      ) : (
                        "-"
                      )}
                    </td>
                    <td>
                      {doc.room ? (
                        <Link to={`/rooms/${doc.room.id}`} className="link-secondary">
                          {doc.room.title}
                        </Link>
                      ) : (
                        <span className="text-muted">—</span>
                      )}
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
        </div>
    </div>
  </div>
      </div>

      {/* Футер */}
      <footer
        className={`py-3 text-center ${
          theme === "dark" ? "bg-dark text-secondary" : "bg-light text-muted"
        }`}
      >
        &copy; {new Date().getFullYear()} VlaDO
      </footer>

      {renameModalVisible && (
        <div
          className="modal fade show"
          style={{ display: "block", background: "rgba(0,0,0,0.5)" }}
        >
          <div className="modal-dialog modal-dialog-centered">
            <div className="modal-content" style={modalContentStyle(theme)}>
              <div className={`modal-header ${modalBodyClass(theme)}`}>
                <h5 className="modal-title">Переименовать документ</h5>
                <button className="btn-close" onClick={() => setRenameModalVisible(false)} />
              </div>

              <div className={`modal-body ${modalBodyClass(theme)}`}>
                <input
                  className="form-control"
                  value={newName}
                  onChange={(e) => setNewName(e.target.value)}
                />
              </div>

              <div className={`modal-footer ${modalBodyClass(theme)}`}>
                <button className="btn btn-secondary" onClick={() => setRenameModalVisible(false)}>
                  Отмена
                </button>
                <button className="btn btn-primary" onClick={confirmRename}>
                  ОК
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      {copyModalVisible && (
        <div className="modal fade show"
            style={{display:"block", background:"rgba(0,0,0,.5)"}}>
          <div className="modal-dialog modal-dialog-centered">
            <div className="modal-content" style={modalContentStyle(theme)}>

              <div className={`modal-header ${modalBodyClass(theme)}`}>
                <h5 className="modal-title">Копировать документ</h5>
                <button className="btn-close"
                        onClick={()=>setCopyModalVisible(false)}/>
              </div>

              <div className={`modal-body ${modalBodyClass(theme)}`}>
                <label className="form-label fw-bold mb-2">
                  Выберите место
                </label>

                <select
                  className="form-select"
                  value={selectedRoomId ?? ""}
                  onChange={e => {
                    const val = e.target.value;
                    setSelectedRoomId(val === "" ? null : val.trim());
                  }}
                >

                  {/* вне комнаты */}
                  <option value="">(Вне комнаты)</option>

                  {/* доступные пользователю комнаты */}
                  {rooms.map(r => (
                    <option
                      key={r.id}
                      value={r.id}
                      disabled={r.accessLevel === "Read"}
                    >
                      {r.title} — {r.accessLevel === "Read"
                        ? "только чтение"
                        : "можно изменять"}
                    </option>
                  ))}
                </select>
              </div>

              <div className={`modal-footer ${modalBodyClass(theme)}`}>
                <button className="btn btn-secondary"
                        onClick={()=>setCopyModalVisible(false)}>
                  Отмена
                </button>

                <button className="btn btn-primary"
                        onClick={confirmCopy}>
                  Копировать
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      {addRoomModal && docForRoom && (
        <div className="modal fade show" style={{display:"block", background:"rgba(0,0,0,.5)"}}>
          <div className="modal-dialog modal-dialog-centered">
            <div className="modal-content" style={modalContentStyle(theme)}>
              <div className={`modal-header ${modalBodyClass(theme)}`}>
                <h5 className="modal-title">Добавить «{docForRoom.name}» в комнату</h5>
                <button className="btn-close" onClick={()=>setAddRoomModal(false)}/>
              </div>

              <div className={`modal-body ${modalBodyClass(theme)}`}>
                <select
                  className="form-select"
                  value={targetRoomId ?? ""}
                  onChange={e => setTargetRoomId(e.target.value || null)}
                >
                  <option value="">Выберите комнату…</option>
                  {rooms.map(r => (
                    <option key={r.id} value={r.id}>
                      {r.title}
                    </option>
                  ))}
                </select>
              </div>

              <div className={`modal-footer ${modalBodyClass(theme)}`}>
                <button className="btn btn-secondary" onClick={()=>setAddRoomModal(false)}>
                  Отмена
                </button>
                <button
                  className="btn btn-primary"
                  disabled={!targetRoomId}
                  onClick={async () => {
                    try {
                      await api.post(
                        `/documents/${docForRoom.id}/add-to-room/${targetRoomId}`
                      );
                      setAddRoomModal(false);
                      await loadDocs();
                    } catch (e) {
                      console.error(e);
                      push("Не удалось добавить в комнату", "danger");
                    }
                  }}
                >
                  Добавить
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      <DocumentPreviewModal
        docId={previewId}
        show={!!previewId}
        onClose={() => setPreviewId(null)}
        onOk={() => {
          setPreviewId(null);
          loadDocs();
        }}
        theme={theme}   
        accessLevel={selectedDoc?.effectiveAccessLevel}
        onDownload={handleDownload}
      />

      {accessModal && accessDoc && (
        <div className="modal fade show"
            style={{display:"block", background:"rgba(0,0,0,.5)"}}>
          <div className="modal-dialog modal-dialog-centered">
            <div className="modal-content" style={modalContentStyle(theme)}>

              <div className={`modal-header ${modalBodyClass(theme)}`}>
                <h5 className="modal-title">Изменить доступ</h5>
                <button className="btn-close"
                        onClick={()=>setAccessModal(false)}/>
              </div>

              <div className={`modal-body ${modalBodyClass(theme)}`}>
                {/* ─── Документ + версия ───────────────────────────── */}
                <div className="d-flex justify-content-between mb-2">
                  <span className="fw-bold">{accessDoc.name}</span>
                  <span className="badge bg-secondary">v{accessDoc.version}</span>
                </div>

                {/* ─── Комната и дефолтный уровень ────────────────── */}
                <div className="d-flex justify-content-between mb-3">
                  <span>
                    Комната:&nbsp;
                    {accessDoc.room
                      ? <strong>{accessDoc.room.title}</strong>
                      : <em className="text-muted">(без&nbsp;комнаты)</em>}
                  </span>
                  <span className="badge bg-info text-dark">
                    {accessDoc.room ? LEVEL_LABEL[accessDoc.accessLevel] : "—"}
                  </span>
                </div>

                {/* ─── Пользователь ───────────────────────────── */}
                <label className="form-label">Пользователь</label>
                <select className="form-select mb-3"
                  value={selUserId}
                  onChange={e=>{
                    const uid = e.target.value;
                    setSelUser(uid);
                    const sh = shares.find(s=>s.userId===uid);
                    setSelLvl(sh?.accessLevel ?? "Read");
                  }}>
                  <option value="" disabled>— выберите пользователя —</option>
                  {/* Уже имеющие доступ — из токенов */}
                  {shares.map(s => (
                    <option key={s.userId} value={s.userId}>
                      {s.userName} — {LEVEL_LABEL[s.accessLevel]}
                    </option>
                  ))}
                  {/* Все остальные пользователи */}
                  {availableUsers.map(u => (
                    <option key={u.id} value={u.id}>
                      {u.name}
                    </option>
                  ))}
                </select>

                {/* ─── Уровень доступа ────────────────────────── */}
                <label className="form-label">Уровень доступа</label>
                <select className="form-select"
                  value={selLevel}
                  onChange={e => setSelLvl(e.target.value)}>
                  {Object.entries(LEVEL_LABEL).map(([val, txt]) => (
                    <option key={val} value={val}>{txt}</option>
                  ))}
                </select>
              </div>

              <div className={`modal-footer ${modalBodyClass(theme)}`}>
                <button className="btn btn-secondary"
                        onClick={()=>setAccessModal(false)}>
                  Отмена
                </button>
                <button className="btn btn-primary"
                  disabled={!selUserId}
                  onClick={async ()=>{
                    try{
                      const share = shares.find(s=>s.userId===selUserId);

                      if (selLevel==="Close"){
                        if (share) {
                          await api.delete(`/documents/${accessDoc.id}/token/user/${selUserId}`);
                          push("Доступ закрыт", "success");
                        } else {
                          push("У пользователя нет доступа", "warning");
                        }
                      }else{
                        await api.post(`/documents/${accessDoc.id}/token`, {
                          userId: selUserId,
                          accessLevel: LEVEL_VALUE[selLevel]
                        });
                        push("Доступ предоставлен", "success");
                      }
                      const updatedShares = await loadShares(accessDoc.id);
                      setShares(updatedShares);
                      setAccessModal(false);
                    }catch(e){
                      console.error(e);
                      push("Не удалось изменить доступ", "danger");
                    }
                  }}>
                  Ок
                </button>
              </div>

            </div>
          </div>
        </div>
      )}

      {modalVisible && selectedDoc && (
        <div
          ref={modalRef}
          style={{
            position: "absolute",
            top: `${menuPosition.y}px`,
            left: `${menuPosition.x}px`,
            zIndex: 1000,
            backgroundColor: theme==="dark" ? "#1e1e1e" : "#fff",
            color: theme==="dark" ? "#f8f9fa" : "#212529",
            border: theme==="dark" ? "1px solid #555" : "1px solid #ccc",
            borderRadius: "6px",
            padding: "0.5rem",
            boxShadow: "0 0 10px rgba(0,0,0,0.2)",
            minWidth: "200px",
          }}
        >
          <div className="fw-bold mb-2">{selectedDoc.name}</div>
          <div className="list-group list-group-flush">
            {menuItems
              .filter(item => allowedKeys.includes(item.key))
              .map(item => (
                <button
                  key={item.key}
                  className="list-group-item list-group-item-action"
                  onClick={item.action}
                >
                  {item.label}
                </button>
              ))}
          </div>
        </div>
      )}

      {showTreeModal && versionTreeData && (
        <div className="modal fade show" style={{display:"block", background:"rgba(0,0,0,.5)"}}>
          <div className="modal-dialog modal-lg modal-dialog-centered">
            <div className="modal-content" style={{backgroundColor: theme === "dark" ? "#1e1e1e" : "#fff",
              color: theme === "dark" ? "#f8f9fa" : "#212529"}}>
              <div className={`modal-header ${theme === "dark" ? "bg-dark text-light" : ""}`}>
                <h5 className="modal-title">Дерево версий</h5>
                <button className="btn-close" onClick={() => setShowTreeModal(false)} />
              </div>
              <div className="modal-body">
                <ul className="list-unstyled">
  {([...versionTreeData].sort(treeCompare)).map(v => {
    const isFirst = !seen.has(v.forkPath);
    seen.add(v.forkPath);

    return (
      <li key={v.id} className="mb-2">
        <span style={{ color:"#d63384", fontFamily:"monospace" }}>
          {`v${v.version}${v.forkPath!=="0" ? `-${v.forkPath}` : ""}`}{" "}
          {isFirst && v.forkPath!=="0" ? "┣" : "|"}{" "}
          {v.forkPath.replace(/\./g," → ")}
        </span>
        {" | "}
        <strong>{v.name}</strong>
        {" — "}
        {new Date(v.createdOn).toLocaleString("ru-RU")}
      </li>
    );
  })}
</ul>
              </div>
              <div className={`modal-footer ${theme === "dark" ? "bg-dark text-light" : ""}`}>
                <button className="btn btn-secondary" onClick={() => setShowTreeModal(false)}>Закрыть</button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}