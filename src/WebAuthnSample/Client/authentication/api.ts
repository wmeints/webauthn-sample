import { toBase64Url, toArrayBuffer } from "../shared/conversions";
import {ValidationResult} from "../shared/validation";

/**
 * Gets the options to start an assertion operation
 * @param userName User name for which to retrieve the assertion options.
 * @returns The assertion options to use for the assertion operation.
 */
export async function getAuthenticationOptions(userName: string): Promise<PublicKeyCredentialRequestOptions | ValidationResult> {
    let response = await fetch("/api/authentication/options", {
        method: "POST",
        body: JSON.stringify({
            userName: userName
        }),
        headers: {
            "Content-Type": "application/json",
            "Accept": "application/json"
        }
    });

    if (response.status == 400) {
        let errorResponseData = await response.json();
        return {messages: errorResponseData};
    }

    let assertionOptions = await response.json();
    
    assertionOptions.challenge = toArrayBuffer(assertionOptions.challenge);
    assertionOptions.allowCredentials = assertionOptions.allowCredentials.map((credential: any) => {
        return {
            ...credential,
            id: toArrayBuffer(credential.id)
        };
    });
    
    return assertionOptions;
}

/**
 * Sends the public key credential information to the server to authenticate the user.
 * @param credentials Public key credentials to send.
 */
export async function authenticate(credentials: PublicKeyCredential) {
    const authenticatorResponse = credentials.response as AuthenticatorAssertionResponse;
    
    const data = {
        id: credentials.id,
        rawId: toBase64Url(credentials.rawId),
        type: credentials.type,
        extensions: credentials.getClientExtensionResults(),
        response: {
            authenticatorData: toBase64Url(authenticatorResponse.authenticatorData),
            clientDataJSON: toBase64Url(authenticatorResponse.clientDataJSON),
            userHandle: authenticatorResponse.userHandle !== null ? toBase64Url(authenticatorResponse.userHandle) : undefined,
            signature: toBase64Url(authenticatorResponse.signature)
        }
    };
    
    let response = await fetch("/api/authentication/login", {
        method: "POST",
        body: JSON.stringify(data),
        headers: {
            "Content-Type": "application/json",
            "Accept": "application/json"
        }
    });
    
    if(response.status !== 200) {
        throw new Error("Assertion failed. Couldn't authenticate the user.");
    }
}