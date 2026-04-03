document.addEventListener('DOMContentLoaded', function () {
    // Check status on load
    checkStatus();

    // Event Delegation for all buttons with data-action
    document.addEventListener('click', function (event) {
        const button = event.target.closest('button[data-action]');
        if (!button) return;

        const action = button.getAttribute('data-action');

        if (action === 'download-fonts') {
            handleFontDownload();
        } else if (action === 'download-bootstrap') {
            handleFrameworkDownload('/api/admin/download-bootstrap', 'Bootstrap');
        } else if (action === 'download-jquery') {
            handleFrameworkDownload('/api/admin/download-jquery', 'jQuery');
        }
    });

    async function handleFontDownload() {
        const familyInput = document.getElementById('fontFamily');
        const weightsInput = document.getElementById('weights');
        const resultDiv = document.getElementById('fontResult');

        if (!familyInput || !weightsInput || !resultDiv) return;

        const family = familyInput.value;
        const weights = weightsInput.value;

        // Show loading
        resultDiv.innerHTML = '';
        const spinner = document.createElement('div');
        spinner.className = 'spinner-border spinner-border-sm text-primary';
        spinner.setAttribute('role', 'status');
        const span = document.createElement('span');
        span.className = 'visually-hidden';
        span.textContent = 'Downloading...';
        spinner.appendChild(span);
        resultDiv.appendChild(spinner);

        try {
            const response = await fetch('/api/admin/download-fonts', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ fontFamily: family, weights: weights })
            });
            const data = await response.json();
            showResult(resultDiv, data.success, data.message);
        } catch (e) {
            showResult(resultDiv, false, 'Error: ' + e.message);
        }
        checkStatus();
    }

    async function handleFrameworkDownload(url, name) {
        const resultDiv = document.getElementById('frameworkResult');
        if (!resultDiv) return;

        resultDiv.innerHTML = '';
        const spinner = document.createElement('div');
        spinner.className = 'spinner-border spinner-border-sm text-' + (name === 'Bootstrap' ? 'success' : 'info');
        spinner.setAttribute('role', 'status');
        const span = document.createElement('span');
        span.className = 'visually-hidden';
        span.textContent = 'Downloading...';
        spinner.appendChild(span);
        resultDiv.appendChild(spinner);

        try {
            const response = await fetch(url, { method: 'POST' });
            const data = await response.json();
            showResult(resultDiv, data.success, data.message);
        } catch (e) {
            showResult(resultDiv, false, 'Error: ' + e.message);
        }
        checkStatus();
    }

    function showResult(container, success, message) {
        container.innerHTML = '';
        const alertDiv = document.createElement('div');
        alertDiv.className = success ? 'alert alert-success' : 'alert alert-danger';
        alertDiv.textContent = message;
        container.appendChild(alertDiv);
    }

    async function checkStatus() {
        const statusList = document.getElementById('statusList');
        if (!statusList) return;

        try {
            const response = await fetch('/api/admin/status');
            const data = await response.json();

            statusList.innerHTML = ''; // Clear list

            const items = [
                { label: 'Bootstrap', key: 'bootstrap', successClass: 'bg-success', failClass: 'bg-danger' },
                { label: 'jQuery', key: 'jquery', successClass: 'bg-success', failClass: 'bg-danger' },
                { label: 'Fonts', key: 'fontCount', successClass: 'bg-info', failClass: 'bg-secondary', textClass: 'text-dark' }
            ];

            items.forEach(item => {
                const li = document.createElement('li');
                li.className = 'list-group-item d-flex justify-content-between align-items-center';

                const labelSpan = document.createElement('span');
                labelSpan.textContent = item.label;

                const badge = document.createElement('span');
                const value = data[item.key];
                const isInstalled = item.key === 'fontCount' ? value > 0 : value;
                
                badge.className = 'badge ' + (isInstalled ? item.successClass : item.failClass);
                if (item.textClass) badge.classList.add(item.textClass);
                
                badge.textContent = item.key === 'fontCount' ? value + ' files' : (isInstalled ? 'Installed' : 'Missing');

                li.appendChild(labelSpan);
                li.appendChild(badge);
                statusList.appendChild(li);
            });

        } catch (e) {
            console.error('Status check failed', e);
        }
    }
});