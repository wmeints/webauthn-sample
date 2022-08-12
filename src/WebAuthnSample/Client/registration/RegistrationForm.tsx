import React, {useState} from "react";
import FormTextInput from "../shared/FormTextInput";
import {getCredentialCreateOptions, registerPublicKeyCredential} from "./api";

/**
 * Component used to render the registration form.
 */
export default function RegistrationForm(): React.ReactElement {
    const [userName, setUserName] = useState("");
    const [displayName, setDisplayName] = useState("");

    async function startRegistration() {
        console.log("Registration started");

        let credentialCreateOptions = await getCredentialCreateOptions(displayName, userName);

        let credential = await navigator.credentials.create({
            publicKey: credentialCreateOptions
        });

        if (credential !== null && credential instanceof PublicKeyCredential) {
            await registerPublicKeyCredential(credential);
        } else {
            throw new Error("Failed to register public key credential. Received no credential object that is usable.");
        }

        return false;
    }

    return (
        <form onSubmit={startRegistration}>
            <FormTextInput label="Full Name" identifier="fullName" setValue={setDisplayName} value={displayName}/>
            <FormTextInput label="Email Address" identifier="userName" setValue={setUserName} value={userName}/>
            <button type="submit" className="btn btn-primary">Register</button>
        </form>
    );
}