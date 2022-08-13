import React from "react";
import ReactDOM from "react-dom/client";
import LoginForm from "./LoginForm";

const rootElement = document.getElementById("login-form");

if (rootElement !== null) {
    const root = ReactDOM.createRoot(rootElement);
    root.render(<LoginForm/>);
} else {
    throw new Error("Root element with id login-form not found on the page!");
}