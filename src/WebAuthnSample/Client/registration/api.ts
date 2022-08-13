import {toBase64Url, toArrayBuffer} from "../shared/conversions";
import {ValidationResult} from "../shared/validation";

/**
 * Retrieves the options for creating a new public key credential on the website.
 * @param displayName The display name for the user.
 * @param userName  The user name for the user.
 * @returns The options to use for creating a new public key credential in the browser using a USB key.
 */
export async function getCredentialCreateOptions(displayName: string, userName: string): Promise<PublicKeyCredentialCreationOptions | ValidationResult> {
    let data = {
        userName,
        displayName
    };

    let response = await fetch("/api/registration/credentialoptions", {
        method: "POST",
        body: JSON.stringify(data),
        headers: {
            "Content-Type": "application/json",
            "Accept": "application/json"
        }
    });

    if (response.status == 400) {
        let errorResponseData = await response.json();
        return {messages: errorResponseData};
    }

    let credentialCreateOptions = await response.json();

    credentialCreateOptions.user.id = toArrayBuffer(credentialCreateOptions.user.id);
    credentialCreateOptions.challenge = toArrayBuffer(credentialCreateOptions.challenge);

    credentialCreateOptions.excludeCredentials = credentialCreateOptions.excludeCredentials.map((credential: any) => {
        return {
            ...credential,
            id: toArrayBuffer(credential.id)
        };
    });

    if (credentialCreateOptions.authenticatorSelection?.authenticatorAttachment === null) {
        credentialCreateOptions.authenticatorSelection.authenticatorAttachment = undefined;
    }

    return credentialCreateOptions;
}

/**
 * Registers the public key credential with the server.
 * @param credential The credential to store on the server.
 */
export async function registerPublicKeyCredential(credential: PublicKeyCredential) {

    let attestationObject = credential.response instanceof AuthenticatorAttestationResponse && credential.response.attestationObject || null;

    if (attestationObject === null) {
        throw new Error("No attestation object found in the public key credential. Please make sure to use " +
            "'navigator.credentials.create' to make the public key credential object.");
    }

    let data = {
        id: credential.id,
        rawId: toBase64Url(credential.rawId),
        type: credential.type,
        extensions: credential.getClientExtensionResults(),
        response: {
            attestationObject: toBase64Url(attestationObject),
            clientDataJson: toBase64Url(credential.response.clientDataJSON)
        }
    };

    let response = await fetch("/api/registration/complete", {
        method: "POST",
        body: JSON.stringify(data),
        headers: {
            "Accept": "application/json",
            "Content-Type": "application/json"
        }
    });

    if (response.status != 202) {
        throw new Error("Failed to register the credential with the server");
    }
}