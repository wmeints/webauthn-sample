export interface ValidationMessage {
    message: string
}

export interface ValidationMessageDictionary {
    [key: string]: string[]
}

export interface ValidationResult {
    messages: ValidationMessageDictionary
}

export function isInvalid<R>(result: R | ValidationResult): result is ValidationResult { 
    return (result as ValidationResult).messages !== undefined;
} 

export function isValid<R>(result: R | ValidationResult): result is R {
    return (result as ValidationResult).messages === undefined;
}