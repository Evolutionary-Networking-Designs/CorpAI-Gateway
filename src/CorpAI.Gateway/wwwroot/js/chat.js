document.addEventListener('DOMContentLoaded', function () {
    const chatContainer = document.getElementById('chat-container');
    const promptInput = document.getElementById('prompt-input');
    const sendBtn = document.getElementById('send-btn');

    if (!chatContainer || !promptInput || !sendBtn) return;

    // Load history
    loadHistory();

    // Send button click
    sendBtn.addEventListener('click', sendMessage);

    // Enter key support
    promptInput.addEventListener('keydown', function (e) {
        if (e.key === 'Enter' && !e.shiftKey) {
            e.preventDefault();
            sendMessage();
        }
    });

    // Quick actions delegation
    document.addEventListener('click', function (e) {
        const btn = e.target.closest('.quick-action');
        if (btn) {
            promptInput.value = btn.getAttribute('data-query');
            sendMessage();
        }
    });

    async function loadHistory() {
        try {
            const res = await fetch('/api/chat/history');
            const messages = await res.json();
            renderMessages(messages);
        } catch (e) {
            console.error('Failed to load history', e);
        }
    }

    function renderMessages(messages) {
        chatContainer.innerHTML = '';
        messages.forEach(msg => {
            const wrapper = document.createElement('div');
            wrapper.className = 'mb-2 text-' + (msg.role === 'user' ? 'end' : 'start');

            const badge = document.createElement('span');
            badge.className = 'badge ' + (msg.role === 'user' ? 'chat-bubble-user' : 'chat-bubble-assistant');
            badge.style.cssText = 'white-space: pre-wrap; max-width: 80%;';
            badge.textContent = msg.content;

            wrapper.appendChild(badge);
            chatContainer.appendChild(wrapper);
        });
        scrollToBottom();
    }

    async function sendMessage() {
        const text = promptInput.value.trim();
        if (!text) return;

        // Optimistic UI: Add user message
        const userMsg = { role: 'user', content: text };
        appendMessage(userMsg);
        promptInput.value = '';

        // Show loading
        const loadingId = 'loading-' + Date.now();
        const loadingWrapper = document.createElement('div');
        loadingWrapper.id = loadingId;
        loadingWrapper.className = 'mb-2 text-start';
        const loadingBadge = document.createElement('span');
        loadingBadge.className = 'badge bg-secondary';
        loadingBadge.textContent = 'Thinking...';
        loadingWrapper.appendChild(loadingBadge);
        chatContainer.appendChild(loadingWrapper);
        scrollToBottom();

        try {
            const res = await fetch('/api/chat/send', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ prompt: text })
            });
            const messages = await res.json();
            
            // Remove loading
            const loadingEl = document.getElementById(loadingId);
            if (loadingEl) loadingEl.remove();

            renderMessages(messages);
        } catch (e) {
            const loadingEl = document.getElementById(loadingId);
            if (loadingEl) {
                loadingEl.querySelector('.badge').textContent = 'Error: Connection failed.';
                loadingEl.querySelector('.badge').className = 'badge bg-danger';
            }
        }
    }

    function appendMessage(msg) {
        const wrapper = document.createElement('div');
        wrapper.className = 'mb-2 text-' + (msg.role === 'user' ? 'end' : 'start');
        const badge = document.createElement('span');
        badge.className = 'badge ' + (msg.role === 'user' ? 'chat-bubble-user' : 'chat-bubble-assistant');
        badge.style.cssText = 'white-space: pre-wrap; max-width: 80%;';
        badge.textContent = msg.content;
        wrapper.appendChild(badge);
        chatContainer.appendChild(wrapper);
        scrollToBottom();
    }

    function scrollToBottom() {
        chatContainer.scrollTop = chatContainer.scrollHeight;
    }
});