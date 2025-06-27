import { useEffect, useState, useMemo } from "react";
import { Document, Page, pdfjs } from "react-pdf";
import { Light as SyntaxHighlighter } from "react-syntax-highlighter";
import { atomOneDark } from "react-syntax-highlighter/dist/esm/styles/hljs";
import { github as codeTheme } from 'react-syntax-highlighter/dist/esm/styles/hljs';
import api from "./api";
import typescript from "react-syntax-highlighter/dist/esm/languages/hljs/typescript";
import javascript from "react-syntax-highlighter/dist/esm/languages/hljs/javascript";
import json from "react-syntax-highlighter/dist/esm/languages/hljs/json";
import xml from 'react-syntax-highlighter/dist/esm/languages/hljs/xml';
import css from "react-syntax-highlighter/dist/esm/languages/hljs/css";
import bash from "react-syntax-highlighter/dist/esm/languages/hljs/bash";
import yaml from "react-syntax-highlighter/dist/esm/languages/hljs/yaml";
import plaintext from 'react-syntax-highlighter/dist/esm/languages/hljs/plaintext';
import csharp from 'react-syntax-highlighter/dist/esm/languages/hljs/csharp';
import workerSrc from './pdf-worker.js';
import go from "react-syntax-highlighter/dist/esm/languages/hljs/go";
import php from "react-syntax-highlighter/dist/esm/languages/hljs/php";

import Editor from "react-simple-code-editor";
import { highlight, languages } from "prismjs/components/prism-core";
import "prismjs/components/prism-clike";
import "prismjs/components/prism-javascript";
import "prismjs/components/prism-typescript";
import "prismjs/components/prism-css";
import "prismjs/components/prism-markup";
import "prismjs/components/prism-json";
import "prismjs/components/prism-yaml";
import "prismjs/components/prism-csharp";
import "prismjs/themes/prism-tomorrow.css";

SyntaxHighlighter.registerLanguage("txt", plaintext);
SyntaxHighlighter.registerLanguage("text", plaintext);
SyntaxHighlighter.registerLanguage("md", plaintext);
SyntaxHighlighter.registerLanguage("typescript", typescript);
SyntaxHighlighter.registerLanguage("ts", typescript);
SyntaxHighlighter.registerLanguage("javascript", javascript);
SyntaxHighlighter.registerLanguage("js", javascript);
SyntaxHighlighter.registerLanguage("go", go);
SyntaxHighlighter.registerLanguage("php", php);
SyntaxHighlighter.registerLanguage("json", json);
SyntaxHighlighter.registerLanguage("xml", xml);
SyntaxHighlighter.registerLanguage("html", xml);
SyntaxHighlighter.registerLanguage("css", css);
SyntaxHighlighter.registerLanguage("bash", bash);
SyntaxHighlighter.registerLanguage("sh", bash);
SyntaxHighlighter.registerLanguage("yaml", yaml);
SyntaxHighlighter.registerLanguage("csharp", csharp);
SyntaxHighlighter.registerLanguage("cs", csharp);

pdfjs.GlobalWorkerOptions.workerSrc = workerSrc;

const FRIENDLY = [
  "pdf", "jpg", "jpeg", "png", "gif",
  "txt","md","json","csv","js","ts","css","html","cs","xml","go","php"
];

const EDITABLE = ["txt", "md", "json", "js", "ts", "cs", "xml", "html", "yaml","go","php"];

const prismMap = {
  javascript: languages.javascript,
  typescript: languages.typescript,
  css:        languages.css,
  xml:        languages.markup,
  html:       languages.markup,
  json:       languages.json,
  yaml:       languages.yaml,
  csharp:     languages.clike,
  text:       languages.clike
};


export default function DocumentPreviewModal({ docId, show, onClose, onOk, theme, accessLevel, onDownload }) {
  const [meta, setMeta] = useState(null);
  const [blob, setBlob] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError]   = useState(null);
  const selectedTheme = theme === "dark" ? atomOneDark : codeTheme;
  const currentUser = sessionStorage.getItem("userId");
  const [note, setNote] = useState("");
  const [fileBlob, setFileBlob] = useState(null);

  const canEdit = useMemo(() => {
    if (!meta) return false;

    const isAuthor = meta.createdById === currentUser;

    return isAuthor || accessLevel === "Edit" || accessLevel === "Full";
  }, [meta, accessLevel]);

  useEffect(() => {
    if (!show || !docId) return;

    (async () => {
      try {
        setLoading(true);
        setError(null);

        const { data } = await api.get(`/documents/${docId}/meta`);
        setMeta(data);

        setNote(data.note || "");

        if (FRIENDLY.includes(data.extension?.toLowerCase())) {
          const resp = await api.get(`/documents/${docId}/download`, {
            responseType: "blob"
          });
          setBlob(resp.data);
          setFileBlob(resp.data);
        } else {
          setBlob(null);
          setFileBlob(null);
        }
      } catch (e) {
        setError("Не удалось загрузить документ");
      } finally {
        setLoading(false);
      }
    })();
  }, [show, docId]);

  const extension = meta?.extension;
  const url = blob ? URL.createObjectURL(blob) : null;

  const modalContentStyle = thm =>
    thm === "dark" ? { background: "#1e1e1e", color: "#f8f9fa" } : {};

  const modalBodyClass = thm =>
    thm === "dark" ? "bg-dark text-light" : "bg-light text-dark";


  const isEditorOpen = useMemo(() => {
    return canEdit && EDITABLE.includes(meta?.extension?.toLowerCase());
  }, [canEdit, meta]);

  if (!show) return null;

  const handleOk = async () => {
    if (!canEdit || !meta || !fileBlob) return;

    const formData = new FormData();
    const fileName = meta.name || "document.txt";
    const file = new File([fileBlob], fileName, { type: blob?.type });

    formData.append("file", file);
    formData.append("note", note);

    try {
      if (meta.roomTitle) {
        const roomId = meta.roomId || meta.room?.id;
        await api.post(`/api/rooms/${roomId}/docs/${meta.id}/new-version`, formData);
      } else {
        await api.post(`/documents/${meta.id}/version`, formData);
      }
      onOk?.();
      onClose?.();
    } catch (e) {
      alert("Не удалось обновить документ");
      console.error(e);
    }
  };


  return (
    <div className="modal fade show" style={{display:"block", background:"rgba(0,0,0,.5)"}}>
      <div className="modal-dialog modal-xl modal-dialog-centered modal-dialog-scrollable">
        <div className="modal-content" style={modalContentStyle(theme)}>
          <div className={`modal-header ${modalBodyClass(theme)}`}>
            <h5 className="modal-title">{meta?.name || "Документ"}</h5>
            <button className="btn-close" onClick={onClose} />
          </div>

          <div className={`modal-body row g-0 ${modalBodyClass(theme)}`}>
            {/* Левая колонка – превью */}
            <div className="col-md-8 pe-3" style={{minHeight:400}}>
              {loading && <p className="text-muted">Загрузка…</p>}
              {error && <p className="text-danger small">{error}</p>}
             {!loading && !error && meta && renderViewer(meta.extension, blob, url, meta, theme, canEdit, setFileBlob, onDownload)}
            </div>

            {/* Правая колонка – метаданные */}
            <div className="col-md-4 border-start ps-3">
              {meta && (
                <>
                  <table className={`table table-sm mb-3 ${theme === "dark" ? "table-dark table-dark-meta" : ""}`}>
                    <tbody>
                      <tr>
                        <th>Версия</th>
                        <td>
                          {`v${meta.version}${meta.forkPath && meta.forkPath !== "0" ? `-${meta.forkPath}` : ""}`}
                        </td>
                      </tr>
                      <tr><th>Размер</th><td>{(meta.size/1024).toFixed(1)}&nbsp;КБ</td></tr>
                      <tr><th>Формат</th><td>{meta.extension}</td></tr>
                      {meta.roomTitle && <tr><th>Комната</th><td>{meta.roomTitle}</td></tr>}
                      {meta.createdBy && <tr><th>Автор</th><td>{meta.createdBy}</td></tr>}
                      {meta.createdAt && <tr><th>Дата</th><td>{new Date(meta.createdAt).toLocaleString()}</td></tr>}
                    </tbody>
                  </table>

                  {canEdit && (
                    <div className="mb-3">
                      <label className="form-label">Примечание:</label>
                      <textarea
                        className={`note-box ${theme === "dark" ? "dark" : "light"}`}
                        rows={3}
                        value={note}
                        onChange={(e) => setNote(e.target.value)}
                        readOnly={!isEditorOpen}
                      />
                    </div>
                  )}
                </>
              )}
            </div>
          </div>

          <div className={`modal-footer ${modalBodyClass(theme)}`}>
            <button className="btn btn-primary" disabled={!isEditorOpen} onClick={handleOk}>
              Сохранить
            </button>
            <button className="btn btn-secondary" onClick={onClose}>Закрыть</button>
          </div>
        </div>
      </div>
    </div>
  );
}

// ──────────────────────────────────────────────────────────────────────────
// View helpers
function renderViewer(extension, blob, url, meta, theme, canEdit, setFileBlob, onDownload) {
  if (!blob || !url) {
    return (
      <div className="d-flex flex-column align-items-center justify-content-center h-100 text-muted">
        <p>Формат не поддерживается для превью.</p>
        <button
          className="btn btn-outline-secondary btn-sm"
          onClick={onDownload}
        >
          Скачать
        </button>
      </div>
    );
  }

  // картинки
  if (["jpg","jpeg","png","gif"].includes(extension)) {
    const blob = new Blob([document.data], { type: document.mimeType });
    const imgUrl = URL.createObjectURL(blob);

    return (
      <div className="img-wrapper">
        <img className="preview-img" src={url} alt={meta.name} />
      </div>
    );
  }

  // pdf
  if (extension === "pdf") {
    return <PDFViewer blob={blob} theme={theme} />;
  }

  if (canEdit && EDITABLE.includes(extension)) {
    return (
      <TextEditor
        blob={blob}
        extension={extension}
        theme={theme}
        canEdit={canEdit}
        onTextChange={(val) => setFileBlob(new Blob([val], { type: "text/plain" }))}
      />
    );
  } else if (EDITABLE.includes(extension)) {
    return (
      <TextViewer
        blob={blob}
        extension={extension}
        theme={theme}
      />
    );
  }
}

function TextViewer({ blob, extension, theme }) {
  const [text, setText] = useState("Загрузка…");
  const selectedTheme = theme === "dark" ? atomOneDark : codeTheme;

  const EXT_TO_LANG = {
    txt: "text",
    md:  "text",
    js:  "javascript",
    ts:  "typescript",
    cs:  "csharp",
    go:  "go",
    php:  "php"
  };

  useEffect(() => {
    const reader = new FileReader();
    reader.onload = () => setText(reader.result);
    reader.readAsText(blob, "utf-8");
  }, [blob]);

  return (
    <SyntaxHighlighter
      language={EXT_TO_LANG[extension] ?? extension}
      style={selectedTheme}
      customStyle={{
        maxHeight: 600,
        fontSize: 14,
        padding: "1rem",
        lineHeight: 1.5,
        fontFamily: "monospace",
        whiteSpace: "pre-wrap",
        wordBreak: "break-word",
        textAlign: "left",
        backgroundColor: theme === "dark" ? "#1e1e1e" : "transparent"
      }}
      showLineNumbers
    >
      {text}
    </SyntaxHighlighter>
  );
}

export function PDFViewer({ blob, theme }) {
  const [pageNumber, setPageNumber] = useState(1);
  const [numPages, setNumPages] = useState(null);

  const onLoadSuccess = ({ numPages }) => {
    setNumPages(numPages);
    if (pageNumber > numPages) setPageNumber(numPages);
  };

  const goToPage = (e) => {
    const value = e.target.value.trim();
    const val = parseInt(value, 10);
    if (!isNaN(val) && val >= 1 && val <= numPages) {
      setPageNumber(val);
    }
  };

  const goNext = () => setPageNumber(p => Math.min(p + 1, numPages));
  const goPrev = () => setPageNumber(p => Math.max(p - 1, 1));

  return (
    <div className={`preview-container ${theme === "dark" ? "preview-dark" : ""}`}>
      <div style={{ maxHeight: "calc(100vh - 200px)", overflowY: "auto" }}>
        <Document file={blob} onLoadSuccess={onLoadSuccess}>
          <Page pageNumber={pageNumber} width={600} />
        </Document>
      </div>

      {numPages && (
        <div className={`pdf-nav-bar ${theme === "dark" ? "pdf-nav-dark" : ""}`}>
          <button className="btn btn-outline-secondary btn-sm" onClick={goPrev} disabled={pageNumber <= 1}>◀</button>
          <input
            type="number"
            min="1"
            max={numPages}
            value={pageNumber}
            onChange={goToPage}
            className="form-control form-control-sm"
            style={{ width: "60px", textAlign: "center" }}
          />
          <span className="text-muted small">/ {numPages}</span>
          <button className="btn btn-outline-secondary btn-sm" onClick={goNext} disabled={pageNumber >= numPages}>▶</button>
        </div>
      )}
    </div>
  );
}

function TextEditor({ blob, extension, theme, canEdit, onTextChange }) {
  const [text, setText] = useState("");

  const isEditable = canEdit;

  useEffect(() => {
    const reader = new FileReader();
    reader.onload = () => setText(reader.result);
    reader.readAsText(blob, "utf-8");
  }, [blob]);

  const selectedTheme = theme === "dark" ? atomOneDark : codeTheme;

  const EXT_TO_LANG = {
    txt: "text",
    md:  "text",
    js:  "javascript",
    ts:  "typescript",
    json: "json",
    html: "xml",
    xml:  "xml",
    yaml: "yaml",
    css:  "css",
    sh:   "bash",
    bash: "bash",
    cs:   "csharp",
    php:  "php"
  };

  const lang = EXT_TO_LANG[extension] ?? "text";

  useEffect(() => {
    if (onTextChange) onTextChange(text);
  }, [text]);

  return canEdit ? (
    <Editor
      value={text}
      onValueChange={val => setText(val)}
      highlight={code => highlight(code, prismMap[lang] || languages.clike)}
      padding={12}
      textareaId="codeArea"
      className={theme === "dark" ? "editor-dark" : "editor-light"}
      style={{
        minHeight: 600,
        fontFamily: "monospace",
        fontSize: 14,
        background: theme === "dark" ? "#1e1e1e" : "#f8f9fa",
        color:      theme === "dark" ? "#f8f9fa" : "#212529",
        whiteSpace: "pre"
      }}
    />
  ) : (
    <SyntaxHighlighter
      language={lang}
      style={selectedTheme}
      showLineNumbers
      customStyle={{ /* как было */ }}
    >
      {text}
    </SyntaxHighlighter>
  );
}
