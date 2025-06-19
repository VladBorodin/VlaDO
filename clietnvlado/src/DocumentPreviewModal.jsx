import { useEffect, useState } from "react";
import { Document, Page } from "react-pdf"; // pdf.js wrapper
import { Light as SyntaxHighlighter } from "react-syntax-highlighter";
import { github as codeTheme } from "react-syntax-highlighter/dist/esm/styles/hljs";
import api from "./api";

const FRIENDLY = [
  "pdf", "jpg", "jpeg", "png", "gif",
  "txt", "md", "json", "csv", "js", "ts", "css", "html"
];

export default function DocumentPreviewModal({ docId, show, onClose, onOk }) {
  const [meta, setMeta] = useState(null);
  const [blob, setBlob] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError]   = useState(null);

  useEffect(() => {
    if (!show || !docId) return;

    (async () => {
      try {
        setLoading(true);
        setError(null);

        // 1. метаданные
        const { data } = await api.get(`/documents/${docId}/meta`);
        setMeta(data);

        // 2. если дружественный формат – качаем файл
        if (FRIENDLY.includes(data.ext)) {
          const resp = await api.get(`/documents/${docId}/download`, {
            responseType: "blob"
          });
          setBlob(resp.data);
        } else {
          setBlob(null);
        }
      } catch (e) {
        setError("Не удалось загрузить документ");
      } finally {
        setLoading(false);
      }
    })();
  }, [show, docId]);

  const ext = meta?.ext;
  const url = blob ? URL.createObjectURL(blob) : null;

  if (!show) return null; // ничего не рендерим, пока не нужно

  return (
    <div className="modal fade show" style={{display:"block", background:"rgba(0,0,0,.5)"}}>
      <div className="modal-dialog modal-xl modal-dialog-centered modal-dialog-scrollable">
        <div className="modal-content">
          <div className="modal-header">
            <h5 className="modal-title">{meta?.name || "Документ"}</h5>
            <button className="btn-close" onClick={onClose}/>
          </div>

          <div className="modal-body row g-0">
            {/* Левая колонка – превью */}
            <div className="col-md-8 pe-3" style={{minHeight:400}}>
              {loading && <p className="text-muted">Загрузка…</p>}
              {error && <p className="text-danger small">{error}</p>}
              {!loading && !error && renderViewer(ext, blob, url, meta)}
            </div>

            {/* Правая колонка – метаданные */}
            <div className="col-md-4 border-start ps-3">
              {meta && (
                <table className="table table-sm mb-3">
                  <tbody>
                    <tr><th>Версия</th><td>{meta.version}</td></tr>
                    <tr><th>Размер</th><td>{(meta.size/1024).toFixed(1)}&nbsp;КБ</td></tr>
                    <tr><th>Формат</th><td>{meta.ext}</td></tr>
                    {meta.roomTitle && <tr><th>Комната</th><td>{meta.roomTitle}</td></tr>}
                    {meta.createdBy && <tr><th>Автор</th><td>{meta.createdBy}</td></tr>}
                    {meta.createdAt && <tr><th>Дата</th><td>{new Date(meta.createdAt).toLocaleString()}</td></tr>}
                  </tbody>
                </table>
              )}
            </div>
          </div>

          <div className="modal-footer">
            <button className="btn btn-primary" onClick={onOk}>ОК</button>
            <button className="btn btn-secondary" onClick={onClose}>Закрыть</button>
          </div>
        </div>
      </div>
    </div>
  );
}

// ──────────────────────────────────────────────────────────────────────────
// View helpers
function renderViewer(ext, blob, url, meta) {
  if (!blob || !url) {
    return (
      <div className="d-flex flex-column align-items-center justify-content-center h-100 text-muted">
        <p>Формат не поддерживается для превью.</p>
        <a className="btn btn-outline-secondary btn-sm" href={`/api/documents/${meta.id}/download`}>
          Скачать
        </a>
      </div>
    );
  }

  // картинки
  if (["jpg","jpeg","png","gif"].includes(ext)) {
    return <img className="img-fluid" src={url} alt={meta.name}/>;
  }

  // pdf
  if (ext === "pdf") {
    return (
      <Document file={blob} loading="Загрузка PDF…">
        <Page pageNumber={1} width={600} />
      </Document>
    );
  }

  // текст/код/markdown
  return <TextViewer blob={blob} ext={ext} />;
}

function TextViewer({ blob, ext }) {
  const [text, setText] = useState("Загрузка…");

  useEffect(() => {
    const reader = new FileReader();
    reader.onload = () => setText(reader.result);
    reader.readAsText(blob, "utf-8");
  }, [blob]);

  if (ext === "md") {
    return <pre className="p-3 bg-light border overflow-auto" style={{whiteSpace:"pre-wrap"}}>{text}</pre>;
  }

  return (
    <SyntaxHighlighter language={ext} style={codeTheme} customStyle={{maxHeight:600}}>
      {text}
    </SyntaxHighlighter>
  );
}
