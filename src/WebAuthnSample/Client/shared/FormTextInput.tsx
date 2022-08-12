import React from "react";

interface FormTextInputProps {
  label: string;
  identifier: string;
  setValue: (value: string) => void;
  value: string;
}

export default function FormTextInput({
  label,
  identifier,
  setValue,
  value,
}: FormTextInputProps): React.ReactElement {
  return (
    <div className="mb-3">
      <label htmlFor={identifier} className="form-label">
        {label}
      </label>
      <input type="text" id={identifier} className="form-control" onChange={(args) => setValue(args.target.value)} value={value}></input>
    </div>
  );
}
