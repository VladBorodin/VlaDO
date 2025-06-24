import { useState, useEffect, useRef, useMemo } from "react";
import { FaSun, FaMoon, FaFolderOpen, FaArrowLeft, FaTimes } from "react-icons/fa";
import { Link, useNavigate } from "react-router-dom";
import api from "./api";
import { useAlert } from "./contexts/AlertContext"
import DocumentPreviewModal from "./DocumentPreviewModal";
import { LevelLabel, AccessLevelOptions } from "./constants";
import LoadingSpinner from "./LoadingSpinner";


export default function RoomManagerPage() {
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
	const [docForRoom, setDocForRoom] = useState(null);
	const [targetRoomId, setTargetRoomId] = useState(null);

	const [accessModal, setAccessModal] = useState(false);
	const [accessDoc , setAccessDoc ] = useState(null);

	const [users, setUsers] = useState([]);
	const [selUserId, setSelUser] = useState("");
	const [selLevel, setSelLvl ] = useState("Read");

	const [selectedRowId, setSelectedRowId] = useState(null);

	const [versionTreeData, setVersionTreeData] = useState(null);
	const [showTreeModal, setShowTreeModal] = useState(false);

	const [roomCtxVisible, setRoomCtxVisible] = useState(false);
	const [roomCtxPos, setRoomCtxPos] = useState({x:0,y:0});
	const [ctxRoom, setCtxRoom] = useState(null);

	const [roomAccessModal, setRoomAccessModal] = useState(false);
	const [roomForAccess , setRoomForAccess ] = useState(null);
	const [roomShares , setRoomShares] = useState([]);
	const [selRoomUserId , setSelRoomUserId] = useState("");
	const [selRoomLevel , setSelRoomLevel] = useState("Read");

	const [isLoading, setIsLoading] = useState(true);
	const [fadeOut, setFadeOut] = useState(false);

	const [deleteRoomModal, setDeleteRoomModal] = useState({
		show : false,
		room : null
	});

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

	const ROOM_TABS = {
			mine : { key: "mine", label: "Мои" },
			other : { key: "other", label: "Другие" },
			archive: { key: "archive",label: "Архив" }
	};

	const [activeRoomTab, setActiveRoomTab] = useState(ROOM_TABS.mine.key);
	const [roomDocuments, setRoomDocuments] = useState([]); 

	const [mineRooms, setMineRooms] = useState([]);
	const [otherRooms, setOtherRooms] = useState([]);

	const [roomUsersModal, setRoomUsersModal] = useState(false);
	const [roomUsers, setRoomUsers ] = useState([]);
	const [usersRoomTitle, setUsersRoomTitle] = useState("");
		
	const [addUserModal , setAddUserModal ] = useState(false);
	const [roomForAddUser, setRoomForAddUser] = useState(null);
	const [newUserId , setNewUserId ] = useState("");
	const [newUserLevel , setNewUserLevel ] = useState("Read");

	const [unarchModal, setUnarchModal] = useState(false);
	const [unarchDocs , setUnarchDocs] = useState([]);
	const [targetRoom , setTargetRoom] = useState("");

	const allRooms = useMemo(() => [...mineRooms, ...otherRooms], [mineRooms, otherRooms]);

	const loadRooms = async () => {
		const { data } = await api.get("/rooms/grouped");

		if (data && data.mine && data.other) {
			setMineRooms(data.mine);
			setOtherRooms(data.other);
			setRooms([...data.mine, ...data.other]);
		}
	};

	useEffect(() => {
		loadRooms();
		setFadeOut(true);
		setTimeout(() => setIsLoading(false), 400);
	}, []);
	
	const [renameState, setRenameState] = useState({
		show : false,
		type : null,
		item : null,
		value: ""
	});

	const loadRoomDocs = async () => {
		if (activeRoomTab === ROOM_TABS.archive.key) {
			const { data } = await api.get("/documents?type=archived");
			setRoomDocuments(data);
			setDocuments(data);
			return;
		}

		const { data } = await api.get("/documents?type=all");
		const roomDocs = data.filter(d => d.room?.id === selectedRoomId);
		setRoomDocuments(roomDocs);
		setDocuments(roomDocs);
	};
	
	useEffect(() => { loadRooms(); }, []);

	useEffect(() => { loadRoomDocs(); }, [activeRoomTab, selectedRoomId]);

	const roomWritable = r => {
		if (!r) return false;

		const al = r.accessLevel;

		if (typeof al === "number") return al >= 1;

		const num = Number(al);
		if (!Number.isNaN(num)) return num >= 1;

		const s = String(al).toLowerCase();
		return s === "edit" || s === "full";
	};
	const openRenameModal = (type, item) => {
		setRenameState({
			show : true,
			type,
			item,
			value: item.name || item.title || ""
		});
		setModalVisible(false);
		setRoomCtxVisible(false);
	};

	const confirmRename = async () => {
		const {type, item, value} = renameState;
		const name = value.trim();
		try {
			if (type === "doc") {
				await api.patch(`/documents/${selectedDoc.id}/rename`, { name });
				await loadRoomDocs();
			} else {
				await api.patch(`/rooms/${item.id}/rename`, { name });
				await loadRooms();
			}
			push("Название изменено", "success");
		} catch {
			push("Ошибка при переименовании", "danger");
		} finally {
			setRenameState(s => ({...s, show:false}));
		}
	};

	const [theme, setTheme] = useState(
		() => localStorage.getItem("theme") || "light"
	);
	
    useEffect(() => {
      document.body.classList.toggle("dark", theme === "dark");
      document.body.classList.toggle("light", theme !== "dark");
    }, [theme]);
		
	const menuRef = useRef(null);

	useEffect(() => {
		if (!roomCtxVisible) return;

		const handleMouseDown = (e) => {
			if (menuRef.current && !menuRef.current.contains(e.target)) {
				setRoomCtxVisible(false);
			}
		};

		document.addEventListener('mousedown', handleMouseDown);
		return () => document.removeEventListener('mousedown', handleMouseDown);
	}, [roomCtxVisible]);

	const onRoomContextMenu = (e, room) => {
		e.preventDefault();
		const isMine = mineRooms.some(r => r.id === room.id);
		if (!isMine) {
			push("Вы не можете управлять чужими комнатами", "warning");
			return;
		}
		setCtxRoom(room);
		setRoomCtxPos({x:e.pageX, y:e.pageY});
		setRoomCtxVisible(true);
	};

	const cardBg = thm => thm === "dark" ? "bg-dark bg-gradient text-light" : "bg-white text-dark";

		const modalContentStyle = thm => thm === "dark" ? { background: "#1e1e1e", color: "#f8f9fa" } : {};

		const modalBodyClass = thm => thm === "dark" ? "bg-dark text-light" : "bg-light text-dark";

		const LEVEL_VALUE = {
			Read: 0,
			Edit: 1,
			Full: 2
		};

		const saveRoomAccess = async () => {
			if (!selRoomUserId) return;
			if (selRoomLevel === "Close") {
				await api.delete(`/rooms/${roomForAccess.id}/users/${selRoomUserId}`);
			} else {
				await api.patch(`/rooms/${roomForAccess.id}/users/${selRoomUserId}`, { accessLevel: LEVEL_VALUE[selRoomLevel] });
			}
			const { data } = await api.get(`/rooms/${roomForAccess.id}/users`);
			setRoomShares(data);
			setRoomAccessModal(false);
		};

		const closeAllAccess = async () => {
			await api.delete(`/rooms/${roomForAccess.id}/users`);
			setRoomAccessModal(false);
		};

		const [documents, setDocuments] = useState([]);
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

	const handleShowPrev = async prevId => {
		try {
			const { data } = await api.get(`/documents/${prevId}/meta`);

			if (data.roomId) {
			const isMine = mineRooms.some(r=>r.id===data.roomId);
			setActiveRoomTab(isMine ? ROOM_TABS.mine.key : ROOM_TABS.other.key);
			setSelectedRoomId(data.roomId);
			} else {
			setActiveRoomTab(ROOM_TABS.mine.key);
			setSelectedRoomId(null);
			}

			setTimeout(()=>setSelectedRowId(prevId), 400);
		} catch {
			push("У вас нет прав на просмотр документа", "warning");
		}
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
		if (!selectedRowId) return;

		// Ждём, пока DOM обновится
		const timeout = setTimeout(() => {
			const rowEl = document.getElementById(`doc-${selectedRowId}`);
			if (rowEl) {
				rowEl.scrollIntoView({ behavior: "smooth", block: "center" });
			}
		}, 100); // 100-200 мс достаточно после рендера

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
			await loadRoomDocs();
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
			setCopyModalVisible(false);
			await loadRoomDocs();
			push("Документ скопирован", "success");
		} catch (err) {
			push("Ошибка при копировании", "danger");
		}
	};

	const openUnarchiveModal = async () => {
		const { data } = await api.get(`/documents/${selectedDoc.id}/versions`);
		setUnarchDocs(data);
		setTargetRoom("");
		setUnarchModal(true);
		setModalVisible(false);
	};


	const handleChangeAccess = async () => {
		if (!selectedDoc) return;

		const lst = await loadShares(selectedDoc.id);
		setShares(lst);
		await loadUsers();

		setSelUser(lst[0]?.userId ?? "");
		setSelLvl (lst[0]?.accessLevel ?? "Read");
		setAccessModal(true);
		setModalVisible(false);
	};

	const handleRemoveFromRoom = async (doc) => {
		if (!doc.room) return;
		try {
			await api.post(`/documents/${doc.id}/remove-room`);
			await loadRoomDocs();
			push("Документ удалён из комнаты", "success");
		} catch (e) {
			push("Не удалось удалить из комнаты", "danger");
		}
	};

	useEffect(() => { 
		loadRoomDocs();
	}, [activeRoomTab, selectedRoomId]);

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

	const clearArchive = async () => {
		if (!window.confirm("Вы уверены, что хотите полностью очистить архив?"))
			return;
		try {
			await api.delete("/documents/archived");
			await loadRoomDocs();
			push("Архив очищен", "success");
		} catch {
			push("Не удалось очистить архив", "danger");
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

	const getSortedDocuments = (list = documents) => {
		if (!sortField) return list;

		const sorted = [...list].sort((a, b) => {
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

	const defaultMenuItems = [
		{ key: "edit", label: "Изменить", action: handleEdit },
		{ key: "delete", label: "Удалить", action: handleDelete },
		{ key: "archive", label: "Архивировать", action: handleArchive },
		{ key: "copy", label: "Копировать", action: handleCopy },
		{ key: "rename", label: "Переименовать", action: () => openRenameModal("doc", selectedDoc) },
		{ key: "changeAccess", label: "Изменить доступ", action: handleChangeAccess },
		{ key: "toggleRoom", label: selectedDoc?.room ? "Удалить из комнаты" : "Добавить в комнату", action: handleToggleRoom },
		{ key: "download", label: "Скачать", action: handleDownload },
		{ key: "tree", label: "Дерево версий", action: handleShowVersionTree }
	];

	const confirmDeleteRoom = async () => {
		const room = deleteRoomModal.room;
		try {
			await api.delete(`/rooms/${room.id}`);
			if (room.id === selectedRoomId) setSelectedRoomId(null);
			await loadRooms();
			push(`Комната «${room.title}» удалена`, "success");
		} catch {
			push("Не удалось удалить комнату", "danger");
		} finally {
			setDeleteRoomModal({ show:false, room:null });
		}
	};

	const downloadRoom = async (room) => {
		try {
			const { data: docs } = await api.get(`/rooms/${room.id}/docs`);
			const ids = docs.map(d => d.id);

			if (ids.length === 0) {
			push("В комнате нет документов для скачивания", "info");
			return;
			}

			const resp = await api.post(
				`/rooms/${room.id}/docs/archive`,
				{ documentIds: ids },
				{ responseType: "blob" }
			);

			const blob = resp.data;
			const url = URL.createObjectURL(blob);
			const a = document.createElement("a");
			a.href = url;
			a.download = `${room.title || "room"}.zip`;
			document.body.appendChild(a);
			a.click();
			a.remove();
			URL.revokeObjectURL(url);

			push("Архив комнаты загружен", "success");
		} catch (e) {
			console.error(e);
			push("Не удалось скачать архив комнаты", "danger");
		}
	};

	const openAddUserModal = async room => {
		await loadUsers();
		setRoomForAddUser(room);
		setNewUserId("");
		setNewUserLevel("Read");
		setAddUserModal(true);
		setRoomCtxVisible(false);
	};

	const usersOfRoom = async room => {
		try {
			const { data } = await api.get(`/rooms/${room.id}/users`);
			setRoomUsers(data);
			setUsersRoomTitle(room.title);
			setRoomUsersModal(true);
			setRoomCtxVisible(false);
		} catch {
			push("Не удалось получить список пользователей", "danger");
		}
	};

	const handleUnarchive = async () => {
		try {
			await api.patch(`/documents/${selectedDoc.id}/unarchive`);
			push("Документ восстановлен из архива", "success");
			await loadRoomDocs();
		} catch {
			push("Не удалось разархивировать", "danger");
		} finally {
			setModalVisible(false);
		}
	};

	const archiveMenuItems = [
		{ key:"unarchive", label:"Разархивировать", action: openUnarchiveModal },
		{ key:"delete", label:"Удалить", action: handleDelete }
	];

	const handleRoomChangeAccess = async (room) => {
		const { data } = await api.get(`/rooms/${room.id}/users`);
		await loadUsers();
		setRoomForAccess(room);
		setRoomShares(data);
		setSelRoomUserId(data[0]?.userId ?? "");
		setSelRoomLevel(data[0]?.accessLevel ?? 0);
		setRoomAccessModal(true);
		setRoomCtxVisible(false);
	};


	const roomMenuItems = [
		{key:"add", label:"Добавить пользователя", action:openAddUserModal},
		{key:"list", label:"Пользователи комнаты", action:usersOfRoom},
		{key:"access", label:"Изменить доступ", action: handleRoomChangeAccess},
		{key:"dl", label:"Скачать", action: downloadRoom},
		{key:"ren", label:"Переименовать", action: room => openRenameModal("room", room) },
		{key:"del", label:"Удалить", action: room => setDeleteRoomModal({ show:true, room })}
	];

	const menuItems = activeRoomTab === ROOM_TABS.archive.key
		? archiveMenuItems
		: defaultMenuItems;

	const confirmAddUser = async () => {
		if (!newUserId) return;
		try {
			await api.post(`/rooms/${roomForAddUser.id}/users`, {
				userId: newUserId,
				accessLevel: LEVEL_VALUE[newUserLevel]
			});
			push("Пользователь добавлен", "success");
			setAddUserModal(false);
		} catch {
			push("Не удалось добавить пользователя", "danger");
		}
	};

	const allowedKeys = activeRoomTab === ROOM_TABS.archive.key ? ["unarchive", "delete"]
    : (accessMatrix[selectedDoc?.accessLevel] || []);

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
							Менеджер комнат
						</span>
					</button>

					<div className="ms-auto d-flex align-items-center gap-3">
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
				<div className="room-row d-flex flex-wrap gap-4">
					{/* ЛЕВЫЙ: документы выбранной комнаты или архива */}
					<div className="left-pane flex-grow-1">
						<div className={`card shadow h-100 ${cardBg(theme)}`}>
							<div className="table-responsive">
								<table
									className={`table table-hover align-middle ${theme === 'dark' ? 'table-dark' : ''}`}
								>
									<thead className={theme === 'dark' ? 'table-dark' : 'table-light'}>
										<tr>
											<th onClick={() => handleSort('name')} style={{ cursor: 'pointer' }}>
												Название {sortField === 'name' ? (sortDirection === 'asc' ? '↑' : '↓') : ''}
											</th>
											<th onClick={() => handleSort('version')} style={{ cursor: 'pointer' }}>
												Версия {sortField === 'version' ? (sortDirection === 'asc' ? '↑' : '↓') : ''}
											</th>
											<th onClick={() => handleSort('createdAt')} style={{ cursor: 'pointer' }}>
												Дата создания {sortField === 'createdAt' ? (sortDirection === 'asc' ? '↑' : '↓') : ''}
											</th>
											<th onClick={() => handleSort('createdBy')} style={{ cursor: 'pointer' }}>
												Создал {sortField === 'createdBy' ? (sortDirection === 'asc' ? '↑' : '↓') : ''}
											</th>
											<th onClick={() => handleSort('prev')} style={{ cursor: 'pointer' }}>
												Пред. версия {sortField === 'prev' ? (sortDirection === 'asc' ? '↑' : '↓') : ''}
											</th>
										</tr>
									</thead>

									<tbody>
										{roomDocuments.length === 0 ? (
											<tr>
												<td colSpan="6" className="text-center text-muted py-4">
													Документы не найдены
												</td>
											</tr>
										) : (
											getSortedDocuments(roomDocuments).map((doc) => (
												<tr
													key={doc.id}
													id={`doc-${doc.id}`}
													className={[
														theme === 'dark' ? 'border-secondary' : '',
														selectedRowId === doc.id ? 'row-selected' : '',
													].join(' ').trim()}
													style={{ cursor: 'pointer' }}
													onClick={() => handleRowClick(doc.id)}
													onDoubleClick={(e) => {
														e.stopPropagation();
														handleRowDoubleClick(doc.id);
													}}
													onContextMenu={(e) => onRowContextMenu(e, doc)}
												>
													<td>{doc.name}</td>
													<td>{`v${doc.version}${doc.forkPath && doc.forkPath !== '0' ? `-${doc.forkPath}` : ''}`}</td>
													<td>{new Date(doc.createdAt).toLocaleString('ru-RU')}</td>
													<td>{doc.createdBy?.name || '-'}</td>
													<td>
														{doc.previousVersionId ? (
															<button
																className="btn btn-link p-0"
																onClick={() => handleShowPrev(doc.previousVersionId)}
															>
																Просмотр
															</button>
														) : (
															'-'
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

					{/* ПРАВЫЙ: табы + список комнат */}
					<div className="right-pane">
						<div className={`card shadow h-100 ${cardBg(theme)}`}>
							{/* табы */}
							<ul className="nav nav-tabs nav-fill flex-nowrap">
								{Object.values(ROOM_TABS).map((t) => (
									<li className="nav-item" key={t.key}>
										<button
											type="button"
											className={`nav-link ${activeRoomTab === t.key ? 'active' : ''}`}
											onClick={() => {
												if (t.key!==activeRoomTab){
													setActiveRoomTab(t.key);
												}
											}}
										>
											{t.label}
										</button>
									</li>
								))}
							</ul>

							{/* список комнат / архив */}
							<div className="list-group list-group-flush">
								{activeRoomTab === ROOM_TABS.archive.key ? (
								<div className="p-3 d-flex flex-column align-items-center gap-3">
									<span className="text-muted small">Полностью очистить архив.</span>
									<button
									className="btn btn-outline-danger"
									style={{opacity:.8}}
									onClick={clearArchive}>
									Очистить
									</button>
								</div>
								) : (
									(activeRoomTab === ROOM_TABS.mine.key
										? mineRooms
										: otherRooms
									).map((r) => (
										<button
											key={r.id}
											className={`list-group-item list-group-item-action text-center
														${selectedRoomId===r.id?'active':''}`}
											onClick={()=>setSelectedRoomId(r.id)}
											onContextMenu={e=>onRoomContextMenu(e,r)}
											>
											{r.title}
										</button>
									))
								)}
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

								<button className="btn btn-primary" onClick={confirmCopy}>
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
											await loadRoomDocs();
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
									{versionTreeData.map((v, i) => {
										const path = v.forkPath.split(".");
										const isFork = path.length > 1 && path[path.length - 1] !== "0";

										return (
											<li key={v.id} className="mb-2">
												<span style={{ color: "#d63384", fontFamily: "monospace" }}>
													{isFork ? "┣" : "|"} {v.forkPath.replace(/\./g, " → ")}
												</span>
												&nbsp;|&nbsp;
												<strong>{v.name}</strong>
												&nbsp;— {new Date(v.createdOn).toLocaleString("ru-RU")}
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

			{roomCtxVisible && ctxRoom && (
				<div
					ref={menuRef}
					style={{
						position:'absolute',
						top:roomCtxPos.y, left:roomCtxPos.x, zIndex:1000,
						background: theme==="dark" ? "#1e1e1e" : "#fff",
						color: theme==="dark" ? "#f8f9fa" : "#212529",
						border: theme==="dark" ? "1px solid #555" : "1px solid #ccc",
						borderRadius:6, padding:6, minWidth:220,
						boxShadow:"0 0 10px rgba(0,0,0,.2)"
				}}>
					<div className="fw-bold mb-2">{ctxRoom.title}</div>
					<div className="list-group list-group-flush">
					{roomMenuItems.map(mi=>(
						<button key={mi.key}
								className="list-group-item list-group-item-action"
								onClick={() => mi.action(ctxRoom)}>
						{mi.label}
						</button>
					))}
					</div>
				</div>
			)}

			{renameState.show && (
				<div className="modal fade show" style={{display:"block",background:"rgba(0,0,0,.5)"}}>
					<div className="modal-dialog modal-dialog-centered">
						<div className="modal-content" style={modalContentStyle(theme)}>
							<div className={`modal-header ${modalBodyClass(theme)}`}>
								<h5 className="modal-title">
									Переименовать {renameState.type === "doc" ? "документ" : "комнату"}
								</h5>
								<button className="btn-close"
									onClick={() => setRenameState(s => ({...s, show:false}))}/>
							</div>

						<div className={`modal-body ${modalBodyClass(theme)}`}>
						<input className="form-control"
								value={renameState.value}
								onChange={e=>setRenameState(s=>({...s, value:e.target.value}))}/>
						</div>

						<div className={`modal-footer ${modalBodyClass(theme)}`}>
						<button className="btn btn-secondary"
								onClick={() => setRenameState(s => ({...s, show:false}))}>
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

			{deleteRoomModal.show && (
			<div className="modal fade show" style={{display:"block",background:"rgba(0,0,0,.5)"}}>
				<div className="modal-dialog modal-dialog-centered">
				<div className={`modal-content ${theme==="dark"?"bg-dark text-light":""}`}>
					<div className="modal-header">
					<h5 className="modal-title">Удалить комнату</h5>
					<button className="btn-close"
							onClick={()=>setDeleteRoomModal({show:false,room:null})}/>
					</div>

					<div className="modal-body">
					<p className="mb-0">
						Вы уверены, что хотите удалить комнату&nbsp;
						<strong>«{deleteRoomModal.room.title}»</strong>
						&nbsp;и все документы в ней?
					</p>
					</div>

					<div className="modal-footer">
					<button className="btn btn-secondary"
							onClick={()=>setDeleteRoomModal({show:false,room:null})}>
						Отмена
					</button>
					<button className="btn btn-danger" onClick={confirmDeleteRoom}>
						Ок
					</button>
					</div>
				</div>
				</div>
			</div>
			)}

			{roomAccessModal && roomForAccess && (
				<div className="modal fade show" style={{display:"block",background:"rgba(0,0,0,.5)"}}>
					<div className="modal-dialog modal-dialog-centered">
						<div className="modal-content" style={modalContentStyle(theme)}>

							{/* ─────────────── header ─────────────── */}
							<div className={`modal-header ${modalBodyClass(theme)}`}>
								<h5 className="modal-title">
									Доступ к комнате «{roomForAccess.title}»
								</h5>
								<button className="btn-close" onClick={() => setRoomAccessModal(false)}/>
							</div>

							{/* ─────────────── body ─────────────── */}
							<div className={`modal-body ${modalBodyClass(theme)}`}>

								{/* текущий уровень по-умолчанию */}
								<div className="d-flex justify-content-between mb-3">
									<span>Текущий уровень по-умолчанию</span>
									<span className="badge bg-info text-dark">
										{LevelLabel[roomForAccess.defaultAccessLevel]}
									</span>
								</div>

								{/* новый уровень */}
								<label className="form-label">Новый уровень</label>
								<select
									className="form-select"
									value={selRoomLevel}
									onChange={e => setSelRoomLevel(Number(e.target.value))}
								>
									{Object.entries(LevelLabel).map(([val, txt]) => (
										<option key={val} value={val}>{txt}</option>
									))}
								</select>

								{/* закрыть доступ всем (кроме владельца) */}
								<button
									className="btn btn-outline-danger btn-sm mt-3"
									onClick={async () => {
										if (!window.confirm("Удалить всех участников?")) return;
										await api.delete(`/rooms/${roomForAccess.id}/users`);
										push("Доступ закрыт для всех", "success");
										setRoomAccessModal(false);
									}}
								>
									Закрыть доступ для всех
								</button>
							</div>

							{/* ─────────────── footer ─────────────── */}
							<div className={`modal-footer ${modalBodyClass(theme)}`}>
								<button className="btn btn-secondary" onClick={() => setRoomAccessModal(false)}>
									Отмена
								</button>
								<button
									className="btn btn-primary"
									onClick={async () => {
										await api.patch(
											`/rooms/${roomForAccess.id}/access-level`,
											{ accessLevel: selRoomLevel }
										);
										push("Уровень доступа обновлён", "success");
										await loadRooms();
										setRoomAccessModal(false);
									}}
								>
									ОК
								</button>
							</div>

						</div>
					</div>
				</div>
			)}

			{roomUsersModal && (
				<div className="modal fade show" style={{display:"block",background:"rgba(0,0,0,.5)"}}>
					<div className="modal-dialog modal-dialog-centered">
						<div className="modal-content" style={modalContentStyle(theme)}>

							{/* header */}
							<div className={`modal-header ${modalBodyClass(theme)}`}>
								<h5 className="modal-title">
									Пользователи комнаты «{usersRoomTitle}»
								</h5>
								<button className="btn-close" onClick={()=>setRoomUsersModal(false)}/>
							</div>

							{/* body */}
							<div className={`modal-body ${modalBodyClass(theme)}`}>
								{/* Доступ по-умолчанию */}
								<p className="mb-2">
									<strong>Доступ по-умолчанию:&nbsp;</strong>
									<span className="badge bg-info text-dark">
									{LevelLabel[
										rooms.find(r=>r.title===usersRoomTitle)?.defaultAccessLevel ?? 0
									]}
									</span>
								</p>

								{/* Список участников */}
								{roomUsers.length === 0
									? <p className="text-muted">В комнате нет участников.</p>
									: (
										<ul className="list-group">
											{roomUsers.map(u=>(
												<li key={u.userId} className="list-group-item d-flex justify-content-between">
													<span>{u.name}</span>
													<span className="badge bg-secondary">{LevelLabel[u.accessLevel]}</span>
												</li>
											))}
										</ul>
									)}
							</div>

							{/* footer */}
							<div className={`modal-footer ${modalBodyClass(theme)}`}>
								<button className="btn btn-primary" onClick={()=>setRoomUsersModal(false)}>
									Закрыть
								</button>
							</div>
						</div>
					</div>
				</div>
			)}
			{addUserModal && roomForAddUser && (
				<div className="modal fade show" style={{display:"block",background:"rgba(0,0,0,.5)"}}>
					<div className="modal-dialog modal-dialog-centered">
						<div className="modal-content" style={modalContentStyle(theme)}>

							{/* header */}
							<div className={`modal-header ${modalBodyClass(theme)}`}>
								<h5 className="modal-title">
									Добавить пользователя в «{roomForAddUser.title}»
								</h5>
								<button className="btn-close" onClick={()=>setAddUserModal(false)}/>
							</div>

							{/* body */}
							<div className={`modal-body ${modalBodyClass(theme)}`}>
								{/* ─ Пользователь ─ */}
								<label className="form-label">Пользователь</label>
								<select className="form-select mb-3" value={newUserId} onChange={e=>setNewUserId(e.target.value)}>
									<option value="" disabled>— выберите пользователя —</option>
									{users.map(u=>(
										<option key={u.id} value={u.id}>{u.name}</option>
									))}
								</select>

								{/* ─ Уровень доступа ─ */}
								<label className="form-label">Уровень доступа</label>
								<select className="form-select" value={newUserLevel} onChange={e=>setNewUserLevel(e.target.value)}>
									{Object.entries(LevelLabel).map(([val,txt])=>(
										<option key={val} value={val}>{txt}</option>
									))}
								</select>
							</div>

							{/* footer */}
							<div className={`modal-footer ${modalBodyClass(theme)}`}>
								<button className="btn btn-secondary" onClick={()=>setAddUserModal(false)}>
									Отмена
								</button>
								<button className="btn btn-primary" disabled={!newUserId} onClick={confirmAddUser}>
									ОК
								</button>
							</div>

						</div>
					</div>
				</div>
			)}
			{unarchModal && (
			<div className="modal fade show" style={{display:"block",background:"rgba(0,0,0,.5)"}}>
				<div className="modal-dialog modal-lg modal-dialog-centered">
					<div className="modal-content" style={modalContentStyle(theme)}>

						{/* header */}
						<div className={`modal-header ${modalBodyClass(theme)}`}>
							<h5 className="modal-title">Разархивировать «{selectedDoc.name}»</h5>
							<button className="btn-close" onClick={()=>setUnarchModal(false)}/>
						</div>

						{/* body */}
						<div className={`modal-body ${modalBodyClass(theme)}`}>
							<p className="mb-2 small text-muted">Будут восстановлены все версии ветки:</p>
							<ul className="list-group mb-3">
								{unarchDocs.map(v=>(
									<li key={v.id} className="list-group-item d-flex justify-content-between">
										<span>{v.name}</span>
										<span className="badge bg-secondary">v{v.version}</span>
									</li>
								))}
							</ul>

							<label className="form-label">Куда:</label>
							<select className="form-select"
											value={targetRoom}
											onChange={e=>setTargetRoom(e.target.value)}>
								<option value="">(Вне комнаты)</option>
								{mineRooms.map(r => (
									<option key={r.id} value={r.id}>{r.title}</option>
								))}
							</select>
						</div>

						{/* footer */}
						<div className={`modal-footer ${modalBodyClass(theme)}`}>
							<button className="btn btn-secondary" onClick={()=>setUnarchModal(false)}>Отмена</button>
							<button className="btn btn-primary" onClick={async ()=>{
								try{
									await api.patch(`/documents/${selectedDoc.id}/unarchive`,
										targetRoom ? { targetRoomId: targetRoom } : { targetRoomId: null });
									push("Документы восстановлены", "success");
									await loadRoomDocs();
								}catch{
									push("Не удалось разархивировать", "danger");
								}finally{
									setUnarchModal(false);
								}
							}}>
								ОК
							</button>
						</div>

					</div>
				</div>
			</div>
		)}
		</div>
	);
}