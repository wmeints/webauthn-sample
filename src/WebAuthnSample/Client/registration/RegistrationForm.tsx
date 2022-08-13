import React, {useState} from "react";
import FormTextInput from "../shared/FormTextInput";
import {getCredentialCreateOptions, registerPublicKeyCredential} from "./api";
import {getRedirectUrl, redirect} from "../shared/navigation";
import {isInvalid, ValidationMessage} from "../shared/validation";
import ErrorMessage from "../shared/ErrorMessage";

/**
 * Component used to render the registration form.
 */
export default function RegistrationForm(): React.ReactElement {
    const [userName, setUserName] = useState("");
    const [displayName, setDisplayName] = useState("");
    const [userNameFeedback, setUserNameFeedback] = useState<Array<ValidationMessage>>([]);
    const [displayNameFeedback, setDisplayNameFeedback] = useState<Array<ValidationMessage>>([]);
    const [error, setError] = useState({message: "", visible: false});

    function validateInput() {
        let valid = true;

        if(!displayName) {
            setDisplayNameFeedback([{ message: "Full Name is required."}]);
            valid = false;
        }

        if(!userName) {
            setUserNameFeedback([{ message: "E-mail address is required."}]);
            valid = false;
        }
        
        return valid;
    }
    
    async function startRegistration() {
        setError({message: "", visible: false});
        
        if(!validateInput()) {
            return false;
        }
        
        try {
            let result = await getCredentialCreateOptions(displayName, userName);

            if(isInvalid(result)) {
                setError({ message: result.messages[""].join("\r\n"), visible: true});
                return;
            }
            
            let credential = await navigator.credentials.create({
                publicKey: result
            });

            if (credential !== null && credential instanceof PublicKeyCredential) {
                await registerPublicKeyCredential(credential);
            } else {
                setError({
                    message: "Unable to obtain the required information to register. Please try again.",
                    visible: true
                });

                return;
            }

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
            <FormTextInput
                label="Full Name" 
                identifier="fullName" 
                setValue={setDisplayName} 
                value={displayName}
                isRequired={true} 
                feedback={displayNameFeedback}
            />
            <FormTextInput 
                label="Email Address" 
                identifier="userName" 
                setValue={setUserName} 
                value={userName}
                isRequired={true} 
                feedback={userNameFeedback}
            />
            <button type="button" onClick={startRegistration} className="btn btn-primary">Register</button>
        </>
    );
}