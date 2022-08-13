/**
 * Gets all query string parameters from the current page.
 */
function getQueryParams(): { [item: string]: string } {
    return location.search
        ? location.search.substring(1).split("&").reduce((qd: any, item: any) => {
            let [k, v] = item.split("=");
            v = v && decodeURIComponent(v);
            (qd[k] = qd[k] || []).push(v);
            return qd
        }, {})
        : {}
}

/**
 * Returns the redirect URL for the current page.
 */
export function getRedirectUrl() {
    const queryParams = getQueryParams();
    
    if(queryParams["returnUrl"]) {
        return queryParams["returnUrl"];    
    }
    
    return "/";
}

/**
 * Redirects the user to the specified URL.
 * @param url URL to redirect to.
 */
export function redirect(url: string) {
    window.location.replace(url);
}