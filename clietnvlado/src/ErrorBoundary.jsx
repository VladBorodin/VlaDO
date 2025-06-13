// src/ErrorBoundary.jsx
import React from "react";
import Error500 from "./Error500";

export default class ErrorBoundary extends React.Component {
  constructor(props) {
    super(props);
    this.state = { hasError: false };
  }

  static getDerivedStateFromError(/* error */) {
    // Обновляем состояние, чтобы отобразить запасной UI
    return { hasError: true };
  }

  componentDidCatch(error, info) {
    // Можно залогировать на сервере
    console.error("ErrorBoundary caught:", error, info);
  }

  render() {
    if (this.state.hasError) {
      // Рендерим нашу 500-страницу
      return <Error500 />;
    }
    return this.props.children;
  }
}
