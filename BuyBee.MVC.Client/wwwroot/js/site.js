// Buybee Global Scripts
document.addEventListener("DOMContentLoaded", function () {

    // 1. Sticky Navbar shadow on scroll
    const header = document.querySelector(".bb-header");
    const btnTop = document.getElementById("btnTop");

    window.addEventListener("scroll", function () {
        const scrolled = window.scrollY;
        if (header) {
            if (scrolled > 20) {
                header.classList.add("scrolled");
            } else {
                header.classList.remove("scrolled");
            }
        }

        if (btnTop) {
            if (scrolled > 300) {
                btnTop.style.display = "flex";
            } else {
                btnTop.style.display = "none";
            }
        }
    }, { passive: true });

    if (btnTop) {
        btnTop.addEventListener("click", function (e) {
            e.preventDefault();
            window.scrollTo({ top: 0, behavior: "smooth" });
        });
    }

    // 2. Intercept all add-to-cart forms for smooth AJAX addition
    document.addEventListener("submit", function (e) {
        const form = e.target.closest(".add-to-cart-form");
        if (!form) return;

        e.preventDefault();

        const submitBtn = form.querySelector("button[type='submit']");
        const originalContent = submitBtn ? submitBtn.innerHTML : "";

        if (submitBtn) {
            submitBtn.disabled = true;
            submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-1" role="status" aria-hidden="true"></span> Adding...';
        }

        const formData = new FormData(form);

        fetch(form.action || "/Cart/AddToCart", {
            method: "POST",
            body: formData,
            headers: {
                "X-Requested-With": "XMLHttpRequest"
            }
        })
        .then(async response => {
            if (response.redirected) {
                window.location.href = response.url;
                return null;
            }
            if (!response.ok) {
                if (response.status === 401) {
                    window.location.href = "/Account/Login";
                    return null;
                }
                throw new Error("Network response was not ok");
            }
            const contentType = response.headers.get("content-type");
            if (contentType && contentType.includes("application/json")) {
                return await response.json();
            }
            // If server returned non-JSON (e.g. login HTML), navigate to login
            window.location.href = "/Account/Login";
            return null;
        })
        .then(data => {
            if (!data) return;

            if (data.requiresAuth) {
                showToast(data.message || "Please sign in to add products to your cart.", false);
                setTimeout(() => {
                    window.location.href = data.redirect || "/Account/Login";
                }, 800);
                return;
            }

            if (data.success) {
                // Update all cart count badges
                if (data.cartCount !== undefined) {
                    const countBadges = document.querySelectorAll("#cartCount, .cart-count-badge, .bb-cart-badge");
                    countBadges.forEach(b => {
                        b.textContent = data.cartCount;
                        b.classList.remove("d-none");
                        b.classList.add("pulse");
                        setTimeout(() => b.classList.remove("pulse"), 600);
                    });
                }

                showToast(data.message || "Product added to cart!", true);
            } else {
                showToast(data.message || "Failed to add product to cart.", false);
                if (data.redirect) {
                    setTimeout(() => {
                        window.location.href = data.redirect;
                    }, 1000);
                }
            }
        })
        .catch(err => {
            console.error("Cart error:", err);
            showToast("Failed to add to cart. Please try again.", false);
        })
        .finally(() => {
            if (submitBtn) {
                submitBtn.disabled = false;
                submitBtn.innerHTML = originalContent;
            }
        });
    });

    // 3. Toast Helper
    window.showToast = function (message, isSuccess) {
        const toastEl = document.getElementById("cartToast");
        if (!toastEl) return;

        const msgEl = document.getElementById("cartToastMsg");
        const iconEl = document.getElementById("cartToastIcon");

        if (msgEl) msgEl.textContent = message;
        if (iconEl) {
            if (isSuccess) {
                iconEl.className = "bi bi-check-circle-fill text-warning fs-5 me-2";
            } else {
                iconEl.className = "bi bi-exclamation-triangle-fill text-danger fs-5 me-2";
            }
        }

        if (window.bootstrap && bootstrap.Toast) {
            const toast = bootstrap.Toast.getOrCreateInstance(toastEl, { delay: 3500 });
            toast.show();
        }
    };

    // 4. Quantity Increment/Decrement handler
    document.addEventListener("click", function (e) {
        const btnMinus = e.target.closest(".qty-btn-minus");
        const btnPlus = e.target.closest(".qty-btn-plus");

        if (btnMinus) {
            const input = btnMinus.closest(".qty-group")?.querySelector(".qty-input");
            if (input) {
                let currentVal = parseInt(input.value) || 1;
                if (currentVal > 1) {
                    input.value = currentVal - 1;
                    input.dispatchEvent(new Event("change", { bubbles: true }));
                }
            }
        } else if (btnPlus) {
            const input = btnPlus.closest(".qty-group")?.querySelector(".qty-input");
            if (input) {
                let currentVal = parseInt(input.value) || 1;
                let max = parseInt(input.getAttribute("max")) || 99;
                if (currentVal < max) {
                    input.value = currentVal + 1;
                    input.dispatchEvent(new Event("change", { bubbles: true }));
                }
            }
        }
    });

    // 5. Product Details Image Thumbnail Switcher
    document.addEventListener("click", function (e) {
        const thumb = e.target.closest(".thumb-switch-img");
        if (thumb) {
            const newSrc = thumb.getAttribute("data-src") || thumb.getAttribute("src");
            const mainImg = document.getElementById("mainProductImage");
            if (mainImg && newSrc) {
                mainImg.style.opacity = "0.4";
                setTimeout(() => {
                    mainImg.src = newSrc;
                    mainImg.style.opacity = "1";
                }, 150);

                // Update active thumbnail border
                document.querySelectorAll(".thumb-switch-img").forEach(t => {
                    t.classList.remove("border-warning", "active");
                });
                thumb.classList.add("border-warning", "active");
            }
        }
    });

    // 6. Global Wishlist Toggle (Catalog & Product Cards)
    document.addEventListener("click", function (e) {
        const btn = e.target.closest(".bb-wishlist-toggle");
        if (!btn) return;

        e.preventDefault();
        e.stopPropagation();

        const prodId = btn.getAttribute("data-product-id");
        if (!prodId) return;

        const icon = btn.querySelector("i");
        const tokenInput = document.querySelector("input[name='__RequestVerificationToken']");
        const formData = new FormData();
        formData.append("productId", prodId);
        if (tokenInput) {
            formData.append("__RequestVerificationToken", tokenInput.value);
        }

        btn.disabled = true;

        fetch("/Wishlist/Toggle", {
            method: "POST",
            body: formData,
            headers: { "X-Requested-With": "XMLHttpRequest" }
        })
        .then(res => res.json())
        .then(data => {
            if (data.requiresAuth) {
                if (window.showToast) window.showToast(data.message || "Please sign in to save to your wishlist.", false);
                setTimeout(() => {
                    window.location.href = data.redirect || "/Account/Login";
                }, 800);
                return;
            }

            if (data.success) {
                if (icon) {
                    if (data.isWishlisted) {
                        icon.className = "bi bi-heart-fill text-danger";
                        btn.classList.add("bb-wishlisted");
                    } else {
                        icon.className = "bi bi-heart text-danger";
                        btn.classList.remove("bb-wishlisted");
                    }
                    btn.classList.add("pulse");
                    setTimeout(() => btn.classList.remove("pulse"), 500);
                }

                // Update all header wishlist badges
                const badges = document.querySelectorAll(".bb-wishlist-badge");
                badges.forEach(b => {
                    b.textContent = data.wishlistCount;
                    if (data.wishlistCount > 0) b.classList.remove("d-none");
                    else b.classList.add("d-none");
                });

                if (window.showToast) window.showToast(data.message, true);
            }
        })
        .catch(err => {
            console.error("Wishlist toggle error:", err);
        })
        .finally(() => {
            btn.disabled = false;
        });
    });

    // Sync wishlist product heart states on load
    fetch("/Wishlist/Count")
        .then(res => res.json())
        .then(data => {
            if (data && data.productIds && Array.isArray(data.productIds)) {
                const idSet = new Set(data.productIds);
                document.querySelectorAll(".bb-wishlist-toggle").forEach(btn => {
                    const pid = parseInt(btn.getAttribute("data-product-id"));
                    const icon = btn.querySelector("i");
                    if (pid && idSet.has(pid) && icon) {
                        icon.className = "bi bi-heart-fill text-danger";
                        btn.classList.add("bb-wishlisted");
                    }
                });
            }
        })
        .catch(() => {});
});
