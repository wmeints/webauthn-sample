import React from "react";
import InvalidFeedback from "./InvalidFeedback";
import {ValidationMessage} from "./validation";
import classNames from "classnames";

interface FormTextInputProps {
    label: string
    identifier: string
    setValue: (value: string) => void
    value: string
    isRequired: boolean
    feedback: ValidationMessage[]
}

export default function FormTextInput(
    {label, identifier, setValue, value, feedback = [], isRequired = false}: FormTextInputProps
): React.ReactElement {
    return (
        <div className={classNames("mb-3", {"was-validated": feedback.length > 0})}>
            <label htmlFor={identifier} className="form-label">
                {label}
            </label>
            <input type="text" id={identifier}
                   className="form-control"
                   onChange={(args) => setValue(args.target.value)}
                   value={value} required={isRequired}
            />
            {feedback.length > 0 && <InvalidFeedback messages={feedback}/>}
        </div>
    );
}
