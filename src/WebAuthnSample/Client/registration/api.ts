type BufferLike = string | Array<number> | Uint8Array | ArrayBuffer;

/**
 * Converts an array-like object to an array buffer.
 * @param data  The input data to convert.
 * @returns The converted output.
 */
function toArrayBuffer(data: BufferLike): ArrayBuffer {
    if (typeof data === "string") {
        data = data.replace(/-/g, "+").replace(/_/g, "/");

        let str = window.atob(data);
        let bytes = new Uint8Array(str.length);

        for (let i = 0; i < str.length; i++) {
            bytes[i] = str.charCodeAt(i);
        }

        return bytes;
    }

    if (Array.isArray(data)) {
        return new Uint8Array(data);
    }

    if (data instanceof Uint8Array) {
        return data.buffer;
    }

    return data;
}

/**
 * Converts an array-like object to a base64 URL-encoded string.
 * @param data The input data to convert.
 * @returns The URL-encoded base64 string.
 */
function toBase64Url(data: Uint8Array | ArrayBuffer): string {
    if (data instanceof ArrayBuffer) {
        data = new Uint8Array(data);
    }

    if (data instanceof Uint8Array) {
        let str = "";
        let length = data.byteLength;

        for (let i = 0; i < length; i++) {
            str += String.fromCharCode(data[i]);
        }

        return window.btoa(str).replace(/\+/g, "-").replace(/\//g, "_").replace(/=*$/g, "");
    }

    throw new Error(`Invalid input data: ${data}`);
}

/**
 * Retrieves the options for creating a new public key credential on the website.
 * @param displayName The display name for the user.
 * @param userName  The user name for the user.
 * @returns The options to use for creating a new public key credential in the browser using a USB key.
 */
export async function getCredentialCreateOptions(displayName: string, userName: string): Promise<PublicKeyCredentialCreationOptions | undefined> {
    let data = {
        userName,
        displayName
    };

    try {
        let response = await fetch("/api/registration/credentialoptions", {
            method: "POST",
            body: JSON.stringify(data),
            headers: {
                "Content-Type": "application/json",
                "Accept": "application/json"
            }
        });

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
    } catch (ex) {
        throw ex;
    }
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
    
    if(response.status != 202) {
        throw new Error("Failed to register the credential with the server");
    }
}