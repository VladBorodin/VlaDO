import { useEffect, useState } from "react";
import { FaSearch, FaPlus, FaTrash, FaTimes } from "react-icons/fa";
import api from "./api";

export default function ContactsModal({ show, onClose, theme }) {
  const [contacts , setContacts ] = useState([]);
  const [query    , setQuery    ] = useState("");
  const [results  , setResults  ] = useState([]);
  const [loading  , setLoading  ] = useState(false);

  const dark = theme === "dark";
  const txt = dark ? "text-light" : "";
  const closeStyle = dark ? { filter: "invert(1)" } : {};

  /* ───── загрузить текущие контакты ───── */
  useEffect(() => {
    if (!show) return;
    api.get("/contacts").then(r => setContacts(r.data));
  }, [show]);

  /* ───── поиск пользователей ───── */
  const search = async () => {
    if (!query.trim()) return setResults([]);
    setLoading(true);
    try {
      const { data } = await api.get("/users/search", { params:{ q: query }});
      const ids = new Set(contacts.map(c => c.id));
      setResults(data.filter(u => !ids.has(u.id)));
    } finally { setLoading(false); }
  };

  /* ───── добавить / удалить ───── */
  const addContact = async id => {
    await api.post(`/contacts/${id}`);
    setContacts(await (await api.get("/contacts")).data);
    setResults(r => r.filter(u => u.id !== id));
  };
  const removeContact = async id => {
    await api.delete(`/contacts/${id}`);
    setContacts(c => c.filter(u => u.id !== id));
  };

  if (!show) return null;

  return (
    <div className="modal fade show" style={{display:"block",background:"rgba(0,0,0,.5)",color:   dark ? "#f8f9fa" : "#212529"}}>
      <div className="modal-dialog modal-lg modal-dialog-centered">
        <div className="modal-content" style={{background: dark ? "#1e1e1e" : "#fff"}}>
          {/* header */}
          <div className={`modal-header ${dark?"bg-dark text-light":""}`}>
            <h5 className="modal-title">Мои контакты</h5>
            <button className="btn-close" onClick={onClose} style={closeStyle} />
          </div>

          {/* body */}
          <div className="modal-body">
            {/* ── список контактов ── */}
            <h6 className={`modal-title ${txt}`}>Сохранённые</h6>
            {contacts.length===0 && <p className="text-muted">Пусто</p>}
            <ul className="list-group mb-3">
              {contacts.map(c=>(
                <li key={c.id} className={
                    `list-group-item d-flex justify-content-between ` +
                    (theme === "dark" ? "bg-dark text-light border-secondary" : "")
                }>
                  <span>{c.name}</span>
                  <button className={
                    `btn btn-sm ` +
                    (theme === "dark"
                        ? "btn-outline-light"   // светлый контур в тёмной теме
                        : "btn-outline-danger") // красный контур в светлой
                    }
                    onClick={()=>removeContact(c.id)}>
                    <FaTrash/>
                  </button>
                </li>
              ))}
            </ul>

            {/* ── поиск ── */}
            <h6 className={`modal-title ${txt}`}>Найти пользователя</h6>
            <div className="input-group mb-2">
              <input className="form-control"
                     placeholder="Имя пользователя…"
                     value={query}
                     onChange={e=>setQuery(e.target.value)}
                     onKeyDown={e=>e.key==="Enter"&&search()}/>
              <button className="btn btn-primary" onClick={search}>
                {loading? "..." : <FaSearch/>}
              </button>
            </div>

            {results.length>0 && (
              <ul className="list-group">
                {results.map(u=>(
                  <li key={u.id} className="list-group-item d-flex justify-content-between">
                    <span>{u.name}</span>
                    <button className="btn btn-sm btn-success"
                            title="Добавить в контакты"
                            onClick={()=>addContact(u.id)}>
                      <FaPlus/>
                    </button>
                  </li>
                ))}
              </ul>
            )}
          </div>

          {/* footer */}
          <div className={`modal-footer ${dark?"bg-dark":""}`}>
            <button className="btn btn-secondary" onClick={onClose}>
              <FaTimes className="me-1"/> Закрыть
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}