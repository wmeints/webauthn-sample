export type BufferLike = string | Array<number> | Uint8Array | ArrayBuffer;

/**
 * Converts an array-like object to an array buffer.
 * @param data  The input data to convert.
 * @returns The converted output.
 */
export function toArrayBuffer(data: BufferLike): ArrayBuffer {
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
export function toBase64Url(data: Uint8Array | ArrayBuffer): string {
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