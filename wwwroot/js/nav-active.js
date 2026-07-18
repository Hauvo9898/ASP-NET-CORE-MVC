// Lab 10: active nav bang JS thuan - so khop location.pathname voi href cua cac the <a>
// trong thanh dieu huong. Chay song song voi cach server dang tinh san class active
// (navColorClass trong _Layout.cshtml, activeTab trong _AdminLayout.cshtml) - khong thay
// the, chi bo sung thao tac client-side dung ky thuat giao trinh yeu cau.
document.addEventListener('DOMContentLoaded', function () {
  var links = document.querySelectorAll('.nav-links-desktop a, .admin-nav a');
  var currentPath = window.location.pathname.toLowerCase();

  links.forEach(function (link) {
    var href = link.getAttribute('href');
    if (!href || href === '/') return;

    if (currentPath === href.toLowerCase() || currentPath.indexOf(href.toLowerCase() + '/') === 0) {
      link.classList.add('nav-link-active-js');
    }
  });
});
