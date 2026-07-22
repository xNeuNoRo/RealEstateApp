(function () {
  "use strict";

  function initSearchForm() {
    var form = document.querySelector("[data-home-search]");
    if (!form || form.dataset.bound) return;
    form.dataset.bound = "1";
    form.addEventListener("submit", function () {
      var button = form.querySelector("[data-home-search-submit]");
      if (!button || button.disabled) return;
      button.disabled = true;
      button.setAttribute("aria-busy", "true");
      button.innerHTML = '<span class="spinner spinner-sm border-white/40 border-r-white"></span>Buscando...';
    });
  }

  function initGallery() {
    var mainImage = document.getElementById("property-main-image");
    if (!mainImage) return;
    document.querySelectorAll("[data-gallery-image]").forEach(function (button) {
      button.addEventListener("click", function () {
        var source = button.dataset.galleryImage;
        if (!source || source === mainImage.src) return;
        mainImage.style.opacity = "0";
        window.setTimeout(function () {
          mainImage.src = source;
          mainImage.style.opacity = "1";
        }, 160);
      });
    });
  }

  document.addEventListener("DOMContentLoaded", function () {
    initSearchForm();
    initGallery();
    if (window.lucide) window.lucide.createIcons();
  });
})();
