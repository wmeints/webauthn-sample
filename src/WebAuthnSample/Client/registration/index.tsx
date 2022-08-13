import React from "react";
import ReactDOM from "react-dom/client";
import RegistrationForm from "./RegistrationForm";

const rootElement = document.getElementById("registration-form");

if (rootElement !== null) {
    const root = ReactDOM.createRoot(rootElement);
    root.render(<RegistrationForm/>);
} else {
    throw new Error("Root element with id registration-form not found on the page!");
}