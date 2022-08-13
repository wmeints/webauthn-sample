import React, {useState} from "react";
import FormTextInput from "../shared/FormTextInput";
import {authenticate, getAuthenticationOptions} from "./api";
import {getRedirectUrl, redirect} from "../shared/navigation";
import ErrorMessage from "../shared/ErrorMessage";
import {ValidationMessage} from "../shared/validation";

export default function LoginForm() {
    const [userName, setUserName] = useState("");
    const [userNameFeedback, setUserNameFeedback] = useState<Array<ValidationMessage>>([]);
    const [error, setError] = useState({message: "", visible: false});

    function validateInput() {
        if (!userName) {
            setUserNameFeedback([{message: "E-mail address is required."}]);
            return false;
        }

        return true;
    }

    async function startAssertion() {
        setError({message: "", visible: false});

        if (!validateInput()) {
            return false;
        }

        try {
            let assertionOptions = await getAuthenticationOptions(userName);

            let credentials = await navigator.credentials.get({
                publicKey: assertionOptions
            });

            if (credentials == null || !(credentials instanceof PublicKeyCredential)) {
                setError({
                    message: "Unable to obtain the required information to login. Please try again.",
                    visible: true
                });

                return;
            }

            await authenticate(credentials);

            redirect(getRedirectUrl() ?? "/");
        } catch (error: any) {
            setError({
                message: `Failed to login due to a technical problem: ${error.message}. Please try again.`,
                visible: true
            });
        }

        return false;
    }

    return (
        <>
            {error.visible && <ErrorMessage title={"Unable to login"} message={error.message}/>}
            <FormTextInput label="Email Address" identifier="userName" setValue={setUserName} value={userName}
                           isRequired={true} feedback={userNameFeedback}/>
            <button type="button" onClick={startAssertion} className="btn btn-primary">Login</button>
            <a href={`/Identity/Account/Register?returnUrl=${encodeURIComponent(getRedirectUrl())}`}
               className="btn btn-link">Register new account</a>
        </>
    );
}