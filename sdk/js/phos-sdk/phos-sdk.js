// sdk/js/phos-sdk/phos-sdk.js
// Minimal browser SDK (no bundler). Exposes global PhosSDK.
(function (root) {
  const listeners = new Set();
  let context = { userId: null, orgId: null, scopes: [] };
  let accessToken = null;

  function init(opts = {}) {
    context = { ...context, ...opts.context };
    accessToken = opts.accessToken ?? null;
    // fire initial context
    listeners.forEach(l => { try { l({ type: "context", context }); } catch {} });
    return { ok: true };
  }

  function onContext(cb) {
    if (typeof cb !== "function") throw new Error("onContext requires a function");
    listeners.add(cb);
    // deliver current context immediately
   try { cb({ type: "context", context }); } catch {}
    return () => listeners.delete(cb);
  }

  function setAccessToken(token) { accessToken = token || null; }
  function getAccessToken() { return accessToken; }
  function getContext() { return { ...context }; }

  // Placeholder FHIR request. In MVP we just hit gateway directly with fetch.
  async function requestFHIR(path, init = {}) {
    if (!path || !path.startsWith("/")) throw new Error("requestFHIR requires absolute path starting with '/'");
    const headers = new Headers(init.headers || {});
    if (accessToken) headers.set("Authorization", `Bearer ${accessToken}`);
    headers.set("Accept", "application/json");
    const res = await fetch(path, { ...init, headers });
    if (!res.ok) {
      const text = await res.text();
      throw new Error(`FHIR ${res.status}: ${text}`);
    }
    return res.json();
  }

  root.PhosSDK = {
    init, onContext, getContext,
    setAccessToken, getAccessToken,
    requestFHIR
  };
})(window);
