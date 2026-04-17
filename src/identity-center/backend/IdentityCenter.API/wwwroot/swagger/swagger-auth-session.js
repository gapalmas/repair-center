(function () {
  const csrfStorageKey = 'identitycenter.csrfToken';
  const deviceStorageKey = 'identitycenter.deviceId';

  function ensureDeviceId() {
    let deviceId = window.localStorage.getItem(deviceStorageKey);
    if (!deviceId) {
      deviceId = 'swagger-' + (window.crypto && window.crypto.randomUUID ? window.crypto.randomUUID() : String(Date.now()));
      window.localStorage.setItem(deviceStorageKey, deviceId);
    }
    return deviceId;
  }

  function getCsrfToken() {
    return window.sessionStorage.getItem(csrfStorageKey) || '';
  }

  function setCsrfToken(token) {
    if (token) {
      window.sessionStorage.setItem(csrfStorageKey, token);
    }
  }

  function clearCsrfToken() {
    window.sessionStorage.removeItem(csrfStorageKey);
  }

  const originalFetch = window.fetch.bind(window);
  window.fetch = async function (input, init) {
    const request = new Request(input, init || {});
    const url = request.url || '';
    const headers = new Headers(request.headers || {});

    if (url.includes('/api/Auth/login') || url.includes('/api/Auth/refresh') || url.includes('/api/Auth/revoke')) {
      headers.set('X-Device-Id', ensureDeviceId());
    }

    if (url.includes('/api/Auth/refresh') || url.includes('/api/Auth/revoke')) {
      const csrfToken = getCsrfToken();
      if (csrfToken) {
        headers.set('X-CSRF-TOKEN', csrfToken);
      }
    }

    const enrichedRequest = new Request(request, {
      headers,
      credentials: 'include'
    });

    const response = await originalFetch(enrichedRequest);

    if (url.includes('/api/Auth/login') || url.includes('/api/Auth/refresh')) {
      try {
        const data = await response.clone().json();
        if (data && data.csrfToken) {
          setCsrfToken(data.csrfToken);
        }
      } catch (error) {
      }
    }

    if (url.includes('/api/Auth/revoke') && response.ok) {
      clearCsrfToken();
    }

    return response;
  };
})();