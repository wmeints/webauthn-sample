import React from "react";

interface ErrorMessageProps {
    title: string
    message: string
}

export default function ErrorMessage({title, message}: ErrorMessageProps): React.ReactElement {
    return (
        <div className="alert alert-danger" role="alert">
            <h4 className="alert-heading">{title}</h4>
            <hr/>
            <p>{message}</p>
        </div>
    );
}