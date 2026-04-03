document.addEventListener('DOMContentLoaded', function () {
    const loginForm = document.getElementById('login-form');
    const statusContainer = document.getElementById('navbar-slot');

    // Initial Check
    checkAuthStatus();

    // Periodic Check (every 60 seconds)
    setInterval(checkAuthStatus, 60000);

    if (loginForm) {
        // Attach event listener via addEventListener, not onclick
        loginForm.addEventListener('submit', function (e) {
            e.preventDefault();
            handleLogin();
        });
    }

    async function handleLogin() {
        const usernameInput = document.getElementById('username');
        const passwordInput = document.getElementById('password');
        const resultDiv = document.getElementById('login-result');

        if (!usernameInput || !passwordInput) return;

        const username = usernameInput.value;
        const password = passwordInput.value;

        // Create loading indicator
        if (resultDiv) {
            resultDiv.innerHTML = ''; // Clear previous
            const spinner = document.createElement('div');
            spinner.className = 'spinner-border spinner-border-sm text-primary';
            spinner.setAttribute('role', 'status');
            const span = document.createElement('span');
            span.className = 'visually-hidden';
            span.textContent = 'Loading...';
            spinner.appendChild(span);
            resultDiv.appendChild(spinner);
        }

        try {
            const response = await fetch('/api/auth/login', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                credentials: 'include',
                body: JSON.stringify({ username, password })
            });

            const data = await response.json();

            if (resultDiv) {
                resultDiv.innerHTML = ''; // Clear spinner
                
                const alertDiv = document.createElement('div');
                alertDiv.className = data.success ? 'alert alert-success' : 'alert alert-danger';
                
                const textNode = document.createTextNode(data.message || (data.success ? 'Login successful!' : 'Login failed'));
                alertDiv.appendChild(textNode);
                
                resultDiv.appendChild(alertDiv);

                if (data.success) {
                    setTimeout(function () {
                        window.location.href = '/';
                    }, 1000);
                }
            }
        } catch (error) {
            if (resultDiv) {
                resultDiv.innerHTML = '';
                const alertDiv = document.createElement('div');
                alertDiv.className = 'alert alert-danger';
                alertDiv.textContent = 'Network error: ' + error.message;
                resultDiv.appendChild(alertDiv);
            }
        }
    }

    async function checkAuthStatus() {
        if (!statusContainer) return;

        try {
            const response = await fetch('/api/auth/status', { credentials: 'include' });
            const data = await response.json();

            statusContainer.innerHTML = ''; // Clear current content

            if (data.isAuthenticated) {
                const wrapper = document.createElement('div');
                wrapper.className = 'd-flex align-items-center';

                // Badge
                const badge = document.createElement('span');
                badge.className = 'badge bg-light text-dark me-2';
                badge.textContent = data.username;

                // Logout Button
                const logoutBtn = document.createElement('button');
                logoutBtn.className = 'btn btn-outline-danger btn-sm';
                logoutBtn.textContent = 'Logout';
                logoutBtn.setAttribute('id', 'logout-btn');
                
                // Attach event listener
                logoutBtn.addEventListener('click', async function () {
                    try {
                        await fetch('/api/auth/logout', { method: 'POST', credentials: 'include' });
                        window.location.reload();
                    } catch (err) {
                        console.error('Logout failed', err);
                    }
                });

                wrapper.appendChild(badge);
                wrapper.appendChild(logoutBtn);
                statusContainer.appendChild(wrapper);
            } else {
                const loginLink = document.createElement('a');
                loginLink.className = 'btn btn-primary-custom btn-sm';
                loginLink.href = '/auth/login';
                loginLink.textContent = 'Login';
                statusContainer.appendChild(loginLink);
            }
        } catch (error) {
            console.error('Auth status check failed:', error);
        }
    }
});