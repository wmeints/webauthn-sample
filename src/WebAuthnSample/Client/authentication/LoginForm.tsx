import React, {useState} from "react";
import FormTextInput from "../shared/FormTextInput";
import {authenticate, getAuthenticationOptions} from "./api";

export default function LoginForm() {
    const [userName, setUserName] = useState("");
    
    async function startAssertion() {
        console.log("Assertion started");
        
        let assertionOptions = await getAuthenticationOptions(userName);
        
        let credentials = await navigator.credentials.get({
            publicKey: assertionOptions
        });
        
        if(credentials == null || !(credentials instanceof PublicKeyCredential))
        {
            throw new Error("Can't obtain public key credentials to authenticate the user.");
        }
        
        await authenticate(credentials);
        
        return false;
    }
    
    return (
        <form>
            <FormTextInput label="Email Address" identifier="userName" setValue={setUserName} value={userName}/>
            <button type="button" onClick={startAssertion} className="btn btn-primary">Register</button>
        </form>
    );
}