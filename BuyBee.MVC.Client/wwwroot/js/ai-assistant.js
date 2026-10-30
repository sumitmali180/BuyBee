/**
 * Buybee AI Shopping Assistant Client JS
 * Supports both the bottom-right floating widget and the full /ai page
 */

(function () {
    'use strict';

    // -------------------------------------------------------------------------
    // Markdown / Safe Text Formatter
    // -------------------------------------------------------------------------
    function formatMarkdown(text) {
        if (!text) return '';
        let escaped = text
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;');

        // Bold **text**
        escaped = escaped.replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>');
        // Italic *text*
        escaped = escaped.replace(/\*(.*?)\*/g, '<em>$1</em>');
        // Inline code `text`
        escaped = escaped.replace(/`(.*?)`/g, '<code class="px-1 py-0.5 rounded bg-light">$1</code>');

        // Lists: convert bullet points
        const lines = escaped.split('\n');
        let inList = false;
        let htmlLines = [];

        for (let i = 0; i < lines.length; i++) {
            let line = lines[i].trim();
            if (line.startsWith('- ') || line.startsWith('* ')) {
                if (!inList) {
                    htmlLines.push('<ul class="mb-2 ps-3">');
                    inList = true;
                }
                htmlLines.push('<li>' + line.substring(2) + '</li>');
            } else if (/^\d+\.\s/.test(line)) {
                if (!inList) {
                    htmlLines.push('<ol class="mb-2 ps-3">');
                    inList = true;
                }
                htmlLines.push('<li>' + line.replace(/^\d+\.\s/, '') + '</li>');
            } else {
                if (inList) {
                    htmlLines.push('</ul>');
                    inList = false;
                }
                if (line.length > 0) {
                    htmlLines.push('<p class="mb-1">' + line + '</p>');
                }
            }
        }
        if (inList) {
            htmlLines.push('</ul>');
        }

        return htmlLines.join('');
    }

    // -------------------------------------------------------------------------
    // Add to Cart helper
    // -------------------------------------------------------------------------
    function handleAddToCart(productId, btnElement) {
        if (!productId) return;

        const originalHtml = btnElement.innerHTML;
        btnElement.disabled = true;
        btnElement.innerHTML = '<span class="spinner-border spinner-border-sm" role="status"></span> Adding...';

        const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
        const token = tokenInput ? tokenInput.value : '';

        const formData = new URLSearchParams();
        formData.append('productId', productId);
        formData.append('quantity', '1');
        if (token) {
            formData.append('__RequestVerificationToken', token);
        }

        fetch('/Cart/AddToCart', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                'X-Requested-With': 'XMLHttpRequest'
            },
            body: formData.toString()
        })
        .then(res => res.json())
        .then(data => {
            if (data.redirect) {
                window.location.href = data.redirect;
                return;
            }

            btnElement.innerHTML = '<i class="bi bi-check-lg"></i> Added!';
            btnElement.classList.remove('bb-ai-btn-cart');
            btnElement.classList.add('btn', 'btn-sm', 'btn-success');

            // Update badge if exists
            const cartBadges = document.querySelectorAll('.bb-cart-badge, #cartBadge, .badge.bg-warning');
            cartBadges.forEach(b => {
                if (data.count !== undefined) b.textContent = data.count;
            });

            // Trigger Buybee Cart Toast if present
            const toastEl = document.getElementById('cartToast');
            if (toastEl && typeof bootstrap !== 'undefined') {
                const toast = new bootstrap.Toast(toastEl);
                const toastMsg = document.getElementById('cartToastMsg');
                if (toastMsg) toastMsg.textContent = 'Product added to cart!';
                toast.show();
            }

            setTimeout(() => {
                btnElement.disabled = false;
                btnElement.innerHTML = originalHtml;
                btnElement.classList.remove('btn', 'btn-sm', 'btn-success');
                btnElement.classList.add('bb-ai-btn-cart');
            }, 2500);
        })
        .catch(err => {
            console.error('Cart add error:', err);
            btnElement.disabled = false;
            btnElement.innerHTML = originalHtml;
            alert('Could not add product to cart. Please check your connection or sign in.');
        });
    }

    // -------------------------------------------------------------------------
    // AI Chat Controller Class
    // -------------------------------------------------------------------------
    class BuybeeAiChat {
        constructor(options) {
            this.container = options.container;
            this.messagesEl = options.messagesEl;
            this.inputEl = options.inputEl;
            this.sendBtn = options.sendBtn;
            this.micBtn = options.micBtn;
            this.isFullPage = options.isFullPage || false;
            this.history = [];
            this.isProcessing = false;

            this.init();
        }

        init() {
            if (!this.container) return;

            // Submit on click
            if (this.sendBtn) {
                this.sendBtn.addEventListener('click', () => this.sendMessage());
            }

            // Submit on Enter (without Shift)
            if (this.inputEl) {
                this.inputEl.addEventListener('keydown', (e) => {
                    if (e.key === 'Enter' && !e.shiftKey) {
                        e.preventDefault();
                        this.sendMessage();
                    }
                });
            }

            // Speech Recognition
            if (this.micBtn) {
                this.setupVoiceInput();
            }

            // Quick pill triggers
            const suggestionPills = this.container.querySelectorAll('.bb-ai-pill, .bb-ai-quick-tag');
            suggestionPills.forEach(pill => {
                pill.addEventListener('click', () => {
                    const prompt = pill.getAttribute('data-prompt') || pill.textContent.trim();
                    if (this.inputEl) {
                        this.inputEl.value = prompt;
                    }
                    this.sendMessage();
                });
            });

            // Delegate "Add to Cart" inside messages
            if (this.messagesEl) {
                this.messagesEl.addEventListener('click', (e) => {
                    const cartBtn = e.target.closest('.bb-ai-btn-cart');
                    if (cartBtn) {
                        e.preventDefault();
                        const pid = cartBtn.getAttribute('data-product-id');
                        handleAddToCart(pid, cartBtn);
                    }
                });
            }
        }

        setupVoiceInput() {
            const SpeechRec = window.SpeechRecognition || window.webkitSpeechRecognition;
            if (!SpeechRec) {
                this.micBtn.style.display = 'none';
                return;
            }

            const recognition = new SpeechRec();
            recognition.continuous = false;
            recognition.interimResults = false;
            recognition.lang = 'en-IN';

            let isListening = false;

            this.micBtn.addEventListener('click', () => {
                if (isListening) {
                    recognition.stop();
                    return;
                }
                try {
                    recognition.start();
                    isListening = true;
                    this.micBtn.classList.add('listening');
                    this.micBtn.setAttribute('title', 'Listening... click to stop');
                } catch (err) {
                    console.warn('Speech start error', err);
                }
            });

            recognition.onresult = (event) => {
                const transcript = event.results[0][0].transcript;
                if (this.inputEl) {
                    this.inputEl.value = transcript;
                }
                this.micBtn.classList.remove('listening');
                isListening = false;
            };

            recognition.onerror = () => {
                this.micBtn.classList.remove('listening');
                isListening = false;
            };

            recognition.onend = () => {
                this.micBtn.classList.remove('listening');
                isListening = false;
            };
        }

        scrollToBottom() {
            if (this.messagesEl) {
                this.messagesEl.scrollTop = this.messagesEl.scrollHeight;
            }
        }

        showTyping() {
            const typingEl = document.createElement('div');
            typingEl.className = 'bb-ai-msg assistant bb-ai-typing-wrap';
            typingEl.id = 'bbAiTypingIndicator';
            typingEl.innerHTML = `
                <div class="bb-ai-msg-avatar p-0 overflow-hidden"><img src="/images/buybee-mascot-icon.png" alt="BeeBot" width="30" height="30" style="object-fit: cover; border-radius: 8px;" /></div>
                <div class="bb-ai-typing">
                    <div class="bb-ai-typing-dot"></div>
                    <div class="bb-ai-typing-dot"></div>
                    <div class="bb-ai-typing-dot"></div>
                </div>
            `;
            this.messagesEl.appendChild(typingEl);
            this.scrollToBottom();
        }

        hideTyping() {
            const el = document.getElementById('bbAiTypingIndicator');
            if (el) el.remove();
        }

        appendUserMessage(text) {
            const msgEl = document.createElement('div');
            msgEl.className = 'bb-ai-msg user';
            msgEl.innerHTML = `
                <div class="bb-ai-msg-avatar"><i class="bi bi-person-fill"></i></div>
                <div class="bb-ai-bubble">
                    <p class="mb-0">${text.replace(/</g, '&lt;').replace(/>/g, '&gt;')}</p>
                </div>
            `;
            this.messagesEl.appendChild(msgEl);
            this.history.push({ role: 'user', content: text });
            this.scrollToBottom();
        }

        appendAssistantMessage(text, products = [], toolsUsed = []) {
            const msgEl = document.createElement('div');
            msgEl.className = 'bb-ai-msg assistant';

            let toolsHtml = '';
            if (toolsUsed && toolsUsed.length > 0) {
                toolsHtml = `
                    <div class="mb-2">
                        ${toolsUsed.map(t => `<span class="bb-ai-tool-pill"><i class="bi bi-lightning-fill"></i> ${t}</span>`).join(' ')}
                    </div>
                `;
            }

            let productsHtml = '';
            if (products && products.length > 0) {
                if (this.isFullPage) {
                    // Grid for full page
                    productsHtml = `
                        <div class="bb-ai-page-products">
                            ${products.map(p => this.renderFullPageProductCard(p)).join('')}
                        </div>
                    `;
                } else {
                    // Mini cards for widget
                    productsHtml = `
                        <div class="bb-ai-product-grid">
                            ${products.map(p => this.renderMiniProductCard(p)).join('')}
                        </div>
                    `;
                }
            }

            msgEl.innerHTML = `
                <div class="bb-ai-msg-avatar p-0 overflow-hidden"><img src="/images/buybee-mascot-icon.png" alt="BeeBot" width="30" height="30" style="object-fit: cover; border-radius: 8px;" /></div>
                <div class="bb-ai-bubble">
                    ${toolsHtml}
                    <div>${formatMarkdown(text)}</div>
                    ${productsHtml}
                </div>
            `;

            this.messagesEl.appendChild(msgEl);
            this.history.push({ role: 'assistant', content: text });
            this.scrollToBottom();
        }

        renderMiniProductCard(p) {
            const img = p.thumbnailImage || '/images/products/placeholder.jpg';
            const price = Number(p.price).toLocaleString('en-IN', { maximumFractionDigits: 2 });
            const inStock = p.quantity > 0;
            const stockBadge = inStock 
                ? `<span class="bb-ai-stock-badge bb-ai-stock-in"><i class="bi bi-check-circle"></i> In Stock (${p.quantity})</span>`
                : `<span class="bb-ai-stock-badge bb-ai-stock-out"><i class="bi bi-x-circle"></i> Out of Stock</span>`;

            return `
                <div class="bb-ai-product-card">
                    <img src="${img}" alt="${p.productName}" class="bb-ai-product-thumb" onerror="this.src='/images/buybee-icon.svg'" />
                    <div class="bb-ai-product-info">
                        <div>
                            <a href="/Products/Details/${p.productId}" target="_blank" class="bb-ai-product-title" title="${p.productName}">
                                ${p.productName}
                            </a>
                            <div class="bb-ai-product-meta">
                                <span class="bb-ai-product-price">₹${price}</span>
                                ${stockBadge}
                            </div>
                        </div>
                        <div class="bb-ai-product-actions">
                            <button type="button" class="bb-ai-btn-cart" data-product-id="${p.productId}" ${!inStock ? 'disabled' : ''}>
                                <i class="bi bi-cart-plus"></i> Add to Cart
                            </button>
                            <a href="/Products/Details/${p.productId}" target="_blank" class="bb-ai-btn-view">
                                View <i class="bi bi-arrow-up-right"></i>
                            </a>
                        </div>
                    </div>
                </div>
            `;
        }

        renderFullPageProductCard(p) {
            const img = p.thumbnailImage || '/images/products/placeholder.jpg';
            const price = Number(p.price).toLocaleString('en-IN', { maximumFractionDigits: 2 });
            const inStock = p.quantity > 0;
            const stockBadge = inStock 
                ? `<span class="bb-ai-stock-badge bb-ai-stock-in"><i class="bi bi-check-circle"></i> In Stock (${p.quantity})</span>`
                : `<span class="bb-ai-stock-badge bb-ai-stock-out"><i class="bi bi-x-circle"></i> Out of Stock</span>`;

            return `
                <div class="bb-ai-page-product-card">
                    <div class="bb-ai-page-thumb-wrap">
                        <img src="${img}" alt="${p.productName}" class="bb-ai-page-thumb" onerror="this.src='/images/buybee-icon.svg'" />
                    </div>
                    <div class="d-flex justify-content-between align-items-center mb-1">
                        <small class="text-muted fw-semibold">${p.brandName || p.categoryName || 'Buybee'}</small>
                        ${stockBadge}
                    </div>
                    <a href="/Products/Details/${p.productId}" class="bb-ai-product-title fw-bold text-decoration-none" title="${p.productName}">
                        ${p.productName}
                    </a>
                    <div class="mt-auto pt-2">
                        <div class="d-flex justify-content-between align-items-baseline mb-2">
                            <span class="fs-5 fw-extrabold text-dark">₹${price}</span>
                            ${p.sku ? `<small class="text-muted">${p.sku}</small>` : ''}
                        </div>
                        <div class="d-flex gap-2">
                            <button type="button" class="bb-ai-btn-cart w-100 py-2" data-product-id="${p.productId}" ${!inStock ? 'disabled' : ''}>
                                <i class="bi bi-cart-plus"></i> Add to Cart
                            </button>
                            <a href="/Products/Details/${p.productId}" class="bb-ai-btn-view px-3 py-2">
                                <i class="bi bi-eye"></i>
                            </a>
                        </div>
                    </div>
                </div>
            `;
        }

        async sendMessage() {
            if (!this.inputEl || this.isProcessing) return;

            const text = this.inputEl.value.trim();
            if (!text) return;

            this.inputEl.value = '';
            this.isProcessing = true;
            if (this.sendBtn) this.sendBtn.disabled = true;

            this.appendUserMessage(text);
            this.showTyping();

            try {
                const payload = {
                    messages: this.history,
                    contextUrl: window.location.pathname
                };

                const res = await fetch('/ai/chat', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'X-Requested-With': 'XMLHttpRequest'
                    },
                    body: JSON.stringify(payload)
                });

                this.hideTyping();

                if (!res.ok) {
                    this.appendAssistantMessage('Sorry, I encountered an issue communicating with the server. Please try again in a moment.');
                    return;
                }

                const data = await res.json();
                if (data.success) {
                    this.appendAssistantMessage(data.message, data.products || [], data.toolsUsed || []);
                } else {
                    this.appendAssistantMessage(data.message || 'I could not process your request at the moment.');
                }
            } catch (err) {
                console.error('AI chat error:', err);
                this.hideTyping();
                this.appendAssistantMessage('Connection error. Please ensure the server is active and try again.');
            } finally {
                this.isProcessing = false;
                if (this.sendBtn) this.sendBtn.disabled = false;
                if (this.inputEl) this.inputEl.focus();
            }
        }
    }

    // -------------------------------------------------------------------------
    // Initialize Widget & Page
    // -------------------------------------------------------------------------
    document.addEventListener('DOMContentLoaded', () => {
        // 1. Floating Widget Setup
        const launcherBtn = document.getElementById('bbAiLauncherBtn');
        const floatingWindow = document.getElementById('bbAiFloatingWindow');
        const closeBtn = document.getElementById('bbAiCloseBtn');
        const clearBtn = document.getElementById('bbAiClearBtn');

        if (launcherBtn && floatingWindow) {
            launcherBtn.addEventListener('click', () => {
                floatingWindow.classList.toggle('active');
                if (floatingWindow.classList.contains('active')) {
                    const input = floatingWindow.querySelector('.bb-ai-input');
                    if (input) input.focus();
                }
            });

            if (closeBtn) {
                closeBtn.addEventListener('click', () => {
                    floatingWindow.classList.remove('active');
                });
            }

            if (clearBtn) {
                clearBtn.addEventListener('click', () => {
                    const msgs = floatingWindow.querySelector('.bb-ai-messages');
                    if (msgs) {
                        msgs.innerHTML = `
                            <div class="bb-ai-msg assistant">
                                <div class="bb-ai-msg-avatar"><i class="bi bi-stars"></i></div>
                                <div class="bb-ai-bubble">
                                    <p class="mb-1"><strong>Chat cleared!</strong></p>
                                    <p class="mb-0">How can I assist you with your shopping today?</p>
                                </div>
                            </div>
                        `;
                    }
                    if (window.floatingAiChat) {
                        window.floatingAiChat.history = [];
                    }
                });
            }

            window.floatingAiChat = new BuybeeAiChat({
                container: floatingWindow,
                messagesEl: floatingWindow.querySelector('.bb-ai-messages'),
                inputEl: floatingWindow.querySelector('.bb-ai-input'),
                sendBtn: floatingWindow.querySelector('.bb-ai-btn-send'),
                micBtn: floatingWindow.querySelector('.bb-ai-btn-mic'),
                isFullPage: false
            });
        }

        // 2. Full /ai Page Setup
        const pageContainer = document.getElementById('bbAiPageContainer');
        if (pageContainer) {
            window.pageAiChat = new BuybeeAiChat({
                container: pageContainer,
                messagesEl: pageContainer.querySelector('.bb-ai-page-messages'),
                inputEl: pageContainer.querySelector('.bb-ai-input'),
                sendBtn: pageContainer.querySelector('.bb-ai-btn-send'),
                micBtn: pageContainer.querySelector('.bb-ai-btn-mic'),
                isFullPage: true
            });

            const pageClearBtn = document.getElementById('bbAiPageClearBtn');
            if (pageClearBtn) {
                pageClearBtn.addEventListener('click', () => {
                    const msgs = pageContainer.querySelector('.bb-ai-page-messages');
                    if (msgs) {
                        msgs.innerHTML = `
                            <div class="bb-ai-msg assistant">
                                <div class="bb-ai-msg-avatar"><i class="bi bi-stars"></i></div>
                                <div class="bb-ai-bubble">
                                    <p class="mb-1"><strong>Conversation reset!</strong></p>
                                    <p class="mb-0">Ask me anything about Buybee products, categories, stock, or deals.</p>
                                </div>
                            </div>
                        `;
                    }
                    if (window.pageAiChat) {
                        window.pageAiChat.history = [];
                    }
                });
            }
        }
    });

})();
