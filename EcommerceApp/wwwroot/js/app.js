const CART_KEY = 'shoplocal_cart';

// --- Utilities ---
const formatCurrency = (amount) => '$' + parseFloat(amount).toFixed(2);
const getCart = () => JSON.parse(localStorage.getItem(CART_KEY) || '[]');
const setCart = (cart) => {
    localStorage.setItem(CART_KEY, JSON.stringify(cart));
    updateCartCount();
};
const updateCartCount = () => {
    const cart = getCart();
    const count = cart.reduce((sum, item) => sum + item.quantity, 0);
    document.querySelectorAll('#cart-count').forEach(el => el.textContent = count);
};

const categoryColors = {
    'Electronics': '#3b82f6',
    'Clothing': '#8b5cf6',
    'Home': '#10b981',
    'default': '#64748b'
};

const getPlaceholderColor = (category) => categoryColors[category] || categoryColors.default;

function hashColor(name) {
    const colors = ['#3b82f6','#8b5cf6','#10b981','#f59e0b','#ef4444','#ec4899'];
    const hash = name.split('').reduce((a, b) => a + b.charCodeAt(0), 0);
    return colors[hash % colors.length];
}

// --- API ---
async function fetchProducts(category = '', search = '') {
    const params = new URLSearchParams();
    if (category) params.append('category', category);
    if (search) params.append('search', search);
    const res = await fetch(`/api/products?${params}`);
    if (!res.ok) throw new Error('Failed to load products');
    return res.json();
}

async function fetchProduct(id) {
    const res = await fetch(`/api/products/${id}`);
    if (!res.ok) throw new Error('Product not found');
    return res.json();
}

async function placeOrder(orderData) {
    const res = await fetch('/api/orders', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(orderData)
    });
    if (!res.ok) {
        const err = await res.json().catch(() => ({ error: 'Unknown error' }));
        throw new Error(err.error || 'Order failed');
    }
    return res.json();
}

// --- Pages ---
function initIndex() {
    const grid = document.getElementById('product-grid');
    const searchInput = document.getElementById('search-input');
    const searchBtn = document.getElementById('search-btn');
    const filterContainer = document.getElementById('category-filters');
    let currentCategory = '';
    let currentSearch = '';

    async function load() {
        grid.innerHTML = '<p>Loading products...</p>';
        try {
            const products = await fetchProducts(currentCategory, currentSearch);
            if (products.length === 0) {
                grid.innerHTML = '<p class="empty-state">No products found.</p>';
                return;
            }
            grid.innerHTML = products.map(p => renderProductCard(p)).join('');
            attachProductEvents(products);
        } catch (e) {
            grid.innerHTML = `<p class="error-message">${e.message}</p>`;
        }
    }

    function renderProductCard(p) {
        const outOfStock = p.stockQuantity <= 0;
        const initial = p.name.charAt(0);
        const color = getPlaceholderColor(p.category);
        return `
            <div class="product-card">
                <div class="product-image" style="background:${color}">${initial}</div>
                <div class="product-info">
                    <div class="product-category">${p.category}</div>
                    <div class="product-name">${p.name}</div>
                    <div class="product-desc">${p.description}</div>
                    <div class="product-footer">
                        <span class="product-price">${formatCurrency(p.price)}</span>
                        ${outOfStock
                            ? '<span class="out-of-stock">Out of Stock</span>'
                            : `<button class="btn add-to-cart" data-id="${p.id}">Add to Cart</button>`
                        }
                    </div>
                </div>
            </div>
        `;
    }

    function attachProductEvents(products) {
        document.querySelectorAll('.add-to-cart').forEach(btn => {
            btn.addEventListener('click', async () => {
                const id = parseInt(btn.dataset.id);
                const product = products.find(p => p.id === id);
                if (!product) return;

                const cart = getCart();
                const existing = cart.find(i => i.productId === id);
                const currentQty = existing ? existing.quantity : 0;

                if (currentQty + 1 > product.stockQuantity) {
                    alert('Cannot add more than available stock.');
                    return;
                }

                if (existing) {
                    existing.quantity += 1;
                } else {
                    cart.push({
                        productId: product.id,
                        productName: product.name,
                        quantity: 1,
                        unitPrice: product.price
                    });
                }
                setCart(cart);
                btn.textContent = 'Added!';
                setTimeout(() => btn.textContent = 'Add to Cart', 1000);
            });
        });
    }

    async function buildFilters() {
        const products = await fetchProducts();
        const categories = [...new Set(products.map(p => p.category))];
        filterContainer.innerHTML = `<button class="filter-btn active" data-category="">All</button>` +
            categories.map(c => `<button class="filter-btn" data-category="${c}">${c}</button>`).join('');

        filterContainer.querySelectorAll('.filter-btn').forEach(btn => {
            btn.addEventListener('click', () => {
                filterContainer.querySelectorAll('.filter-btn').forEach(b => b.classList.remove('active'));
                btn.classList.add('active');
                currentCategory = btn.dataset.category;
                load();
            });
        });
    }

    searchBtn.addEventListener('click', () => {
        currentSearch = searchInput.value.trim();
        load();
    });
    searchInput.addEventListener('keypress', (e) => {
        if (e.key === 'Enter') {
            currentSearch = searchInput.value.trim();
            load();
        }
    });

    (async () => {
        await buildFilters();
        await load();
    })();
}

function initCart() {
    const cartItemsEl = document.getElementById('cart-items');
    const emptyEl = document.getElementById('empty-cart');
    const contentEl = document.getElementById('cart-content');

    function render() {
        const cart = getCart();
        if (cart.length === 0) {
            contentEl.style.display = 'none';
            emptyEl.style.display = 'block';
            return;
        }
        contentEl.style.display = 'block';
        emptyEl.style.display = 'none';

        let subtotal = 0;
        cartItemsEl.innerHTML = cart.map(item => {
            const total = item.quantity * item.unitPrice;
            subtotal += total;
            const color = hashColor(item.productName);
            return `
                <div class="cart-item">
                    <div class="cart-item-image" style="background:${color}">${item.productName.charAt(0)}</div>
                    <div class="cart-item-details">
                        <div class="cart-item-name">${item.productName}</div>
                        <div class="cart-item-price">${formatCurrency(item.unitPrice)} each</div>
                    </div>
                    <div class="cart-item-actions">
                        <input type="number" min="1" value="${item.quantity}" data-id="${item.productId}" class="qty-input">
                        <button class="btn btn-danger btn-sm remove-btn" data-id="${item.productId}">Remove</button>
                    </div>
                    <div style="font-weight:600;min-width:70px;text-align:right;">${formatCurrency(total)}</div>
                </div>
            `;
        }).join('');

        const tax = subtotal * 0.08;
        const grand = subtotal + tax;
        document.getElementById('subtotal').textContent = formatCurrency(subtotal);
        document.getElementById('tax').textContent = formatCurrency(tax);
        document.getElementById('grand-total').textContent = formatCurrency(grand);

        cartItemsEl.querySelectorAll('.qty-input').forEach(input => {
            input.addEventListener('change', async () => {
                const id = parseInt(input.dataset.id);
                let qty = parseInt(input.value);
                if (isNaN(qty) || qty < 1) qty = 1;

                const cart = getCart();
                const item = cart.find(i => i.productId === id);
                if (!item) return;

                try {
                    const product = await fetchProduct(id);
                    if (qty > product.stockQuantity) {
                        alert(`Only ${product.stockQuantity} available in stock.`);
                        input.value = item.quantity;
                        return;
                    }
                    item.quantity = qty;
                    setCart(cart);
                    render();
                } catch (e) {
                    alert(e.message);
                    input.value = item.quantity;
                }
            });
        });

        cartItemsEl.querySelectorAll('.remove-btn').forEach(btn => {
            btn.addEventListener('click', () => {
                const id = parseInt(btn.dataset.id);
                let cart = getCart();
                cart = cart.filter(i => i.productId !== id);
                setCart(cart);
                render();
            });
        });
    }

    render();
}

function initCheckout() {
    const form = document.getElementById('checkout-form');
    const errorEl = document.getElementById('checkout-error');
    const itemsEl = document.getElementById('checkout-items');

    function renderSummary() {
        const cart = getCart();
        if (cart.length === 0) {
            window.location.href = '/cart.html';
            return;
        }
        let subtotal = 0;
        itemsEl.innerHTML = cart.map(item => {
            const total = item.quantity * item.unitPrice;
            subtotal += total;
            return `<div class="checkout-item"><span>${item.productName} x${item.quantity}</span><span>${formatCurrency(total)}</span></div>`;
        }).join('');
        const tax = subtotal * 0.08;
        const total = subtotal + tax;
        document.getElementById('checkout-subtotal').textContent = formatCurrency(subtotal);
        document.getElementById('checkout-tax').textContent = formatCurrency(tax);
        document.getElementById('checkout-total').textContent = formatCurrency(total);
    }

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        errorEl.textContent = '';

        const name = document.getElementById('full-name').value.trim();
        const email = document.getElementById('email').value.trim();
        const address = document.getElementById('address').value.trim();

        let valid = true;
        [document.getElementById('full-name'), document.getElementById('email'), document.getElementById('address')].forEach(el => {
            if (!el.value.trim()) {
                el.classList.add('invalid');
                valid = false;
            } else {
                el.classList.remove('invalid');
            }
        });

        if (!valid) {
            errorEl.textContent = 'Please fill in all required fields.';
            return;
        }
        if (!email.includes('@')) {
            document.getElementById('email').classList.add('invalid');
            errorEl.textContent = 'Please enter a valid email address.';
            return;
        }

        const cart = getCart();
        if (cart.length === 0) {
            errorEl.textContent = 'Your cart is empty.';
            return;
        }

        try {
            const confirmation = await placeOrder({
                customerName: name,
                customerEmail: email,
                shippingAddress: address,
                items: cart
            });
            setCart([]);
            localStorage.setItem('last_order', JSON.stringify(confirmation));
            window.location.href = '/confirmation.html';
        } catch (err) {
            errorEl.textContent = err.message;
        }
    });

    renderSummary();
}

function initConfirmation() {
    const order = JSON.parse(localStorage.getItem('last_order') || '{}');
    if (!order.orderId) {
        document.querySelector('.confirmation-box').innerHTML = '<p>No order found.</p><a href="/" class="btn">Go Home</a>';
        return;
    }
    document.getElementById('order-id').textContent = order.orderId;
    document.getElementById('order-email').textContent = order.customerEmail;
    document.getElementById('order-total').textContent = formatCurrency(order.totalAmount);
    document.getElementById('order-date').textContent = new Date(order.orderDate).toLocaleString();
    updateCartCount();
}

// --- Router ---
document.addEventListener('DOMContentLoaded', () => {
    updateCartCount();
    const path = window.location.pathname;
    if (path === '/' || path === '/index.html') initIndex();
    else if (path === '/cart.html') initCart();
    else if (path === '/checkout.html') initCheckout();
    else if (path === '/confirmation.html') initConfirmation();
});
