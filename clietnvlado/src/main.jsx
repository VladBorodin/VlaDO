import React from "react";
import ReactDOM from "react-dom/client";
import { BrowserRouter } from "react-router-dom";

import ErrorBoundary from "./ErrorBoundary";
import App from "./App";
import { AlertProvider } from "./contexts/AlertContext";

import "bootstrap/dist/css/bootstrap.min.css";
import "./index.css";

import 'react-pdf/dist/esm/Page/TextLayer.css';
import 'react-pdf/dist/esm/Page/AnnotationLayer.css';

ReactDOM.createRoot(document.getElementById("root")).render(
  <React.StrictMode>
    <AlertProvider>
      <BrowserRouter>
        <ErrorBoundary>
          <App />
        </ErrorBoundary>
      </BrowserRouter>
    </AlertProvider>
  </React.StrictMode>
);
