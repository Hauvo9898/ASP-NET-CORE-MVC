// ===== AHU site.js =====
// Thay cho các phần tương tác client-side của script.js gốc. Toàn bộ dữ liệu
// sản phẩm/giỏ hàng/đơn hàng giờ lấy qua Controller + EF Core + SQL Server,
// KHÔNG còn dùng localStorage. File này chỉ còn xử lý UI thuần (menu, search,
// toast, modal Quick View) và vài lệnh fetch() nhỏ tới Controller.

function getAfToken() {
  var f = document.getElementById('af-token-form');
  return f ? f.querySelector('input[name="__RequestVerificationToken"]').value : '';
}

// ========== MOBILE MENU ==========
function openMobileMenu() { document.getElementById('mobile-menu').classList.add('open'); }
function closeMobileMenu() { document.getElementById('mobile-menu').classList.remove('open'); }

// ========== USER ACCOUNT DROPDOWN (navbar) ==========
// Yêu cầu chính: chỉ cần DI CHUỘT vào là hiện menu — việc này đã xử lý thuần
// bằng CSS (:hover trong style.css), không cần JS. Hàm dưới đây chỉ lo phần
// dự phòng cho thiết bị không có hover thật (cảm ứng, bàn phím): bấm để mở/đóng,
// và luôn đóng khi rê chuột ra khỏi vùng menu để không bị "kẹt mở".
function toggleUserDropdown(event) {
  event.stopPropagation();
  var wrap = document.getElementById('nav-user-dropdown');
  var menu = document.getElementById('nav-user-dropdown-menu');
  var btn = document.getElementById('nav-user-btn');
  if (!wrap || !menu) return;
  var isOpen = menu.classList.toggle('open');
  wrap.classList.toggle('open', isOpen);
  if (btn) btn.setAttribute('aria-expanded', isOpen ? 'true' : 'false');
}

(function () {
  var wrap = document.getElementById('nav-user-dropdown');
  if (!wrap) return;
  wrap.addEventListener('mouseleave', function () {
    document.getElementById('nav-user-dropdown-menu').classList.remove('open');
    wrap.classList.remove('open');
    var btn = document.getElementById('nav-user-btn');
    if (btn) btn.setAttribute('aria-expanded', 'false');
  });
})();

document.addEventListener('click', function (e) {
  var wrap = document.getElementById('nav-user-dropdown');
  var menu = document.getElementById('nav-user-dropdown-menu');
  if (!wrap || !menu) return;
  if (!wrap.contains(e.target)) {
    menu.classList.remove('open');
    wrap.classList.remove('open');
    var btn = document.getElementById('nav-user-btn');
    if (btn) btn.setAttribute('aria-expanded', 'false');
  }
});

document.addEventListener('keydown', function (e) {
  if (e.key !== 'Escape') return;
  var menu = document.getElementById('nav-user-dropdown-menu');
  var wrap = document.getElementById('nav-user-dropdown');
  if (menu) menu.classList.remove('open');
  if (wrap) wrap.classList.remove('open');
});

// ========== SEARCH OVERLAY ==========
function openSearch() {
  document.getElementById('search-overlay').classList.add('open');
  setTimeout(function () { var inp = document.getElementById('search-input'); if (inp) inp.focus(); }, 100);
}
function closeSearch() {
  document.getElementById('search-overlay').classList.remove('open');
  document.getElementById('search-input').value = '';
  document.getElementById('search-results-wrap').style.display = 'none';
  document.getElementById('search-no-results').style.display = 'none';
}

function escHtml(str) {
  if (!str) return '';
  return String(str).replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');
}
function formatPrice(price) {
  var amount = Math.floor(Number(price));
  if (isNaN(amount)) return '0 VND';
  return amount.toString().replace(/\B(?=(\d{3})+(?!\d))/g, '.') + ' VND';
}

var searchDebounce = null;
function handleSearchInput(val) {
  clearTimeout(searchDebounce);
  if (val.trim().length < 2) {
    document.getElementById('search-results-wrap').style.display = 'none';
    document.getElementById('search-no-results').style.display = 'none';
    return;
  }
  searchDebounce = setTimeout(function () {
    fetch('/Product/SearchSuggestions?q=' + encodeURIComponent(val))
      .then(function (r) { return r.json(); })
      .then(function (results) {
        var grid = document.getElementById('search-results-grid');
        var wrap = document.getElementById('search-results-wrap');
        var noRes = document.getElementById('search-no-results');
        if (results.length > 0) {
          grid.innerHTML = results.map(function (p) {
            return '<a class="search-result-item" href="/Product/Details/' + p.id + '">' +
              '<div class="search-result-img"><img src="' + escHtml(p.image) + '" alt="' + escHtml(p.name) + '" referrerpolicy="no-referrer" onerror="this.src=\'https://picsum.photos/seed/fashion/800/1000\'" /></div>' +
              '<div><p class="search-result-name">' + escHtml(p.name) + '</p>' +
              '<p class="search-result-cat">' + escHtml(p.type) + '</p>' +
              '<p class="search-result-price">' + (p.discount > 0 && p.originalPrice > p.price ? '<span style="text-decoration:line-through;color:#9ca3af;font-weight:400;margin-right:6px">' + formatPrice(p.originalPrice) + '</span>' : '') + formatPrice(p.price) + '</p></div></a>';
          }).join('');
          wrap.style.display = 'block';
          noRes.style.display = 'none';
        } else {
          wrap.style.display = 'none';
          noRes.style.display = 'block';
        }
      });
  }, 250);
}
function submitSearch() {
  var val = document.getElementById('search-input').value.trim();
  if (!val) return;
  closeSearch();
  window.location.href = '/Product/Index?q=' + encodeURIComponent(val);
}

// ========== TOAST ==========
var toastTimer;
function showToast(msg, type) {
  var toast = document.getElementById('toast');
  var toastMsg = document.getElementById('toast-msg');
  var toastIcon = document.getElementById('toast-icon');
  toast.className = 'show ' + (type || 'success');
  toastMsg.textContent = msg;
  toastIcon.innerHTML = type === 'error'
    ? '<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/></svg>'
    : '<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="#D4AF37" stroke-width="2"><polyline points="20 6 9 17 4 12"/></svg>';
  clearTimeout(toastTimer);
  toastTimer = setTimeout(function () { toast.className = type || 'success'; }, 3000);
}

function updateCartBadgeCount(count) {
  var badge = document.getElementById('cart-badge');
  if (!badge) return;
  badge.textContent = count;
  badge.style.display = count > 0 ? 'flex' : 'none';
}

// ========== NAVBAR SCROLL (chỉ đổi màu trên trang chủ, xem _Layout.cshtml) ==========
window.addEventListener('scroll', function () {
  var navbar = document.getElementById('navbar');
  if (!navbar) return;
  var isHome = document.body.getAttribute('data-is-home') === '1';
  if (!isHome) return; // trang khác luôn hiển thị navbar đặc (server-render sẵn)
  var ids = ['nav-logo', 'nav-shop-link', 'nav-featured-link', 'nav-contact-link', 'nav-news-link', 'nav-forum-link', 'nav-search-btn', 'nav-user-btn', 'nav-cart-btn', 'mobile-menu-btn'];
  if (window.scrollY > 50) {
    navbar.classList.add('scrolled');
    ids.forEach(function (id) {
      var el = document.getElementById(id);
      if (el) { el.classList.remove('nav-color-top'); el.classList.add('nav-color-scrolled'); }
    });
  } else {
    navbar.classList.remove('scrolled');
    ids.forEach(function (id) {
      var el = document.getElementById(id);
      if (el) { el.classList.remove('nav-color-scrolled'); el.classList.add('nav-color-top'); }
    });
  }
});

// ========== QUICK ADD (nút thêm nhanh trên thẻ sản phẩm) ==========
function quickAddToCart(productId, event) {
  if (event) event.stopPropagation();
  var body = new URLSearchParams();
  body.set('productId', productId);
  body.set('quantity', 1);
  body.set('__RequestVerificationToken', getAfToken());

  fetch('/Cart/Add', {
    method: 'POST',
    headers: { 'X-Requested-With': 'XMLHttpRequest', 'Content-Type': 'application/x-www-form-urlencoded' },
    body: body.toString()
  }).then(function (r) { return r.json(); })
    .then(function (data) {
      updateCartBadgeCount(data.count);
      showToast('Đã thêm vào giỏ hàng');
    }).catch(function () { showToast('Có lỗi xảy ra', 'error'); });
}

// ========== QUICK VIEW MODAL ==========
var COLOR_HEX = { Black: '#000000', White: '#FFFFFF', Beige: '#F5F5DC', Gold: '#D4AF37', Navy: '#000080', Red: '#FF0000', Grey: '#808080', Brown: '#8B4513', Pink: '#FFC0CB' };
var qv = { product: null, qty: 1, size: null, color: null };

function openQuickView(productId, event) {
  if (event) event.stopPropagation();
  fetch('/Product/QuickView/' + productId).then(function (r) { return r.json(); }).then(function (p) {
    qv.product = p; qv.qty = 1; qv.size = null; qv.color = null;

    document.getElementById('qv-img').src = p.image || '';
    document.getElementById('qv-cat').textContent = p.type || '';
    document.getElementById('qv-name').textContent = p.name || '';
    document.getElementById('qv-price').textContent = formatPrice(p.price);

    var qvBadge = document.getElementById('qv-sale-badge');
    var qvOriginal = document.getElementById('qv-price-original');
    if (p.discount > 0) {
      qvBadge.textContent = '-' + p.discount + '% SALE';
      qvBadge.style.display = 'inline-flex';
    } else {
      qvBadge.style.display = 'none';
    }
    if (p.originalPrice && p.originalPrice > p.price) {
      qvOriginal.textContent = formatPrice(p.originalPrice);
      qvOriginal.style.display = 'inline';
    } else {
      qvOriginal.style.display = 'none';
    }
    document.getElementById('qv-desc').textContent = p.description || '';
    document.getElementById('qv-qty').textContent = '1';
    document.getElementById('qv-detail-link').href = '/Product/Details/' + p.id;

    var sizesWrap = document.getElementById('qv-sizes-wrap');
    var sizesBox = document.getElementById('qv-sizes');
    if (p.sizes && p.sizes.length > 0) {
      qv.size = p.sizes[0];
      sizesBox.innerHTML = p.sizes.map(function (s, i) {
        return '<button type="button" class="size-btn' + (i === 0 ? ' active' : '') + '" onclick="qvSelectSize(\'' + s + '\', this)">' + escHtml(s) + '</button>';
      }).join('');
      sizesWrap.style.display = 'block';
    } else { sizesWrap.style.display = 'none'; }

    var colorsWrap = document.getElementById('qv-colors-wrap');
    var colorsBox = document.getElementById('qv-colors');
    if (p.colors && p.colors.length > 0) {
      qv.color = p.colors[0];
      colorsBox.innerHTML = p.colors.map(function (c, i) {
        var hex = COLOR_HEX[c] || '#ccc';
        return '<button type="button" class="color-btn' + (i === 0 ? ' active' : '') + '" style="background:' + hex + '" title="' + escHtml(c) + '" onclick="qvSelectColor(\'' + c + '\', this)"></button>';
      }).join('');
      colorsWrap.style.display = 'block';
    } else { colorsWrap.style.display = 'none'; }

    document.getElementById('quick-view-modal').classList.add('open');
  });
}
function closeQuickView() { document.getElementById('quick-view-modal').classList.remove('open'); }
function qvSelectSize(s, el) {
  qv.size = s;
  el.parentElement.querySelectorAll('.size-btn').forEach(function (b) { b.classList.remove('active'); });
  el.classList.add('active');
}
function qvSelectColor(c, el) {
  qv.color = c;
  el.parentElement.querySelectorAll('.color-btn').forEach(function (b) { b.classList.remove('active'); });
  el.classList.add('active');
}
function qvQtyChange(delta) {
  qv.qty = Math.max(1, qv.qty + delta);
  document.getElementById('qv-qty').textContent = qv.qty;
}
function qvAddToCart() {
  if (!qv.product) return;
  var body = new URLSearchParams();
  body.set('productId', qv.product.id);
  body.set('quantity', qv.qty);
  if (qv.size) body.set('size', qv.size);
  if (qv.color) body.set('color', qv.color);
  body.set('__RequestVerificationToken', getAfToken());

  fetch('/Cart/Add', {
    method: 'POST',
    headers: { 'X-Requested-With': 'XMLHttpRequest', 'Content-Type': 'application/x-www-form-urlencoded' },
    body: body.toString()
  }).then(function (r) { return r.json(); })
    .then(function (data) {
      updateCartBadgeCount(data.count);
      showToast('Đã thêm vào giỏ hàng');
      closeQuickView();
    }).catch(function () { showToast('Có lỗi xảy ra', 'error'); });
}

// ========== AUTH TABS (Views/Account/Login.cshtml) ==========
function switchAuthMode(mode) {
  document.getElementById('tab-customer').classList.toggle('active', mode === 'customer');
  document.getElementById('tab-admin').classList.toggle('active', mode === 'admin');
  document.getElementById('auth-form-customer').classList.toggle('active', mode === 'customer');
  document.getElementById('auth-form-admin').classList.toggle('active', mode === 'admin');
  document.getElementById('auth-title').textContent = mode === 'admin' ? 'Cổng Quản trị' : 'Đăng nhập Khách hàng';
  document.getElementById('auth-subtitle').textContent = mode === 'admin'
    ? 'Dành riêng cho quản trị viên hệ thống'
    : 'Nhập thông tin của bạn để truy cập tài khoản';
}

// ========== PRODUCT DETAIL PAGE (Views/Product/Details.cshtml) ==========
function pdQtyChange(delta) {
  var input = document.getElementById('pd-qty-input');
  var span = document.getElementById('pd-qty');
  var val = Math.max(1, parseInt(input.value || '1', 10) + delta);
  input.value = val;
  span.textContent = val;
}

// ========== CONTACT FORM (Views/Contact/Index.cshtml) ==========
function selectTopic(topic, btn) {
  document.querySelectorAll('.contact-topic-btn').forEach(function (b) { b.classList.remove('active'); });
  btn.classList.add('active');
  document.getElementById('ct-topic-input').value = topic;
  document.getElementById('ct-order-field').style.display = topic === 'ho-tro' ? 'block' : 'none';
}
function setRating(val) {
  document.getElementById('ct-rating-input').value = val;
  document.querySelectorAll('.rating-star').forEach(function (s) {
    s.classList.toggle('active', parseInt(s.getAttribute('data-val'), 10) <= val);
  });
  var labels = { 1: 'Rất tệ', 2: 'Tệ', 3: 'Bình thường', 4: 'Tốt', 5: 'Rất tốt' };
  document.getElementById('rating-label').textContent = labels[val] || 'Chưa đánh giá';
}
function updateCharCount(el) {
  document.getElementById('ct-char-count').textContent = el.value.length;
}

// ========== FORUM: reaction kiểu Facebook (Views/Forum/Details.cshtml) ==========
// Hover/chạm nút chính bung tray 5 icon (CSS lo phần hiện/ẩn qua :hover).
// Bấm 1 icon trong tray, hoặc bấm thẳng nút chính (dùng lại đúng reaction hiện tại,
// bấm lại = bỏ react) — cả 2 chạy chung 1 hàm vì backend đã tự xử lý đúng logic đó.
var FORUM_REACTION_EMOJI = { like: '👍', love: '❤️', haha: '😂', sad: '😢', angry: '😠' };
var FORUM_REACTION_LABEL = { like: 'Thích', love: 'Yêu thích', haha: 'Haha', sad: 'Buồn', angry: 'Giận' };

function fbPickReaction(postId, type) {
  var body = new URLSearchParams();
  body.set('postId', postId);
  body.set('reactionType', type);
  body.set('__RequestVerificationToken', getAfToken());

  fetch('/Forum/React', {
    method: 'POST',
    headers: { 'X-Requested-With': 'XMLHttpRequest', 'Content-Type': 'application/x-www-form-urlencoded' },
    body: body.toString()
  }).then(function (r) { return r.json(); })
    .then(function (data) {
      if (!data.success) { showToast('Có lỗi xảy ra', 'error'); return; }

      var btn = document.getElementById('fb-main-btn-' + postId);
      var icon = document.getElementById('fb-main-icon-' + postId);
      var label = document.getElementById('fb-main-label-' + postId);
      var summary = document.getElementById('fb-summary-' + postId);
      var tray = document.getElementById('fb-tray-' + postId);
      var activeType = data.myReaction || 'like';

      icon.textContent = FORUM_REACTION_EMOJI[activeType];
      label.textContent = data.myReaction ? FORUM_REACTION_LABEL[activeType] : 'Thích';
      btn.classList.toggle('reacted', !!data.myReaction);
      btn.setAttribute('onclick', "fbPickReaction(" + postId + ",'" + activeType + "')");

      if (tray) {
        tray.querySelectorAll('.fb-tray-emoji').forEach(function (b) {
          b.classList.toggle('mine', data.myReaction && b.getAttribute('data-type') === data.myReaction);
        });
      }

      if (summary) {
        if (data.total > 0) {
          var stack = data.topTypes.map(function (t) { return FORUM_REACTION_EMOJI[t]; }).join('');
          summary.innerHTML = '<span class="emoji-stack">' + stack + '</span><span id="fb-total-' + postId + '">' + data.total + '</span>';
        } else {
          summary.innerHTML = '<span id="fb-total-' + postId + '" style="display:none">0</span>';
        }
      }
    })
    .catch(function () { showToast('Có lỗi xảy ra', 'error'); });
}

// ========== ADMIN: format number input as "5.000.000" while typing (dùng ở Bước 4 phần Admin) ==========
function formatPriceInput(input) {
  var digits = input.value.replace(/\D/g, '');
  input.value = digits.replace(/\B(?=(\d{3})+(?!\d))/g, '.');
}
