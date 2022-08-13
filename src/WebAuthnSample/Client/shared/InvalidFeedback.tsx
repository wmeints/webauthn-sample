import React from "react";
import {ValidationMessage} from "./validation";

interface InvalidFeedbackProps {
    messages: ValidationMessage[]
}

export default function InvalidFeedback({ messages} : InvalidFeedbackProps): React.ReactElement {
    return (
        <>
            {messages.map((item, index) => <div className="invalid-feedback" key={index}>{item.message}</div>)}
        </>
    );
}