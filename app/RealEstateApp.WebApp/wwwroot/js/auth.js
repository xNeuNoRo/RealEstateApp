(function () {
  "use strict";

  function refreshIcons() {
    if (window.lucide) window.lucide.createIcons();
  }

  function scrollBehavior() {
    return window.matchMedia("(prefers-reduced-motion: reduce)").matches
      ? "auto"
      : "smooth";
  }

  function showToast(type, message) {
    if (window.RealEstateApp && window.RealEstateApp.Toast) {
      window.RealEstateApp.Toast[type](message);
    }
  }

  function initSubmittingForms() {
    document.querySelectorAll("[data-auth-form]").forEach(function (form) {
      form.addEventListener("submit", function (event) {
        var isValid =
          window.jQuery && window.jQuery.validator
            ? window.jQuery(form).valid()
            : form.checkValidity();
        if (!isValid) {
          event.preventDefault();
          return;
        }
        var button = form.querySelector('button[type="submit"]:not(.hidden)');
        if (!button || button.disabled) return;
        button.disabled = true;
        button.setAttribute("aria-busy", "true");
        var content = button.querySelector(".auth-submit-content");
        if (content) {
          content.innerHTML =
            '<span class="spinner spinner-sm border-white/40 border-r-white"></span><span>Procesando...</span>';
        }
      });
    });
  }

  function initPhoneMasks() {
    document.querySelectorAll("[data-phone-mask]").forEach(function (input) {
      input.addEventListener("input", function () {
        var value = input.value.replace(/\D/g, "").slice(0, 10);
        if (value.length > 6)
          value =
            value.slice(0, 3) + "-" + value.slice(3, 6) + "-" + value.slice(6);
        else if (value.length > 3)
          value = value.slice(0, 3) + "-" + value.slice(3);
        input.value = value;
      });
    });
  }

  function initPasswordMeters() {
    document
      .querySelectorAll("[data-strength-input]")
      .forEach(function (input) {
        var meter = input
          .closest("form")
          .querySelector("[data-strength-meter]");
        if (!meter) return;
        var bar = meter.querySelector("[data-strength-bar]");
        var label = meter.querySelector("[data-strength-label]");
        var ruleElements = meter.querySelectorAll("[data-password-rule]");

        function update() {
          var value = input.value;
          var rules = {
            length: value.length >= 8,
            case: /[a-z]/.test(value) && /[A-Z]/.test(value),
            number: /\d/.test(value),
            symbol: /[^A-Za-z\d]/.test(value),
          };
          var score = Object.values(rules).filter(Boolean).length;
          var labels = [
            "Sin evaluar",
            "Débil",
            "En progreso",
            "Buena",
            "Segura",
          ];
          var colors = [
            "bg-gray-300",
            "bg-danger-500",
            "bg-warning-500",
            "bg-brand-500",
            "bg-success-500",
          ];
          bar.className =
            "h-full rounded-full transition-all duration-300 " + colors[score];
          bar.style.width = value.length ? score * 25 + "%" : "0%";
          label.textContent = labels[score];
          ruleElements.forEach(function (element) {
            var passed = rules[element.dataset.passwordRule];
            element.classList.toggle("is-valid", passed);
            element.classList.toggle("text-success-700", passed);
          });
        }

        input.addEventListener("input", update);
        update();
      });
  }

  function initPhotoUpload() {
    var zone = document.querySelector("[data-drop-zone]");
    var input = document.querySelector("[data-photo-input]");
    if (!zone || !input) return;
    var placeholder = zone.querySelector("[data-photo-placeholder]");
    var preview = zone.querySelector("[data-photo-preview]");
    var image = preview.querySelector("img");
    var name = zone.querySelector("[data-photo-name]");
    var remove = zone.querySelector("[data-photo-remove]");
    var objectUrl;

    function render(file) {
      if (!file) return;
      if (!/^image\/(jpeg|png|webp)$/.test(file.type)) {
        showToast("error", "Selecciona una imagen JPG, PNG o WebP.");
        input.value = "";
        return;
      }
      if (file.size > 15 * 1024 * 1024) {
        showToast("error", "La imagen no debe superar 15 MB.");
        input.value = "";
        return;
      }
      if (objectUrl) URL.revokeObjectURL(objectUrl);
      objectUrl = URL.createObjectURL(file);
      image.src = objectUrl;
      name.textContent = file.name;
      placeholder.classList.add("hidden");
      preview.classList.remove("hidden");
      preview.classList.add("flex");
      remove.classList.remove("hidden");
      remove.classList.add("flex");
    }

    input.addEventListener("change", function () {
      render(input.files && input.files[0]);
    });
    ["dragenter", "dragover"].forEach(function (eventName) {
      zone.addEventListener(eventName, function (event) {
        event.preventDefault();
        zone.classList.add("is-dragging");
      });
    });
    ["dragleave", "drop"].forEach(function (eventName) {
      zone.addEventListener(eventName, function (event) {
        event.preventDefault();
        zone.classList.remove("is-dragging");
      });
    });
    zone.addEventListener("drop", function (event) {
      var file = event.dataTransfer.files && event.dataTransfer.files[0];
      if (!file) return;
      var transfer = new DataTransfer();
      transfer.items.add(file);
      input.files = transfer.files;
      render(file);
    });
    remove.addEventListener("click", function (event) {
      event.preventDefault();
      input.value = "";
      if (objectUrl) URL.revokeObjectURL(objectUrl);
      preview.classList.add("hidden");
      preview.classList.remove("flex");
      remove.classList.add("hidden");
      remove.classList.remove("flex");
      placeholder.classList.remove("hidden");
    });
  }

  function initRoleCopy(form) {
    function update() {
      var selected = form.querySelector('input[name="SelectedRole"]:checked');
      form.querySelectorAll("[data-role-copy]").forEach(function (element) {
        element.classList.toggle(
          "hidden",
          !selected || element.dataset.roleCopy !== selected.value,
        );
      });
    }
    form
      .querySelectorAll('input[name="SelectedRole"]')
      .forEach(function (input) {
        input.addEventListener("change", update);
      });
    update();
  }

  function initRegisterStepper() {
    var form = document.querySelector("[data-register-stepper]");
    if (!form) return;
    var panels = Array.from(form.querySelectorAll("[data-step-panel]"));
    var indicators = Array.from(
      document.querySelectorAll("[data-step-indicator]"),
    );
    var lines = Array.from(document.querySelectorAll("[data-step-line]"));
    var previous = form.querySelector("[data-step-previous]");
    var next = form.querySelector("[data-step-next]");
    var submit = form.querySelector("[data-step-submit]");
    var counter = document.getElementById("step-counter");
    var current = Math.min(
      Math.max(Number(form.dataset.initialStep) || 1, 1),
      panels.length,
    );

    function firstInvalid(panel) {
      var fields = Array.from(
        panel.querySelectorAll("input, select, textarea"),
      ).filter(function (field) {
        return field.type !== "hidden" && field.name !== "Website";
      });
      var invalid;
      fields.forEach(function (field) {
        var isValid =
          window.jQuery && window.jQuery.validator
            ? window.jQuery(field).valid()
            : field.checkValidity();
        if (!isValid && !invalid) invalid = field;
      });
      return invalid;
    }

    function validateCurrent() {
      var invalid = firstInvalid(panels[current - 1]);
      if (!invalid) return true;
      invalid.focus({ preventScroll: true });
      invalid.scrollIntoView({ behavior: scrollBehavior(), block: "center" });
      return false;
    }

    function updateState() {
      indicators.forEach(function (indicator, index) {
        indicator.classList.toggle("is-active", index + 1 === current);
        indicator.classList.toggle("is-complete", index + 1 < current);
        if (index + 1 === current)
          indicator.setAttribute("aria-current", "step");
        else indicator.removeAttribute("aria-current");
      });
      lines.forEach(function (line, index) {
        line.classList.toggle("is-complete", index + 1 < current);
      });
      previous.classList.toggle("hidden", current === 1);
      next.classList.toggle("hidden", current === panels.length);
      submit.classList.toggle("hidden", current !== panels.length);
      counter.textContent = "Paso " + current + " de " + panels.length;
    }

    function show(step, direction) {
      var oldPanel = panels[current - 1];
      var newPanel = panels[step - 1];
      oldPanel.classList.add(
        direction > 0 ? "auth-panel-leave-left" : "auth-panel-leave-right",
      );
      window.setTimeout(function () {
        oldPanel.classList.add("hidden");
        oldPanel.classList.remove(
          "auth-panel-leave-left",
          "auth-panel-leave-right",
        );
        newPanel.classList.remove("hidden");
        newPanel.classList.add(
          direction > 0 ? "auth-panel-enter-right" : "auth-panel-enter-left",
        );
        window.setTimeout(function () {
          newPanel.classList.remove(
            "auth-panel-enter-right",
            "auth-panel-enter-left",
          );
        }, 320);
        var heading = newPanel.querySelector("h2");
        if (heading) {
          heading.setAttribute("tabindex", "-1");
          heading.focus({ preventScroll: true });
        }
      }, 150);

      current = step;
      updateState();
      window.scrollTo({ top: 0, behavior: scrollBehavior() });
    }

    next.addEventListener("click", function () {
      if (validateCurrent()) show(current + 1, 1);
    });
    previous.addEventListener("click", function () {
      show(current - 1, -1);
    });

    panels.forEach(function (panel, index) {
      panel.classList.toggle("hidden", index + 1 !== current);
    });
    updateState();

    var serverInvalid = panels[current - 1].querySelector(
      ".input-validation-error",
    );
    if (serverInvalid) {
      window.setTimeout(function () {
        serverInvalid.focus();
      }, 0);
    }

    initRoleCopy(form);
  }

  document.addEventListener("DOMContentLoaded", function () {
    initSubmittingForms();
    initPhoneMasks();
    initPasswordMeters();
    initPhotoUpload();
    initRegisterStepper();
    refreshIcons();
  });
})();
