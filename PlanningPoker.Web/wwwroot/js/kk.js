// Small, dependency-free helpers exposed to Blazor as "Kk".
window.Kk = {
    get origin() {
        return window.location.origin;
    },

    copyText: async function (text) {
        if (navigator.clipboard && window.isSecureContext) {
            return navigator.clipboard.writeText(text);
        }

        // Fallback for plain-HTTP deployments (e.g. an internal Docker host).
        const area = document.createElement('textarea');
        area.value = text;
        area.setAttribute('readonly', '');
        area.style.position = 'fixed';
        area.style.opacity = '0';
        document.body.appendChild(area);
        area.select();
        document.execCommand('copy');
        document.body.removeChild(area);
    }
};
